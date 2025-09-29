app.controller('ChatRoomController', function ($scope, $timeout, $filter, ConsultationService, AppointmentService) {

    $scope.messages = [];
    $scope.message = '';
    $scope.user = window.currentUser || 'Unknown User';
    $scope.appointmentID = window.appointmentID;
    $scope.callerName = '';
    $scope.showVideoUI = false;
    $scope.chatDisabled = false;
    $scope.callDisabled = false;
    $scope.remainingTime = 0;
    $scope.currentUserRole = window.currentUserRole === "Doctor";
    $scope.timerDisplay = "";

    let callRequestActive = false;
    let localStream = null;
    let peerConnection = null;
    let pendingCandidates = [];
    const rtcConfig = { iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] };

    let recognition;
    let recognizing = false;

    let mediaRecorder;
    let audioChunks = [];

    let timer = null;

    function startCountdown(endTime) {
        // Make sure we stop any existing timer first
        if (timer) {
            clearInterval(timer);
        }

        timer = setInterval(function () {
            let now = new Date().getTime();
            let distance = endTime - now;

            if (distance <= 0) {
                clearInterval(timer);
                timer = null;
                stopSession();
                chat.server.markCompleted($scope.appointmentID);
                return;
            }

            let hours = Math.floor((distance / (1000 * 60 * 60)) % 24);
            let minutes = Math.floor((distance / (1000 * 60)) % 60);
            let seconds = Math.floor((distance / 1000) % 60);

            $scope.$apply(function () {
                $scope.timerDisplay =
                    ("0" + hours).slice(-2) + ":" +
                    ("0" + minutes).slice(-2) + ":" +
                    ("0" + seconds).slice(-2);
            });
        }, 1000);
    }

    function endSessionUI() {
        $scope.chatDisabled = true;
        $scope.callDisabled = true;
        $scope.$applyAsync();
    }

    function checkAppointmentStatus() {
        AppointmentService.getAppointmentStatus($scope.appointmentID)
            .then(function (response) {
                let status = response.data.status;
                let endTimeObj = $filter('dotNetDate')(response.data.endTime);

                if (!endTimeObj || isNaN(endTimeObj.getTime())) {
                    console.warn("Invalid endTime from API:", response.data.endTime);
                    stopSession();
                    return;
                }

                if (status === 6 || status === "Ended") {
                    stopSession(); // appointment already ended
                } else {
                    startCountdown(endTimeObj.getTime()); // only start if still active
                }
            })
            .catch(function (err) {
                console.error("Error fetching appointment status", err);
            });
    }

    function stopSession() {
        if (timer) {
            clearInterval(timer);
            timer = null;
        }
        $scope.timerDisplay = "00:00:00";
        endSessionUI();
        cleanupCall();
        stopLiveTranscription();
        stopRecording();
    }

    function stopRecording() {
        if (mediaRecorder && mediaRecorder.state !== "inactive") {
            mediaRecorder.stop();
        }
    }

    function uploadRecording(audioBlob) {
        const formData = new FormData();
        formData.append("file", audioBlob, "call_audio.webm");
        formData.append("appointmentId", $scope.appointmentID);

        fetch('/Transcription/UploadAudio', {
            method: 'POST',
            body: formData
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    console.log("Transcript saved:", data.transcript);
                } else {
                    console.error("Transcription failed:", data.message);
                }
            });
    }

    function initLiveTranscription() {
        if (!('webkitSpeechRecognition' in window || 'SpeechRecognition' in window)) {
            console.warn("Speech Recognition API not supported");
            return;
        }

        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        recognition = new SpeechRecognition();
        recognition.continuous = true;
        recognition.interimResults = true;
        recognition.lang = 'en-US';

        recognition.onresult = function (event) {
            let interimTranscript = '';
            let finalTranscript = '';

            for (let i = event.resultIndex; i < event.results.length; ++i) {
                if (event.results[i].isFinal) {
                    finalTranscript += event.results[i][0].transcript.trim() + " ";
                } else {
                    interimTranscript += event.results[i][0].transcript;
                }
            }

            // Only display in local UI
            document.getElementById('liveTranscript').innerText = finalTranscript + interimTranscript;
        };
    }

    function startLiveTranscription() {
        if (recognition && !recognizing) {
            recognition.start();
            recognizing = true;
        }
    }

    function stopLiveTranscription() {
        if (recognition && recognizing) {
            recognition.stop();
            recognizing = false;
        }
    }

    function scrollToBottom() {
        $timeout(() => {
            const el = document.getElementById('chatMessages');
            if (el) el.scrollTop = el.scrollHeight;
        }, 0);
    }

    function cleanupCall() {
        try {
            if (peerConnection) {
                peerConnection.getSenders().forEach(s => s.track && s.track.stop());
                peerConnection.close();
                peerConnection = null;
            }
            if (localStream) {
                localStream.getTracks().forEach(track => track.stop());
                localStream = null;
            }
            ['localVideo', 'remoteVideo'].forEach(id => {
                const vid = document.getElementById(id);
                if (vid) { vid.pause?.(); vid.srcObject = null; }
            });
            pendingCandidates = [];
            $timeout(() => { $scope.showVideoUI = false; });
        } catch (err) {
            console.error('cleanupCall error', err);
        }
    }


    $scope.loadMessages = function () {
        ConsultationService.getChatMessages($scope.appointmentID).then(res => {
            const currentUserId = res.data.currentUserId;
            $scope.messages = res.data.messages.map(m => ({
                name: parseInt(m.senderID) === currentUserId ? 'You' : m.senderName,
                message: m.message
            }));
            scrollToBottom();
        });
    };

    const chat = $.connection.chatHub;

    chat.client.broadcastMessage = (name, msg) => {
        $scope.$apply(() => {
            $scope.messages.push({ name: name === $scope.user ? 'You' : name, message: msg });
            scrollToBottom();
        });
    };

    chat.client.receiveOffer = async offerJson => {
        await initPeerConnection();
        await peerConnection.setRemoteDescription(new RTCSessionDescription(JSON.parse(offerJson)));
        await flushPendingCandidates();
        const answer = await peerConnection.createAnswer();
        await peerConnection.setLocalDescription(answer);
        chat.server.sendAnswer($scope.appointmentID, JSON.stringify(answer));
    };

    chat.client.receiveAnswer = async answerJson => {
        if (!peerConnection) return;
        await peerConnection.setRemoteDescription(new RTCSessionDescription(JSON.parse(answerJson)));
        await flushPendingCandidates();
    };

    chat.client.receiveIceCandidate = async candidateJson => {
        const candidate = new RTCIceCandidate(JSON.parse(candidateJson));
        if (peerConnection?.remoteDescription?.type) {
            await peerConnection.addIceCandidate(candidate);
        } else {
            pendingCandidates.push(candidate);
        }
    };

    chat.client.receiveCallRequest = callerName => {
        if (callRequestActive) return;
        callRequestActive = true;

        const ring = document.getElementById('ringtone');
        ring?.play().catch(() => { });

        Swal.fire({
            title: `Incoming call from ${callerName}`,
            icon: 'info',
            showCancelButton: true,
            confirmButtonText: 'Accept',
            cancelButtonText: 'Reject',
            allowOutsideClick: false
        }).then(res => {
            $scope.$apply(() => {
                $scope.showVideoUI = true;
                callRequestActive = false;
                ring?.pause(); ring.currentTime = 0;
                res.isConfirmed ? $scope.acceptCall() : $scope.rejectCall();
            });
        });
    };

    chat.client.receiveCallResponse = accepted => {
        Swal.close();
        if (accepted) {
            $scope.$apply(() => {
                $scope.showVideoUI = true;
                startWebRTCAsCaller();
            });
        } else {
            Swal.fire({ icon: 'info', title: 'Call Rejected' });
        }
    };

    chat.client.callEnded = () => {
        cleanupCall();
        Swal.fire({ icon: 'info', title: 'Call Ended' });
    };

    chat.client.noOtherUserInRoom = () => {
        Swal.fire({ icon: 'warning', title: 'User is not in the room yet.' });
    };

    chat.client.transcriptReady = function (filePath) {
        // Show download button
        var downloadLink = '<a href="' + filePath + '" class="btn purple">Download Transcript</a>';
        $("#transcriptSection").html(downloadLink);
    };

    chat.client.appointmentEnded = function () {
        stopSession();
        Swal.fire({
            icon: 'info',
            title: 'Appointment Ended',
            text: 'The appointment has ended. You can no longer send messages or make calls.',
            confirmButtonText: "OK"
        })
    };

    chat.client.notifyJoinBlocked = function (reason, dateTime) {
        if (reason === "early") {
            Swal.fire({
                title: "Too Early",
                text: `Your appointment starts at ${dateTime}. Please come back later.`,
                icon: "info",
                confirmButtonText: "OK"
            }).then(() => {
                window.location.href = '/Consultation/Consultations'
            })
        } else if (reason === "ended") {
            Swal.fire({
                title: "Appointment Ended",
                text: `This appointment ended at ${dateTime}. You can still view the transcript in the consultations page.`,
                icon: "warning",
                confirmButtonText: "OK"
            });
        }
    };

    $scope.startCall = () => {
        chat.server.sendCallRequest($scope.appointmentID, $scope.user);
        startLiveTranscription();
        startRecording();
        Swal.fire({
            title: 'Calling...',
            icon: 'info',
            showConfirmButton: false,
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    };

    $scope.acceptCall = () => {
        callRequestActive = false;
        document.getElementById('ringtone')?.pause();
        chat.server.sendCallResponse($scope.appointmentID, true);
        startWebRTCAsReceiver();
        startLiveTranscription();
        startRecording();
    };

    $scope.rejectCall = () => {
        callRequestActive = false;
        document.getElementById('ringtone')?.pause();
        chat.server.sendCallResponse($scope.appointmentID, false);
        cleanupCall();
        stopLiveTranscription();
        Swal.fire({ icon: 'info', title: 'Call Rejected' });
    };

    $scope.endCall = () => {
        cleanupCall();
        stopLiveTranscription();
        stopRecording();
        chat.server.endCall($scope.appointmentID);
        Swal.fire({ icon: 'info', title: 'Call Ended' });
    };

    async function initPeerConnection() {
        if (!localStream) {
            localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
            document.getElementById('localVideo').srcObject = localStream;
        }
        if (!peerConnection) {
            peerConnection = new RTCPeerConnection(rtcConfig);
            peerConnection.ontrack = e => document.getElementById('remoteVideo').srcObject = e.streams[0];
            peerConnection.onicecandidate = e => e.candidate &&
                chat.server.sendIceCandidate($scope.appointmentID, JSON.stringify(e.candidate));
        }
        const existingKinds = peerConnection.getSenders().map(s => s.track?.kind);
        localStream.getTracks().forEach(track => {
            if (!existingKinds.includes(track.kind)) peerConnection.addTrack(track, localStream);
        });
    }

    async function flushPendingCandidates() {
        for (const c of pendingCandidates) {
            await peerConnection.addIceCandidate(c).catch(console.warn);
        }
        pendingCandidates = [];
    }

    async function startWebRTCAsCaller() {
        await initPeerConnection();
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);
        chat.server.sendOffer($scope.appointmentID, JSON.stringify(offer));
    }

    async function startWebRTCAsReceiver() {
        await initPeerConnection();
    }

    async function startRecording() {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        mediaRecorder = new MediaRecorder(stream);

        mediaRecorder.ondataavailable = e => {
            if (e.data.size > 0) {
                audioChunks.push(e.data);
            }
        };

        mediaRecorder.onstop = () => {
            const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });
            uploadRecording(audioBlob);
            audioChunks = [];
        };

        mediaRecorder.start();
    }

    $.connection.hub.start().done(() => {
        chat.server.joinAppointment($scope.appointmentID);
    });

    $scope.sendMessage = () => {
        const msg = $scope.message.trim();
        if (msg) {
            chat.server.send($scope.appointmentID, msg).done(() => {
                $scope.message = '';
                $scope.$apply();
            });
        }
    };

    $scope.endAppointment = () => {
        Swal.fire({
            title: 'End Appointment?',
            text: "This will end the consultation for everyone.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, end it',
            cancelButtonText: 'No, keep it'
        }).then((result) => {
            if (result.isConfirmed) {
                chat.server.endAppointment($scope.appointmentID);
            }
        });
    };

    $scope.loadMessages();

    initLiveTranscription();

    checkAppointmentStatus();
});

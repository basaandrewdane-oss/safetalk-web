app.factory('VideoCallService', function ($q) {
    const videoHub = $.connection.videoCallHub;

    function ensureHubStarted() {
        const d = $q.defer();
        if ($.connection.hub && $.connection.hub.id) {
            d.resolve();
        } else {
            $.connection.hub.start().done(() => d.resolve()).fail(err => d.reject(err));
        }
        return d.promise;
    }

    return {
        init: function (appointmentID) {
            return ensureHubStarted().then(function () {
                return videoHub.server.joinAppointment(appointmentID);
            });
        },
        // Register callbacks
        onOffer: function (cb) { videoHub.client.receiveOffer = cb; },
        onAnswer: function (cb) { videoHub.client.receiveAnswer = cb; },
        onCandidate: function (cb) { videoHub.client.receiveIceCandidate = cb; },
        onCallRequest: function (cb) { videoHub.client.receiveCallRequest = cb; },
        onCallResponse: function (cb) { videoHub.client.receiveCallResponse = cb; },
        onCallEnded: function (cb) { videoHub.client.callEnded = cb; },
        onNoUser: function (cb) { videoHub.client.noOtherUserInRoom = cb; },

        // Sending
        sendOffer: function (appointmentID, offer) { return videoHub.server.sendOffer(appointmentID, offer); },
        sendAnswer: function (appointmentID, answer) { return videoHub.server.sendAnswer(appointmentID, answer); },
        sendCandidate: function (appointmentID, candidate) { return videoHub.server.sendIceCandidate(appointmentID, candidate); },
        sendCallRequest: function (appointmentID, callerName) { return videoHub.server.sendCallRequest(appointmentID, callerName); },
        sendCallResponse: function (appointmentID, accepted) { return videoHub.server.sendCallResponse(appointmentID, accepted); },
        endCall: function (appointmentID) { return videoHub.server.endCall(appointmentID); }
    };
});

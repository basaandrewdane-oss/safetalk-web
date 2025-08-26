app.factory('ChatService', function ($q) {
    const chatHub = $.connection.chatHub;

    function ensureHubStarted() {
        const d = $q.defer();
        // If hub already started (has an id), resolve immediately
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
                return chatHub.server.joinAppointment(appointmentID);
            });
        },
        onMessage: function (callback) {
            chatHub.client.broadcastMessage = callback;
        },
        sendMessage: function (appointmentID, message) {
            return chatHub.server.send(appointmentID, message);
        }
    };
});

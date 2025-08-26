app.service("ResourceService", function ($http) {

    this.loadResources = function () {
        return $http.get("/Resources/GetResources")
    }

    this.saveResource = function (payload) {
        if (payload.resourceID) {
            // If resource has an ID, update it
            return $http.post('/Resources/EditResource/' + payload.resourceID, payload);
        } else {
            // If no ID, add a new resource
            return $http.post('/Resources/AddResource', payload);
        }
    }

    this.deleteResource = function (id) {
        return $http.post('/Resources/DeleteResource', id)
    }

});

app.service("ResourceService", ['$http', 'ApiHelper', function ($http, ApiHelper) {

    this.loadResources = function () {
        return ApiHelper.handleApiResponse(
            $http.get("/Resources/GetResources")
        )
    }

    this.saveResource = function (payload) {
        if (payload.resourceID) {
            // If resource has an ID, update it
            return ApiHelper.handleApiResponse(
                $http.post("/Resources/EditResource/" + payload.resourceID, payload)
            )
        } else {
            // If no ID, add a new resource
            return ApiHelper.handleApiResponse(
                $http.post('/Resources/AddResource', payload)
            )
        }
    }

    this.deleteResource = function (id) {
        return ApiHelper.handleApiResponse(
            $http.post('/Resources/DeleteResource', id)
        );
    }

}]);

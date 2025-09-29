app.factory("ApiHelper", function ($q) {
    function handleApiResponse(promise) {
        return promise.then(function (response) {
            var res = response.data;
            return {
                success: res.success,
                data: res.data,
                message: res.message
            }
        }).catch(function (error) {
            return $q.reject((error.data && error.data.message) || "An unexpected error occurred.");
        });
    }

    return {
        handleApiResponse: handleApiResponse
    };
});

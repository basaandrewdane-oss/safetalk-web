app.controller("ResourcesController", function ($scope, $filter, ResourceService) {
    $scope.resources = [];
    $scope.resource = {};
    $scope.formMode = "Add";

    let resourceModal;

    // Load resources
    $scope.loadResources = function () {
        ResourceService.loadResources().then(function (response) {
            $scope.resources = response.data;
            $scope.resources.forEach(function (res) {
                res.publishedDate = $filter('dotNetDate')(res.publishedDate);
            });
        });
    };

    // Open Add
    $scope.openAddForm = function () {
        $scope.formMode = "Add";
        $scope.resource = {};
        resourceModal.open();
    };

    // Open Edit
    $scope.openEditForm = function (res) {
        $scope.formMode = "Edit";
        $scope.resource = angular.copy(res);
        resourceModal.open();
        setTimeout(function () { M.updateTextFields(); }, 100);
    };

    // Save
    $scope.saveResource = function () {
        var payload = angular.copy($scope.resource);
        if (payload.publishedDate instanceof Date) {
            payload.publishedDate = payload.publishedDate.toISOString(); // or format 'yyyy-MM-dd' if you treat it as date-only
        }
        if ($scope.formMode === "Add") {
            Swal.fire({
                icon: 'question',
                title: 'Are you sure you want to add this resource?',
                text: "You can edit it later if needed.",
                showCancelButton: true,
                confirmButtonText: 'Yes, add it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    ResourceService.saveResource(payload)
                        .then(function () {
                            $scope.loadResources();
                            resourceModal.close();
                        });
                    Swal.fire({
                        icon: 'success',
                        title: 'Resource Added!',
                        text: 'The resource has been added successfully.',
                        timer: 2000,
                        showConfirmButton: false
                    })
                } else if (result.dismiss === Swal.DismissReason.cancel) {
                    Swal.fire({
                        icon: 'info',
                        title: 'Action Cancelled',
                        text: 'The resource was not added.',
                        timer: 2000,
                        showConfirmButton: false
                    });
                }
            });
        } else {
            Swal.fire({
                icon: 'question',
                title: 'Are you sure you want to save changes to this resource?',
                text: "You can edit it again later if needed.",
                showCancelButton: true,
                confirmButtonText: 'Yes, save it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    ResourceService.saveResource(payload)
                        .then(function () {
                            $scope.loadResources();
                            resourceModal.close();
                        });
                    Swal.fire({
                        icon: 'success',
                        title: 'Changes Saved!',
                        text: 'The resource has been updated successfully.',
                        timer: 2000,
                        showConfirmButton: false
                    })
                } else if (result.dismiss === Swal.DismissReason.cancel) {
                    Swal.fire({
                        icon: 'info',
                        title: 'Action Cancelled',
                        text: 'The resource was not updated.',
                        timer: 2000,
                        showConfirmButton: false
                    });
                }
            });
        }
    };

    // Delete
    $scope.deleteResource = function (id) {
        Swal.fire({
            icon: 'warning',
            title: 'Are you sure you want to delete this resource?',
            text: "This action cannot be undone.",
            showCancelButton: true,
            confirmButtonText: 'Yes, delete it!',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                ResourceService.deleteResource({ id: id })
                    .then(function () {
                        $scope.loadResources();
                    });
                Swal.fire({
                    icon: 'success',
                    title: 'Resource Deleted!',
                    text: 'The resource has been deleted successfully.',
                    timer: 2000,
                    showConfirmButton: false
                })
            } else if (result.dismiss === Swal.DismissReason.cancel) {
                Swal.fire({
                    icon: 'info',
                    title: 'Action Cancelled',
                    text: 'The resource was not deleted.',
                    timer: 2000,
                    showConfirmButton: false
                });
            }
        });
    };

    // Init
    angular.element(document).ready(function () {
        var elem = document.getElementById('resourceModal');
        resourceModal = M.Modal.init(elem, {
            dismissible: true
        });
    });

    $scope.loadResources();
});

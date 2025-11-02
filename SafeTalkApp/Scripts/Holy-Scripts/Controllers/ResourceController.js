app.controller("ResourcesController", ["$scope", "$filter", "$timeout",
    "$sce", "ResourceService", function ($scope, $filter, $timeout, $sce, ResourceService) {
    $scope.resources = [];
    $scope.filteredResources = [];
    $scope.resource = {};
    $scope.formMode = "Add";
    $scope.categories = [];
    $scope.selectedResource = null;
    $scope.currentPage = 1;
    $scope.itemsPerPage = 6;
    $scope.pageCount = () => Math.ceil($scope.filteredResources.length / $scope.itemsPerPage);

    let resourceModal;
    let viewModal;
    let editorInstance = null;
    function initEditor(initialData) {
        // Destroy existing instance to prevent duplication
        if (editorInstance) {
            editorInstance.destroy().catch(err => console.error(err));
            editorInstance = null;
        }

        setTimeout(() => {
            ClassicEditor.create(document.querySelector('#editorArea'))
                .then(editor => {
                    editorInstance = editor;
                    editor.setData(initialData || '');
                    editor.model.document.on('change:data', () => {
                        $scope.$apply(() => {
                            $scope.resource.content = editor.getData();
                        });
                    });
                })
                .catch(error => console.error(error));
        }, 100);
    }

    // Load resources
    $scope.loadResources = function () {
        ResourceService.loadResources().then(function (response) {
            $scope.resources = response.data;
            $scope.resources.forEach(function (res) {
                res.publishedDate = $filter('dotNetDate')(res.publishedDate);
                if (res.source === 'Internal' && res.content) {
                    res.trustedContent = $sce.trustAsHtml(res.content);
                }
            });
        });
    };

    //filter
    $scope.categories = ['Mental Health', 'Wellness', 'Guides'];
    $scope.$watch('categories', function (newVal) {
        if (newVal) {
            setTimeout(() => {
                var elems = document.querySelectorAll('select');
                M.FormSelect.init(elems);
            }, 0);
        }
    });
    $scope.searchFilter = function (res) {
        const q = ($scope.searchQuery || '').toLowerCase();
        const cat = $scope.selectedCategory;
        return (!q || res.title.toLowerCase().includes(q) || res.category.toLowerCase().includes(q) || res.type.toLowerCase().includes(q))
            && (!cat || res.category === cat);
    };

    //Open View Modal
    $scope.viewInternal = function (res) {
        $scope.selectedResource = angular.copy(res);
        $scope.selectedResource.content = $sce.trustAsHtml(res.content); // trust HTML from CKEditor
        viewModal.open();
    };

    $scope.closeViewModal = function () {
        viewModal.close();
    };

    // Open Add
    $scope.openAddForm = function () {
        $scope.formMode = "Add";
        $scope.resource = {};
        resourceModal.open();
    };
    $scope.onSourceChange = function () {
        $timeout(function () {
            M.updateTextFields();
        })
    };

    // Open Edit
    $scope.openEditForm = function (res) {
        $scope.formMode = "Edit";
        $scope.resource = angular.copy(res);
        resourceModal.open();
    };

    //Close Resource Modal
    $scope.closeModal = function () {
        resourceModal.close();
    }

    // Save
    $scope.saveResource = function () {
        if ($scope.resourceForm && $scope.resourceForm.$invalid) {
            Swal.fire("Error", "Please check required fields.", "error");
            return;
        }
        var payload = angular.copy($scope.resource);
        if (payload.publishedDate instanceof Date) {
            payload.publishedDate = payload.publishedDate.toISOString();
        }

        const action = $scope.formMode === "Add" ? "add" : "update";
        const confirmTitle = action === "add" ? "add this resource?" : "save changes?";
        const successText = action === "add" ? "Resource Added!" : "Changes Saved!";
        const successMsg = action === "add" ? "The resource has been added successfully." : "The resource has been updated successfully.";

        Swal.fire({
            icon: 'question',
            title: `Are you sure you want to ${confirmTitle}`,
            text: "You can edit it later if needed.",
            showCancelButton: true,
            confirmButtonText: 'Yes, confirm!',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                ResourceService.saveResource(payload).then((res) => {
                    if (res.success) {
                        $scope.loadResources();
                        resourceModal.close();
                        Swal.fire({ icon: 'success', title: successText, text: successMsg, timer: 2000, showConfirmButton: false });
                    } else {
                        // Server responded but with success: false
                        Swal.fire({
                            icon: 'error',
                            title: 'Save Failed',
                            text: res.message || 'The server was unable to save the resource.',
                            confirmButtonText: 'OK'
                        });
                    }
                }).catch((error) => {
                    console.error("Error saving resource:", error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error Saving Resource',
                        text: error.message || 'An unexpected error occurred while saving the resource.',
                        confirmButtonText: 'OK'
                    });
                });
            } else if (result.dismiss === Swal.DismissReason.cancel) {
                Swal.fire({
                    icon: 'info',
                    title: 'Action Cancelled',
                    text: 'No changes were made.',
                    timer: 2000,
                    showConfirmButton: false
                });
            }
        });
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
            dismissible: false,
            onOpenEnd: function () {
                M.FormSelect.init(document.querySelectorAll('select'));
                M.updateTextFields();
                initEditor($scope.resource.content || '');
            }
        });

        var viewModalElem = document.getElementById('viewResourceModal');
        viewModal = M.Modal.init(viewModalElem, {
            dismissible: true
        });
    });

    $scope.loadResources();
}]);

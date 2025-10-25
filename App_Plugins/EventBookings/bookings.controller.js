(function () {
  'use strict';

  function controller($scope, $http, $routeParams, contentResource, notificationsService) {
    $scope.loading = true;
    $scope.isEvent = false;
    $scope.nodeName = "";
    $scope.bookings = [];
    $scope.error = null;

    function load() {
      // Load current node info to get the GUID key and verify doc type
      contentResource.getById($routeParams.id)
        .then(function (content) {
          $scope.nodeName = content.name;
          var alias = (content.contentTypeAlias || "").toLowerCase();
          $scope.isEvent = alias === "event";

          if (!$scope.isEvent) {
            $scope.loading = false;
            return; // Only applicable to Event document type
          }

          var eventKey = content.key; // GUID
          var url = "/umbraco/surface/booking/geteventbookings?eventKey=" + encodeURIComponent(eventKey);
          return $http.get(url)
            .then(function (res) {
              $scope.bookings = res.data || [];
              $scope.loading = false;
            }, function (err) {
              $scope.error = (err && err.data) ? err.data : "Failed to load bookings";
              notificationsService.error("Bookings", "Failed to load bookings for this event.");
              $scope.loading = false;
            });
        }, function (err) {
          $scope.error = "Failed to load content details";
          $scope.loading = false;
        });
    }

    $scope.refresh = function () {
      $scope.loading = true;
      $scope.error = null;
      load();
    };

    load();
  }

  angular.module('umbraco').controller('eventBookingsController', controller);
})();

(function () {
    'use strict';

    angular
        .module("testApp")
        .factory('valuesService', ['$http', 'ngAuthSettings',
        valuesService]);


    function valuesService($http, ngAuthSettings) {

        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var service = {
            getValues: getValues,
            getValueById: getValueById,
            saveValue: saveValue,
            addValue: addValue,
            removeValue: removeValue
        };

        function getValues() {
            return $http.get(serviceBase + '/api/values/get');
        }

        function getValueById(id) {
            return $http.post(serviceBase + '/api/values/get', id);
        }

        function saveValue(value) {
            return $http.put(serviceBase + '/api/values/put', value);
        }

        function addValue(value) {
            return $http.post(serviceBase + '/api/values/post', value);
        }

        function removeValue(value) {
            return $http.delete(serviceBase + '/api/values/delete', value);
        }

        return service;
    }
}());
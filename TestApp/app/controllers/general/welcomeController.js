(function () {
    'use strict';

    angular
        .module("testApp")
        .controller('welcomeController', ['valuesService', welcomeController]);

    function welcomeController(valuesService) {

        var vm = this;

        vm.valuesList = [];
        vm.message = 'Hello';

        function getValues() {
            valuesService.getValues().then(function (result) { vm.valuesList = result.data.map(function (item) { return item; });});
        }

        getValues();
    }
}());
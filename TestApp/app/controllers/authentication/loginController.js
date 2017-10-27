(function () {
    'use strict';

    angular
        .module("testApp")
        .controller('loginController',
        ['$state',
            'authService',
            loginController]);

    function loginController($state, authService) {

        var vm = this;

        vm.loginData = {
            userName: "",
            password: "",
            dbName: "",
            dbServer: "",
            useRefreshTokens: false
        };

        vm.message = "";

        vm.login = function () {

            authService.login(vm.loginData).then(function (response) {
                $state.go("home");
            },
                function (err) {
                    vm.message = err.error_description;
                });
        };
    }
}());
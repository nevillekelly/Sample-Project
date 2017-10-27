(function () {
    'use strict';

    angular
        .module("testApp")
        .controller('logoutController',
        ['$state',
            'authService',
            logoutController]);

    function logoutController($state, authService) {

        var vm = this;

        authService.logOut();

        $state.go("login");

    }
}());
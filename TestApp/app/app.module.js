(function () {
    "use strict";

    var testApp = angular.module("testApp", [
        // Angular modules
        "ui.router",
        "ngResource",
        "ngAnimate",
        "ngMaterial",
        // Custom Modules
        "LocalStorageModule"
    ]);

    var baseURL = window.location.origin + '/';

    console.log(baseURL);

    testApp.constant("appSettings", { serverPath: baseURL });

    testApp.constant("ngAuthSettings", {
        apiServiceBaseUri: baseURL,
        clientId: 'testApp'
    });

    testApp.config(function ($httpProvider) {
        $httpProvider.interceptors.push('authInterceptorService');
    });

    testApp.config(function (localStorageServiceProvider) {
        localStorageServiceProvider
            .setPrefix('testApp')
            .setNotify(true, true)
    });



    testApp.run(["authService", function (authService) {
        authService.fillAuthData();
    }]);

    testApp.run(function ($rootScope, $state, authService) {
        $rootScope.$on("$stateChangeStart", function (event, toState, toParams, fromState, fromParams) {

            if (toState.authenticate && !authService.authentication.isAuth) {
                // User isn’t authenticated
                $state.transitionTo("login");
                event.preventDefault();
            }

            // don't let the user go back to the login screen if they are authenticated
            if (toState.name === 'login' && authService.authentication.isAuth) {
                $state.transitionTo("home");
                event.preventDefault();
            }

        });
    });
}());
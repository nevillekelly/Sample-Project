(function () {
    "use strict";
    angular.module("testApp")
        .config(["$stateProvider",
            "$urlRouterProvider",
            function ($stateProvider, $urlRouterProvider) {

                $urlRouterProvider.otherwise("/unavailable");

                $stateProvider
                    .state("main", {
                        url: "/",
                        templateUrl: "app/templates/general/welcome.html",
                        controller: "welcomeController as vm",
                        authenticate: true
                    })
                    .state("home", {
                        url: "/home",
                        templateUrl: "app/templates/general/welcome.html",
                        controller: "welcomeController as vm",
                        authenticate: true
                    })
                    .state("login", {
                        url: "/login",
                        templateUrl: "app/templates/authentication/login.html",
                        controller: "loginController as vm",
                        authenticate: false
                    })
                    .state("logout", {
                        url: "/logout",
                        templateUrl: "app/templates/authentication/logout.html",
                        controller: "logoutController as vm",
                        authenticate: false
                    })


                    .state("unavailable", {
                        url: "/unavailable",
                        templateUrl: "app/templates/general/unavailable.html",
                        authenticate: true
                    })



            }]
        );


}());
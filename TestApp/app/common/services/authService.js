(function () {
    'use strict';

    angular
        .module("testApp")
        .factory('authService',
        authService);

    authService.$inject = ['$http', '$q', 'localStorageService', 'ngAuthSettings'];

    function authService($http, $q, localStorageService, ngAuthSettings) {
        const administrator = 'Administrator';
        const systemAdministrator = 'System Administrator';
        const canEditAffiliationsPermission = 'canEditAffiliations';
        const canEditContractsPermission = 'canEditContracts';
        const canViewAffiliationsPermission = 'canViewAffiliations';
        const canViewContractsPermission = 'canViewContracts';

        let serviceBase = ngAuthSettings.apiServiceBaseUri;

        let _authentication = {
            isAuth: false,
            userName: "",
            useRefreshTokens: false
        };

        let service = {
            saveRegistration: saveRegistration,
            login: login,
            logOut: logOut,
            fillAuthData: fillAuthData,
            authentication: _authentication,
            refreshToken: refreshToken,
            resetpwd: resetpwd,
            getClientName: getClientName,
            isSysAdmin: isSysAdmin,
            isClientAdmin: isClientAdmin,
            canViewContracts: canViewContracts,
            canEditContracts: canEditContracts,
            canViewAffiliations: canViewAffiliations,
            canEditAffiliations: canEditAffiliations,
            getDBName: getDBName
        };

        return service;

        //////////////////////

        function saveRegistration(registration) {
            return $http.post(serviceBase + '/register', registration).then(function (response) {
                return response;
            });
        };

        function login(loginData) {

            let data = "grant_type=password&username=" + loginData.userName + "&password=" + loginData.password +
                "&dbServer=" + loginData.dbServer + "&dbName=" + loginData.dbName;

            if (loginData.useRefreshTokens) {
                data = data + "&client_id=" + ngAuthSettings.clientId;
            }

            let deferred = $q.defer();

            $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                let d = {
                    token: response.access_token,
                    userName: loginData.userName,
                    refreshToken: "",
                    useRefreshTokens: loginData.useRefreshTokens
                };
                if (loginData.useRefreshTokens) {
                    refreshToken: response.refresh_token;
                }

                localStorageService.set('authorizationData', d);

                _authentication.isAuth = true;
                _authentication.userName = loginData.userName;
                _authentication.useRefreshTokens = loginData.useRefreshTokens;

                deferred.resolve(response);

            }).error(function (err, status) {
                logOut();
                deferred.reject(err);
            });

            return deferred.promise;

        };

        function logOut() {

            localStorageService.remove('authorizationData');

            _authentication.isAuth = false;
            _authentication.userName = "";
            _authentication.useRefreshTokens = false;

        };

        function fillAuthData() {

            let authData = localStorageService.get('authorizationData');
            if (authData) {
                _authentication.isAuth = true;
                _authentication.userName = authData.userName;
                _authentication.useRefreshTokens = authData.useRefreshTokens;
            }

        };

        function refreshToken() {

            let deferred = $q.defer();

            let authData = localStorageService.get('authorizationData');

            if (authData) {

                if (authData.useRefreshTokens) {

                    let data = "grant_type=refresh_token&refresh_token=" + authData.refreshToken + "&client_id=" + ngAuthSettings.clientId;

                    localStorageService.remove('authorizationData');

                    $http.post(serviceBase + 'token', data, {
                        headers: {
                            'Content-Type': 'application/x-www-form-urlencoded'
                        }
                    }).success(function (response) {

                        localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: response.refresh_token, useRefreshTokens: true });

                        deferred.resolve(response);

                    }).error(function (err, status) {
                        logOut();
                        deferred.reject(err);
                    });
                }
            }

            return deferred.promise;
        };

        function resetpwd(pwdata) {
            return $http.post(serviceBase + '/resetpassword', pwdata).then(function (response) {
                return response;
            });

        };

        function getClientName() {
            let claims = getCurrentClaims();
            return claims["client"];
        }

        function isSysAdmin() {
            let claims = getCurrentClaims();
            if (Array.isArray(claims.role)) {
                for (let i = 0; i < claims.role.length; i++) {
                    if (claims.role[i] === systemAdministrator) {
                        return true;
                    }
                }
            }
            else {
                return claims["role"] === systemAdministrator;
            }

        }

        function isClientAdmin() {
            let claims = getCurrentClaims();
            if (Array.isArray(claims.role)) {
                for (let i = 0; i < claims.role.length; i++) {
                    if (claims.role[i] === administrator) {
                        return true;
                    }
                }
            }
            else {
                return claims["role"] === administrator;
            }
        }

        function canViewContracts() {
            let claims = getCurrentClaims();
            if (Array.isArray(claims.appPermission)) {
                for (let i = 0; i < claims.appPermission.length; i++) {
                    if (claims.appPermission[i] === canViewContractsPermission) {
                        return true;
                    }
                }
            }
        }

        function canEditContracts() {
            let claims = getCurrentClaims();
            if (Array.isArray(claims.appPermission)) {
                for (let i = 0; i < claims.appPermission.length; i++) {
                    if (claims.appPermission[i] === canEditContractsPermission) {
                        return true;
                    }
                }
            }
        }

        function canViewAffiliations() {
            let claims = getCurrentClaims();
            if (Array.isArray(claims.appPermission)) {
                for (let i = 0; i < claims.appPermission.length; i++) {
                    if (claims.appPermission[i] === canViewAffiliationsPermission) {
                        return true;
                    }
                }
            }
        }

        function canEditAffiliations() {
            let claims = getCurrentClaims();
            if (Array.isArray(claims.appPermission)) {
                for (let i = 0; i < claims.appPermission.length; i++) {
                    if (claims.appPermission[i] === canEditAffiliationsPermission) {
                        return true;
                    }
                }
            }
        }

        function getDBName() {
            let claims = getCurrentClaims();
            if (Array.isArray(claims)) {
                for (let i = 0; i < claims.length; i++) {
                    if (claims[i] === "dbName") {
                        return true;
                    }
                }
            }
            else {
                return claims["dbName"];
            }
        }

        function urlBase64Decode(str) {
            let output = str.replace('-', '+').replace('_', '/');
            switch (output.length % 4) {
                case 0:
                    break;
                case 2:
                    output += '==';
                    break;
                case 3:
                    output += '=';
                    break;
                default:
                    throw 'Illegal base64url string!';
            }
            return window.atob(output);
        }

        function getCurrentClaims() {

            let claims = {};

            let authData = localStorageService.get('authorizationData')

            if (authData != null) {
                let token = authData.token;
                if (typeof token !== 'undefined') {
                    claims = JSON.parse(urlBase64Decode(token.split('.')[1]));
                }
            }

            return claims;

        }


    }

}());
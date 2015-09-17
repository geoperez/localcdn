(function () {
    "use strict";

    angular.module('app', []);

    angular.module('app').controller('ctrlBooks', [
        '$scope', '$http', function($scope, $http) {
            $http.get('/api/books/').then(function (response) {
                $scope.books = response.data;
            });

            $scope.other = {
                Title: "HOLA",
                Author: "Este author",
                PublishedDate: new Date()
            };
        }
    ]).directive('bookView', function() {
        return {
            scope: {
                scopeModel: '=scopeModel'  
            },
            template: "<div><h2>{{scopeModel.Title}}</h2>" +
                "<p>Author: {{scopeModel.Author}}</p>" +
                "<p>Published Date: {{scopeModel.PublishedDate | date}}</p>" +
                "</div>"
        };
    });
})();
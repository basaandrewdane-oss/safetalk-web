var app = angular.module("SafeTalkApp", []);

app.filter('dotNetDate', function () {
    return function (input) {
        if (!input) return '';
        var parsedDate = new Date(parseInt(input.replace(/[^0-9+-]/g, '')));
        return parsedDate;
    };
});

app.filter('timespanToTime', function () {
    return function (timespan) {
        if (!timespan || typeof timespan !== 'object') return '';

        const hours = timespan.Hours;
        const minutes = timespan.Minutes;

        if (hours == null || minutes == null) return '';

        let h = hours % 12 || 12;
        let m = minutes < 10 ? '0' + minutes : minutes;
        let ampm = hours >= 12 ? 'PM' : 'AM';

        return `${h}:${m} ${ampm}`;
    };
});
var myMap;
var UserCoordinateX = "";
var UserCoordinateY = "";

$(document).ready(function () {

    UserCoordinateX = $("#UserCoordX").val();
    if (UserCoordinateX == null) { UserCoordinateX = "55.76" }
    UserCoordinateY = $("#UserCoordY").val();
    if (UserCoordinateY == null) { UserCoordinateY = "37.64" }
});
ymaps.ready(init);

function init() {
    var searchControl = new ymaps.control.SearchControl(
        {
            options:
            {
                provider: 'yandex#map',
                noPlacemark: 'true'
            }
        }
    );
    searchControl.events.add('resultselect', function (e) {
        var index = e.get('index');
        searchControl.getResult(index).then(function (res) {
            var adress = res.properties._data.text;
            $('#UserAdress').val(adress);

            var coordinates = res.geometry.getCoordinates();

            $('#UserCoordinates').val(coordinates);

            if (myPlacemark) {
                myPlacemark.geometry.setCoordinates(coordinates);
                $('#UserCoordX').val(coordinates[0]);
                $('#UserCoordY').val(coordinates[1]);
            }
            else {
                myPlacemark = createPlacemark(coordinates);
                myMap.geoObjects.add(myPlacemark);
                myPlacemark.events.add('dragend', function () {
                    getAddress(myPlacemark.geometry.getCoordinates());
                });
            }
        });
    });
    myMap = new ymaps.Map("UserLocationMap", {
        type: 'yandex#map',
        center: [UserCoordinateX, UserCoordinateY],
        zoom: 16,
        controls: ['geolocationControl', searchControl, 'zoomControl']
    });
    var myPlacemark;

    // с этим условием разобраться!!!
    if (UserCoordinateX != null && UserCoordinateY != null) {
        var coords = new Array();
        coords[0] = UserCoordinateX;
        coords[1] = UserCoordinateY;
        myPlacemark = createPlacemark(coords);
        myMap.geoObjects.add(myPlacemark);
        myPlacemark.events.add('dragend', function () {
            getAddress(myPlacemark.geometry.getCoordinates());

        });
    }
    myMap.events.add('click', function (e) {
        var coords = e.get('coords');
        if (myPlacemark) {
            myPlacemark.geometry.setCoordinates(coords);
            $('#UserCoordX').val(coords[0]);
            $('#UserCoordY').val(coords[1]);
        }
        else {
            myPlacemark = createPlacemark(coords);
            myMap.geoObjects.add(myPlacemark);
            myPlacemark.events.add('dragend', function () {
                getAddress(myPlacemark.geometry.getCoordinates());
            });
        }
        getAddress(coords);
    });
    function createPlacemark(coords) {
        var circleLayout = ymaps.templateLayoutFactory.createClass("<div class='task_placemark_layout_container'><div class='task_circle_layout'/></div>");

        return new ymaps.GeoObject({
            geometry:
            {
                type: "Point",
                coordinates: coords
            },
            properties:
            {
                draggable: true
            }
        },
            {
                iconLayout: 'default#imageWithContent',
                iconImageHref: '/Images/Empty.png',
                iconImageSize: [48, 48],
                iconImageOffset: [-24, -24],
                iconContentLayout: circleLayout
            });

    }
    function getAddress(coords) {
        ymaps.geocode(coords).then(function (res) {
            var firstGeoObject = res.geoObjects.get(0);
            $('#UserCoordinates').val(coords);
            var adrs = firstGeoObject.getAddressLine();
            $('#UserAdress').val(adrs);
            $('#UserCoordX').val(coords[0]);
            $('#UserCoordY').val(coords[1]);
        });
    }
}
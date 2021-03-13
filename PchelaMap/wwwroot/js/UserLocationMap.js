var myMap;
var jsonUserArray = [];
var UserPicture = "";
var UserCoordinateX = "55.76";
var UserCoordinateY = "37.64";
$(document).ready(function () {

    if ($("#UserID").length) {
        jsonFolder = "/js/jsons/" + $("#UserID").val() + "/UserPersonal.json";
    }
    $.getJSON(jsonFolder, function (json) {
        UserPicture = json.PhotoUrl;
        if (json.CoordinateX != null && json.CoordinateY != null && json.CoordinateX != "" && json.CoordinateY != "")
        {
        UserCoordinateX = json.CoordinateX;
            UserCoordinateY = json.CoordinateY;
        }
    });
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
        zoom: 11,
        controls: ['geolocationControl', searchControl, 'zoomControl']
    });
    var myPlacemark;
    if (UserCoordinateX != null) {
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
        var circleLayout = ymaps.templateLayoutFactory.createClass("<div class='placemark_layout_container'><div class='circle_layout' style='background-image:url(" + UserPicture + ");' /></div>");

        return new ymaps.GeoObject({
            geometry:
            {
                type: "Point",
                coordinates: coords
            },
            properties:
            {
                //hintContent: item.Name + item.Adress,
                draggable: true

            }
        },
            {
                iconLayout: 'default#imageWithContent',
                iconImageHref: '/Images/EmptyUserRound.png',
                iconImageSize: [48, 48],
                iconImageOffset: [-24, -24],
                iconContentLayout: circleLayout
            }
        );
    }
    // Определяем адрес по координатам (обратное геокодирование).
    function getAddress(coords) {
        ymaps.geocode(coords).then(function (res) {
            var firstGeoObject = res.geoObjects.get(0);
            $('#UserCoordinates').val(coords);
            var adrs = firstGeoObject.getAddressLine();
            $('#UserAdress').val(adrs);
        });
    }
}

function UserBigAvatarClick() {
    $('#fileInputLabel').click();
}
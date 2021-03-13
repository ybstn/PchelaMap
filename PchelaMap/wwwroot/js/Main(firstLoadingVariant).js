var myMap;
var jsonUsersArray = [];
var jsonTasksArray = [];
var ObjectManagerArray = [];
$(document).ready(function () {
    $.getJSON('/js/Users.json', function (json) {

        json.forEach(function GetValues(item, index) {
            jsonUsersArray.push(item);
        });
    });
    $.getJSON('/js/Tasks.json', function (json) {

        json.forEach(function GetValues(item, index) {
            jsonTasksArray.push(item);
        });
    });


    var a = $('#ModalTrigger').val();
    if ($('#ModalTrigger').val() == "true") {
        var _header = $('#ModalHeader').val();
        var _message = $('#ModalMessage').val();
        $.get('Home/UserModal', { Header: _header, Message: _message }, function (data) {
            $('#TaskModContent').html(data);
            $('#TaskModDialog').modal('show');
        });
        $('#ModalTrigger').val("false")
    }
});

ymaps.ready(init);

function init() {
    var searchControl = new ymaps.control.SearchControl(
        {
            options:
            {
                float: 'none',
                position:
                {
                    left: '1vw',
                    top: '6vh'
                }
            }
        }
    );
    var geolocationControl = new ymaps.control.GeolocationControl(
        {
            options:
            {
                float: 'right',
                position:
                {
                    right: '1vw',
                    top: '6vh'
                }
            }
        }
    );
    var zoomControl = new ymaps.control.ZoomControl(
        {
            options:
            {
                float: 'none',
                position:
                {
                    left: '1vw',
                    top: '11vh'
                }
            }
        }
    );
    myMap = new ymaps.Map("map", {
        type: 'yandex#map',
        center: [55.76, 37.64],
        zoom: 11,
        controls: [geolocationControl, searchControl, zoomControl]
    });
    var UsersClaster = new ymaps.Clusterer(
        {
            groupByCoordinates: false,
            preset: "islands#orangeClusterIcons",
            gridSize: 128
        });
    UsersClaster.events
        .add("mouseenter", function (e) {
            e.get("target").options.set("preset", "islands#greenClusterIcons");
        })
        .add('mouseleave', function (e) {
            e.get("target").options.set("preset", "islands#orangeClusterIcons");
        });

      var UsersPoints = GenerateUsers();
      UsersClaster.add(UsersPoints);
      myMap.geoObjects.add(UsersClaster);
      GenerateUserTasks();
}

function GenerateUsers() {
    var Users = jsonUsersArray;
    var UsersPoints = [];

    Users.forEach(function GetValues(item, index) {
        var circleLayout = ymaps.templateLayoutFactory.createClass("<div class='placemark_layout_container'><div class='circle_layout' style='background-image:url(" + item.PhotoUrl + ");' /></div>");

        var userPlaceMark = new ymaps.GeoObject({
            geometry:
            {
                type: "Point",
                coordinates: [item.CoordinateX, item.CoordinateY]
            },
            properties:
            {
                hintContent: item.Name + item.Adress
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

        UsersPoints[index] = userPlaceMark;
    });
    return UsersPoints;
}

function GenerateUserTasks() {
    var UsersTasks = jsonTasksArray;

    UsersTasks.forEach(function GetTasksValues(item, index) {
        var circleLayout = ymaps.templateLayoutFactory.createClass("<div class='task_placemark_layout_container'><div class='task_circle_layout'/></div>");
        var userPlaceMark = new ymaps.GeoObject({
            geometry:
            {
                type: "Point",
                coordinates: [item.CoordinateX, item.CoordinateY]
            },
            properties:
            {
                hintContent: item.Adress
            }
        },
            {
                iconLayout: 'default#imageWithContent',
                iconImageHref: '/Images/Empty.png',
                iconImageSize: [48, 48],
                iconImageOffset: [-24, -24],
                iconContentLayout: circleLayout
            });

        userPlaceMark.events.add('click', function (e) {
            e.preventDefault();

            $.get('Home/TaskView', { id: item.id }, function (data) {
                $('#TaskModContent').html(data);
                $('#TaskModDialog').modal('show');
            });
        });
        myMap.geoObjects.add(userPlaceMark);
    });
}

function AddHelpPointClick() {
    //e.preventDefault();

    $.get('Home/AddHelpPoint', function (data) {
        $('#TaskModContent').html(data);
        $('#TaskModDialog').modal('show');
    });
}

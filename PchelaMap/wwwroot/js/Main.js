var myMap;
var jsonUsersArray = [];
var jsonTasksArray = [];
var ObjectManagerArray = [];
var jsonUrl = "";
$(document).ready(function () {
    if ($("#UserID").length) {
        jsonFolder = "/js/jsons/" + $("#UserID").val() + "/";
    }
    else {
        jsonFolder = "/js/jsons/Unregistered/";
    }
});

ymaps.ready(init);

function init() {
    var searchControl = new ymaps.control.SearchControl(
        {
            options:
            {
                provider: 'yandex#map',
                noPlacemark: 'true',
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
                    top: '14vh'
                }
            }
        }
    );
    var CurrUserCoordX = "55.76";
    var CurrUserCoordY = "37.64";
    if ($("#CurrentUserCoordX").length && $("#CurrentUserCoordX").val() != "") {
        CurrUserCoordX = $("#CurrentUserCoordX").val();
        CurrUserCoordY = $("#CurrentUserCoordY").val();
    }
    myMap = new ymaps.Map("map", {
        type: 'yandex#map',
        center: [CurrUserCoordX, CurrUserCoordY],
        zoom: 11,
        controls: [geolocationControl, searchControl, zoomControl]
    });
    var UsersObjectManager = new ymaps.ObjectManager(
        {
            clusterize: true,
            gridSize: 64
        });
    var TasksObjectManager = new ymaps.ObjectManager(
        {
            clusterize: false
        });
    var UrgentTasksObjectManager = new ymaps.ObjectManager(
        {
            clusterize: false
        });
    //var DoneTasksObjectManager = new ymaps.ObjectManager(
    //    {
    //        clusterize: false
    //    });
    $.ajax({
        url: jsonFolder + "UsersGeoObjects.json"
    }).done(function (data) {

        var geoObjects = data.features;
        geoObjects.forEach(function GetValues(item, index) {
            var UserPicture = item.options.fillImageHref;
            var UserscircleLayout = ymaps.templateLayoutFactory.createClass("<div class='placemark_layout_container'><div class='circle_layout' style='background-image:url(" + UserPicture + ");'/></div>");
            item.options.iconContentLayout = UserscircleLayout;
        });
        UsersObjectManager.add(data);

        UsersObjectManager.clusters.options.set('preset', 'islands#orangeClusterIcons');
        UsersObjectManager.clusters.options.set('groupByCoordinates', 'false');
        UsersObjectManager.clusters.events.add(['mouseenter', 'mouseleave'], onClusterEvent)
    });
    $.ajax({
        url: jsonFolder + "TasksGeoObjects.json"
    }).done(function (data) {
        var TaskscircleLayout = ymaps.templateLayoutFactory.createClass("<div class='task_placemark_layout_container'><div class='task_circle_layout'/></div>");
        TasksObjectManager.add(data);
        TasksObjectManager.objects.options.set('iconContentLayout', TaskscircleLayout);
        TasksObjectManager.objects.events.add('click', onTaskClick)
    });
    $.ajax({
        url: jsonFolder + "UrgentTasksGeoObjects.json"
    }).done(function (data) {
        var TaskscircleLayout = ymaps.templateLayoutFactory.createClass("<div class='urgent_task_placemark_layout_container'><div class='urgent_task_circle_layout'/></div>");
        UrgentTasksObjectManager.add(data);
        UrgentTasksObjectManager.objects.options.set('iconContentLayout', TaskscircleLayout);
        UrgentTasksObjectManager.objects.events.add('click', onTaskClick)
    });
    //$.ajax({
    //    url: jsonFolder+ "DoneTasksObjectsJson.json"
    //}).done(function (data) {
    //    var TaskscircleLayout = ymaps.templateLayoutFactory.createClass("<div class='done_task_placemark_layout_container'><div class='done_task_circle_layout'/></div>");
    //    DoneTasksObjectManager.add(data);
    //    DoneTasksObjectManager.objects.options.set('iconContentLayout', TaskscircleLayout);
    //    DoneTasksObjectManager.objects.events.add('click', onTaskClick)
    //});
    function onTaskClick(e) {
        //e.preventDefault();
        var objID = e.get('objectId');
        $.get('Home/TaskView', { id: objID }, function (data) {
            $('#TaskModContent').html(data);
            $('#TaskModDialog').modal('show');
        });
    }
    function onClusterEvent(e) {
        var objID = e.get('objectId');
        if (e.get('type') == 'mouseenter') {
            UsersObjectManager.clusters.setClusterOptions(objID,
                { preset: 'islands#greenClusterIcons' });
        }
        else {
            UsersObjectManager.clusters.setClusterOptions(objID,
                { preset: 'islands#orangeClusterIcons' });
        }
    }
    $("#ShowOnlyTasks").click(function () {
        if ($(this).hasClass("ShowOnlyButtonOff")) {
            $(this).toggleClass("ShowOnlyButtonOn ShowOnlyButtonOff");
            if ($("#ShowAll").hasClass("ShowOnlyButtonOn")) {
                $("#ShowAll").toggleClass("ShowOnlyButtonOn ShowOnlyButtonOff");
            }
            if ($("#UserID").length) {
                var CurrUserID = $("#UserID").val();
                UsersObjectManager.setFilter(function (object) {
                    return object.id == CurrUserID
                });
            }
            else {
                UsersObjectManager.setFilter('id==0');
            }
            document.cookie = "ShowStatus = 1";
        }

    });
    $("#ShowAll").click(function () {
        if ($(this).hasClass("ShowOnlyButtonOff")) {
            $(this).toggleClass("ShowOnlyButtonOn ShowOnlyButtonOff");
            if ($("#ShowOnlyTasks").hasClass("ShowOnlyButtonOn")) {
                $("#ShowOnlyTasks").toggleClass("ShowOnlyButtonOn ShowOnlyButtonOff");
            }
            UsersObjectManager.setFilter('id!=0');
            document.cookie = "ShowStatus = 0";
        }
    });
    var ShowStatus = getCookie("ShowStatus");
    if (ShowStatus == "1") {
        $("#ShowOnlyTasks").trigger("click");
    }
    else {
        $("#ShowAll").trigger("click");
    }
    myMap.geoObjects.add(UsersObjectManager);
    myMap.geoObjects.add(TasksObjectManager);
    myMap.geoObjects.add(UrgentTasksObjectManager);
    //myMap.geoObjects.add(DoneTasksObjectManager);
}

function getCookie(Cname) {
    var name = Cname + '=';
    var decodedCookie = decodeURIComponent(document.cookie);
    var cookieArray = decodedCookie.split(';');
    for (var i = 0; i < cookieArray.length; i++) {
        var c = cookieArray[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    document.cookie = "ShowStatus = 0";
    return "0";
}

function AddHelpPointClick() {
    $.get('Home/AddHelpPoint', function (data) {
        $('#TaskModContent').html(data);
        $('#TaskModDialog').modal('show');
    });
}
function TakeTaskClick(taskId) {
    $(".LoaderContainer").show();
    window.location.href = '/Home/TakeTask/?id=' + taskId;
}

function SberModalClick(taskId) {
    $(".LoaderContainer").show();
    $.get('Home/TakeTask', { id: taskId, SberTask:'sber' }, function (data) {
        $('#TaskModContent').html(data);
        $('#TaskModDialog').modal('show');
    });
}
function LoaderContainerClick(event) {
    //event.stopPropagotion();
    event.preventDefault();
}

function EditHelpPointClick() {

    $.get('Home/EditHelpPoint', function (data) {
        $('#TaskModContent').html(data);
        $('#TaskModDialog').modal('show');
    });
}


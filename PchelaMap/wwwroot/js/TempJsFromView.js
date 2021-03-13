<script type="text/javascript">
    ymaps.ready(init);
function init() {
    var myMap = new ymaps.Map("map", {
        type: 'yandex#map',
    center: [55.76, 37.64],
    zoom: 11,
    controls: ['geolocationControl', 'searchControl', 'zoomControl']
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

var UsersPoints = [];
var counter = 0;
@foreach(User users in Model.UsersList)
    {
        <text>
            var circleLayout = ymaps.templateLayoutFactory.createClass("<div class='placemark_layout_container'><div class='circle_layout' style='background-image:url(@users.PhotoUrl);' /></div>");
            var userPlaceMark = new ymaps.Placemark(
            [@users.CoordinateX, @users.CoordinateY],
            {
                hintContent: '@users.Name' + '@users.Adress'
        },
            {
                iconLayout: 'default#imageWithContent',
        iconImageHref: '/Images/EmptyUserRound.png',
        iconImageSize: [48, 48],
        iconImageOffset: [-24, -24],
         iconContentLayout: circleLayout
     });
 UsersPoints[counter] = userPlaceMark;
         counter++;
         </text>
    }
    UsersClaster.add(UsersPoints);
    myMap.geoObjects.add(UsersClaster);

    @foreach(UserWithTasks tasks in Model.UsersTaskList)
    {
        <text>
            var circleLayout = ymaps.templateLayoutFactory.createClass("<div class='task_placemark_layout_container'><div class='task_circle_layout' /></div>");
            var userPlaceMark = new ymaps.Placemark(
            [@tasks.CoordinateX, @tasks.CoordinateY],
            {
                hintContent: '@tasks.Adress'
    },
               {
                iconLayout: 'default#imageWithContent',
        iconImageHref: '/Images/Empty.png',
        iconImageSize: [48, 48],
        iconImageOffset: [-24, -24],
     iconContentLayout: circleLayout
 });
 myMap.geoObjects.add(userPlaceMark);
     counter++;
       </text>
    }
    }
</script>
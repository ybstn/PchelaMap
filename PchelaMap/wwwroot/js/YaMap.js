var myMap;
var UserCoordinateX = "59.478627";
var UserCoordinateY = "32.043131";
var PointText = "Kimet";
ymaps.ready(init);

function init() {
    myMap = new ymaps.Map("UserLocationMap", {
        type: 'yandex#map',
        center: [UserCoordinateX, UserCoordinateY],
        zoom: 15,
        controls: ['geolocationControl', 'zoomControl', 'trafficControl']
    });
    var myPlacemark;
    if (UserCoordinateX != null) {
        var coords = new Array();
        coords[0] = UserCoordinateX;
        coords[1] = UserCoordinateY;
        myPlacemark = createPlacemark(coords);
        myMap.geoObjects.add(myPlacemark);
    }
    function createPlacemark(coords) {
       
        return new ymaps.GeoObject({
            geometry:
            {
                type: "Point",
                coordinates: coords
            },
            properties:
            {
                //hintContent: item.Name + item.Adress,
                //iconContent: PointText
                iconCaption: PointText
            }
        },
            {
                preset: 'islands#nightFactoryIcon'
            }
        );
    }
}


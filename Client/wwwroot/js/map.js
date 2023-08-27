
window.mapModule = {
    markers: [],
    map: null, // This property will hold the map instance
    loading: true, 
    dotNetReference: null,
    mapsCallback: function () {
        console.log("MapsCallback");
        window.mapModule.loading = false;
    },
    init: function (ref) {
        window.mapModule.dotNetReference = ref;
        window.mapModule.getCurrentLocation();
    },
    getCurrentLocation: function () {
        navigator.geolocation.getCurrentPosition(function (position) {

            console.log("getCurrentLocation - PositionCallback");
            window.mapModule.dotNetReference.invokeMethodAsync('PositionCallback', position.coords.latitude, position.coords.longitude);
        });
    },
    initMap: function (myLocation) {
        
        if(window.mapModule.loading === false) {
            console.log("Init map", myLocation);
            window.mapModule.map = new google.maps.Map(document.getElementById("map"), {
                zoom: 16,
                center: { lat: myLocation.latitude, lng: myLocation.longitude },
                mapTypeId: 'terrain'
            });
            console.log("Init map - done", myLocation);
        } else {
            console.log("Init map - loading...", myLocation);
        }
        //window.mapModule.dotNetReference.invokeMethodAsync('PostLocation', myLocation);
    },
    updateMap: function (newMarkers) {
        console.log("update map", newMarkers);
        for(const key in newMarkers) {
            if(!window.mapModule.markers.hasOwnProperty(key)) {
                const markerIcon = {
                    path: 'M -12.5,7.23 L 0,-14.46 L 12.5,7.23 Z', // Adjusted triangle path
                    fillColor: newMarkers[key].color, // Fill color in hex format
                    fillOpacity: 1,
                    strokeColor: '#000000', // Stroke color in hex format
                    strokeWeight: 2,
                    scale: 1
                };

                window.mapModule.markers[key] = new google.maps.Marker({
                    position: { lat: newMarkers[key].latitude, lng: newMarkers[key].longitude },
                    map: window.mapModule.map,
                    icon: markerIcon
                });
            } else {
                // update case
                window.mapModule.markers[key].setPosition({ lat: newMarkers[key].latitude, lng: newMarkers[key].longitude });
                window.mapModule.markers[key].setMap(window.mapModule.map);
            }
        }
        for(const key in window.mapModule.markers) {
            if(!newMarkers.hasOwnProperty(key)) {
                window.mapModule.markers[key].setMap(null);
                delete window.mapModule.markers[key];
            }
        }

    }
};

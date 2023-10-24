
window.mapModule = {
    markers: [],
    map: null, // This property will hold the map instance
    dotNetReference: null,
    firstLoad: true,
    isLoading: function () {
        return window.mapModule.loading;
    },
    mapsCallback: function () {
        console.log("mapsCallback");
        window.mapModule.loading = false;
    },
    addListenerOnceMapIdle: function (ref) {
        console.log("addListenerOnceMapIdle");
        window.mapModule.dotNetReference = ref;
        window.mapModule.map = new google.maps.Map(document.getElementById("map"), {
            zoom: 5,
            center: { lat: 36.0, lng: -98.0 },
            mapTypeId: 'terrain'
        });
        google.maps.event.addListenerOnce(window.mapModule.map, 'idle', function () {
            console.log("addListenerOnceMapIdle - fired");
            window.mapModule.watchPosition();
        });
    },
    watchPosition: function () {
        console.log("watchPosition");
        window.mapModule.positionWatch = navigator.geolocation.watchPosition(function (position) {
            if(window.mapModule.firstLoad) {
                window.mapModule.firstLoad = false;
                window.mapModule.setCenter(position.coords.latitude, position.coords.longitude);
            }
            window.mapModule.dotNetReference.invokeMethodAsync("UpdateCurrentPosition", position.coords.latitude, position.coords.longitude);
        }, function (error) {
            console.error(`ERROR(${error.code}): ${error.message}`);
        }, {
            enableHighAccuracy: true
        });
    },
    updateMap: function (newMarkers) {
        console.log("updateMap");
        for(const key in newMarkers) {
            if(!window.mapModule.markers.hasOwnProperty(key)) {
                const markerIcon = {
                    path: 'M -12.5,7.23 L 0,-14.46 L 12.5,7.23 Z', // Adjusted triangle path
                    fillColor: newMarkers[key].color, // Fill color in hex format
                    fillOpacity: 1,
                    strokeColor: '#000000', // Stroke color in hex format
                    strokeWeight: 2,
                    scale: 1,
                    labelOrigin: new google.maps.Point(0, 18)
                };

                window.mapModule.markers[key] = new google.maps.Marker({
                    position: { lat: newMarkers[key].latitude, lng: newMarkers[key].longitude },
                    map: window.mapModule.map,
                    icon: markerIcon,
                    label: newMarkers[key].label,
                    fontSize: "26px",
                    fontWeight: "bold"
                    
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
    },
    setCenter: function (lat, lng) {
        window.mapModule.map.setCenter({ lat: lat, lng: lng });
        window.mapModule.map.setZoom(16.0);
    }
};

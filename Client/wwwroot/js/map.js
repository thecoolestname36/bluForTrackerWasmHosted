
window.mapModule = {
    markers: [],
    map: null, // This property will hold the map instance
    loading: true,
    dotNetReference: null,
    sendingPosition: false,
    enableHighAccuracy: true,
    mapsCallback: function () {
        window.mapModule.loading = false;
    },
    getCurrentPosition: function (ref) {
        window.mapModule.dotNetReference = ref;
        navigator.geolocation.getCurrentPosition(function (position) {
            if(window.mapModule.loading === true) {
                console.log("watchPosition - loading...");
                return;
            }
            if(window.mapModule.map === null) {
                console.log("watchPosition - init map");
                window.mapModule.map = new google.maps.Map(document.getElementById("map"), {
                    zoom: 16,
                    center: { lat: position.coords.latitude, lng: position.coords.longitude },
                    mapTypeId: 'terrain'
                });
            }
            if(window.mapModule.sendingPosition === true) {
                console.log("watchPosition - SendingPosition...")
                return;
            }
            window.mapModule.sendingPosition = true;
            console.log("watchPosition - SendPosition");
            window.mapModule.dotNetReference.invokeMethodAsync("SendPosition", position.coords.latitude, position.coords.longitude);
            window.mapModule.sendPositionDebounce = null;
        });
    },
    doneSendingPosition: function () {
        console.log("doneSendingPosition");
        window.mapModule.sendingPosition = false;
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

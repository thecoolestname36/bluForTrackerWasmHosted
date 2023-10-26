
window.mapModule = {
    markers: [],
    infoMarkers: [],
    map: null, // This property will hold the map instance
    dotNetReference: null,
    firstLoad: true,
    loadingState: 0, // 0 is start, -1 is done
    getLoadingState: function () {
        return window.mapModule.loadingState;
    },
    mapsCallback: function () {
        console.info("mapsCallback", 1);
        window.mapModule.loadingState = 1;
    },
    addListenerOnceMapIdle: function (ref) {
        console.info("addListenerOnceMapIdle");
        window.mapModule.dotNetReference = ref;
        window.mapModule.map = new google.maps.Map(document.getElementById("map"), {
            zoom: 5,
            center: { lat: 36.0, lng: -98.0 },
            mapTypeId: 'terrain'
        });
        for(let i = 0; i < window.mapModule.markers.length; i++) {
            window.mapModule.markers[i].setMap(null);
            delete window.mapModule.markers[i];
        }
        for(let i = 0; i < window.mapModule.infoMarkers.length; i++) {
            window.mapModule.infoMarkers[i].close();
            delete window.mapModule.infoMarkers[i];
        }
        window.mapModule.map.addListener("click", (mapsMouseEvent) => {
            let message = prompt("OK - Send the marker.\nCancel - Abort.\n * A message is optional and not required *", "");
            if(message != null) {
                window.mapModule.dotNetReference.invokeMethodAsync("UpdateMyInfoMarker", message, mapsMouseEvent.latLng.lat(), mapsMouseEvent.latLng.lng());
            }
        });
        google.maps.event.addListenerOnce(window.mapModule.map, 'idle', function () {
            console.info("addListenerOnceMapIdle - fired");
            window.mapModule.watchPosition();
            window.mapModule.loadingState = -1;
        });
        window.mapModule.loadingState = 2;
    },
    watchPosition: function () {
        console.info("watchPosition");
        window.mapModule.firstLoad = true;
        if(window.mapModule.positionWatch != null) {
            navigator.geolocation.clearWatch(window.mapModule.positionWatch);
        }
        window.mapModule.positionWatch = navigator.geolocation.watchPosition(function (position) {
            if(window.mapModule.firstLoad) {
                window.mapModule.firstLoad = false;
                window.mapModule.setCenter(position.coords.latitude, position.coords.longitude);
            }
            window.mapModule.dotNetReference.invokeMethodAsync("UpdateCurrentPosition", position.coords.latitude, position.coords.longitude);
        }, function (error) {
            console.error(error);
            window.mapModule.firstLoad = true;
            navigator.geolocation.clearWatch(window.mapModule.positionWatch);
            if(error.code == GeolocationPositionError.PERMISSION_DENIED) {
                alert("Location permission denied. Please allow location access.");
                window.mapModule.dotNetReference.invokeMethodAsync("UpdateCurrentPositionError");
                return;
            }
            window.mapModule.watchPosition();
        }, {
            enableHighAccuracy: true
        });
    },
    syncMarkers: function (newMarkers) {
        //console.info("syncMarkers");
        for(let i = 0; i < window.mapModule.markers.length; i++) {
            window.mapModule.markers[i].setMap(null);
            delete window.mapModule.markers[i];
        }
        newMarkers.forEach((marker) => window.mapModule.receiveMarker(marker));
    },
    receiveMarker: function (marker) {
        //console.info("receiveMarker", marker);
        if(!window.mapModule.markers.hasOwnProperty(marker.id)) {
            const markerIcon = {
                path: 'M -12.5,7.23 L 0,-14.46 L 12.5,7.23 Z', // Adjusted triangle path
                fillColor: marker.color, // Fill color in hex format
                fillOpacity: 1,
                strokeColor: '#000000', // Stroke color in hex format
                strokeWeight: 2,
                scale: 1,
                labelOrigin: new google.maps.Point(0, 18)
            };

            window.mapModule.markers[marker.id] = new google.maps.Marker({
                //position: { lat: marker.latitude, lng: marker.longitude },
                //map: window.mapModule.map,
                icon: markerIcon,
                label: marker.label,
                fontSize: "26px",
                fontWeight: "bold"

            });
        }
        window.mapModule.markers[marker.id].setPosition({ lat: marker.latitude, lng: marker.longitude });
        window.mapModule.markers[marker.id].setMap(window.mapModule.map);
    },
    removeMarker: function (key) {
        window.mapModule.markers[key].setMap(null);
        delete window.mapModule.markers[key];
    },
    syncInfoMarkers: function (newInfoMarkers) {
        //console.info("receiveInfoMarkers");
        for(let i = 0; i < window.mapModule.infoMarkers.length; i++) {
            window.mapModule.infoMarkers[i].close();
            delete window.mapModule.infoMarkers[i];
        }
        newInfoMarkers.forEach((infoMarker) => window.mapModule.receiveInfoMarker(infoMarker));
    },
    receiveInfoMarker: function (infoMarker) {
        //console.info("receiveInfoMarker", infoMarker);
        if(!window.mapModule.infoMarkers.hasOwnProperty(infoMarker.id)) {
            window.mapModule.infoMarkers[infoMarker.id] = new google.maps.InfoWindow();
        }
        window.mapModule.infoMarkers[infoMarker.id].setPosition({ lat: infoMarker.latitude, lng: infoMarker.longitude });

        // Step 1: Parse the ISO 8601 string
        const dateObject = new Date(infoMarker.createdOn);

        // Step 2: Extract the hour and minute components
        const hours = dateObject.getHours();
        const minutes = dateObject.getMinutes();

        // Step 3: Format the time
        const amOrPm = hours >= 12 ? "PM" : "AM";
        const formattedHours = hours % 12 || 12; // Convert 0 to 12 for 12-hour format

        // Step 4: Create the date time format
        const dateTimeFormat = `${formattedHours}:${minutes.toString().padStart(2, "0")} ${amOrPm}`;

        window.mapModule.infoMarkers[infoMarker.id].setContent("<h6 style='color:" + infoMarker.labelColor + "'>" + infoMarker.label + "</h6>" + infoMarker.message + dateTimeFormat);
        window.mapModule.infoMarkers[infoMarker.id].open(window.mapModule.map);
    },
    removeInfoMarker: function (key) {
        window.mapModule.infoMarkers[key].close();
        delete window.mapModule.infoMarkers[key];
    },
    setCenter: function (lat, lng) {
        window.mapModule.map.setCenter({ lat: lat, lng: lng });
        window.mapModule.map.setZoom(16.0);
    }
};

export class MapModule {
    markers = [];
    infoMarkers = [];
    map = null; // This property will hold the map instance
    dotNetReference = null;
    firstLoad = true;
    isGoogleMapsApiLoading() {
        return window.mapModule.loading;
    }
    googleMapClickListener(mapsMouseEvent) {
        //console.info("googleMapClickListener", mapsMouseEvent);
        let message = prompt("OK - Send the marker.\nCancel - Abort.\n * A message is optional and not required *", "");
        if(message != null) {
            this.dotNetReference.invokeMethodAsync("UpdateMyInfoMarker", message, mapsMouseEvent.latLng.lat(), mapsMouseEvent.latLng.lng());
        }
    }
    addListenerOnceMapIdle(ref) {
        //console.info("addListenerOnceMapIdle", ref);
        this.dotNetReference = ref;
        window.mapModule.map = new google.maps.Map(document.getElementById("map"), {
            zoom: 5,
            center: { lat: 36.0, lng: -98.0 },
            mapTypeId: 'terrain'
        });
        for(let i = 0; i < this.markers.length; i++) {
            this.markers[i].setMap(null);
            delete this.markers[i];
        }
        for(let i = 0; i < this.infoMarkers.length; i++) {
            this.infoMarkers[i].setMap(null);
            delete this.infoMarkers[i];
        }
        window.mapModule.map.addListener("click", (mapsMouseEvent) => this.googleMapClickListener(mapsMouseEvent));
        google.maps.event.addListenerOnce(window.mapModule.map, 'idle', () => this.watchPosition());
    }
    watchPositionSuccess(position) {
        //console.info("watchPositionSuccess", position);
        if(this.firstLoad) {
            this.firstLoad = false;
            this.setCenter(position.coords.latitude, position.coords.longitude);
        }
        this.dotNetReference.invokeMethodAsync("UpdateCurrentPosition", position.coords.latitude, position.coords.longitude);
    }
    watchPositionError(error) {
        //console.info("watchPositionError", error);
        this.firstLoad = true;
        navigator.geolocation.clearWatch(window.mapModule.positionWatch);
        if(error.code == GeolocationPositionError.PERMISSION_DENIED) {
            alert("Location permission denied. Please allow location access.");
            this.dotNetReference.invokeMethodAsync("UpdateCurrentPositionError");
            return;
        }
        this.watchPosition();
    }
    watchPosition() {
        //console.info("watchPosition");
        this.loadingState = -1;
        this.firstLoad = true;
        if(window.mapModule.positionWatch != null) {
            navigator.geolocation.clearWatch(window.mapModule.positionWatch);
        }
        window.mapModule.positionWatch = navigator.geolocation.watchPosition(
            (position) => this.watchPositionSuccess(position),
            (error) => this.watchPositionError(error), {
            enableHighAccuracy: true
        });
    }
    syncMarkers(newMarkers) {
        //console.info("syncMarkers", newMarkers);
        for(let i = 0; i < this.markers.length; i++) {
            this.markers[i].setMap(null);
            delete this.markers[i];
        }
        newMarkers.forEach((marker) => this.receiveMarker(marker));
    }
    receiveMarker(marker) {
        //console.info("receiveMarker", marker);
        if(!this.markers.hasOwnProperty(marker.id)) {
            const markerIcon = {
                path: 'M -12.5,7.23 L 0,-14.46 L 12.5,7.23 Z', // Adjusted triangle path
                fillColor: marker.color, // Fill color in hex format
                fillOpacity: 1,
                strokeColor: '#000000', // Stroke color in hex format
                strokeWeight: 2,
                scale: 1,
                labelOrigin: new google.maps.Point(0, 18)
            };

            this.markers[marker.id] = new google.maps.Marker({
                icon: markerIcon,
                label: marker.label,
                fontSize: "26px",
                fontWeight: "bold"

            });
        }
        this.markers[marker.id].setPosition({ lat: marker.latitude, lng: marker.longitude });
        this.markers[marker.id].setMap(window.mapModule.map);
    }
    removeMarker(key) {
        //console.info("removeMarker", key);
        this.markers[key].setMap(null);
        delete this.markers[key];
    }
    syncInfoMarkers(newInfoMarkers) {
        //console.info("syncInfoMarkers", newInfoMarkers);
        for(let i = 0; i < this.infoMarkers.length; i++) {
            this.infoMarkers[i].setMap(null);
            delete this.infoMarkers[i];
        }
        newInfoMarkers.forEach((infoMarker) => this.receiveInfoMarker(infoMarker));
    }
    googleMapsCloseClickEvent(id) {
        //console.info("googleMapsCloseClickEvent", id);
        this.dotNetReference.invokeMethodAsync("RemoveInfoMarker", id);
    }
    receiveInfoMarker(infoMarker) {
        //console.info("receiveInfoMarker", infoMarker);
        if(!this.infoMarkers.hasOwnProperty(infoMarker.id)) {
            this.infoMarkers[infoMarker.id] = new google.maps.InfoWindow();
            google.maps.event.addListener(this.infoMarkers[infoMarker.id], 'closeclick', () => this.googleMapsCloseClickEvent(infoMarker.id));
        }
        this.infoMarkers[infoMarker.id].setPosition({ lat: infoMarker.latitude, lng: infoMarker.longitude });

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

        this.infoMarkers[infoMarker.id].setContent("<h6 style='color:" + infoMarker.labelColor + "'>" + infoMarker.label + "</h6>" + infoMarker.message + dateTimeFormat);
        this.infoMarkers[infoMarker.id].open(window.mapModule.map);
    }
    removeInfoMarker(key) {
        //console.info("removeInfoMarker", key);
        this.infoMarkers[key].setMap(null);
        delete this.infoMarkers[key];
    }
    setCenter(lat, lng) {
        window.mapModule.map.setCenter({ lat: lat, lng: lng });
        window.mapModule.map.setZoom(16.0);
    }
}

export function CreateMapModule() {
    //console.info("CreateMapModule");
    return new MapModule();
}

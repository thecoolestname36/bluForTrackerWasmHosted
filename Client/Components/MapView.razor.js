export class MapModule {
    markers = [];
    infoMarkers = [];
    map = null; // This property will hold the map instance
    dotNetReference = null;
    isGoogleMapsApiLoading() {
        //console.info("isGoogleMapsApiLoading", window.mapModule.loading);
        return window.mapModule.loading;
    }
    googleMapClickListener(mapsMouseEvent) {
        //console.info("googleMapClickListener", mapsMouseEvent.latLng);
        let message = prompt("OK - Send the marker.\nCancel - Abort.\n * A message is optional and not required *", "");
        if(message != null) {
            this.dotNetReference.invokeMethodAsync("BroadcastInfoMarker", message, mapsMouseEvent.latLng.lat(), mapsMouseEvent.latLng.lng());
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
            // Might need to move this back down?
            this.markers[marker.id].setMap(window.mapModule.map);
        }
        this.markers[marker.id].setPosition({ lat: marker.spike.latitude, lng: marker.spike.longitude });
        
        //console.info("receiveMarker - done");
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
        this.infoMarkers[infoMarker.id].setPosition({ lat: infoMarker.spike.latitude, lng: infoMarker.spike.longitude });

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
        //console.info("setCenter", lat, lng);
        window.mapModule.map.setCenter({ lat: lat, lng: lng });
        window.mapModule.map.setZoom(16.0);
    }
}

export function CreateMapModule() {
    //console.info("CreateMapModule");
    return new MapModule();
}

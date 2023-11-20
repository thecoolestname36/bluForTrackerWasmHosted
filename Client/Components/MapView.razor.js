export class MapModule {
    mapMarkers = [];
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
    setupMapModule(ref) {
        //console.info("addListenerOnceMapIdle", ref);
        this.dotNetReference = ref;
        window.mapModule.map = new google.maps.Map(document.getElementById("map"), {
            zoom: 4,
            center: { lat: 43.0, lng: -95.0 },
            mapTypeId: 'terrain'
        });
        for(let i = 0; i < this.mapMarkers.length; i++) {
            this.mapMarkers[i].setMap(null);
            delete this.mapMarkers[i];
        }
        for(let i = 0; i < this.infoMarkers.length; i++) {
            this.infoMarkers[i].setMap(null);
            delete this.infoMarkers[i];
        }
        window.mapModule.map.addListener("click", (mapsMouseEvent) => this.googleMapClickListener(mapsMouseEvent));
        this.dotNetReference.invokeMethodAsync("MapModuleSetupComplete");
    }
    receiveTeam(googleMapsInfoMarkers) {
        console.info("receiveTeam", googleMapsInfoMarkers);
        for(let i = 0; i < this.mapMarkers.length; i++) {
            this.mapMarkers[i].setMap(null);
            delete this.mapMarkers[i];
        }
        for(let i = 0; i < this.infoMarkers.length; i++) {
            this.infoMarkers[i].setMap(null);
            delete this.infoMarkers[i];
        }
        googleMapsInfoMarkers.forEach((googleMapsInfoMarker) => this.receiveTeamMember(googleMapsInfoMarker));
    }
    receiveTeamMember(googleMapsInfoMarker) {
        //console.info("receiveTeamMember", googleMapsInfoMarker);
        if(this.mapMarkers.hasOwnProperty(googleMapsInfoMarker.connectionId)) {
            this.removeTeamMember(googleMapsInfoMarker.connectionId);
        }
        const markerIcon = {
            path: 'M -12.5,7.23 L 0,-14.46 L 12.5,7.23 Z', // Adjusted triangle path
            fillColor: googleMapsInfoMarker.color, // Fill color in hex format
            fillOpacity: 1,
            strokeColor: '#000000', // Stroke color in hex format
            strokeWeight: 2,
            scale: 1,
            labelOrigin: new google.maps.Point(0, 18)
        };

        this.mapMarkers[googleMapsInfoMarker.connectionId] = new google.maps.Marker({
            icon: markerIcon,
            label: googleMapsInfoMarker.label,
            fontSize: "26px",
            fontWeight: "bold"

        });
        // Maybe a bug here, the map can sometimes be null
        this.mapMarkers[googleMapsInfoMarker.connectionId].setMap(window.mapModule.map);
        if(googleMapsInfoMarker.latitude != 0.0 && googleMapsInfoMarker.longitude != 0.0) {
            this.mapMarkers[googleMapsInfoMarker.connectionId].setPosition({ lat: googleMapsInfoMarker.latitude, lng: googleMapsInfoMarker.longitude });
        }
    }
    removeTeamMember(connectionId) {
        //console.info("removeTeamMember", connectionId);
        this.mapMarkers[connectionId].setMap(null);
        delete this.mapMarkers[connectionId];
    }
    receiveMapMarker(connectionId, mapMarker) {
        console.info("receiveMapMarker", connectionId, mapMarker);
        if(this.mapMarkers.hasOwnProperty(connectionId) && mapMarker.latitude != 0.0 && mapMarker.longitude != 0.0) {
            this.mapMarkers[connectionId].setPosition({ lat: mapMarker.latitude, lng: mapMarker.longitude });
        }
    }
    receiveInfoMarker(connectionId, username, color, infoMarker) {
        console.info("receiveInfoMarker", connectionId, username, color, infoMarker);
        if(!this.infoMarkers.hasOwnProperty(connectionId)) {
            this.infoMarkers[connectionId] = new google.maps.InfoWindow();
            //google.maps.event.addListener(this.infoMarkers[infoMarker.id], 'closeclick', () => this.googleMapsCloseClickEvent(infoMarker.id));
        }
        this.infoMarkers[connectionId].setPosition({ lat: infoMarker.latitude, lng: infoMarker.longitude });

        // Step 1: Parse the ISO 8601 string
        const dateObject = new Date(infoMarker.timestamp);

        // Step 2: Extract the hour and minute components
        const hours = dateObject.getHours();
        const minutes = dateObject.getMinutes();

        // Step 3: Format the time
        const amOrPm = hours >= 12 ? "PM" : "AM";
        const formattedHours = hours % 12 || 12; // Convert 0 to 12 for 12-hour format

        // Step 4: Create the date time format
        const dateTimeFormat = `${formattedHours}:${minutes.toString().padStart(2, "0")} ${amOrPm}`;

        this.infoMarkers[connectionId].setContent("<h6 style='color:" + color + "'>" + username + "</h6>" + infoMarker.message + dateTimeFormat);
        this.infoMarkers[connectionId].open(window.mapModule.map);
    }


    //removeInfoMarker(key) {
    //    //console.info("removeInfoMarker", key);
    //    this.infoMarkers[key].setMap(null);
    //    delete this.infoMarkers[key];
    //}
    //syncInfoMarkers(newInfoMarkers) {
    //    //console.info("syncInfoMarkers", newInfoMarkers);
    //    for(let i = 0; i < this.infoMarkers.length; i++) {
    //        this.infoMarkers[i].setMap(null);
    //        delete this.infoMarkers[i];
    //    }
    //    newInfoMarkers.forEach((infoMarker) => this.receiveInfoMarker(infoMarker));
    //}
    googleMapsCloseClickEvent(id) {
        console.info("googleMapsCloseClickEvent", id);
        this.dotNetReference.invokeMethodAsync("RemoveInfoMarker", id);
    }
    setCenter(lat, lng) {
        //console.info("setCenter", lat, lng);
        window.mapModule.map.setCenter({ lat: lat, lng: lng });
        window.mapModule.map.setZoom(16.0);
    }
}

export function CreateMapModule() {
    console.info("CreateMapModule");
    return new MapModule();
}

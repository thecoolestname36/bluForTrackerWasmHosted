export class GeolocationService {
    positionWatch;
    constructor(ref) {
        this.dotNetReference = ref;
        this.status = 0;
    }
    getStatus() {
        return this.status;
    }
    watchPositionSuccess(position) {
        //console.info("watchPositionSuccess", position);
        this.dotNetReference.invokeMethodAsync("UpdateCurrentPosition", position.coords.latitude, position.coords.longitude);
        this.status = 3;
    }
    watchPositionError(error) {
        //console.info("watchPositionError", error);
        navigator.geolocation.clearWatch(this.positionWatch);
        this.status = -1;
        if(error.code == GeolocationPositionError.PERMISSION_DENIED) {

            alert("Location permission denied. Please allow location access.");
        }
        this.watchPosition();
        this.dotNetReference.invokeMethodAsync("UpdateCurrentPositionError");
    }
    watchPosition() {
        //console.info("watchPosition");
        this.status = 1;
        if(this.positionWatch != null) {
            navigator.geolocation.clearWatch(this.positionWatch);
        }
        this.positionWatch = navigator.geolocation.watchPosition(
            (position) => this.watchPositionSuccess(position),
            (error) => this.watchPositionError(error), {
            enableHighAccuracy: true
        });
        this.status = 2;
    }
    dispose() {
        if(this.positionWatch != null) {
            navigator.geolocation.clearWatch(this.positionWatch);
        }
        this.status = 0;
    }
}

export function CreateGeolocationService(dotNetReference) {
    //console.info("CreateGeolocationService");
    return new GeolocationService(dotNetReference);
}

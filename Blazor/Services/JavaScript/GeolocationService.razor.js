export class GeolocationService {
    writeLine(string) {
        console.info("writeLine", string);
    }
}

export function CreateGeolocationService() {
    console.info("CreateGeolocationService");
    return new GeolocationService();
}

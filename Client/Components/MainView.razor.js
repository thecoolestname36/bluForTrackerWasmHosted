export class MainModule {
	lockScreenButtonElement = null;
	screenLockOverlayElement = null;
	tryAllowFullscreen(_lockScreenButtonContainerElement, _lockScreenButtonElement, _screenLockOverlayElement) {
		if(document.fullscreenEnabled) {
			_lockScreenButtonContainerElement.classList.remove("display-none");
			this.lockScreenButtonElement = _lockScreenButtonElement;
			this.screenLockOverlayElement = _screenLockOverlayElement;
			document.documentElement.addEventListener("fullscreenchange", (event) => {
				if(document.fullscreenElement === null) {
					this.screenLockOverlayElement.classList.add("display-none");
					this.lockScreenButtonElement.classList.remove("locked");
					try {
						navigator.wakeLock.release("screen");
					} catch(err) {
						console.info(err);
					}
				}
			});
		}
		return document.fullscreenEnabled;
	}
	requestFullscreen() {
		document.documentElement.requestFullscreen();
		this.lockScreenButtonElement.classList.add("locked");
		this.screenLockOverlayElement.classList.remove("display-none");
		try {
			navigator.wakeLock.request("screen");
		} catch(err) {
			console.info(err);
		}
	}
}
export function CreateMainModule() {
	//console.info("CreateMapModule");
	return new MainModule();
}

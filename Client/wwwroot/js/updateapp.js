const bc = new BroadcastChannel('update-channel');
bc.onmessage = function (message) {
    if (message && message.data == "new-version-found") {
        bc.postMessage("skip-waiting");
    }
    if (message && message.data == "reload") {
        restartApp();
        // bc.postMessage("restarted")
    }
    // if (message && message.data == "notify") {
    //     notifyNewVersion();
    // }    
}
let dotnetObj;
window.Updater = {
    Initialize: function (interop) {
        dotnetObj = interop;
    },
    Dispose: function () {

        if (dotnetObj != null) {
            dotnetObj.dispose();
        }
    }
}
async function notifyNewVersion() {
    await dotnetObj.invokeMethodAsync("Updater.NotifyUpdateAvailable", true);
    localStorage.setItem("new-version", false);
}
function restartApp() {
    localStorage.setItem("new-version", true);
    window.location.reload();    
}
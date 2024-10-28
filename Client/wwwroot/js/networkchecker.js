window.onload = ()=> {
    if (localStorage.getItem("mode") === null) {
        localStorage.setItem("mode", true);
    }    
}

let handler;

window.Connection = {
    Initialize: function (interop) {

        handler = function () {
            var status = navigator.onLine
            interop.invokeMethodAsync("Connection.StatusChanged", status);
            localStorage.setItem("mode", status);
        }

        window.addEventListener("online", handler);
        window.addEventListener("offline", handler);

        handler(navigator.onLine);
    },
    Dispose: function () {

        if (handler != null) {

            window.removeEventListener("online", handler);
            window.removeEventListener("offline", handler);
        }
    }
};
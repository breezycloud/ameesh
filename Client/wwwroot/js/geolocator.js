function getLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(showPosition);
    } else {
        alert("Geolocation is not supported by this browser.");
    }
}

function showPosition(position) {
    console.log(position);    
    document.getElementById("#lat").innerText = position.coords.latitude;
    document.getElementById("#long").innerText = position.coords.longitude;    
}
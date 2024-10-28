function exportFile(reportName, byteArray) {
    var link = document.createElement('a');
    link.download = reportName;
    link.href = "data:application/octet-stream;base64," + byteArray;
    link.target = '_blank';
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
}       
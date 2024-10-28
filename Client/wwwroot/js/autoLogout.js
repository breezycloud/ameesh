window.initAutoLogoutListener = (helper) => {
    dotNetHelper = helper;
    document.addEventListener('mousemove', resetTimer);
    document.addEventListener('keypress', resetTimer);
};

let timer;
let dotNetHelper;
function resetTimer() {
    clearTimeout(timer);
    timer = setTimeout(() => {
        dotNetHelper.invokeMethodAsync('LogOut');
    }, 10 * 60 * 1000); // 10 minutes timeout    
}

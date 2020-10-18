function downloadFromUrl(url) {

    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = '';
    anchorElement.click();
    anchorElement.remove();
}

function showToastr(message, flag) {

    switch (flag) {
        case 0:            
            toastr.success(message);
            break;
        case 1:
            toastr.info(message);
            break;
        case 2:
            toastr.warning(message);
            break;
        case 3:
            toastr.error(message);
            break;
    }
}

(function () {
    window.blazorLocalStorage = {
        get: key => key in localStorage ? JSON.parse(localStorage[key]) : null,
        set: (key, value) => { localStorage[key] = JSON.stringify(value); },
        delete: key => { delete localStorage[key]; }
    };
})();
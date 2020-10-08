function downloadFromUrl(url) {

    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = '';
    anchorElement.click();
    anchorElement.remove();
}
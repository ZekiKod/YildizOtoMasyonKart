window.JSFunctions = {
    clearElementById: function (elementId) {
        document.getElementById(elementId).value = '';
    },
    focusElementById: function (elementId) {
        document.getElementById(elementId).focus();
    }
};
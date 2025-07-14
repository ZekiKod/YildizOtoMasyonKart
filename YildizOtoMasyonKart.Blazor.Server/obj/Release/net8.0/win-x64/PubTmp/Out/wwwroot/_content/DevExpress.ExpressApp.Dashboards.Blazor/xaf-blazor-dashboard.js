
const dashboardContainerSelector = ".xaf-dashboard-container";
const newDashboardId = "_internal_XAF_Blazor_New_Dashboard_ID";

globalThis.xafBlazorDashboard = {
    isDevExtremeAddedExplicitly: function () {
        return window.DevExpress?.VERSION != undefined;
    },
    setModified: (isModified) => { /* will be set at runtime */ },
    getDashboardControl: null,
    reloadDashboard: async function () {
        const dashboardControl = this.getDashboardControl(),
            id = dashboardControl.getDashboardId(),
            openDashboardExtension = dashboardControl.findExtension("openDashboard");
        await openDashboardExtension.loadDashboard(id);
    },
    uploadFileToClient: function (fileName, xml) {
        const link = document.createElement('a');
        link.download = fileName;
        const xmlEncoded = encodeURIComponent(xml);
        link.href = "data:text/xml," + xmlEncoded;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },
    isFullScreen: false,
    fullScreenToggle: function () {
        const fullscreenClassName = "xaf-dashboard-fullscreen",
            dashboardContainer = document.querySelector(dashboardContainerSelector);
        this.isFullScreen = !this.isFullScreen;
        this.isFullScreen
            ? dashboardContainer.classList.add(fullscreenClassName)
            : dashboardContainer.classList.remove(fullscreenClassName);
        this.getDashboardControl().findExtension("viewerApi").updateDashboardTitleToolbar();
    },
    options: {},
    setOption: function (name, value) {
        this.options[name] = value;
    },
    getOption: function (name) {
        return this.options[name];
    },
    DxDashboardContainerReference: null,
    setDxDashboardContainerReference: function (DxDashboardContainerReference) {
        this.DxDashboardContainerReference = DxDashboardContainerReference;
    },
    dashboardModifiedChanged: async function (isModified) {
        try {
            await globalThis.xafBlazorDashboard.DxDashboardContainerReference.invokeMethodAsync("SetModified", isModified);
        }
        catch { } // the dashboard container is probably disposed at this point, so any errors can be ignored
    },
    // web dashboard control subscriptions
    extensions: {
        viewerApi: {
            // add a fullscreen mode toggle button
            onDashboardTitleToolbarUpdated: function (e) {
                const fullScreenButton = {
                    icon: globalThis.xafBlazorDashboard.isFullScreen
                        ? "dx-dashboard-restore-item"
                        : "dx-dashboard-maximize-item",
                    type: "button",
                    text: "Full Screen",
                    disabled: false,
                    click: function (_) {
                        globalThis.xafBlazorDashboard.fullScreenToggle();
                    },
                    name: "fullscreen",
                    hint: "Toggle full screen mode",
                };
                e.options.actionItems.push(fullScreenButton);
            }
        }
    },
    onOptionChanged: function (args) {
        // remove the "Create data source..." button again if necessary
        if (args.fullName === "extensions" && args.value.dataSourceWizard) {
            conditionalHideCreateDataSourceButton(args.component);
        }
    },
    onBeforeRender: function (args) {
        const dashboardControl = args.component;
        globalThis.xafBlazorDashboard.getDashboardControl = () => dashboardControl;
        conditionalHideCreateDataSourceButton(dashboardControl);
        setupUnsavedChangesConfirmation();
        customizeMenuItems();
        setupLoadingIndicators();
        // resize observer subscription for dynamic resizing by height
        setupResizeObservers();
        // execute user scripts
        if (globalThis.xafBlazorDashboardUserScripts) {
            globalThis.xafBlazorDashboardUserScripts.forEach(script => {
                if (script.onBeforeRender) {
                    script.onBeforeRender(dashboardControl);
                }
            });
        }
    }
};


function setupResizeObservers() {
    const dashboardResizeObserver = "dashboardResizeObserver";
    window.xaf.resizeObserverUtils.unsubscribeFromResizeObservers(dashboardResizeObserver);
    window.xaf.resizeObserverUtils.subscribeToResizeObservers(dashboardContainerSelector, dashboardResizeObserver);
}

function setupLoadingIndicators() {
    // dashboard loading helper classes
    const dashboardControl = globalThis.xafBlazorDashboard.getDashboardControl(),
        loadingIndicatorClassName = "xaf-dashboard-loading";
    dashboardControl.on("dashboardBeginUpdate", args => {
        document.querySelector(dashboardContainerSelector).classList.add(loadingIndicatorClassName);
    });
    dashboardControl.on("dashboardEndUpdate", args => {
        document.querySelector(dashboardContainerSelector).classList.remove(loadingIndicatorClassName);
    });
}

function customizeMenuItems() {
    const toolbox = globalThis.xafBlazorDashboard.getDashboardControl().findExtension('toolbox');
    // toolbox can be null in the ViewerOnly mode
    if (toolbox) {
        // remove the "create new dashboard" menu item
        const createNewButton = toolbox.menuItems().find(menuItem => menuItem.id === "create-dashboard");
        toolbox.menuItems.remove(createNewButton);
        // remove the "Open" menu item
        const openDashboardButton = toolbox.menuItems().find(menuItem => menuItem.id === "open-dashboard");
        toolbox.menuItems.remove(openDashboardButton);
        // save button override
        const saveButton = toolbox.menuItems().find((menuItem) => menuItem.id === "save"),
            saveBase = saveButton.click;
        saveButton.click = () => {
            const dashboardControl = globalThis.xafBlazorDashboard.getDashboardControl(),
                isDashboardNew = dashboardControl.dashboardContainer().id === newDashboardId;
            if (isDashboardNew) {
                const toolboxExtension = dashboardControl.findExtension('toolbox');
                if (toolboxExtension) {
                    toolboxExtension.menuVisible(false);
                }
                const dashboardJSON = dashboardControl.dashboardContainer().dashboard.getJSON(),
                    dashboardTitle = dashboardControl.dashboard().title.text();
                dashboardControl.findExtension('create-dashboard').performCreateDashboard(dashboardTitle, dashboardJSON);
            } else {
                saveBase();
            }
        };
    }
}

function setupUnsavedChangesConfirmation() {
    const undoRedoExtension = globalThis.xafBlazorDashboard.getDashboardControl().findExtension("undoRedo");
    if (undoRedoExtension) {
        undoRedoExtension.isChanged.subscribe(isChanged => {
            globalThis.xafBlazorDashboard.dashboardModifiedChanged(isChanged);
        });
        globalThis.xafBlazorDashboard.setModified = (isModified) => {
            const undoRedoExtension = globalThis.xafBlazorDashboard.getDashboardControl().findExtension("undoRedo");
            undoRedoExtension.isChanged(isModified);
        };
    }
}

function conditionalHideCreateDataSourceButton(dashboardControl) {
    if (globalThis.xafBlazorDashboard.getOption("HideDirectDataSourceConnections")) {
        dashboardControl.unregisterExtension('dataSourceWizard');
    }
}

window.DxDashboard = window.DxDashboard || (() => {
    function getOptions(options, additionalOptions, dotNetCallbacks) {
        setCallbacks(options, additionalOptions.clientToServerCallbacksMap, dotNetCallbacks);
        setUserOptions(options, additionalOptions);
        return options;
    }
    function setCallbacks(options, callbacksMap, dotNetCallbacks) {
        if(callbacksMap) {
            Object.keys(callbacksMap).forEach(key => {
                options[key] = (args) => dotNetCallbacks.invokeMethodAsync(callbacksMap[key], getCallbackArgs(key, args));
            });
        }
    }
    function getCallbackArgs(event, args) {
        switch(event) {
            case 'onOptionChanged':
                return {
                    fullName: args.fullName,
                    value: args.value
                }
        }
        return args;
    }
    function setUserOptions(options, additionalOptions) {
        if(additionalOptions && additionalOptions.identifier) {
            let parts = additionalOptions.identifier.split(/[.[]/);
            let userOptions = window;
            for(let i = 0; i < parts.length && userOptions; i++) {
                userOptions = userOptions[parts[i]]
            }
            if(userOptions)
                addUserOptions(options, userOptions);
        }
    }
    function addUserOptions(options, userOpts) {
        if(!userOpts || !options)
            return;
        for(let name in userOpts) {
            let userOpt = userOpts[name];
            if(userOpt) {
                let opt = options[name];
                if(typeof (opt) === 'object' && typeof (userOpt) === 'object')
                    addUserOptions(opt, userOpt);
                else if(typeof (opt) === 'function' && typeof (userOpt) === 'function')
                    options[name] = (args) => { opt(args); userOpt(args); }
                else if(!opt)
                    options[name] = userOpt;
            }
        }
    }
    function removeUnchanged(options, current) {
        if(!options || !current)
            return;
        for(let name in options) {
            let opt = options[name];
            let curOpt = current[name];
            if(opt && typeof (opt) === 'object' && typeof (curOpt) === 'object') {
                removeUnchanged(opt, curOpt);
                if(Object.keys(opt).length === 0)
                    delete options[name];
            } else if(opt === curOpt)
                delete options[name];
        }
    }
    function init(elt, options, additionalOptions, dotNetCallbacks) {
        const control = new DevExpress.Dashboard.DashboardControl(elt, getOptions(options, additionalOptions, dotNetCallbacks));
        control.render();
        return control;
    }
    function update(control, options, dotNetCallbacks) {
        let newOptions = getOptions(options, {}, dotNetCallbacks);
        removeUnchanged(newOptions, control.option());
        if(newOptions && Object.keys(newOptions).length > 0)
            control.option(newOptions);
    }
    function setGlobalization(culture) {
        DevExpress.localization.locale(culture);
    }
    function setLocalization(messages) {
        DevExpress.Dashboard.ResourceManager.setLocalizationMessages(messages);
    }
    function dispose(dashboardControl) {
        dashboardControl && dashboardControl.dispose();
    }
    return {
        init,
        update,
        setGlobalization,
        setLocalization,
        dispose
    };
})();
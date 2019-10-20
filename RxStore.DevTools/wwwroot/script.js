window.___RxStore___DevTools___ = {

    connections: { },

    IsEnabled() {
        return '__REDUX_DEVTOOLS_EXTENSION__' in window;
    },

    OnInitialState(instanceName, stateJson) {
        if (!!this.connections[instanceName]) {
            return;
        }

        const options = {
            name: instanceName,
            features: {
                pause: false,
                lock: false,
                persist: false,
                export: false,
                import: false,
                jump: false,
                skip: false,
                reorder: false,
                dispatch: false,
                test: false
            }
        };

        this.connections[instanceName] = window.__REDUX_DEVTOOLS_EXTENSION__.connect(options);

        const state = JSON.parse(stateJson);

        this.connections[instanceName].init(state);
    },

    OnAction(instanceName, actionJson, stateJson) {
        const action = JSON.parse(actionJson);
        const state = JSON.parse(stateJson);

        this.connections[instanceName].send(action, state);
    }

};

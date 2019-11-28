window.___RxStore___DevTools___ =
    '__REDUX_DEVTOOLS_EXTENSION__' in window
        ? whenAvailable()
        : whenNotAvailable();


function whenAvailable() {
    return {
        connections: { },

        OnInitialState(instanceName, stateJson) {
            if (!!this.connections[instanceName]) {
                return false;
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

            return true;
        },

        OnAction(instanceName, actionJson, stateJson) {
            if (!this.connections[instanceName]) {
                return false;
            }

            const action = JSON.parse(actionJson);
            const state = JSON.parse(stateJson);

            this.connections[instanceName].send(action, state);

            return true;
        }
    };
}


function whenNotAvailable() {
    return {
        OnInitialState(instanceName, stateJson) {
            return false;
        },

        OnAction(instanceName, actionJson, stateJson) {
            return false;
        }
    };
}


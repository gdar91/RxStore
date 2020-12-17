(function() {
    window['__RxStore_ForBlazor_DevTools__'] =
        '__REDUX_DEVTOOLS_EXTENSION__' in window
            ? whenExtensionAvailable()
            : whenExtensionNotAvailable();


    function whenExtensionAvailable() {
        return {
            connections: { },

            OnInitial(instanceName, stateJson) {
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

            OnEvent(instanceName, eventJson, stateJson) {
                if (!this.connections[instanceName]) {
                    return false;
                }

                const event = JSON.parse(eventJson);
                const state = JSON.parse(stateJson);

                this.connections[instanceName].send(event, state);

                return true;
            }
        };
    }


    function whenExtensionNotAvailable() {
        return {
            OnInitialState(instanceName, stateJson) {
                return false;
            },

            OnAction(instanceName, actionJson, stateJson) {
                return false;
            }
        };
    }
})();

var Behaviour = {
    /*"var parent = document.currentScript.parentElement;"+
            "parent.removeChild(document.currentScript);"+*/
    GetCode: function (className, ...parameters){
        return "new "+className+"(parent,"+parameters.map(function(x) {return JSON.stringify(x)}) +");"
    },
    Initialize: function (className, ...parameters){
        let code = Behaviour.GetCode(className, ...parameters)
        let parent = document.currentScript.parentElement;
        parent.removeChild(document.currentScript);

        Behaviour.InitializeInternal(className, parent, ...parameters);
    },

    InitializeInternal: function (className, element, ...parameters) {
        let x = new window[className](element, ...parameters)
        if (x.hasOwnProperty("OnWindowResize")) {
            window.addEventListener("resize", function () { x.OnWindowResize() });
        }
        if (x.hasOwnProperty("OnDOMContentLoaded")) {
            document.addEventListener("DOMContentLoaded", function () { x.OnDOMContentLoaded() });
        }

        if (x.hasOwnProperty("OnResourcesLoaded")) {
            document.addEventListener("readystatechange", function () {
                if (document.readyState == "complete") x.OnResourcesLoaded()
            });
        }
    },

    /*Create: function(methodName, ...parameters){
        var result = document.createElement('script');        
        result.innerHTML = Behaviour.GetCode(methodName,...parameters)
        return result
    },*/

    Print: function(element, ...parameters){
        parameters.map(function(x){console.log(x)})
    },
    PrintParent: function(element, ...parameters){
        console.log(element)
    }
}
var Behaviour = {
    GetCode: function(methodName, ...parameters){
        return "var parent = document.currentScript.parentElement;"+
            "parent.removeChild(document.currentScript);"+
            methodName+"(parent,"+parameters.map(function(x) {return JSON.stringify(x)}) +");"
    },
    Initialize: function(methodName, ...parameters){
        var code = Behaviour.GetCode(methodName,...parameters)
        
        eval(code)
    },
    Create: function(methodName, ...parameters){
        var result = document.createElement('script');
        
        result.innerHTML = Behaviour.GetCode(methodName,...parameters)
        return result
    },
    Print: function(element, ...parameters){
        parameters.map(function(x){console.log(x)})
    },
    PrintParent: function(element, ...parameters){
        console.log(element)
    }
}
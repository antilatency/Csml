function MaterialCard(element) {

    var resize = function(){
        var width = element.offsetWidth;
        element.style.height = width+"px"
        var scale = 1;
        var scaleStart = 240;
        if (width<scaleStart){
            scale = width/scaleStart;

        }
        element.style.setProperty('--scale', scale);

        //element.style.fontSize = scale*16+"px"
        console.log(element.offsetWidth)
    }

    this.OnWindowResize = function (){
        resize();

    }

    resize();

}
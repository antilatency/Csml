function RoiImage(element, aspect, roi) {
    this.element = element;
    this.aspect = aspect;
    this.roi = roi;
    this.previousContainerWidth = -1;
    this.image = this.element.getElementsByTagName("img")[0];


    this.OnWindowResize = function (){
            
        var width = this.element.offsetWidth;
            
        if (this.previousContainerWidth == width) {
            return;
        }
        this.previousContainerWidth = width;
            

        var x0 = roi[0] / 100;
        var x1 = roi[1] / 100;
        var y0 = roi[2] / 100;
        var y1 = roi[3] / 100;
        var w = x1 - x0;
        var h = y1 - y0;
            
        var height = this.element.offsetHeight;
        var minHeight = width * h * aspect;
        var maxHeight = width / w * aspect;
        this.element.style.minHeight = minHeight + "px";
        this.element.style.maxHeight = maxHeight + "px";

        height = Math.max(height, minHeight);            
        height = Math.min(height, maxHeight);

        console.log("Align " + height)
        var containerAspect = height / width;
            
        if (containerAspect < aspect) {
            this.image.style.width = "100%"
            this.image.style.height = "auto"
            var offset = (1 - (containerAspect / aspect - h) / (1 - h)) * y0 * 100.0
            this.image.style.transform = "translate(0, -" + offset + "%)"
        } else {
            this.image.style.width = "auto"
            this.image.style.height = "100%"
            var offset = (1 - (aspect / containerAspect - w) / (1 - w)) * x0 * 100.0
            this.image.style.transform = "translate(-" + offset + "%, 0)"
        }
    }

    this.OnDOMContentLoaded = function (event) {
        this.OnWindowResize();
    }

}
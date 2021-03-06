function RoiImage(element, aspect, roi) {
    this.element = element;
    this.aspect = aspect;
    this.roi = roi;
    this.previousContainerWidth = -1;
    this.previousContainerHeight = -1;
    this.image = this.element.getElementsByTagName("img")[0];
    const parent = this.element.parentElement.className == "Section" || this.element.parentElement.className == "Header" ?
        null : this.element.parentElement;

    this.OnWindowResize = function() {
        //console.log(window.getComputedStyle(parent, null).getPropertyValue("height").replace("px", ""));
        let width = this.element.offsetWidth;
        let height = this.element.offsetHeight;

        // if (this.previousContainerWidth == width && this.previousContainerHeight == height) {
        //     return;
        // }
        this.previousContainerWidth = width;
        this.previousContainerHeight = height;


        let x0 = roi[0] / 100;
        let x1 = roi[1] / 100;
        let y0 = roi[2] / 100;
        let y1 = roi[3] / 100;
        let w = x1 - x0;
        let h = y1 - y0;

        let widthAspect = width * aspect;
        let minHeight = widthAspect * h;
        let maxHeight = widthAspect / w;
        if (parent) {
            const parentHeight = parent.offsetHeight;
            minHeight = Math.max(minHeight, parentHeight);
            maxHeight = Math.min(maxHeight, parentHeight);
            console.log(parent.className, minHeight, parentHeight);
        }

        height = Math.max(height, minHeight);
        height = Math.min(height, maxHeight);
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

        this.element.style.minHeight = minHeight + "px";
        this.element.style.maxHeight = maxHeight + "px";
    }

    this.OnResourcesLoaded = function() {
        this.OnWindowResize();
    }

    this.OnDOMContentLoaded = function(event) {
        this.OnWindowResize();
        //setTimeout(() => this.OnWindowResize(), 1000);
    }

}
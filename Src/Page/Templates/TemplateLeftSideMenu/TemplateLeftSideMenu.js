function TemplateLeftSideMenu(element, contentWidth, anchorLineWidth = 48) {
    // document.addEventListener("readystatechange", function () { console.log("readystatechange = " + document.readyState) })
    this.element = element;
    this.contentWidth = contentWidth;
    this.anchorLineWidth = anchorLineWidth;
    this.buttonChecked = false;
    //console.log(contentWidth)

    this.OnWindowResize = function () {
        this.Align();
    }

    this.Align = function () {
        var width = this.element.parentElement.offsetWidth;
        var menuWidth = this.leftSideMenu.offsetWidth;

        var enoughSpaceForMenu = (width - menuWidth) > (this.contentWidth + this.anchorLineWidth);

        if (enoughSpaceForMenu) {
            this.leftSideMenu.style.left = 0;
            this.button.style.visibility = "hidden"
        } else {
            this.button.style.visibility = "visible"
            if (this.buttonChecked) {
                this.leftSideMenu.style.left = 0;
            } else {
                this.leftSideMenu.style.left = -menuWidth + "px";
            }
        }


        if (enoughSpaceForMenu) {

            let rightSpace = (width - menuWidth) - this.contentWidth - this.anchorLineWidth;

            this.content.style.paddingLeft = anchorLineWidth + "px"
            this.element.style.marginLeft = menuWidth + "px";
            this.element.style.marginRight = Math.min(0.5 * rightSpace, menuWidth) + "px"

        } else {

            // setProperty("visible", "hidden", priority);

            this.element.style.marginLeft = 0;
            this.element.style.marginRight = 0;

            if (width < (this.contentWidth + this.anchorLineWidth)) {
                this.content.style.paddingLeft = 0
                this.content.classList.add("anchorsInside");
                this.content.classList.remove("anchorsOutside");
            } else {
                this.content.style.paddingLeft = anchorLineWidth + "px"
                this.content.classList.add("anchorsOutside");
                this.content.classList.remove("anchorsInside");
            }
        }

    }


    this.HamburgerButtonToggle = function () {
        console.log(this);
        this.buttonChecked = !this.buttonChecked;
        this.button.classList.toggle("Checked", this.buttonChecked);
        this.Align();
    }


    this.OnDOMContentLoaded = function (event) {
        this.leftSideMenu = this.element.querySelector(".LeftSideMenu");

        this.button = document.createElement('div');
        this.button.className = "HamburgerButton";
        var _this = this;
        this.button.onclick = function () { _this.HamburgerButtonToggle() };
        this.leftSideMenu.appendChild(this.button);

        this.content = this.element.querySelector(".Content");
        this.content.style.maxWidth = this.contentWidth + "px";
        this.content.classList.add("anchorsOutside");
        this.Align();

    }

    this.OnResourcesLoaded = function () {
        this.Align();

    }

}
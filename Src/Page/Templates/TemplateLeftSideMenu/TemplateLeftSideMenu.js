function TemplateLeftSideMenu(element, contentWidth, anchorLineWidth = 48) {
    // document.addEventListener("readystatechange", function () { console.log("readystatechange = " + document.readyState) })
    this.buttonChecked = false;
    this.OnWindowResize = function() {
        this.Align();
    }

    this.Align = function() {
        var width = element.parentElement.offsetWidth;
        var menuWidth = this.leftSideMenu.offsetWidth;
        var enoughSpaceForMenu = (width - menuWidth) > (contentWidth + anchorLineWidth);

        if (enoughSpaceForMenu) {
            this.leftSideMenu.style.left = 0;
            this.button.style.visibility = "hidden"
        } else {
            this.button.style.visibility = "visible"
            let elementStyle = element.style;
            let lefSideMenuStyle = this.leftSideMenu.style;
            if (this.buttonChecked) {
                elementStyle.overflow = "hidden";
                lefSideMenuStyle.left = 0;
                lefSideMenuStyle.overflowY = "auto";
                this.content.style.pointerEvents = "none"
            } else {
                this.content.style.pointerEvents = "auto"
                elementStyle.overflow = "auto";
                lefSideMenuStyle.overflowY = "visible";
                lefSideMenuStyle.left = -menuWidth + "px";
            }
        }

        if (enoughSpaceForMenu) {

            let rightSpace = (width - menuWidth) - contentWidth - anchorLineWidth;
            element.style.paddingLeft = menuWidth + anchorLineWidth + "px";
            element.style.paddingRight = Math.min(0.5 * rightSpace, menuWidth) + "px"

        } else {
            element.style.paddingLeft = 0;
            element.style.paddingRight = 0;

            if (width < (contentWidth + anchorLineWidth)) {
                this.content.classList.add("anchorsInside");
                this.content.classList.remove("anchorsOutside");
            } else {
                this.content.classList.add("anchorsOutside");
                this.content.classList.remove("anchorsInside");
            }
        }

    }


    this.HamburgerButtonToggle = function() {
        this.buttonChecked = !this.buttonChecked;
        this.button.classList.toggle("Checked", this.buttonChecked);
        this.Align();
    }

    this.ShowLeftMenu = function(show) {
        this.buttonChecked = show;
        if (show == true) this.leftSideMenu.removeChild(this.button);
        else this.leftSideMenu.appendChild(this.button);
        this.button.classList.toggle("Checked", show);
        this.Align();
    }


    this.OnDOMContentLoaded = function(event) {
        this.leftSideMenu = element.querySelector(".LeftSideMenu");

        this.button = document.createElement('div');
        this.button.className = "HamburgerButton";
        var _this = this;
        this.button.onclick = function() {
            _this.ShowLeftMenu(!this.buttonChecked);
        };
        this.leftSideMenu.appendChild(this.button);
        this.content = element.querySelector(".Content");
        this.content.style.maxWidth = contentWidth + "px";
        this.content.classList.add("anchorsOutside");
        this.Align();
    }


    this.contextHit = function(event, x) {
        if (Math.abs(x) > this.leftSideMenu.offsetWidth && this.buttonChecked == true) {
            event.preventDefault();
            this.ShowLeftMenu(false);
        }
    }

    this.moveForward = function(move) {
        move = parseInt(move);
        while (move < 0) {
            move += 1;
            this.leftSideMenu.style.left = move + "px";
        }
    }

    this.moveBackward = function(move) {
        move = parseInt(move);
        while (move > -this.leftSideMenu.offsetWidth) {
            move -= 1;
            this.leftSideMenu.style.left = move + "px";
        }
    }

    this.maxFontSize = null;

    this.OnResourcesLoaded = function() {
        this.Align();
        let dx;
        let dy;
        let xPos;
        let yPos;
        let _this = this;
        let leftMenuOpening = false;
        let leftMenuClosing;
        let leftMenuOpened = false;
        let startX,
            startY,
            dXThreshold = 15,
            dYThreshold = 5,
            verticalScrolled;

        let menuItems = element.querySelector(".LeftSideMenu").getElementsByClassName("Text");
        let path = window.location.pathname;
        for (let i = 0; i < menuItems.length; i++) {
            if (menuItems[i].href.split('/').reverse()[0] == path.split('/').reverse()[0]) {
                menuItems[i].style.backgroundColor = "#333333";
                break;
            }
        }

        window.addEventListener("click", function(event) {
            _this.contextHit(event, event.pageX);
            leftMenuOpening = leftMenuOpened = leftMenuClosing = false;
        }, { passive: false });

        window.addEventListener("touchstart", function(event) {
            if (event.touches.length > 1) return;
            let touch = event.touches[0];
            verticalScrolled = leftMenuOpening = leftMenuOpened = leftMenuClosing = false;
            startX = touch.pageX;
            startY = touch.pageY;
        }, { passive: false });

        window.addEventListener('touchend', function(event) {
            if (event.changedTouches.length > 1) {
                return;
            }
            if (leftMenuOpening) {
                if (dx > _this.leftSideMenu.offsetWidth * 0.5) {
                    _this.moveForward(_this.leftSideMenu.style.left);
                    _this.ShowLeftMenu(true);
                    leftMenuOpened = true;
                } else {
                    _this.moveBackward(_this.leftSideMenu.style.left);
                }
                leftMenuOpening = false;
            } else if (leftMenuClosing) {
                if (-dx > _this.leftSideMenu.offsetWidth * 0.5) {
                    _this.moveBackward(_this.leftSideMenu.style.left);
                    _this.ShowLeftMenu(false);
                    leftMenuOpened = false;
                } else {
                    _this.moveForward(_this.leftSideMenu.style.left);
                }
                leftMenuClosing = false;
            }

        }, { passive: false });

        window.addEventListener("scroll", function(event) {
            event.preventDefault();
            //alert("scroll");
        })


        window.addEventListener("touchmove", function(event) {
            var touch = event.touches[0];
            let verticalMove = false;
            if (event.touches.length > 1) {
                return;

            }
            if (Math.abs(dx) > Math.abs(dy)) {
                event.preventDefault();
                verticalMove = true;
            }
            xPos = touch.pageX;
            yPos = touch.pageY;
            dx = xPos - startX;
            dy = yPos - startY;
            if ((xPos == null && yPos == null) || verticalScrolled) {
                return false;
            }
            let opened = (leftMenuOpened || _this.buttonChecked);
            if (dx > dXThreshold && !opened) {
                leftMenuOpening = true;
                event.preventDefault();
                _this.leftSideMenu.style.left = (dx < _this.leftSideMenu.offsetWidth ? -_this.leftSideMenu.offsetWidth + dx : 0) + "px";
                return false;
            } else if (dx < -dXThreshold && opened) {
                leftMenuClosing = true;
                event.preventDefault();
                _this.leftSideMenu.style.left = (-dx < _this.leftSideMenu.offsetWidth ? dx : -_this.leftSideMenu.offsetWidth) + "px";
                return false;
            }
            let absDY = Math.abs(dy);
            if (absDY > dYThreshold && !verticalMove) {
                verticalScrolled = true;

                if (dy < -dYThreshold) {
                    console.log("hide");
                    _this.button.classList.toggle("Hide", true);
                } else if (dy > dYThreshold) {
                    console.log("show");
                    _this.button.classList.toggle("Hide", false);
                }
                return false;
            }


        }, {
            passive: false
        });
    }
}
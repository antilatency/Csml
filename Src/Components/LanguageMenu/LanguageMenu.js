function LanguageMenu(element) {

    this.OnResourcesLoaded = function() {

        let path = window.location.pathname;

        let lang = document.documentElement.lang;
        //element.className += " " + lang;
        let trimmedPath = path.split('_').reverse()[0].split('.')[0].toUpperCase()
        console.log(trimmedPath);
        if (element.innerHTML == trimmedPath) {
            element.style.backgroundColor = "#616161";
        }
    }
    element.onclick = () => element.href += window.location.hash;

    let startX,
        startY,
        parent = element.parentElement,
        verticalScroll = false,
        horizontalScroll = false,
        dYThreshold = 5,
        dXThreshold = 15;
    window.addEventListener("touchstart", function(event) {
        if (event.touches.length > 1) return;
        let touch = event.touches[0];
        startX = touch.pageX;
        startY = touch.pageY;
    }, { passive: false });

    window.addEventListener("touchmove", function(event) {
        var touch = event.touches[0];
        if (event.touches.length > 1) {
            return;
        }
        if ((touch.pageX == null && touch.pageY == null)) {
            return false;
        }
        dx = touch.pageX - startX;
        dy = touch.pageY - startY;
        let absDY = Math.abs(dy);
        let absDX = Math.abs(dx);
        if (absDX > dXThreshold) {
            horizontalScroll = true;
            //return;
        }
        if (absDY > dYThreshold && !horizontalScroll) {
            verticalScroll = true;
            if (dy < -dYThreshold) {
                console.log("hide");
                parent.classList.toggle("Hide", true);
            } else if (dy > dYThreshold) {
                console.log("show");
                parent.classList.toggle("Hide", false);
            }
            return false;
        }
    }, { passive: false });

    window.addEventListener("touchend", () => verticalScroll = horizontalScroll = false, { passive: false });
}
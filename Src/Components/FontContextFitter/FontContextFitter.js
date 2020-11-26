function FontContextFitter(element) {
    let initialStyle = window.getComputedStyle(element, null);
    let maxFontSize = parseInt(initialStyle.fontSize, 10);

    this.OnWindowResize = function() {
        this.ChangeTitleFontSize();
    }
    this.OnResourcesLoaded = function() {
        this.ChangeTitleFontSize();
    }


    this.ChangeTitleFontSize = function() {
        var minFontSize = 10;
        //let fontsSizeArray = Array.from(Array(maxFontSize).keys());
        //fontsSizeArray.shift();
        //let fontSize = maxFontSize;
        //let h = element.offsetHeight;
        //let w = element.offsetWidth;
        //let scrollWidth = element.scrollWidth;
        //let scrollHeight = element.scrollHeight;
        //let ar = Array.from(Array(this.maxFontSize).keys());
        //let n = ar.length - 1;
        //let m = 0;
        //while (m <= n) {
        //    let k = (n + m) >> 1;
        //    var cmp = fontSize - ar[k];
        //    if (cmp > 0) {
        //        m = k + 1;
        //    } else if (cmp < 0) {
        //        n = k - 1;
        //    } else {
        //        break
        //    }
        //    element.style.fontSize = fontSize + 'px';
        //    h = element.offsetHeight;
        //    w = element.offsetWidth;
        //    scrollWidth = element.scrollWidth;
        //    scrollHeight = element.scrollHeight;
        //    console.log(element.style.fontSize, " ", w + " x " + h, "scrollWidth: " + scrollWidth, "scrollHeight: " + scrollHeight);
        //    //fontSize--;
        //    //if ((scrollHeight == h) && (scrollWidth == w)) break;
        //}
        for (var i = maxFontSize; i >= minFontSize; i--) {
            element.style.fontSize = i + 'px';
            var h = element.offsetHeight;
            var w = element.offsetWidth;
            var scrollWidth = element.scrollWidth;
            var scrollHeight = element.scrollHeight;
            console.log("scrollHeight: " + scrollHeight + " h " + h + " scrollWidth: " + scrollWidth + " w " + w);
            if ((scrollHeight == h) && (scrollWidth <= w)) break;

        }
    }

    this.BinarySearch = (list, val) => {
        let left = 0;
        let right = list.length - 1;
        let mid = Math.floor((left + right) / 2);

        while (list[mid] !== val && left <= right) {
            if (val < list[mid]) {
                right = mid - 1
            } else {
                left = mid + 1
            }
            mid = Math.floor((left + right) / 2);
        }
        if (list[mid] === val) {
            return mid;
        } else {
            return -1
        }

    };

    this.binarySearch = function(ar, el, element) {
        var m = 0;
        var n = ar.length - 1;
        while (m <= n) {
            var k = (n + m) >> 1;
            var cmp = el - ar[k];
            if (cmp > 0) {
                m = k + 1;
            } else if (cmp < 0) {
                n = k - 1;
            } else {
                return k;
            }
        }
        return -m - 1;
    }
}
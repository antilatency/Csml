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
        for (var i = maxFontSize; i >= minFontSize; i--) {
            element.style.fontSize = i + 'px';
            var h = element.offsetHeight;
            var w = element.offsetWidth;
            var scrollWidth = element.scrollWidth;
            var scrollHeight = element.scrollHeight;
            //console.log("scrollHeight: " + scrollHeight + " offsetHeight " + h + " scrollWidth " + scrollWidth + " offsetWidth " + w +" fontSize " + element.style.fontSize); 
            if ((scrollHeight == h) && (scrollWidth == w)) break;

        }
        //console.log("DONE!");
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
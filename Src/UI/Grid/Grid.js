


var Grid = {
    Constructor: function (element, elementWidthPx, numColumnsVariants) {
        this.element = element;
        this.elementWidthPx = elementWidthPx;
        this.numColumnsVariants = numColumnsVariants;
        this.numColumnsVariants.sort(function (a, b) {
            return a-b
        })
        this.previousContainerWidth = -1;
        this.previousClass = "";
        this.previousNumColumns = 0;

        this.Align = function () {

            var width = this.element.offsetWidth;
            if (this.previousContainerWidth == width) {
                return;
            }
            this.previousContainerWidth = width;           

            var numColumnsID = 0;
            var dif = Math.abs(width / numColumnsVariants[0] - this.elementWidthPx);
            for (let i = 1; i < numColumnsVariants.length; i++) {                
                var newDif = Math.abs(width / numColumnsVariants[i] - this.elementWidthPx);
                if (newDif < dif) {
                    dif = newDif;
                    numColumnsID = i;
                }
            }
            var newNumColumns = numColumnsVariants[numColumnsID];
            if (this.previousNumColumns != newNumColumns) {
                this.element.classList.remove("columns" + this.previousNumColumns);
                this.element.classList.add("columns" + newNumColumns);
                this.previousNumColumns = newNumColumns;
            }
        }

        this.Align();
        
    },

    Create: function (element, ...parameters) {
        var x = new Grid.Constructor(element, ...parameters);
        window.addEventListener("resize", function () { x.Align() });
	}
}
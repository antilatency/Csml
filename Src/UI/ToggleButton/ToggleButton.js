function ToggleButton(element, ...parameters) {
    this.element = element;
    this.button = document.createElement('div');		
    this.button.className = "button toggle"

    this.Toggle = function () {
        if (this.button.classList.contains("checked")){
            this.element.classList.remove("checked")
            this.button.classList.remove("checked")
		}else{
            this.element.classList.add("checked")
            this.button.classList.add("checked")
		}
    }

    this.button.onclick = this.Toggle;					
    this.element.appendChild(this.button);
}
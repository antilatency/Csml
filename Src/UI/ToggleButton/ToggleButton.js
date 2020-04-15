var ToggleButton = {				
	Create: function(element, ...parameters){
		var button = document.createElement('div');		
		button.className = "button toggle"
		button.onclick = function(){ToggleButton.Toggle(element,button)}					
		button.appendChild(Behaviour.Create("Behaviour.PrintParent","string"));
		element.appendChild(button);
	},	
	Toggle: function(element,button){
		if (button.classList.contains("checked")){
			element.classList.remove("checked")
			button.classList.remove("checked")
		}else{
			element.classList.add("checked")
			button.classList.add("checked")
		}
	}
}
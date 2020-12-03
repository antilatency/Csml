function LanguageMenu(element) {

    this.OnResourcesLoaded = function () {

        let leftMenu = document.getElementsByClassName("menu LeftSideMenu")[0];
        let text = leftMenu.getElementsByClassName("Text");
        let path = window.location.pathname;
        
        let lang = document.documentElement.lang;
        //element.className += " " + lang;
        let trimmedPath = path.split('_').reverse()[0].split('.')[0].toUpperCase()
        console.log(trimmedPath);
        if (element.innerHTML == trimmedPath) {
            element.style.backgroundColor = "#616161";
        }
    }

    
}
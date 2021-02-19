function LanguageSideMenu($element) {
    let $menu = $element.querySelector('.LanguageSideMenu');
    let $tongue = $element.querySelector('.Tongue');


    let $icon = document.createElement('div');
    $icon.classList.add('Before');
    $menu.appendChild($icon);

    let menu = new SideComponent($element, $menu, $tongue);
    let TemplateState = window.TemplateState;

    let isUnpinned = true;
    setLanguageSelected();

    bindToTemplateState(TemplateState, menu);
    bind();

    function bind() {
        window.addEventListener('sideStateChange', e => {
            isUnpinned = !TemplateState.isHidden && TemplateState.isMinimalizeState;
            $element.classList.toggle('Unpinned', isUnpinned);
        });
        $icon.addEventListener('click', e => menu.close());
    }

    function setLanguageSelected() {
        let $items = $menu.querySelectorAll('.Text');
        let code = getLangCode();

        $items.forEach($item => {
            if ($item.innerHTML == code)
                $item.classList.add('Selected');
        });
    }
}



function getLangCode() {
    let path = window.location.pathname;
    let trimmedPath = path.split('_').reverse()[0].split('.')[0].toUpperCase();

    if (path == "/")
        trimmedPath = "EN";

    return trimmedPath;
}
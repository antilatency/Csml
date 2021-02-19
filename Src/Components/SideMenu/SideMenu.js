class MenuSideComponent extends SideComponent {
    pin(isPinned) {
        if (this.isPinned == isPinned)
            return;
        super.pin(isPinned);
        pinTop(this.content, isPinned);
    }
}

function SideMenu($element) {

    let $menu = $element.querySelector('.SideMenu');
    let $tongue = $element.querySelector('.TongueMenu');
    let menu = new MenuSideComponent($element, $menu, $tongue);
    let TemplateState = window.TemplateState;

    bindToTemplateState(TemplateState, menu);
}
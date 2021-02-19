class SideComponent {

    position = 0;
    touchStart = {};
    isPinned = false;
    _isActive = false;
    isAlone = false;
    isClosing = false;
    isMobile = true;
    isBlocked = false;

    constructor(element, content, tongue) {
        this.element = element;
        this.content = content;
        this.tongue = tongue;
        this.element.parentNode.style.visibility = "hidden";

        this.width = content.offsetWidth;
        this.isLeft = element.parentNode.classList.contains('LeftSideContainer');
        this.sideMult = this.isLeft ? 1 : -1;

        this.openBound = this.width * .5;

        this.element.classList.add('SideComponent');
        this.content.classList.add('SideContent');
        this.tongue.classList.add('Tongue');
        this.glass = document.createElement('div');

        this.glass.classList.add('Glass');
        this.element.insertBefore(this.glass, this.element.firstChild);

        this.bind();
    }

    get isActive() { return this._isActive; }
    set isActive(isActive) {
        if (isActive == this._isActive)
            return;
        this._isActive = isActive;
        this.element.classList.toggle('Active', isActive);
        this.forvard(isActive);
    }

    bind() {
        window.addEventListener('resize', e => {
            this.close();
        });
        window.addEventListener('load', e => {
            this.isAlone = this.element.parentNode.children.length == 1;
            this.element.parentNode.style.visibility = "visible";
        });


        window.addEventListener('touchstart', e => {
            let touch = this.getTouch(e);
            if (touch == null || this.isBlocked)
                return;
            this.touchStart = {
                x: touch.clientX,
                y: touch.clientY,
            };
        });

        window.addEventListener('touchend', e => {
            if (this.isBlocked)
                return;
            if (this.position > this.openBound)
                this.open();
            else
                this.close();
        });

        window.addEventListener('touchmove', e => {
            if (this.isActive)
                e.preventDefault();
            let curr = this.getTouch(e);
            if (curr == null || this.isBlocked)
                return;

            let shiftX = curr.clientX - this.touchStart.x;
            let shiftY = curr.clientY - this.touchStart.y;
            let isVertical = Math.abs(shiftX) < Math.abs(shiftY);
            let targetIsHScrollable = canScrollTo(e.target, shiftX);

            if (!this.isActive && (isVertical || targetIsHScrollable)) {
                this.touchStart.x = curr.clientX;
                return;
            }

            if (!this.isDirectionCorrect(shiftX) || !this.isInTongueArea(this.touchStart.y))
                return;


            shiftX = Math.abs(shiftX);

            if (this.isClosing)
                shiftX = this.width - shiftX;

            this.moveTo(shiftX);
        }, { passive: false });


        this.tongue.addEventListener('click', e => this.isBlocked ? null : this.isClosing ? this.close() : this.open());

        window.addEventListener('click', e => {
            if (this.isBlocked || this.content.contains(e.target) || this.tongue.contains(e.target))
                return;
            this.close();
        });

        this.addEventListener('move', e => {
            this.element.classList.toggle('Modal', this.isActive);

            let opacity = this.position / this.width;
            this.glass.style.opacity = opacity;
            if (!this.isPinned)
                this.tongue.style.opacity = 1 - opacity;
        });
    }

    block(isBlocked) {
        this.isBlocked = isBlocked;
    }

    pin(isPinned) {
        if (this.isPinned == isPinned)
            return;
        this.isPinned = isPinned;
        this.element.classList.toggle('Pinned', isPinned);
        this.tongue.classList.toggle('Pinned', isPinned);

        pinTop(this.glass, isPinned);
    }

    open() { this.moveTo(this.width); }

    close() { this.moveTo(0); }


    addEventListener(...rest) { this.element.addEventListener.apply(this.element, rest); }
    removeEventListener(...rest) { this.element.removeEventListener.apply(this.element, rest); }
    dispatchEvent(...rest) { this.element.dispatchEvent.apply(this.element, rest); }

    onMove() {
        this.dispatchEvent(new CustomEvent('move', {
            bubbles: true,
            cancelable: true
        }));
    }

    moveTo(shiftX) {
        this.position = shiftX;
        this.isActive = true;
        if (this.position <= 0) {
            this.position = 0;
            this.isActive = false;
            this.isClosing = false;
        }
        if (this.position >= this.width) {
            this.position = this.width;
            this.isClosing = true;
        }

        this.element.style.transform = 'translateX(' + (this.position * this.sideMult) + 'px)';
        this.glass.style.transform = 'translateX(' + (-this.position * this.sideMult) + 'px)';
        this.onMove();
    }

    getTouch(e) {
        if (e.touches.length > 1 || !this.isPinned && !this.isAlone)
            return null;
        return e.touches[0];
    }

    isDirectionCorrect(shift) {
        let isLeft = XOR(this.isLeft, this.isClosing);
        return isLeft ? shift > 0 : shift < 0;
    }

    isInTongueArea(y) {
        let bounds = this.tongue.getBoundingClientRect();
        return this.isAlone || y > bounds.top && y < bounds.top + this.tongue.offsetHeight;
    }

    changeMobile(isMobile) {
        this.isMobile = isMobile;
        this.tongue.style.visibility = isMobile ? 'visible' : 'hidden';
    }

    forvard() {
        this.element.classList.toggle("Forward", this.isActive);
    }
}

function isHScrollable(element) {
    return element.scrollWidth > element.clientWidth && element.style.overflow != 'hidden' && element.style.overflowX != 'hidden';
}

function scrollRight(element) {
    return element.scrollWidth - element.scrollLeft - element.clientWidth;
}

function canScrollTo(element, shiftX) {
    while (element != document.body) {
        if (isHScrollable(element))
            return shiftX > 0 ? element.scrollLeft > 0 : scrollRight(element) > 0;
        element = element.parentNode;
    }
    return false;
}

function pinTop($elem, isPinned) {
    $elem.style.top = "0";
    let offset = -$elem.getBoundingClientRect().top;
    $elem.style.top = !isPinned ? "" : offset + "px";
}

function XOR(b1, b2) { return b1 ? !b2 : b2; }

function bindToTemplateState(state, component) {
    component.addEventListener('move', e => {
        state.isBlockedBy = component.isActive ? component : null;
        if (component.isActive)
            state.forwardSide(component.isLeft);
    });
    window.addEventListener('sideStateChange', e => {
        //component.close();
        component.pin(state.isHidden);
        component.changeMobile(state.isMobileState || state.isMinimalizeState);
        component.block(state.isBlockedBy != null && state.isBlockedBy != component);
    });

}
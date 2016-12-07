import $ from 'jquery';

function handler(ev) {
    // trigger any jquery handlers on this element
    const fakeEvent = $.Event(ev);
    fakeEvent.originalEvent = undefined;
    fakeEvent.target = ev.target;

    const original = this.onclick;
    this.onclick = () => {};
    try {
        $(this).triggerHandler(fakeEvent);
    }
    finally {
        this.onclick = original;
    }

    // Now trigger and bubble the event up
    if (!fakeEvent.isPropagationStopped()) {
        $(this.parentNode).trigger(fakeEvent);
    }

    // if Default was prevented, then do not allow the event to capture down into the grid
    if (fakeEvent.isDefaultPrevented()) {
        ev.stopPropagation();
    }
}

/**
 * DHTMLX Grid eats click events, which makes it hard to use event delegation to capture clicks on cells in your app
 * Call this method to prevent the grid from eating the clicks
 */
export default function allowGridClickEvents(grid) {
    // Add an event listener in the CAPTURE phase so we can intercept the click event before it trickles down to the grid
    // after we capture the event, we can bubble up a clone of the click event
    grid.entBox.addEventListener("click", handler, true);
}

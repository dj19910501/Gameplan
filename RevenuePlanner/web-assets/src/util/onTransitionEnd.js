/**
 * More reliable than transitionend, which does not fire in certain circumstances
 * @param $container
 */
export default function onTransitionEnd($container, transitionTime, handler) {
    let timer;

    function evCallback() {
        clearTimeout(timer);
        handler();
    }

    $container.one("transitionend", evCallback);

    const FALLBACK_DELAY = 100;

    // start a timer to fire transitionend in case the browser doesnt
    timer = setTimeout(() => {
        $container.off("transitionend", evCallback);
        handler();
    }, transitionTime + FALLBACK_DELAY);
}

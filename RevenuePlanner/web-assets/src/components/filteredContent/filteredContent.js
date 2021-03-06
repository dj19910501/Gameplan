import $ from 'jquery';
import css from './filteredContent.scss';
import viewTemplate from './filteredContent.ejs';
import onTransitionEnd from 'util/onTransitionEnd';

// Must be set to $SLIDE_DURATION + $SLIDE_DELAY from filteredContent.scss
const SLIDE_TOTAL_TIME = 200;

function createView($container) {
    const o = {
        css
    };

    const html = viewTemplate(o);
    $container.html(html);

    return {
        public: {
            $filterPanel: $container.find(`.${css.filterContent}`),
            $content: $container.find(`.${css.content}`),
        },
        $filteredContent: $container.find(`.${css.filteredContent}`),
        $filterPanel: $container.find(`.${css.filterPanel}`),
        $toggle: $container.find(`.${css.toggle}`),
    };
}

function bindToggle(view) {
    view.$toggle.on("click", () => {
        onTransitionEnd(view.public.$content, SLIDE_TOTAL_TIME, () => $(view.public).trigger("filterToggled"));
        view.$filteredContent.toggleClass(css.hideFilter)
    });
}

export default function createPanel($container) {
    const view = createView($container);
    bindToggle(view);

    // return only *some* of the view
    return view.public;
}

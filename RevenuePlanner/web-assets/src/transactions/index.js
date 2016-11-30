import mainView from './views/main.ejs';
import css from './transactions.scss';
import createGrid from './grid';

export default function main($rootElement) {
    $rootElement
        .addClass("header-content-footer-layout")
        .html(mainView({css}));

    const $gridContainer = $rootElement.find(`.${css.gridContainer}`);
    const $grid = $gridContainer.find(`.${css.grid}`);
    createGrid($gridContainer, $grid);
}

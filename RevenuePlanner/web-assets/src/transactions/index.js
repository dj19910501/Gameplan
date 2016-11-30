import mainView from './views/main.ejs';
import css from './transactions.scss';
import createGrid from './grid';
import TransactionModel from './transactionModel';

export default function main($rootElement) {

    $rootElement
        .addClass("header-content-footer-layout")
        .html(mainView({css}));

    const $gridContainer = $rootElement.find(`.${css.gridContainer}`);
    const $grid = $gridContainer.find(`.${css.grid}`);

    const model = new TransactionModel();
    $(model).on("change-headerMappings", () => createGrid(model, $gridContainer, $grid));
}

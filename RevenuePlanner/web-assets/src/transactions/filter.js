import $ from 'jquery';
import moment from 'moment';
import 'bootstrap-daterangepicker';
import 'bootstrap-daterangepicker/daterangepicker.css';
import viewTemplate from './views/filterPanel.ejs';
import css from './views/filterPanel.scss';

const DATE_RANGE_FORMAT = 'll';

function formatDateRange(filter) {
    return `${filter.startDate.format(DATE_RANGE_FORMAT)} - ${filter.endDate.format(DATE_RANGE_FORMAT)}`;
}

function createView($container, initialFilter) {
    const o = {
        css,
        dateRange: formatDateRange(initialFilter),
    };

    const html = viewTemplate(o);
    $container.html(html);

    const view = {
        $range: $container.find(`.${css.range}`),
    };

    view.$value = view.$range.find(`span`);

    return view;
}

function bindDateRange(view, dataSource, recalculatePageSize) {

    // update the text whenever the datasource filter changes the range
    dataSource.on("change", ev => {
        if (ev.which.filter) {
            view.$value.text(formatDateRange(dataSource.state.filter));
            const picker = view.$range.data('daterangepicker');
            picker.setStartDate(dataSource.state.filter.startDate);
            picker.setEndDate(dataSource.state.filter.endDate);
        }
    });

    // display the picker whenever the user clicks on the range element and update the datasource when the user makes a choice
    view.$range.daterangepicker({
        startDate: dataSource.state.filter.startDate,
        endDate: dataSource.state.filter.endDate,
        alwaysShowCalendars: true,
        opens: "right",
        showDropdowns: true,
        showWeekNumbers: true,
        timePicker: false,
        ranges: {
            'This Quarter': [moment().startOf('quarter'), moment().endOf('quarter')],
            'This Year': [moment().startOf('year'), moment().endOf('year')],
            'Year to Date': [moment().startOf('year'), moment()],
            'Last 12 Months': [moment().subtract(12, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
            'Last 4 Quarters': [moment().subtract(4, 'quarter').startOf('quarter'), moment().subtract(1, 'quarter').endOf('quarter')],
            'Last Calendar Year': [moment().subtract(1, 'year').startOf('year'), moment().subtract(1, 'year').endOf('year')],
        },
        buttonClasses: ['btn btn-default'],
        applyClass: 'btn-sm btn-primary',
        cancelClass: 'btn-sm',
        locale: {
            format: DATE_RANGE_FORMAT,
        }
    }, (startDate, endDate) => {
        dataSource.updateFilter({...dataSource.state.filter, startDate, endDate }, recalculatePageSize());
    });
}

export default function createFilter($container, dataSource, recalculatePageSize) {
    const view = createView($container, dataSource.state.filter);
    bindDateRange(view, dataSource, recalculatePageSize);

    return view;
}

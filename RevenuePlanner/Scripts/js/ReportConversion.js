window.onload = function () {
    var chartDataMQLPerformance = JSON.parse($('#chartDataMQLPerformance').val());

    var xAxisConfig = GetAxisConfiguration(chartDataMQLPerformance.ChartBusinessUnit)
    /* bar chart for Business Unit */
    var barChart1 = new dhtmlXChart({
        view: "barH",
        container: "chartDiv1",
        value: "#Value#",
        tooltip: "#Value#",
        radius: 0,
        border: false,
        color: "#ColorCode#",
        width: 25,
        xAxis: {
            start: 0,
            step: xAxisConfig.stepValue,
            end: xAxisConfig.endValue,
            lines: false,
        },
        yAxis: {
            template: "#Title#",
            lines: false,
        },
        padding: {
            left: 80
        }
    });

    barChart1.parse(chartDataMQLPerformance.ChartBusinessUnit, "json");

    // added by dharmraj for ticket #447 : Alignment is not proper for charts
    $("#chartDiv1 .dhx_canvas_text.dhx_axis_item_y").each(function (index, element) {
        $(element).attr('title', element.innerHTML);
    });

    // added by dharmraj for ticket #348
    $("#chartDiv1 .dhx_canvas_text.dhx_axis_item_x").each(function (index, element) {
        var newText = GetAbberiviatedValue(element.innerHTML.toString());

        if (newText.indexOf('.') > 0) {
            var arr = newText.toString().split('.');
            newText = arr[0] + arr[1].substr(arr[1].length - 1, 1);

            $(element).attr('title', newText);
            $(element).html(newText);
        }
        else {
            $(element).attr('title', newText);
            $(element).html(newText);
        }
    });

    var xAxisConfig = GetAxisConfiguration(chartDataMQLPerformance.ChartGeography)
    /* bar chart for Geography */
    var barChart2 = new dhtmlXChart({
        view: "barH",
        container: "chartDiv2",
        value: "#Value#",
        tooltip: "#Value#",
        radius: 0,
        border: false,
        color: "#ColorCode#",
        width: 25,
        xAxis: {
            start: 0,
            step: xAxisConfig.stepValue,
            end: xAxisConfig.endValue,
            lines: false,
        },
        yAxis: {
            template: "#Title#",
            lines: false,
        },
        padding: {
            left: 75
        }
    });

    barChart2.parse(chartDataMQLPerformance.ChartGeography, "json");

    // added by dharmraj for ticket #447 : Alignment is not proper for charts
    $("#chartDiv2 .dhx_canvas_text.dhx_axis_item_y").each(function (index, element) {
        $(element).attr('title', element.innerHTML);
    });

    // added by dharmraj for ticket #348
    $("#chartDiv2 .dhx_canvas_text.dhx_axis_item_x").each(function (index, element) {
        var newText = GetAbberiviatedValue(element.innerHTML.toString());

        if (newText.indexOf('.') > 0) {
            var arr = newText.toString().split('.');
            newText = arr[0] + arr[1].substr(arr[1].length - 1, 1);

            $(element).attr('title', newText);
            $(element).html(newText);
        }
        else {
            $(element).attr('title', newText);
            $(element).html(newText);
        }
    });

    var xAxisConfig = GetAxisConfiguration(chartDataMQLPerformance.ChartVertical)
    /*bar chart for Veritcal */
    var barChart3 = new dhtmlXChart({
        view: "barH",
        container: "chartDiv3",
        value: "#Value#",
        tooltip: "#Value#",
        radius: 0,
        border: false,
        width: 25,
        color: "#ColorCode#",
        xAxis: {
            start: 0,
            step: xAxisConfig.stepValue,
            end: xAxisConfig.endValue,
            lines: false,
        },
        yAxis: {
            template: "#Title#",
            lines: false,
        },
        padding: {
            left: 90
        }
    });

    barChart3.parse(chartDataMQLPerformance.ChartVertical, "json");

    // added by dharmraj for ticket #447 : Alignment is not proper for charts
    $("#chartDiv3 .dhx_canvas_text.dhx_axis_item_y").each(function (index, element) {
        $(element).attr('title', element.innerHTML);
    });

    // added by dharmraj for ticket #348
    $("#chartDiv3 .dhx_canvas_text.dhx_axis_item_x").each(function (index, element) {
        var newText = GetAbberiviatedValue(element.innerHTML.toString());

        if (newText.indexOf('.') > 0) {
            var arr = newText.toString().split('.');
            newText = arr[0] + arr[1].substr(arr[1].length - 1, 1);

            $(element).attr('title', newText);
            $(element).html(newText);
        }
        else {
            $(element).attr('title', newText);
            $(element).html(newText);
        }
    });

}
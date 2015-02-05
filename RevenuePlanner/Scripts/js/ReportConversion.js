window.onload = function () {
    var chartDataMQLPerformance = JSON.parse($('#chartDataMQLPerformance').val());

    var datasetconversion1 = chartDataMQLPerformance.ChartCustomField1;
    var datasetconversion2 = chartDataMQLPerformance.ChartCustomField2;
    var datasetconversion3 = chartDataMQLPerformance.ChartCustomField3;

    //fill data on reports table
   if (datasetconversion1 != null) {
        $('#NoGraphsMsg').hide();
        $('.report-gray-container').show();
        FillChartSourcePerformanceCustomFieldConversion1();
    }
    else {
        $('.report-gray-container').hide();
        $('#NoGraphsMsg').show();

        $('#chartDiv1Parent').hide();
    }
   if (datasetconversion2 != null) {
        FillChartSourcePerformanceCustomFieldConversion2();
    }
    else {
        $('#chartDiv2Parent').hide();
    }
   if (datasetconversion3 != null) {
        FillChartSourcePerformanceCustomFieldConversion3();
    }
    else {
        $('#chartDiv3Parent').hide();
    }

    // chart for custom field 1 
    function FillChartSourcePerformanceCustomFieldConversion1() {
        $('#chartDiv1Parent').find('#chartDiv1').remove();
        $('#chartDiv1Parent').append('<div id="chartDiv1" class="report-chart4"></div>');

        var xAxisConfig = GetAxisConfiguration(datasetconversion1)

        /* bar chart for Custom Field 1 */
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
                left: 75
        }
    });

        barChart1.parse(datasetconversion1, "json");

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
    }

    // chart for Custom Field 2
    function FillChartSourcePerformanceCustomFieldConversion2() {
        $('#chartDiv2Parent').find('#chartDiv2').remove();
        $('#chartDiv2Parent').append('<div id="chartDiv2" class="report-chart5"></div>');

        var xAxisConfig = GetAxisConfiguration(datasetconversion2)

        /* bar chart for Custom Field 2 */
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

        barChart2.parse(datasetconversion2, "json");

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
    }

    // chart for Custom Field 3
    function FillChartSourcePerformanceCustomFieldConversion3() {
        $('#chartDiv3Parent').find('#chartDiv3').remove();
        $('#chartDiv3Parent').append('<div id="chartDiv3" class="report-chart6"></div>');

        var xAxisConfig = GetAxisConfiguration(datasetconversion3)

        /*bar chart for Custom Field 2 */
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
                left: 85
        }
    });

        barChart3.parse(datasetconversion3, "json");

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

}
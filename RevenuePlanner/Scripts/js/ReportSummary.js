$(document).ready(function () {
    //// Bug 295:Waterfall Conversion Summary Graph misleading
    //// Array to hold chart points.
    var dataset = JSON.parse($('#chartDataSummary').val());

    var arrChartData = [];

    //// Pushing data into array.
    $.each(dataset, function (index, objChardData) {
        arrChartData.push(parseInt(objChardData.Actual));
        arrChartData.push(parseInt(objChardData.Projected));
    });

    //// Finding max from array.
    var endValue = (Math.max.apply(Math, arrChartData));

    //// Checking whether max is not zero.
    if (endValue == 0) {
        endValue = 100;
    }
    else {
        endValue = Math.ceil(endValue / 10) * 10
    }

    //// Calculating step value.
    var stepValue = endValue / 10;

    setgraphdata(dataset)
    function setgraphdata(data) {
        var barChart = new dhtmlXChart({
            view: "bar",
            container: "chartDiv",
            value: "#Actual#",
            tooltip: "#Actual#",//// Reason: To show actual value as tooltip. Added By: Maninder Singh on 1/28/2014
            radius: 0,
            border: false,
            color: "#00a1e4",
            toggle: false,
            xAxis: {
                lines: false,
                template: "#Stage#",
            },
            yAxis: {
                start: 0,
                end: endValue,
                step: stepValue,
                template: function (value) {
                    return FormatCommas(value, false);
                }
            },
            legend: {
                values: [{ text: "Actual", color: "#00A1E4" }, { text: "Projected", color: "#B3B3B3" }],
                valign: "middle",
                align: "right",
                width: 60, //// Reason: Increase width of legend box so that text 'projected' get displayed properly. Added By: Maninder Singh on 1/28/2014
                toggle: false,
                layout: "y",
                marker: {
                    type: "square",
                    width: 10,
                    height: 10,
                    radius: 0,
                }
            }
        });

        barChart.addSeries({
            value: "#Projected#",
            tooltip: "#Projected#", //// Reason: To show projected value as tooltip. Added By: Maninder Singh on 1/28/2014
            color: "#e6e6e6",
            border: false
        });
        barChart.parse(data, "json");
    }
});
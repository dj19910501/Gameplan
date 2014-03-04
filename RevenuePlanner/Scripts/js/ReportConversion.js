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
        }
    });

    barChart1.parse(chartDataMQLPerformance.ChartBusinessUnit, "json");

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
        }
    });

    barChart2.parse(chartDataMQLPerformance.ChartGeography, "json");

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
        }
    });

    barChart3.parse(chartDataMQLPerformance.ChartVertical, "json");
}
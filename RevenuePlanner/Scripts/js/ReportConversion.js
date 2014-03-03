window.onload = function () {
    /* bar chart for Business Unit */
    var barChart1 = new dhtmlXChart({
        view: "barH",
        container: "chartDiv1",
        value: "#report#",
        tooltip: "#report#",
        radius: 0,
        border: false,
        color: "#color#",
        width: 25,
        xAxis: {
            start: 0,
            step: 1,
            end: 10,
            lines: false,
        },
        yAxis: {
            template: "#y#",
            lines: false,
        }
    });

    /* bar chart for Geography */
    var barChart2 = new dhtmlXChart({
        view: "barH",
        container: "chartDiv2",
        value: "#report#",
        tooltip: "#report#",
        radius: 0,
        border: false,
        color: "#color#",
        width: 25,
        xAxis: {
            start: 0,
            step: 1,
            end: 10,
            lines: false,
        },
        yAxis: {
            template: "#y#",
            lines: false,
        }
    });

    /*bar chart for Veritcal */
    var barChart3 = new dhtmlXChart({
        view: "barH",
        container: "chartDiv3",
        value: "#report#",
        tooltip: "#report#",
        radius: 0,
        border: false,
        width: 25,
        color: "#color#",
        xAxis: {
            start: 0,
            step: 1,
            end: 10,
            lines: false,
        },
        yAxis: {
            template: "#y#",
            lines: false,
        }
    });

    var chartDataMQLPerformance = JSON.parse($('#chartDataMQLPerformance').val());
    barChart1.parse(chartDataMQLPerformance.dataset1, "json");
    barChart2.parse(chartDataMQLPerformance.dataset2, "json");
    barChart3.parse(chartDataMQLPerformance.dataset3, "json");
}
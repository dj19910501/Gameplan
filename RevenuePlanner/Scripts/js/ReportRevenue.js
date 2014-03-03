window.onload = function () {

    /*bar chart*/
    var barChart = new dhtmlXChart({
        view: "bar",
        container: "chartDiv3",
        value: "#Actual#",
        tooltip: "#Actual#",
        radius: 0,
        border: false,
        color: "#ColorActual#",
        width: 20,
        xAxis: {
            lines: false,
            template: "#Month#"
        },
        yAxis: {
            start: 0,
            step: 1,
            end: 10,
            lines: false,
        },
        legend: {
            values: [{ text: "Actual", color: "#d4d4d4" }, { text: "Projected", color: "#1a638a" }, { text: "Contribution", color: "#559659" }],
            valign: "middle",
            align: "left",
            width: 50,
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
        tooltip: "#Projected#",
        color: "#ColorProjected#",
        border: false
    });

    barChart.addSeries({
        value: "#Contribution#",
        tooltip: "#Contribution#",
        color: "#ColorContribution#",
        border: false
    });

    var dataset = JSON.parse($('#chartDataRevenueToPlan').val());
    FillChartRevenueToPlan();


    function FillChartRevenueToPlan() {
        barChart.parse(dataset, "json");
    }

    /*bar chart*/
    var barChart2 = new dhtmlXChart({
        view: "barH",
        container: "chartDiv4",
        value: "#Value#",
        tooltip: "#Value#",
        radius: 0,
        border: false,
        color: "#ColorCode#",
        width: 20,
        xAxis: {
            start: 0,
            step: 1,
            end: 10,
            lines: false,
        },
        yAxis: {
            template: "#Title#",
            lines: false,
        }
    });

    /*bar chart*/
    var barChart3 = new dhtmlXChart({
        view: "barH",
        container: "chartDiv5",
        value: "#Value#",
        tooltip: "#Value#",
        radius: 0,
        border: false,
        color: "#ColorCode#",
        width: 20,
        xAxis: {
            start: 0,
            step: 1,
            end: 10,
            lines: false,
        },
        yAxis: {
            template: "#Title#",
            lines: false,
        }
    });


    /*bar chart*/
    var barChart4 = new dhtmlXChart({
        view: "barH",
        container: "chartDiv6",
        value: "#Value#",
        tooltip: "#Value#",
        radius: 0,
        border: false,
        color: "#ColorCode#",
        width: 20,
        xAxis: {
            start: 0,
            step: 1,
            end: 10,
            lines: false,
        },
        yAxis: {
            template: "#Title#",
            lines: false,
        }
    });

    var sourcePerformanceData = JSON.parse($('#chartDataSourcePerformance').val());
    var dataset1 = sourcePerformanceData.ChartBusinessUnit, dataset2 = sourcePerformanceData.ChartGeography, dataset3 = sourcePerformanceData.ChartVertical;
    FillChartSourcePerformanceBusinessUnit();
    FillChartSourcePerformanceGeography();
    FillChartSourcePerformanceVertical();

    function FillChartSourcePerformanceBusinessUnit() {
        barChart2.parse(dataset1, "json");
    }

    function FillChartSourcePerformanceGeography() {
        barChart3.parse(dataset2, "json");
    }
    function FillChartSourcePerformanceVertical() {
        barChart4.parse(dataset3, "json");
    }
}
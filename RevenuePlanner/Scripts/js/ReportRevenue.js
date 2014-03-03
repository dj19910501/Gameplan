window.onload = function () {
    var dataset = JSON.parse($('#chartDataRevenueToPlan').val());

    FillChartRevenueToPlan();

    function FillChartRevenueToPlan() {
        if (dataset != null && dataset.length != 0) {
            //// Modified By: Maninder Singh Wadhva Bug 298:Revenue to plan graph is incorrect.
            //// Array to hold chart points.
            var arrChartData = [];

            //// Pushing data into array.
            $.each(dataset, function (index, objChardData) {
                if (objChardData.Actual != null) {
                    arrChartData.push(parseInt(objChardData.Actual));
                }

                if (objChardData.Projected != null) {
                    arrChartData.push(parseInt(objChardData.Projected));
                }

                if (objChardData.Contribution != null) {
                    arrChartData.push(parseInt(objChardData.Contribution));
                }
            });

            //// Finding max from array.
            var endValue = (Math.max.apply(Math, arrChartData));

            //// Checking whether max is not zero.
            if (endValue == 0 || arrChartData.length == 0) {
                endValue = 100;
            }
            else {
                endValue = Math.ceil(endValue / 10) * 10
            }

            //// Calculating step value.
            var stepValue = endValue / 10;

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
                    end: endValue,
                    step: stepValue,
                    template: function (value) {
                        return FormatCommas(value, false);
                    },
                    lines: false
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

            barChart.parse(dataset, "json");
        }
    }

    var sourcePerformanceData = JSON.parse($('#chartDataSourcePerformance').val());
    var dataset1 = sourcePerformanceData.ChartBusinessUnit, dataset2 = sourcePerformanceData.ChartGeography, dataset3 = sourcePerformanceData.ChartVertical;
    FillChartSourcePerformanceBusinessUnit();
    FillChartSourcePerformanceGeography();
    FillChartSourcePerformanceVertical();

    function FillChartSourcePerformanceBusinessUnit() {
        var xAxisConfig = GetAxisConfiguration(dataset1)

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
                step: xAxisConfig.stepValue,
                end: xAxisConfig.endValue,
                lines: false,
            },
            yAxis: {
                template: "#Title#",
                lines: false,
            }
        });

        barChart2.parse(dataset1, "json");
    }
    function FillChartSourcePerformanceGeography() {
        var xAxisConfig = GetAxisConfiguration(dataset2)

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
                step: xAxisConfig.stepValue,
                end: xAxisConfig.endValue,
                lines: false,
            },
            yAxis: {
                template: "#Title#",
                lines: false,
            }
        });

        barChart3.parse(dataset2, "json");
    }
    function FillChartSourcePerformanceVertical() {
        var xAxisConfig = GetAxisConfiguration(dataset3)

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
                step: xAxisConfig.stepValue,
                end: xAxisConfig.endValue,
                lines: false,
            },
            yAxis: {
                template: "#Title#",
                lines: false,
            }
        });

        barChart4.parse(dataset3, "json");
    }
}
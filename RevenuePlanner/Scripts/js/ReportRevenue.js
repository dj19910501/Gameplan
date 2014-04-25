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
            },
            padding: {
                left: 80
            }
        });

        barChart2.parse(dataset1, "json");

        // added by dharmraj for ticket #447 : Alignment is not proper for charts
        $("#chartDiv4 .dhx_canvas_text.dhx_axis_item_y").each(function (index, element) {
            $(element).attr('title', element.innerHTML);
        });

        // added by dharmraj for ticket #348
        $("#chartDiv4 .dhx_canvas_text.dhx_axis_item_x").each(function (index, element) {
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
            },
            padding: {
                left: 75
            }
        });

        barChart3.parse(dataset2, "json");

        // added by dharmraj for ticket #447 : Alignment is not proper for charts
        $("#chartDiv5 .dhx_canvas_text.dhx_axis_item_y").each(function (index, element) {
            $(element).attr('title', element.innerHTML);
        });

        // added by dharmraj for ticket #348
        $("#chartDiv5 .dhx_canvas_text.dhx_axis_item_x").each(function (index, element) {
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
            },
            padding: {
                left: 90
            }
        });

        barChart4.parse(dataset3, "json");

        // added by dharmraj for ticket #447 : Alignment is not proper for charts
        $("#chartDiv6 .dhx_canvas_text.dhx_axis_item_y").each(function (index, element) {
            $(element).attr('title', element.innerHTML);
        });

        // added by dharmraj for ticket #348
        $("#chartDiv6 .dhx_canvas_text.dhx_axis_item_x").each(function (index, element) {
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
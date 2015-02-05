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
    var dataset1 = sourcePerformanceData.ChartCustomField1, dataset2 = sourcePerformanceData.ChartCustomField2, dataset3 = sourcePerformanceData.ChartCustomField3;

    //fill data on reports table
    if (dataset1 != null) {
        $('#NoGraphsMsg').hide();
        $('.report-gray-container').show();
        FillChartSourcePerformanceChartCustomField1();
    }
    else {
        $('.report-gray-container').hide();
        $('#NoGraphsMsg').show();
        $('#chartDiv4Parent').hide();
    }
    if (dataset2 != null) {
        FillChartSourcePerformanceChartCustomField2();
    }
    else {
        $('#chartDiv5Parent').hide();
    }
    if (dataset3 != null) {
        FillChartSourcePerformanceChartCustomField3();
    }
    else {
        $('#chartDiv6Parent').hide();
    }

    function FillChartSourcePerformanceChartCustomField1() {
        $('#chartDiv4Parent').find('#chartDiv4').remove();
        $('#chartDiv4Parent').append('<div id="chartDiv4" class="report-chart4"></div>');

        var xAxisConfig = GetAxisConfiguration(dataset1);

        /*bar chart*/
        var barChart2 = new dhtmlXChart({
            view: "barH",
            container: "chartDiv4",
            value: "#Value#",
            tooltip: "#Value#%",
            radius: 0,
            border: false,
            color: "#ColorCode#",
            width: 20,
            xAxis: {
                start: 0,
                step: xAxisConfig.stepValue,
                end: xAxisConfig.endValue,
                lines: false
            },
            yAxis: {
                template: "#Title#",
                lines: false,
            },
            padding: {
                left: 75
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

    function FillChartSourcePerformanceChartCustomField2() {
        $('#chartDiv5Parent').find('#chartDiv5').remove();
        $('#chartDiv5Parent').append('<div id="chartDiv5" class="report-chart5"></div>');

        var xAxisConfig = GetAxisConfiguration(dataset2)

        /*bar chart*/
        var barChart3 = new dhtmlXChart({
            view: "barH",
            container: "chartDiv5",
            value: "#Value#",
            tooltip: "#Value#%",
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

    function FillChartSourcePerformanceChartCustomField3() {
        $('#chartDiv6Parent').find('#chartDiv6').remove();
        $('#chartDiv6Parent').append('<div id="chartDiv6" class="report-chart6"></div>');

        var xAxisConfig = GetAxisConfiguration(dataset3)

        /*bar chart*/
        var barChart4 = new dhtmlXChart({
            view: "barH",
            container: "chartDiv6",
            value: "#Value#",
            tooltip: "#Value#%",
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
                left: 85
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
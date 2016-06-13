

function setAttribute(options, JsonModel, IsExportVisible) {
    if (JsonModel.ChartHeight == 0) {
        JsonModel.ChartHeight = null;
    }

    if (JsonModel.ChartBorderWidth == 0) {
        JsonModel.ChartBorderWidth = null;
    }

    if (JsonModel.LegendSymbolWidth == 0) {
        JsonModel.LegendSymbolWidth = null;
    }

    if (JsonModel.LegendSymbolHeight == 0) {
        JsonModel.LegendSymbolHeight = null;
    }

    if (JsonModel.XLineWidth == 0) {
        JsonModel.XLineWidth = null;
    }

    if (JsonModel.YLineWidth == 0) {
        JsonModel.YLineWidth = null;
    }
    var ActualDecimalPlaces = JsonModel.TotalDecimalPlaces;
    if (JsonModel.TotalDecimalPlaces == -1) {
        ActualDecimalPlaces = 2;
    }
    //Added by kausha somiya on 09/22/2015 for #503
    options.chart.events = {
        afterPrint: function () {
            Highcharts.charts.forEach(function (chart) {
                if (chart !== undefined) {
                    chart.reflow();
                }
            });
        }
    }
    // Start - Added By Arpita Soni for Ticket #426 on 07/29/2015
    if (JsonModel.ChartType == 'gauge' || JsonModel.ChartType == 'solidgauge') {
        options.chart.options3d.enabled = false;

        if (JsonModel.ChartType == 'solidgauge') {
            options.yAxis.minorTickInterval = null;
            //options.yAxis.tickPixelInterval = JsonModel.YMaxValue;
            options.yAxis.lineWidth = 0;
            options.yAxis.tickWidth = 0;
            options.pane.startAngle = -90;
            options.pane.endAngle = 90;
            options.pane.center = ['50%', '50%'];
            options.pane.size = '100%';
            options.pane.background = {
                innerRadius: '60%',
                outerRadius: '100%',
                shape: 'arc',
                backgroundColor: JsonModel.paneBackgroundColor// Configured for #530
            }
            options.yAxis.plotBands[0] = [];
            // Configured for #530
            options.yAxis.stops = $.parseJSON(JsonModel.YAxisStops);
            //  options.yAxis.stops =  $.parseJSON("[[0.1,\"#55bf3b\"], [0.5,\"#DDDF0D\"], [0.8,\"#DF5353\"]]");
        }
        else {
            //options.yAxis.minorTickInterval = 'auto';
            options.yAxis.minorTickWidth = 1;
            options.yAxis.minorTickLength = 10;
            options.yAxis.minorTickPosition = 'inside';
            options.yAxis.minorTickColor = '#666';
            //Configuration for gauge chart
            options.yAxis.minorTickInterval = JsonModel.minorTickInterval;// Configured for #530
            options.yAxis.tickPixelInterval = JsonModel.tickPixelInterval;//Configured for #530
            //  options.yAxis.tickPixelInterval = 10;
            options.yAxis.tickWidth = 2;
            options.yAxis.tickPosition = 'inside';
            options.yAxis.tickLength = 10;
            options.yAxis.tickColor = '#666';
            options.pane.background = [{
                backgroundColor: '#DDD',
                outerRadius: '105%',
                innerRadius: '103%',
                borderWidth: 0
            }, {}];
            options.pane.startAngle = -150;
            options.pane.endAngle = 150;
            options.yAxis.plotBands[0].color = $.parseJSON(JsonModel.ChartColor)[0];
            //Configured for #530
            //'[{"from":"0","to":"120","color":"green"},{"from":"120","to":"160","color":"orange"},{"from":"160","to":"200","color":"red"}]'
            options.yAxis.plotBands = $.parseJSON(JsonModel.plotBands);
            //options.yAxis.plotBands = "[{from: 0, to: 120, color: 'green'}, {from: 120, to: 160, color: 'orange'}, {from: 160, to: 200, color: 'red'}]";
        }
    }
    else {
        if (options.yAxis.plotBands != null && options.yAxis.plotBands != 'undefined')
            options.yAxis.plotBands[0] = [];
        if (options.pane != null && options.pane != 'undefined')
            options.pane = [];
    }
    // End - Added By Arpita Soni for Ticket #426 on 07/29/2015
    // Start - Added By Parth Joshi for Ticket #395 on 06/07/2015
    options.chart.backgroundColor = JsonModel.ChartBGColor;
    options.chart.borderColor = JsonModel.ChartBorderColor;
    options.chart.borderWidth = JsonModel.ChartBorderWidth;
    options.chart.plotBackgroundColor = JsonModel.ChartPlotBGColor;
    options.chart.zoomType = JsonModel.ChartZoomType;
    options.chart.height = JsonModel.ChartHeight;
    options.chart.margin = JsonModel.ChartMargin;
    //Added by kausha on 27/07/2015  
    if (JsonModel.ChartType == 'line')
        options.chart.polar = JsonModel.IsPolarSupport;

    //options.chart.animation = false;
    options.chart.series = { animation: { duration: 3500 } };
    options.chart.style = {
        fontFamily: '"Source Sans Pro","Arial"',
        fontSize: '12px',
        fontWeight: 300
    };
    // End  - Added By Parth Joshi for Ticket #395  on 06/07/2015



    if (JsonModel.ChartType != 'combo' || JsonModel.ChartType != 'errorbar') {
        options.xAxis.alternateGridColor = JsonModel.XAlternateGridColor;
        options.xAxis.reversed = JsonModel.XReversed;
        options.xAxis.tickPosition = JsonModel.XTickPosition;
        options.xAxis.tickmarkPlacement = JsonModel.XTickmarkPlacement;
        options.xAxis.lineColor = JsonModel.XLineColor;
        options.xAxis.lineWidth = JsonModel.XLineWidth;

        // End - X- Axis
        // Start - Y -Axis
        options.yAxis.alternateGridColor = JsonModel.YAlternateGridColor;
        options.yAxis.reversed = JsonModel.YReversed;
        options.yAxis.lineColor = JsonModel.YLineColor;
        options.yAxis.lineWidth = JsonModel.YLineWidth;
        // End - Y -Axis
        // End  - Added By Parth Joshi for Ticket #402  on 07/07/2015
    }

    // Start - Added By Parth Joshi for Ticket No #401 on 07/17/2015

    options.tooltip.enabled = JSON.parse(JsonModel.TooltipEnabled);
    options.tooltip.crosshairs = JSON.parse(JsonModel.TooltipCrosshairFormat);
    options.tooltip.backgroundColor = JsonModel.TooltipBackgroundColor;
    options.tooltip.borderWidth = JsonModel.TooltipBorderWidth;
    options.tooltip.borderColor = JsonModel.TooltipBorderColor;




    // End - Added By Parth Joshi for Ticket No #401 on 07/17/2015


    if (JsonModel.ChartType == 'bubble' || JsonModel.ChartType == 'scatter' || JsonModel.ChartType == 'heatmap' || JsonModel.ChartType == 'coheatmap') {
        options.plotOptions = {

            series: {
                shadow: false,
                //animation: false,
                dataLabels: {
                    //align: 'left',
                    //enabled: true
                    style: {
                        textShadow: 'none'// added by sunita for ticket #452 important
                    }
                },
                marker: {}
            },
            scatter: {
                marker: {
                    radius: 5,
                    symbol: 'circle',
                    states: {
                        hover: {
                            enabled: true,
                            lineColor: 'rgb(100,100,100)'
                        }
                    }
                },
                states: {
                    hover: {
                        marker: {
                            enabled: false
                        }
                    }
                },
                tooltip: {
                    headerFormat: '<b>{series.name}</b><br>',
                    pointFormat: '{point.x}, {point.y}',
                    formatter: function () {
                                  return "<span>" + this.headerFormat + this.pointFormat +"</span>";
                    }
                },
                dataLabels: {
                    enabled: false,
                    style: {
                        textShadow: 'none'// added by sunita for ticket #452 important
                    }
                    //useHTML: true,
                    //style: {
                    //    fontFamily: '"Source Sans Pro","Arial"',
                    //    fontSize: '12px',
                    //    fontWeight: 300
                    //},
                    //formatter: function () {
                    //    if (this.y < 999) {
                    //        return GetSymbolforValues(this.y, symbolType);
                    //    }
                    //    else {
                    //        return GetSymbolforValues(GetAbberiviatedValue(this.y), symbolType);
                    //    }
                    //}
                }
            },
            bubble: {
                dataLabels: {
                    enabled: true,
                    //  useHTML: true,
                    style: {
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    marker: {},
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.point.z < 999)
                        //    return GetSymbolforValues(this.point.z, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.point.z.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.point.z, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.point.z, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.point.z, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.point.z, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.point.z.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.point.z, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.point.z, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.point.z, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }
                },
                //Start - Added By Parth Joshi for Bubble Tooltip format
                tooltip: {
                    //Added by kausha on 06-07-2015 for ticket not #413
                    headerFormat: '{series.name}<br>',
                    //pointFormat: '{point.x},{point.y},{point.z}',
                    pointFormatter: function () {
                        var value = this.z;
                        if (value < 0 && (JsonModel.IsNegativeRequired == null || JsonModel.IsNegativeRequired == true)) {
                            value = value * (-1);
                        }
                        if (value > 999)
                            value = number_format(value, 0, '.', ',')
                        if (this.x != null && this.x != 'undefined')
                            return '&nbsp;&nbsp;&nbsp;' + "(" + this.x + "," + this.y + ")" + ':<b>' + GetSymbolforValues(value, JsonModel.Symbol) + '</b>';
                        else
                            return this.series.name + ':<b>' + GetSymbolforValues(value, JsonModel.Symbol) + '</b>';
                    }
                }
            },
            heatmap: {
                dataLabels: {
                    enabled: true,
                    //useHTML: true,
                    style: {
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.point.value < 999)
                        //    return GetSymbolforValues(this.point.value, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.point.value.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.point.value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.point.value, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.point.value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.point.value, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.point.value.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.point.value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.point.value, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.point.value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }
                },
                tooltip: {
                    //Added by kausha on 06-07-2015 for ticket not #413
                    headerFormat: '{series.name}<br>',
                    //  pointFormat: '{point.x}: <b>{point.value}</b><br/>'
                    //Start - Added By Parth Joshi for Heatmap Tooltip format
                    pointFormatter: function () {
                        var value = this.value;
                        if (value < 0 && (JsonModel.IsNegativeRequired == null || JsonModel.IsNegativeRequired == true)) {
                            value = value * (-1);
                        }
                        if (value > 999)
                            value = number_format(value, 0, '.', ',')
                        if (this.x != null && this.x != 'undefined')
                            return '&nbsp;' + "(" + this.x + "," + this.y + ")" + ':<b>' + GetSymbolforValues(value, JsonModel.Symbol) + '</b>';
                        else
                            return this.series.name + ':<b>' + GetSymbolforValues(value, JsonModel.Symbol) + '</b>';
                    }
                    //End - Added By Parth Joshi for Heatmap Tooltip format
                }
            },
        };
        options.legend = {
            enabled: true,
            align: 'right',
            verticalAlign: 'middle',
            shadow: false,
            labelFormatter: function () {
                if (JsonModel.ChartType != 'heatmap') {
                    var legendName = this.name;
                    var match = legendName.match(/.{1,15}/g);
                    return match.toString().replace(/\,/g, "<br/>");
                }
                else {
                    return this.name;
                }
            },
            useHTML: true,


        };
    }
    if (JsonModel.ChartType == 'bullet') {
        options.plotOptions = {
            series: {
                // stacking: stackingseries,
                //shadow: false,
                //animation: false,
                dataLabels: {
                    //align: 'left',
                    //enabled: true
                    style: {
                        textShadow: 'none'// added by sunita for ticket #452 important
                    }
                },
                marker: {}
            },
            bar: {
                colorByPoint: null,
                dataLabels: {
                    enabled: true,
                    // useHTML: true,
                    style: {
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.y < 999)
                        //    return GetSymbolforValues(this.y, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.y, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.y, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }
                }
            },
            column: {
                colorByPoint: null,
                dataLabels: {
                    enabled: true,
                    style: {
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.y < 999)
                        //    return GetSymbolforValues(this.y, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.y, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.y, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }
                }
            },
            line: {
                marker: {
                    enabled: true
                },
                dataLabels: {
                    enabled: true,
                    //  useHTML: true,
                    style: {
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.y < 999)
                        //    return GetSymbolforValues(this.y, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.y, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.y, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }
                }
            },
            scatter: {
                marker: {
                    symbol: 'line',
                    lineWidth: 4,
                    radius: 12,
                    lineColor: '#E67300'
                },
                dataLabels: {
                    style: {
                        textShadow: 'none'// added by sunita for ticket #452 important
                    }
                }
            }
        };
        options.legend = {
            enabled: true,
            align: 'right',
            verticalAlign: 'middle',
            shadow: false,
            labelFormatter: function () {
                var legendName = this.name;
                var match = legendName.match(/.{1,15}/g);
                return match.toString().replace(/\,/g, "<br/>");
            },
            useHTML: true,
        };
    }
    if (JsonModel.ChartType == 'pie' || JsonModel.ChartType == 'donut' || JsonModel.ChartType == 'racetrack') {

        options.plotOptions = {
            series: {
                //shadow: true,
                //animation: false,
                dataLabels: {
                    //align: 'left',
                    //enabled: true
                    style: {
                        textShadow: 'none'// added by sunita for ticket #452 important
                    }
                },
                marker: {}
            },
            line: {
                marker: {
                    //  enabled: false
                },
                dataLabels: {
                    enabled: true,
                    //useHTML: true,
                    style: {
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.y < 999)
                        //    return GetSymbolforValues(this.y, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.y, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.y, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }

                }
            },
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                depth: JsonModel.Depth3D,
                dataLabels: {
                    formatter:
                            function () {
                                // Modified by Arpita Soni for Ticket #204 on 03/13/2015
                                var legendName = parseFloat(this.point.percentage).toFixed(1) + '%';
                                return legendName;
                            },
                    crop: false,
                    overflow: 'none',
                    // Commented By parth Joshi for Ticket No .#293
                    // useHTML: true,
                    style: {
                        color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black',
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        width: '110px',
                        textShadow: 'none'// added by sunita for ticket #452 important

                    },
                    distance: 18,
                },
                showInLegend: true,

            },
            //Added by sunita for 466
            bar: {
                dataLabels: {
                    enabled: true,
                    style: {
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.y < 999)
                        //    return GetSymbolforValues(this.y, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.y, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.y, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }
                }
            }
        };
        options.legend = {
            enabled: true,
            shadow: false,
            //floating: false,
            align: 'right',
            verticalAlign: 'middle',
            //layout: 'vertical',
            labelFormatter: function () {
                var legendName = this.name;
                //var match = legendName.match(/.{1,12}/g);
                var match = legendName;
                if ((JsonModel.LegnedPositions[2] == 'x' && JsonModel.LegnedPositions[0] == 'center')) {
                    match = legendName.match(/.{1,15}/g);
                }
                else {
                    if (legendName.length > 19) {
                        match = legendName.substring(0, 20) + '...';
                    }
                    else {
                        match = legendName;
                    }
                }
                return match.toString().replace(/\,/g, "<br/>");
            },
            //useHTML: true,
            itemWidth: 130
        };
        if (JsonModel.IsDataLabelVisible == true) {
            options.plotOptions.pie.dataLabels.enabled = true;
            options.plotOptions.bar.dataLabels.enabled = true; //Added by sunita for 466
        }
        else {
            options.plotOptions.pie.dataLabels.enabled = false;
            options.plotOptions.bar.dataLabels.enabled = false; //Added by sunita for 466
        }
    }
    if (JsonModel.ChartType == 'combo' || JsonModel.ChartType == 'errorbar') {
        options.plotOptions = {
            series: {
                //stacking: 'normal',
                //shadow: false,
                //animation: false,
                dataLabels: {
                    //align: 'left',
                    //enabled: true
                    style: {
                        textShadow: 'none'// added by sunita for ticket #452 important
                    }
                },
                marker: {}
            },
            line: {
                marker: {
                    enabled: false
                },
                dataLabels: {
                    enabled: true,
                    //useHTML: true,
                    style: {
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.y < 999)
                        //    return GetSymbolforValues(this.y, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.y, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.y, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }

                }
            },
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                showInLegend: false,
                dataLabels: {
                    formatter:
                            function () {
                                // Modified by Arpita Soni for Ticket #204 on 03/13/2015
                                var legendName = parseFloat(this.point.percentage).toFixed(1) + '%';
                                return legendName;
                            },
                    crop: false,
                    overflow: 'none',
                    //useHTML: true,
                    style: {
                        color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black',
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        width: '110px',
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    distance: 18,
                },
            },
            bar: {
                //shadow: false,
                dataLabels: {
                    enabled: true,
                    //useHTML: true,
                    style: {
                        fontFamily: '"Source Sans Pro","Arial"',
                        fontSize: '12px',
                        fontWeight: 300,
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.y < 999)
                        //    return GetSymbolforValues(this.y, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.y, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.y, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }
                }
            },
            column: {
                dataLabels: {
                    enabled: true,
                    style: {
                        textShadow: 'none'// added by sunita for ticket #452 important
                    },
                    formatter: function () {
                        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                        //if (this.y < 999)
                        //    return GetSymbolforValues(this.y, JsonModel.Symbol);
                        //else
                        if (JsonModel.MagnitudeValue != 0) {
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                            }
                        }
                        else {
                            //return GetSymbolforValues(GetAbberiviatedValue(this.y, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                            if (JsonModel.TotalDecimalPlaces == -1) {
                                if (this.y.toString().indexOf(".") != -1) {
                                    return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.y, 0, '.', ','), JsonModel.Symbol);
                                }
                            }
                            else {
                                return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                            }
                        }
                    }
                }
            }
        };
        options.legend = {
            enabled: true,
            shadow: false,
            align: 'right',
            verticalAlign: 'middle',

            labelFormatter: function () {
                var legendName = this.name;
                //var match = legendName.match(/.{1,12}/g);
                var match = legendName;
                if ((JsonModel.LegnedPositions[2] == 'x' && JsonModel.LegnedPositions[0] == 'center')) {
                    match = legendName.match(/.{1,15}/g);
                }
                else {
                    if (legendName.length > 19) {
                        match = legendName.substring(0, 20) + '...';
                    }
                    else {
                        match = legendName;
                    }
                }
                return match.toString().replace(/\,/g, "<br/>");
            },
            // useHTML: true,

            itemWidth: 130
        };
    }
    if (JsonModel.ChartType == 'columnrange') {
        options.plotOptions = {
            columnrange: {
                grouping: true,
            },
            series: {
                stacking: 'normal',
                // shadow: true,
                //animation: false,
                dataLabels: {
                    //align: 'left',
                    //enabled: true
                    style: {
                        textShadow: 'none'// added by sunita for ticket #452 important
                    }
                },
                marker: {}
            },
            dataLabels: {
                enabled: true,
                useHTML: true,
                style: {
                    fontFamily: '"Source Sans Pro","Arial"',
                    fontSize: '15px',
                    fontWeight: 300,
                    textShadow: 'none'// added by sunita for ticket #452 important
                },
                marker: {},

            },
        };
        options.legend = {
            enabled: true,
            shadow: false,
            // floating: false,
            align: 'right',
            verticalAlign: 'middle',
            //layout: 'vertical',
            labelFormatter: function () {
                var legendName = this.name;
                //var match = legendName.match(/.{1,12}/g);
                var match = legendName;
                if ((JsonModel.LegnedPositions[2] == 'x' && JsonModel.LegnedPositions[0] == 'center')) {
                    match = legendName.match(/.{1,15}/g);
                }
                else {
                    if (legendName.length > 19) {
                        match = legendName.substring(0, 20) + '...';
                    }
                    else {
                        match = legendName;
                    }
                }
                return match.toString().replace(/\,/g, "<br/>");
            },
            //  useHTML: true,

            itemWidth: 130
        };
    }
    if (JsonModel.ChartType == 'negativebar' || JsonModel.ChartType == 'negativecol' || JsonModel.ChartType == 'negativearea' || JsonModel.ChartType == 'bar' || JsonModel.ChartType == 'column' || JsonModel.ChartType == 'stackbar' || JsonModel.ChartType == 'stackcol' || JsonModel.ChartType == 'gauge' || JsonModel.ChartType == 'solidgauge' || JsonModel.ChartType == 'line' || JsonModel.ChartType == 'area') {
        if (JsonModel.ChartType == 'negativebar') {
            JsonModel.ChartType = 'bar';
        }
        if (JsonModel.ChartType == 'negativecol') {
            JsonModel.ChartType = 'column';
        }
        if (JsonModel.ChartType == 'negativearea') {
            JsonModel.ChartType = 'area';
        }

        if (JsonModel.IsMutlipleAxis == false) {

            options.plotOptions = {
                series: {
                    //animation: false,
                    dataLabels: {
                        style: {
                            textShadow: 'none'// added by sunita for ticket #452 important
                        }
                    },
                    marker: {}
                },
                bar: {
                    depth: JsonModel.Depth3D,
                    dataLabels: {
                        enabled: true,
                        // useHTML: true,
                        style: {
                            fontFamily: '"Source Sans Pro","Arial"',
                            fontSize: '12px',
                            fontWeight: 300,
                            textShadow: 'none'// added by sunita for ticket #452 important
                        },
                        formatter: function () {
                            var value = this.y;
                            if (value < 0 && (JsonModel.IsNegativeRequired == null || JsonModel.IsNegativeRequired == true))
                                value = value * (-1);
                            // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                            //if (value > -999 && value < 999)
                            //    return GetSymbolforValues(value, JsonModel.Symbol);
                            //else
                            if (JsonModel.MagnitudeValue != 0) {
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                //return GetSymbolforValues(GetAbberiviatedValue(value, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(number_format(value, 0, '.', ','), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                            }
                        }
                    }
                },
                column: {
                    depth: JsonModel.Depth3D,
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: 'none'// added by sunita for ticket #452 important
                        },
                        formatter: function () {
                            var value = this.y;
                            if (value < 0 && (JsonModel.IsNegativeRequired == null || JsonModel.IsNegativeRequired == true))
                                value = value * (-1);
                            // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                            //if (value > -999 && value < 999)
                            //    return GetSymbolforValues(value, JsonModel.Symbol);
                            //else
                            if (JsonModel.MagnitudeValue != 0) {
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                //return GetSymbolforValues(GetAbberiviatedValue(value, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(number_format(value, 0, '.', ','), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                            }
                        }
                    }
                },
                line: {
                    marker: {
                    },
                    dataLabels: {
                        enabled: true,
                        // useHTML: true,
                        style: {
                            fontFamily: '"Source Sans Pro","Arial"',
                            fontSize: '12px',
                            fontWeight: 300,
                            textShadow: 'none'// added by sunita for ticket #452 important
                        },
                        formatter: function () {
                            var value = this.y;
                            if (value < 0 && (JsonModel.IsNegativeRequired == null || JsonModel.IsNegativeRequired == true))
                                value = value * (-1);
                            // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                            //if (value > -999 && value < 999)
                            //    return GetSymbolforValues(value, JsonModel.Symbol);
                            //else
                            if (JsonModel.MagnitudeValue != 0) {
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                //return GetSymbolforValues(GetAbberiviatedValue(value, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(number_format(value, 0, '.', ','), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                            }
                        }

                    }
                },
                gauge: {
                    dataLabels: {
                        enabled: true,
                        // useHTML: true,
                        borderWidth: 0,
                        style: {
                            fontFamily: '"Source Sans Pro","Arial"',
                            fontSize: JsonModel.GaugeFontSize,
                            fontWeight: 300,
                            textShadow: 'none'// added by sunita for ticket #452 important
                        },
                        formatter: function () {
                            var value = this.y;
                            // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                            //if (value > -999 && value < 999)
                            //    return GetSymbolforValues(value, JsonModel.Symbol);
                            //else
                            if (JsonModel.MagnitudeValue != 0) {
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                //return GetSymbolforValues(GetAbberiviatedValue(value, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(number_format(value, 0, '.', ','), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                            }
                        }
                    }
                },
                solidgauge: {
                    dataLabels: {
                        enabled: true,
                        //  useHTML: true,
                        borderWidth: 0,
                        style: {
                            fontFamily: '"Source Sans Pro","Arial"',
                            //fontSize: '12px',
                            fontSize: JsonModel.SolidGaugeFontSize, //Configured regarding #530
                            fontWeight: 300,
                            textShadow: 'none'// added by sunita for ticket #452 important
                        },
                        formatter: function () {
                            var value = this.y;
                            // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                            //if (value > -999 && value < 999)
                            //    return GetSymbolforValues(value, JsonModel.Symbol);
                            //else
                            if (JsonModel.MagnitudeValue != 0) {
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                //return GetSymbolforValues(GetAbberiviatedValue(value, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(number_format(value, 0, '.', ','), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                            }
                        }
                    }
                }
            };

            options.legend = {
                enabled: true,
                shadow: false,
                align: 'right',
                verticalAlign: 'middle',
                labelFormatter: function () {
                    var legendName = this.name;
                    var match = legendName;
                    if ((JsonModel.LegnedPositions[2] == 'x' && JsonModel.LegnedPositions[0] == 'center')) {
                        match = legendName.match(/.{1,15}/g);
                    }
                    else {
                        if (legendName.length > 19) {
                            match = legendName.substring(0, 20) + '...';
                        }
                        else {
                            match = legendName;
                        }
                    }
                    return match.toString().replace(/\,/g, "<br/>");
                },
                // useHTML: true,
                itemWidth: 120
            };

            //Data Label
            if (JsonModel.IsDataLabelVisible == true) {
                options.plotOptions.bar.dataLabels.enabled = true;
                options.plotOptions.column.dataLabels.enabled = true;
                options.plotOptions.line.dataLabels.enabled = true;
                options.plotOptions.gauge.dataLabels.enabled = true;
                options.plotOptions.solidgauge.dataLabels.enabled = true;
            }
            else {
                options.plotOptions.bar.dataLabels.enabled = false;
                options.plotOptions.column.dataLabels.enabled = false;
                options.plotOptions.line.dataLabels.enabled = false;
                options.plotOptions.gauge.dataLabels.enabled = false;
                options.plotOptions.solidgauge.dataLabels.enabled = false;
            }
        }
        if (JsonModel.IsMutlipleAxis == true) {

            //var charttype = 'line';
            var stackingseries = null;

            options.plotOptions = {
                series: {
                    //  stacking: stackingseries,
                    // shadow: false,
                    //animation: false,
                    dataLabels: {
                        //align: 'left',
                        //enabled: true
                        style: {
                            textShadow: 'none'// added by sunita for ticket #452 important
                        }
                    },
                    marker: {}
                },
                bar: {
                    //shadow: false,
                    //colorByPoint: colorByPoint,
                    colorByPoint: false,
                    dataLabels: {
                        enabled: true,
                        //  useHTML: true,
                        style: {
                            fontFamily: '"Source Sans Pro","Arial"',
                            fontSize: '12px',
                            fontWeight: 300,
                            textShadow: 'none'// added by sunita for ticket #452 important
                        },
                        formatter: function () {
                            var value = this.y;
                            if (value < 0 && (JsonModel.IsNegativeRequired == null || JsonModel.IsNegativeRequired == true))
                                value = value * (-1);
                            // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                            //if (value > -999 && value < 999)
                            //    return GetSymbolforValues(value, JsonModel.Symbol);
                            //else
                            if (JsonModel.MagnitudeValue != 0) {
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                //return GetSymbolforValues(GetAbberiviatedValue(value, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(number_format(value, 0, '.', ','), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                            }
                        }
                    }
                },
                column: {
                    //colorByPoint: colorByPoint,
                    colorByPoint: false,
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: 'none'// added by sunita for ticket #452 important
                        },
                        formatter: function () {
                            var value = this.y;
                            if (value < 0 && (JsonModel.IsNegativeRequired == null || JsonModel.IsNegativeRequired == true))
                                value = value * (-1);
                            // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                            //if (value > -999 && value < 999)
                            //    return GetSymbolforValues(value, JsonModel.Symbol);
                            //else
                            if (JsonModel.MagnitudeValue != 0) {
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(GetAbberiviatedValue(value, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(value, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                //return GetSymbolforValues(GetAbberiviatedValue(value, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (value.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(number_format(value, 0, '.', ','), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(number_format(value, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                            }
                        }
                    }
                },
                line: {
                    marker: {
                        enabled: false
                    },
                    //colorByPoint: true,
                    //valuePrefix: '$ ',
                    dataLabels: {
                        enabled: true,
                        //  useHTML: true,
                        style: {
                            fontFamily: '"Source Sans Pro","Arial"',
                            fontSize: '12px',
                            fontWeight: 300,
                            textShadow: 'none'// added by sunita for ticket #452 important
                        },
                        //, formatter: function () {
                        //    return '$ ' + this.y ;
                        //}
                        formatter: function () {
                            // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                            //if (this.y < 999)
                            //    return GetSymbolforValues(this.y, JsonModel.Symbol);
                            //else
                            if (JsonModel.MagnitudeValue != 0) {
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (this.y.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(GetAbberiviatedValue(this.y, 0, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(GetAbberiviatedValue(this.y, ActualDecimalPlaces, JsonModel.MagnitudeValue), JsonModel.Symbol);
                                }
                            }
                            else {
                                //return GetSymbolforValues(GetAbberiviatedValue(this.y, JsonModel.TotalDecimalPlaces), JsonModel.Symbol);
                                if (JsonModel.TotalDecimalPlaces == -1) {
                                    if (this.y.toString().indexOf(".") != -1) {
                                        return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                    }
                                    else {
                                        return GetSymbolforValues(number_format(this.y, 0, '.', ','), JsonModel.Symbol);
                                    }
                                }
                                else {
                                    return GetSymbolforValues(number_format(this.y, ActualDecimalPlaces, '.', ','), JsonModel.Symbol);
                                }
                            }
                        }

                    }//,
                    //enableMouseTracking: false
                    //, formatter: function () {
                    //    return Highcharts.numberFormat(this.y, 2);
                    //}

                }
            };
            options.legend = {
                enabled: true,
                shadow: false,
                // layout: 'vertical',
                align: 'right',
                verticalAlign: 'middle',
                //floating: false,
                shadow: false,
                labelFormatter: function () {
                    var legendName = this.name;
                    //var match = legendName.match(/.{1,12}/g);
                    var match = legendName;
                    if ((JsonModel.LegnedPositions[2] == 'x' && JsonModel.LegnedPositions[0] == 'center')) {
                        match = legendName.match(/.{1,15}/g);
                    }
                    else {
                        if (legendName.length > 19) {
                            match = legendName.substring(0, 20) + '...';
                        }
                        else {
                            match = legendName;
                        }
                    }
                    return match.toString().replace(/\,/g, "<br/>");
                },
                //  useHTML: true,
                itemWidth: 120

            };
            //Insertion Start: 24//2016 Sunita Patil #736 - if MultiAxis is true then data labels set as true false according to database configuration.
            if (JsonModel.IsDataLabelVisible == true) {
                options.plotOptions.bar.dataLabels.enabled = true;
                options.plotOptions.column.dataLabels.enabled = true;
                options.plotOptions.line.dataLabels.enabled = true;
            }
            else {
                options.plotOptions.bar.dataLabels.enabled = false;
                options.plotOptions.column.dataLabels.enabled = false;
                options.plotOptions.line.dataLabels.enabled = false;
            }
            //Insertion End: 24//2016 Sunita Patil #736
        }

    }

    options.plotOptions.series.dataLabels.fontSize = JsonModel.GaugeFontSize;


    // Start - Added By Parth Joshi for Ticket No #400 on 07/17/2015
    options.plotOptions.series.fillOpacity = JsonModel.FillOpacity;
    options.plotOptions.series.dataLabels.align = JsonModel.DataLabelAlign;
    options.plotOptions.series.marker.enabled = JsonModel.MarkerEnabled;
    options.plotOptions.series.marker.fillColor = JsonModel.MarkerFillColor;
    options.plotOptions.series.marker.lineColor = JsonModel.MarkerLineColor;
    options.plotOptions.series.marker.lineWidth = JsonModel.MarkerLineWidth;
    options.plotOptions.series.shadow = JsonModel.SeriesShadow;
    if (JsonModel.ChartType != 'errorbar')
        options.plotOptions.series.stacking = JsonModel.SeriesStacking;
    // End - Added By Parth Joshi for Ticket No #400 on 07/17/2015

    // Start - Added By Parth Joshi for Ticket #398 on 06/07/2015
    options.legend.itemDistance = JsonModel.LegendItemDistance;
    if (JsonModel.ChartType != 'heatmap') {
        options.legend.symbolWidth = JsonModel.LegendSymbolWidth;
        options.legend.symbolHeight = JsonModel.LegendSymbolHeight;
    }
    options.legend.floating = JsonModel.LegendFloating;
    options.legend.x = JsonModel.LegendX;
    options.legend.y = JsonModel.LegendY;
    options.legend.reversed = JsonModel.LegendReversed;
    options.title.text = JsonModel.TitleText;
    // End  - Added By Parth Joshi for Ticket #398  on 06/07/2015

    // Start - Added By Parth Joshi for Ticket No #350 on 07/24/2015
    options.credits = {
        enabled: false
    };

    options.navigation = {
        buttonOptions: {
            theme: {
                height: 15,
                'stroke-width': 1,
                stroke: '#CCC',
                fill: '#EFEFEF',
                r: 5,
                states: {
                    hover: {
                        fill: '#ddd',
                        stroke: '#ccc'
                    },
                    select: {
                        fill: '#ddd',
                        stroke: '#ccc'
                    }
                }
            }
        }
    };



    //if (JsonModel.ChartType == 'bubble' || JsonModel.ChartType == 'heatmap' || JsonModel.ChartType == 'scatter') {
    //    options.exporting = {
    //        buttons: {
    //            contextButton: {
    //                symbol: 'menu',
    //                symbolStrokeWidth: 1,
    //                symbolStroke: '#000',
    //                symbolSize: 10,
    //                text: 'Export',
    //                align: 'right',
    //                symbolX: 18.5,
    //                symbolY: 13.5,
    //                verticalAlign: 'top',
    //                width: 0,
    //                x: 0,
    //                y: 15,
    //                menuItems: [{
    //                    text: 'Print Chart',
    //                    textKey: 'printChart',
    //                    onclick: function () {
    //                        this.print();
    //                    }
    //                }, {
    //                    separator: true
    //                },
    //                {
    //                    text: 'Download as PNG',
    //                    onclick: function () {
    //                        this.exportChartLocal();
    //                    },
    //                }, {
    //                    text: 'Download as PDF',
    //                    textKey: 'downloadPDF',
    //                    onclick: function () {
    //                        //this.exportChartLocal({
    //                        //    type: 'application/pdf'
    //                        //});
    //                        PDFExport(this);
    //                    }
    //                }],



    //            }
    //        }

    //    };
    //}
    //else {
    options.exporting = {
        buttons: {
            contextButton: {
                symbol: 'menu',
                symbolStrokeWidth: 1,
                symbolStroke: '#000',
                symbolSize: 10,
                text: 'Export',
                align: 'right',
                symbolX: 18.5,
                symbolY: 13.5,
                verticalAlign: 'top',
                width: 0,
                x: 0,
                y: 15
            }
        },
        //Added by sunita important! for ticket 
        chartOptions: {
            legend: {
                itemStyle: {
                    width: 100 // for full text
                },
                labelFormatter: function () {
                    return this.name;
                }
            }
        }
    };
    //}
    if (JsonModel.ChartType != 'combo') {
        //Legend
        if (JsonModel.IsLegendVisible == true) {
            options.legend.enabled = true;

            options.legend.align = JsonModel.LegnedPositions[0];
            options.legend.verticalAlign = JsonModel.LegnedPositions[1];
            if (JsonModel.LegnedPositions[2] == 'y') {
                options.legend.layout = 'verical';
            }
            else {
                options.legend.layout = 'horizontal';
            }

        }
        else {
            options.legend.enabled = false;
        }
    }

    //Export Button
    if (IsExportVisible.toLowerCase() == 'true') {
        options.exporting.enabled = true;
    }
    else {
        options.exporting.enabled = false;
    }
    // End - Added By Parth Joshi for Ticket No #350 on 07/24/2015

}

function tooltipformatterBootStrap(budgetValue, cell, container, DecimalPlaces, MagnitudePlaces, symbolPreList) {
    if (budgetValue) {
        var decimalValue;
        var magnitudeValue;
        if (DecimalPlaces === undefined || DecimalPlaces == null) {
            decimalValue = 2;
        }
        else {
            decimalValue = DecimalPlaces;
        }

        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
        if (MagnitudePlaces === undefined || MagnitudePlaces == null) {
            magnitudeValue = 0;
        }
        else {
            magnitudeValue = MagnitudePlaces;
        }
        var isDollarAmout = false;
        var isPercentAmout = false;
        var isPostiveSign = false;
        var isNegativeSign = false;
        var CorrectBudgetVal = false;
        if (budgetValue.indexOf('%') > -1) {
            isPercentAmout = true;
        }

        var StrAfterRemExtrChar = [];
        var SpChar = '';
        StrAfterRemExtrChar = RemoveExtraCharactersFromString(budgetValue, symbolPreList);
        budgetValue = StrAfterRemExtrChar[0];
        SpChar = StrAfterRemExtrChar[1];
        var remNumber = '';

        var newremain = '';

        if (budgetValue.indexOf(".") != -1) {
            remNumber = budgetValue.substr(budgetValue.indexOf('.'));
            if (remNumber.length > 2)
                newremain = remNumber.substring(0, decimalValue + 1);
        }
        if (magnitudeValue != 0) {
            if (budgetValue > 999 && !isNaN(budgetValue)) {
                CorrectBudgetVal = true;
                // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
                var ActVal = GetAbberiviatedValue(budgetValue, decimalValue, magnitudeValue);
                if (isPercentAmout) {
                    $(cell).html(ActVal + ' %');
                    if (remNumber == '' && DecimalPlaces == null) {
                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), 0, '.', ',') + ' %');
                    }
                    else if (remNumber == '') {
                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue, '.', ',') + ' %');
                    }
                    else {
                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') + ' %');
                    }
                }
                else {
                    $(cell).html(SpChar == '' ? ActVal : SpChar + ' ' + ActVal);
                    if (remNumber == '' && DecimalPlaces == null) {
                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), 0, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), 0, '.', ','));
                    }
                    else if (remNumber == '') {
                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue, '.', ','));
                    }
                    else {
                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ','));
                    }
                }
                $(cell).addClass('north');
                $(cell).attr('data-toggle', 'popover');
                $(cell).popover({
                    trigger: "hover",
                    placement: 'bottom',
                    container: container,
                    html: true,
                });
            }
            else if (budgetValue < -999 && !isNaN(budgetValue)) {
                CorrectBudgetVal = true;
                var ActVal = GetAbberiviatedValue(budgetValue, decimalValue, magnitudeValue);
                if (isPercentAmout) {
                    $(cell).html(ActVal + ' %');
                    if (remNumber == '' && DecimalPlaces == null) {
                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), 0, '.', ',') + ' %');
                    }
                    else if (remNumber == '') {
                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue, '.', ',') + ' %');
                    }
                    else {
                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') + ' %');
                    }
                }
                else {
                    $(cell).html(SpChar == '' ? ActVal : SpChar + ' ' + ActVal);
                    if (remNumber == '' && DecimalPlaces == null) {
                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), 0, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), 0, '.', ','));
                    }
                    else if (remNumber == '') {
                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue, '.', ','));
                    }
                    else {
                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ','));
                    }
                }
                $(cell).addClass('north');
                $(cell).attr('data-toggle', 'popover');
                $(cell).popover({
                    trigger: "hover",
                    placement: 'bottom',
                    container: container,
                    html: true
                });
            }
            else {
                if (!isNaN(budgetValue)) {
                    CorrectBudgetVal = true;
                    var ActVal = GetAbberiviatedValue(budgetValue, decimalValue, magnitudeValue);
                    if (isPercentAmout) {
                        $(cell).html(ActVal + ' %');
                        if (remNumber == '' && DecimalPlaces == null) {
                            $(cell).attr('data-original-title', number_format(budgetValue.toString(), 0, '.', ',') + ' %');
                        }
                        else if (remNumber == '') {
                            $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue, '.', ',') + ' %');
                        }
                        else {
                            $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') + ' %');
                        }
                    }
                    else {
                        $(cell).html(SpChar == '' ? ActVal : SpChar + ' ' + ActVal);
                        if (remNumber == '' && DecimalPlaces == null) {
                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), 0, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), 0, '.', ','));
                        }
                        else if (remNumber == '') {
                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue, '.', ','));
                        }
                        else {
                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ','));
                        }
                    }
                    $(cell).addClass('north');
                    $(cell).attr('data-toggle', 'popover');
                    $(cell).popover({
                        trigger: "hover",
                        placement: 'bottom',
                        container: container,
                        html: true
                    });
                }
                else {
                    CorrectBudgetVal = false;
                    $(cell).html(budgetValue);
                    $(cell).attr('data-original-title', budgetValue);
                    $(cell).addClass('north');
                    $(cell).attr('data-toggle', 'popover');
                    $(cell).popover({
                        trigger: "hover",
                        placement: 'bottom',
                        container: container,
                        html: true
                    });
                }
            }
            if (!CorrectBudgetVal && !isNaN(budgetValue)) {
                if (isPercentAmout) {
                    $(cell).html(budgetValue.replace(/ /g, '') + ' %');
                    $(cell).attr('data-original-title', number_format(budgetValue.replace(/ /g, '').toString(), decimalValue, '.', ',') + ' %');
                }
                else {
                    $(cell).html(SpChar == '' ? budgetValue.replace(/ /g, '') : SpChar + ' ' + budgetValue.replace(/ /g, ''));
                    $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.replace(/ /g, '').toString(), decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.replace(/ /g, '').toString(), decimalValue, '.', ','));
                }
                $(cell).addClass('north');
                $(cell).attr('data-toggle', 'popover');
                $(cell).popover({
                    trigger: "hover",
                    placement: 'bottom',
                    container: container,
                    html: true
                });
            }
        }
        else {
            if (isNaN(budgetValue)) {
                if (isPercentAmout) {
                    $(cell).html(budgetValue + ' %');
                    $(cell).attr('data-original-title', budgetValue + ' %');
                }
                else {
                    $(cell).html(SpChar == '' ? budgetValue : SpChar + ' ' + budgetValue);
                    $(cell).attr('data-original-title', SpChar == '' ? budgetValue : SpChar + ' ' + budgetValue);
                }
            }
            else {
                if (isPercentAmout) {
                    if (DecimalPlaces == null) {
                        if (remNumber == '') {
                            $(cell).html(number_format(budgetValue, 0, '.', ',') + ' %');
                            $(cell).attr('data-original-title', number_format(budgetValue, 0, '.', ',') + ' %');
                        }
                        else {
                            $(cell).html(number_format(budgetValue, decimalValue, '.', ',') + ' %');
                            $(cell).attr('data-original-title', number_format(budgetValue, decimalValue, '.', ',') + ' %');
                        }
                    }
                    else {
                        $(cell).html(number_format(budgetValue, decimalValue, '.', ',') + ' %');
                        $(cell).attr('data-original-title', number_format(budgetValue, decimalValue, '.', ',') + ' %');
                    }
                }
                else {
                    if (DecimalPlaces == null) {
                        if (remNumber == '') {
                            $(cell).html(SpChar == '' ? number_format(budgetValue, 0, '.', ',') : SpChar + ' ' + number_format(budgetValue, 0, '.', ','));
                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue, 0, '.', ',') : SpChar + ' ' + number_format(budgetValue, 0, '.', ','));
                        }
                        else {
                            $(cell).html(SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
                        }
                    }
                    else {
                        $(cell).html(SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
                        if (remNumber == '') {
                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
                        }
                        else {
                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
                        }
                    }
                }
            }
            $(cell).addClass('north');
            $(cell).attr('data-toggle', 'popover');
            $(cell).popover({
                trigger: "hover",
                placement: 'bottom',
                container: container,
                html: true
            });
        }
    }
}



//function hidePleaseWaitDialog() {
//    if (myApp != 'undefined' && myApp != null) {
//        myApp.hidePleaseWait();
//    }
//}
function SetLabelFormaterWithoutTipsy(obj) {
    try {
        var isAmount = false;
        var budgetValue = $(obj).html();
        if (budgetValue.indexOf("$") != -1) {
            isAmount = true;
        }

        if (budgetValue) { //Check whether the number is empty or not
            var StrAfterRemExtrChar = [];
            StrAfterRemExtrChar = RemoveExtraCharactersFromString(budgetValue); //Function that remove the special char from the string 
            budgetValue = StrAfterRemExtrChar[0];
            if (budgetValue.length >= 4) {
                SetFormatForLabelExtendedWithoutTipsy(obj);
                //$(obj).html('$' + $(obj).html());
                var remNumber = '';
                if (budgetValue.indexOf(".") != -1) {
                    remNumber = budgetValue.substr(budgetValue.indexOf('.'));
                }
            }
        }
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
        //  hidePleaseWaitDialog();
    }
}

function SetFormatForLabelExtendedWithoutTipsy(obj) {

    try {
        var isAmount = false;
        var txtvalue = $(obj).text();
        txtvalue = ConvertToNum(txtvalue);

        if (txtvalue.indexOf("$") != -1) {
            isAmount = true;
        }
        if (txtvalue) {
            var StrAfterRemExtrChar = [];
            StrAfterRemExtrChar = RemoveExtraCharactersFromString(txtvalue); //Function that remove the special char from the string 
            txtvalue = StrAfterRemExtrChar[0];
            if (txtvalue.indexOf('.') != -1) {
                txtvalue = txtvalue.substring(0, txtvalue.indexOf('.'))
            }
            if (isAmount) {
                $(obj).text('$' + GetAbberiviatedValue(txtvalue));
                //$(obj).attr('title', '$' + txtvalue);
            }
            else {
                $(obj).text(GetAbberiviatedValue(txtvalue));
                //$(obj).attr('title', txtvalue);
            }
        }
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
        //hidePleaseWaitDialog();
    }
}
function SetLabelFormaterWithTipsy(obj) {
    try {
        var isAmount = false;
        var budgetValue = $(obj).html();
        if (budgetValue.indexOf("$") != -1) {
            isAmount = true;
        }

        if (budgetValue) { //Check whether the number is empty or not
            var StrAfterRemExtrChar = [];
            StrAfterRemExtrChar = RemoveExtraCharactersFromString(budgetValue); //Function that remove the special char from the string 
            budgetValue = StrAfterRemExtrChar[0];
            if (budgetValue.length >= 4) {
                SetFormatForLabelExtended(obj);
                //$(obj).html('$' + $(obj).html());
                var remNumber = '';
                if (budgetValue.indexOf(".") != -1) {
                    remNumber = budgetValue.substr(budgetValue.indexOf('.'));
                }

                //Add tipsy for the current label.
                $(obj).attr('title', budgetValue);
                if (isAmount) {
                    $(obj).prop('title', "$" + number_format(budgetValue.toString(), 0, '.', ',') + remNumber);
                }
                else {
                    $(obj).prop('title', number_format(budgetValue.toString(), 0, '.', ',') + remNumber);
                }

                $(obj).addClass('north');
                $('.north').tipsy({ gravity: 'n' });
            }
        }
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
        // hidePleaseWaitDialog();
    }
}
function SetFormatForLabelExtended(obj) {
    try {
        var isAmount = false;
        var txtvalue = $(obj).text();
        if (txtvalue.indexOf("$") != -1) {
            isAmount = true;
        }
        if (txtvalue) {
            var StrAfterRemExtrChar = [];
            StrAfterRemExtrChar = RemoveExtraCharactersFromString(txtvalue); //Function that remove the special char from the string 
            txtvalue = StrAfterRemExtrChar[0];
            if (txtvalue.indexOf('.') != -1) {
                txtvalue = txtvalue.substring(0, txtvalue.indexOf('.'))
            }
            if (isAmount) {
                $(obj).text('$' + GetAbberiviatedValue(txtvalue));
                $(obj).attr('title', '$' + txtvalue);
            }
            else {
                $(obj).text(GetAbberiviatedValue(txtvalue));
                $(obj).attr('title', txtvalue);
            }
            $(obj).addClass('north');
            //  $('.north').tipsy({ gravity: 'n' });
        }
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
        //  hidePleaseWaitDialog();
    }
}

function ReturnFormatValue(txtvalue) {
    try {
        var isAmount = false;
        if (txtvalue.indexOf("$") != -1) {
            isAmount = true;
        }
        if (txtvalue) {
            var StrAfterRemExtrChar = [];
            StrAfterRemExtrChar = RemoveExtraCharactersFromString(txtvalue); //Function that remove the special char from the string 
            txtvalue = StrAfterRemExtrChar[0];
            if (txtvalue.indexOf('.') != -1) {
                txtvalue = txtvalue.substring(0, txtvalue.indexOf('.'))
            }
            if (isAmount) {
                txtvalue = '$' + GetAbberiviatedValue(txtvalue);
            }
            else {
                txtvalue = GetAbberiviatedValue(txtvalue);
            }
            //$(obj).addClass('north');
            //$('.north').tipsy({ gravity: 'n' });
        }
        return txtvalue;
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
        // hidePleaseWaitDialog();
    }
}
function RemoveExtraCharactersFromString(value, SpCharArr) {
    var symbolPreList = [];
    var SpChar = '';
    if (SpCharArr === undefined || SpCharArr == null) {
        symbolPreList = ['$', '₭', '%', 'NA', '£', '₹', 'Y', '¥'];
    }
    else {
        symbolPreList = SpCharArr;
    }
    try {
        for (var i = 0; i < symbolPreList.length; i++) {
            if (value.indexOf(symbolPreList[i]) != -1) {
                value = value.replace(symbolPreList[i], "");
                SpChar = symbolPreList[i];
                //break;
            }
        }
        value = value.replace(/[\,]+/g, "");
        return [value, SpChar];
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
        //hidePleaseWaitDialog();
    }
}
function ConvertToNum(value) {
    if (isNaN(value)) {
        if (value != null) {
            var str = value.toString();
            if (str.indexOf(":") >= 0) {
                str = str.substring(str.indexOf(":") + 1, str.length);
                return str;
            }
        }
    }
    return value;
}
function GetAbberiviatedValue(value, DecimalPlaces, MagnitudePlaces) {
    try {
        if (value.toString().trim() == null || value.toString().trim() == 'null' || value.toString().trim() == '0' || value.toString().trim() == ' ' || value.toString().trim() == '') {
            return '0';
        }
        var decimalValue;
        var magnitudeValue;
        value = ConvertToNum(value);
        var absValue = Math.abs(parseFloat(value));
        absValue = absValue.toFixed(2);
        var isNegative = value < 0;
        var postfix = ['k', 'M', 'B', 'T', 'Q'];
        var postIndex = [3, 6, 9, 12, 15];
        var postValue = [1000, 1000000, 1000000000, 1000000000000, 1000000000000000];
        var indexvalue = 0;
        var actualvalue;

        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
        if (DecimalPlaces === undefined || DecimalPlaces == null) {
            if (MagnitudePlaces === undefined || MagnitudePlaces == null) {
                if (absValue < 1000) {
                    actualvalue = (Math.round(absValue * 100) / 100);
                }
                else {
                    if (absValue < 1000000) {
                        indexvalue = 0;
                        if (absValue >= 1000 && absValue < 10000) {
                            value = Math.round((absValue / 1000) * 100) / 100;
                        }
                        else if (absValue >= 10000 && absValue < 100000) {
                            value = Math.round((absValue / 1000) * 10) / 10;
                        }
                        else if (absValue >= 100000 && absValue < 1000000) {
                            value = Math.round((absValue / 1000));
                        }
                    }
                    else if (absValue < 1000000000) {
                        indexvalue = 1;
                        if (absValue >= 1000000 && absValue < 10000000) {
                            value = Math.round((absValue / 1000000) * 100) / 100;
                        }
                        else if (absValue >= 10000000 && absValue < 100000000) {
                            value = Math.round((absValue / 1000000) * 10) / 10;
                        }
                        else if (absValue >= 100000000 && absValue < 1000000000) {
                            value = Math.round((absValue / 1000000));
                        }
                    }
                    else if (absValue < 1000000000000) {
                        indexvalue = 2;
                        if (absValue >= 1000000000 && absValue < 10000000000) {
                            value = Math.round((absValue / 1000000000) * 100) / 100;
                        }
                        else if (absValue >= 10000000000 && absValue < 100000000000) {
                            value = Math.round((absValue / 1000000000) * 10) / 10;
                        }
                        else if (absValue >= 100000000000 && absValue < 1000000000000) {
                            value = Math.round((absValue / 1000000000));
                        }
                    }
                    else if (absValue < 1000000000000000) {
                        indexvalue = 3;
                        if (absValue >= 1000000000000 && absValue < 10000000000000) {
                            value = Math.round((absValue / 1000000000000) * 100) / 100;
                        }
                        else if (absValue >= 10000000000000 && absValue < 100000000000000) {
                            value = Math.round((absValue / 1000000000000) * 10) / 10;
                        }
                        else if (absValue >= 100000000000000 && absValue < 1000000000000000) {
                            value = Math.round((absValue / 1000000000000));
                        }
                    }
                    else if (absValue < 1000000000000000000) {
                        indexvalue = 4;
                        if (absValue >= 1000000000000000 && absValue < 10000000000000000) {
                            value = Math.round((absValue / 1000000000000000) * 100) / 100;
                        }
                        else if (absValue >= 10000000000000000 && absValue < 100000000000000000) {
                            value = Math.round((absValue / 1000000000000000) * 10) / 10;
                        }
                        else if (absValue >= 100000000000000000 && absValue < 1000000000000000000) {
                            value = Math.round((absValue / 1000000000000000));
                        }
                    }

                    if (value >= 1000) {
                        actualvalue = (value / 1000).toFixed(2) + postfix[indexvalue + 1];
                    }
                    else if (value >= 100) {
                        actualvalue = value.toFixed() + postfix[indexvalue];
                    }
                    else if (value >= 10) {
                        actualvalue = value.toFixed(1) + postfix[indexvalue];
                    }
                    else {
                        actualvalue = value.toFixed(2) + postfix[indexvalue];
                    }
                }
                return (isNegative ? '-' + actualvalue.toString() : actualvalue.toString());
            }
            else {
                magnitudeValue = MagnitudePlaces;
                for (var i = 0; i < postIndex.length; i++) {
                    if (postIndex[i] == Number(magnitudeValue)) {
                        value = (absValue / Number(postValue[i]));
                        actualvalue = value + postfix[i];
                        break;
                    }
                }
                return (isNegative ? '-' + actualvalue.toString() : actualvalue.toString());
            }
        }
        else {
            decimalValue = DecimalPlaces;
            if (MagnitudePlaces === undefined || MagnitudePlaces == null) {
                if (absValue < 1000) {
                    actualvalue = ((absValue * 100) / 100).toFixed(Number(decimalValue));
                }
                else {
                    if (absValue < 1000000) {
                        indexvalue = 0;
                        if (absValue >= 1000 && absValue < 10000) {
                            value = (((absValue / 1000) * 100) / 100).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 10000 && absValue < 100000) {
                            value = (((absValue / 1000) * 10) / 10).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 100000 && absValue < 1000000) {
                            value = ((absValue / 1000)).toFixed(Number(decimalValue));
                        }
                    }
                    else if (absValue < 1000000000) {
                        indexvalue = 1;
                        if (absValue >= 1000000 && absValue < 10000000) {
                            value = (((absValue / 1000000) * 100) / 100).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 10000000 && absValue < 100000000) {
                            value = (((absValue / 1000000) * 10) / 10).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 100000000 && absValue < 1000000000) {
                            value = (((absValue / 1000000))).toFixed(Number(decimalValue));
                        }
                    }
                    else if (absValue < 1000000000000) {
                        indexvalue = 2;
                        if (absValue >= 1000000000 && absValue < 10000000000) {
                            value = (((absValue / 1000000000) * 100) / 100).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 10000000000 && absValue < 100000000000) {
                            value = (((absValue / 1000000000) * 10) / 10).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 100000000000 && absValue < 1000000000000) {
                            alue = (((absValue / 1000000000))).toFixed(Number(decimalValue));
                        }
                    }
                    else if (absValue < 1000000000000000) {
                        indexvalue = 3;
                        if (absValue >= 1000000000000 && absValue < 10000000000000) {
                            value = (((absValue / 1000000000000) * 100) / 100).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 10000000000000 && absValue < 100000000000000) {
                            value = (((absValue / 1000000000000) * 10) / 10).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 100000000000000 && absValue < 1000000000000000) {
                            value = (((absValue / 1000000000000))).toFixed(Number(decimalValue));
                        }
                    }
                    else if (absValue < 1000000000000000000) {
                        indexvalue = 4;
                        if (absValue >= 1000000000000000 && absValue < 10000000000000000) {
                            value = (((absValue / 1000000000000000) * 100) / 100).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 10000000000000000 && absValue < 100000000000000000) {
                            value = (((absValue / 1000000000000000) * 10) / 10).toFixed(Number(decimalValue));
                        }
                        else if (absValue >= 100000000000000000 && absValue < 1000000000000000000) {
                            value = (((absValue / 1000000000000000))).toFixed(Number(decimalValue));
                        }
                    }
                    if (value >= 1000) {
                        actualvalue = (value / 1000) + postfix[indexvalue + 1];
                    }
                    else {
                        if (value.trim() != "" && value.trim() != " ") {
                            actualvalue = value + postfix[indexvalue];
                        }
                    }
                }
                return (isNegative ? '-' + actualvalue.toString() : actualvalue.toString());
            }
            else {
                magnitudeValue = MagnitudePlaces;
                for (var i = 0; i < postIndex.length; i++) {
                    if (postIndex[i] == Number(magnitudeValue)) {
                        value = (absValue / Number(postValue[i])).toFixed(Number(decimalValue));
                        actualvalue = value + postfix[i];
                        break;
                    }
                }
                return (isNegative ? '-' + actualvalue.toString() : actualvalue.toString());
            }
        }
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
        //hidePleaseWaitDialog();

    }
}
function number_format(number, decimals, dec_point, thousands_sep) {
    try {
        number = (number + '').replace(/[^0-9+\-Ee.]/g, '');
        var n = !isFinite(+number) ? 0 : +number,
            prec = !isFinite(+decimals) ? 0 : Math.abs(decimals),
            sep = (typeof thousands_sep === 'undefined') ? ',' : thousands_sep,
            dec = (typeof dec_point === 'undefined') ? '.' : dec_point,
            s = '',
            toFixedFix = function (n, prec) {
                var k = Math.pow(10, prec);
                return '' + Math.round(n * k) / k;
            };
        // Fix for IE parseFloat(0.55).toFixed(0) = 0;
        s = (prec ? toFixedFix(n, prec) : '' + Math.round(n)).split('.');
        if (s[0].length > 3) {
            s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep);
        }
        if ((s[1] || '').length < prec) {
            s[1] = s[1] || '';
            s[1] += new Array(prec - s[1].length + 1).join('0');
        }
        return s.join(dec);
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
        //hidePleaseWaitDialog();
    }
}
function FormatAllValues(className) {
    try {
        $('.' + className).each(function (itm, index) {
            if ($(index).html().indexOf('%') == -1) {
                SetLabelFormaterWithTipsy($(index));
            }
            else {
                $(index).html($(index).html().replace('%', ''));
                SetLabelFormaterWithTipsy($(index));
                $(index).html($(index).html() + ' %');
            }
        });
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
        //hidePleaseWaitDialog();

    }
}

//// JavaScript Document
//$(document).ready(function() {
//	$(".table tr").mouseenter(function(){
//		$(this).find("td span.x-icon").removeClass('hide').addClass('show');

//	});
//	$(".table tr").mouseleave(function(){
//		$(this).find("td span.x-icon").removeClass('show').addClass('hide');
//	});
//	$('.navbar-nav li a').click(function(){
//		$('.dropdown-info-user').toggle();
//		});
//  	$('.navbar-nav li a').click(function(){
//		if($('.navbar-nav').hasClass('selected')){
//			$('.navbar-nav').removeClass('selected');
//			$('.nav li').css('background','url("images/arrow.png") no-repeat scroll left center rgba(0, 0, 0, 0)');
//		}
//		else{
//			$('.navbar-nav').addClass('selected');
//			$('.nav li').css('background','url("images/arrow-white-nav.png") no-repeat scroll left center rgba(0, 0, 0, 0)');
//			}

//		});
//});

//// Function to append symbol with labels.
//// Function to append symbol with labels.
function GetSymbolforValues(value, symbolType) {

    if (symbolType == "currency") {
        value = "$" + value;
    }
    else if (symbolType == "%") {
        value = value + '%';
    }
    else if (symbolType != 'undefined' && symbolType != null && symbolType != ' ') {
        if (symbolType.toLowerCase() == "percentage") {
            if (!isNaN(value)) {
                value = (value * 100) + ""
                //value = parseFloat((value * 100)) + "%";
                if (value.indexOf('.') != -1) {
                    var arr = value.split('.');
                    if (arr[1].length > 2)
                        value = parseFloat(value).toFixed(0) + ""
                }
                value = value + "%"
            }
        }
    }


    return value;
}


//// Start - Added by Sohel Pathan on 12/03/2015 for PL ticket #192
function PDFExport(graph) {
    var graphId = graph.renderTo.id;
    var chart = $('#' + graphId).highcharts();
    var ComparisonFlagEnabled = IsComparisonFlagEnabled.toLowerCase() == 'true' ? true : false;
    var DashComparisonVisible = IsDashComparisonVisible.toLowerCase() == 'true' ? true : false;
    if (ComparisonFlagEnabled && DashComparisonVisible) {
        chart.options.title.text = 'Date Range : ' + DateRange + '<br /> Comparison : ' + ComparisionDate;
    }
    else {
        chart.options.title.text = 'Date Range : ' + DateRange;
    }
    chart.exportChart({
        url: ExportServerURL,
        type: 'application/pdf',
        filename: (chart.options.title.text == null || chart.options.title.text.indexOf('Date Range :') > -1) ? GetChartHeaderForExport(graph) : chart.options.title.text + ' ' + getCurrentDateTimeString()
    });
}

function GetChartHeaderForExport(graph) {
    var graphId = graph.renderTo;
    if (parseInt($(graphId).parent().parent().prev('.tab-head').length) > 0 && $(graphId).parent().parent().prev('.tab-head').attr('header') != '') {
        return $(graphId).parent().parent().prev('.tab-head').attr('header');
    }
    else if (parseInt($(graphId).parent().parent().prev('.home-chart-header').length) > 0 && $(graphId).parent().parent().prev('.home-chart-header').attr('header') != '') {
        return $(graphId).parent().parent().prev('.home-chart-header').attr('header');
    }
    else {
        return 'Export_Chart_';
    }
}

function getCurrentDateTimeString() {
    var currentdate = new Date();
    var pad = '00';
    var datetime = (pad + currentdate.getDate().toString()).slice(-pad.length)
                    + (pad + (currentdate.getMonth() + 1).toString()).slice(-pad.length)
                    + currentdate.getFullYear().toString() + "_"
                    + (pad + currentdate.getHours().toString()).slice(-pad.length)
                    + (pad + currentdate.getMinutes().toString()).slice(-pad.length)
                    + (pad + currentdate.getSeconds().toString()).slice(-pad.length);
    return datetime;
}
//// End - Added by Sohel Pathan on 12/03/2015 for PL ticket #192

//Added by kausha somaiya  on 14/10/2015 for ticket no #566
function DisplayIndicator(data, id, symbol, sessionViewByValue) {

    //start indicator
    if (data.IsIndicatorDisplay != null && data.IsIndicatorDisplay != 'undefined' && data.IndicatorCurrentValue != null && data.IndicatorCurrentValue != 'undefined') {
        if (data.IsIndicatorDisplay == true) {

            //start indicator
            var symbolType = '';
            if (symbol != null && symbol != 'undefined') {
                if (symbol == 'currency')
                    symbolType = '$'
                else
                    if (symbol.toLowerCase() == 'percentage' || symbol.toLowerCase() == '%')
                        symbolType = '%'
            }

            if (data.IndicatorCurrentValue != null && data.IndicatorCurrentValue != 'undefined') {
                if (data.IndicatorCurrentValue.toString().indexOf('.') != -1) {
                    var arr = data.IndicatorCurrentValue.toString().split('.');
                    if (arr[1].length > 2)
                        data.IndicatorCurrentValue = parseFloat(data.IndicatorCurrentValue).toFixed(2)
                }
                if (parseFloat(data.IndicatorCurrentValue) > 999)
                    $('#currentValue_' + id).html(number_format(data.IndicatorCurrentValue, 0, '.', ','));
                else
                    $('#currentValue_' + id).html(data.IndicatorCurrentValue);
                if (symbolType == '%')
                    $('#currentValue_' + id).append(symbolType);
                else
                    $('#currentValue_' + id).prepend(symbolType);
            }
            var currentValue = null; var comparisonValue = null; var differenceValue = null;
            if (data.IndicatorComparisonValue != null && data.IndicatorComparisonValue != 'undefined') {


                if (data.IndicatorCurrentValue != null && data.IndicatorCurrentValue != 'undefined')
                    currentValue = parseFloat(data.IndicatorCurrentValue);
                comparisonValue = parseFloat(data.IndicatorComparisonValue);
                if (currentValue != null && comparisonValue != null && currentValue != 0) {

                    differenceValue = parseFloat(currentValue - comparisonValue);
                    if (differenceValue.toString().indexOf('.') != -1) {
                        var arr = differenceValue.toString().split('.');
                        if (arr[1].length > 2)
                            differenceValue = parseFloat(differenceValue).toFixed(2)
                    }
                    //symbol type prefix suffix
                    if (parseFloat(differenceValue) > 999)
                        $('#differenceValue_' + id).html(number_format(differenceValue, 0, '.', ','));
                    else
                        $('#differenceValue_' + id).html(differenceValue);
                    if (symbolType == '%')
                        $('#differenceValue_' + id).append(symbolType);
                    else
                        $('#differenceValue_' + id).prepend(symbolType);

                    if (differenceValue > 0)
                        $('#differenceValue_' + id).css("color", "green");
                    else {
                        if (differenceValue < 0)
                            $('#differenceValue_' + id).css("color", "red");
                    }


                    $('#differenceValue_' + id).css("font-weight", "bold");

                }
                else {
                    if (comparisonValue == 0)
                        $('#differenceValue_' + id).html("0");
                }

            }
            //Goal Value
            //Added by sunita for #566 > 0 condition to hide 0.0 value 
            if (data.IndicatorGoalValue != null && data.IndicatorGoalValue != 'undefined' && data.IndicatorGoalValue > 0) {
                if (parseFloat(data.IndicatorGoalValue) > 999)
                    $('#goalValue_' + id).html(" / " + number_format(data.IndicatorGoalValue, 0, '.', ','));
                else
                    $('#goalValue_' + id).html(" / " + data.IndicatorGoalValue);

                if (data.IndicatorCurrentValue != null && data.IndicatorCurrentValue != 'undefined' && data.IndicatorGoalValue > 0) {
                    if (parseFloat(data.IndicatorCurrentValue) > parseFloat(data.IndicatorGoalValue))
                        $('#currentValue_' + id).css("color", "green");
                    else {
                        if (parseFloat(data.IndicatorCurrentValue) < parseFloat(data.IndicatorGoalValue))
                            $('#currentValue_' + id).css("color", "red");
                    }
                }

            }
            //is indicator display true/false
            $("#divChartIndicator_" + id).css("display", "block");
            var displayName = '';

            if (sessionViewByValue != null && sessionViewByValue != 'undefined') {
                sessionViewByValue = sessionViewByValue.toLowerCase();
                if (sessionViewByValue == "q")
                    displayName = "Quarter";
                else if (sessionViewByValue == "y")
                    displayName = "Year";
                else if (sessionViewByValue == "w")
                    displayName = "Week";
                else if (sessionViewByValue == "m")
                    displayName = "Month";
                else if (sessionViewByValue == "fy")
                    displayName = "Fiscal Year";
                else if (sessionViewByValue == "fq")
                    displayName = "Fiscal Quarter";
                else if (sessionViewByValue == "fm")
                    displayName = "Fiscal Month";

                displayName = "This " + displayName;
            }
            if (data.IndicatorDimensionName != 'undefined' && data.IndicatorDimensionName != null && data.IndicatorDimensionName != '') {
                displayName = data.IndicatorDimensionName;

            }


            $('#viewByValue_' + id).html(displayName);
        }
    }
    //end indicator
}
//end #566 display indicator on chart
//Added by kausha somaiya for ticket no #601,#602(#574)
function DisplayOnTrackOffTrack(id, distributedGoalValue, indicatorCurrentValue, indicatorGoalValue, id) {

    if (distributedGoalValue != null && distributedGoalValue != 'undefined') {


        if (indicatorCurrentValue != null && indicatorCurrentValue != 'undefined' && indicatorGoalValue != null && indicatorGoalValue != 'undefined' && indicatorGoalValue != '0' && distributedGoalValue != '0') {
            var currentValue = parseFloat(indicatorCurrentValue);
            var goalValue = parseFloat(distributedGoalValue);
            if (goalValue > currentValue) {
                $('#distributionSpan_' + id).css("color", "#992233");
                $('#distribution_' + id).addClass('off-indicator');
                $('#distributionSpan_' + id).html('OFF')
            }
            else {
                if (goalValue != 0) {
                    $('#distribution_' + id).addClass('on-indicator');
                    $('#distributionSpan_' + id).html('ON');
                    $('#distributionSpan_' + id).css("color", "#558811");
                }
            }
            $("#divGoalDistribution_" + id).css("display", "block");
        }
    }
}
//Added by sunita for #584 for point 1st  // Commented for #800
//var height = $(window).height() - $("#divDateRange").height() - 200;
//$("#content_1").height(height);

//Added by sunita for #584 for point 1st
//$(window).resize(function () {
    //var height = $(window).height() - $("#divDateRange").height() - 200;
    //$("#content_1").height(height);
//});

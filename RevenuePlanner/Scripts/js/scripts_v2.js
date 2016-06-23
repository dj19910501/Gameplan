//function tooltipformatterBootStrap(budgetValue, cell, container, DecimalPlaces, MagnitudePlaces, symbolPreList) {
//    if (budgetValue) {
//        var decimalValue;
//        var magnitudeValue;
//        if (DecimalPlaces === undefined || DecimalPlaces == null) {
//            decimalValue = 2;
//        }
//        else {
//            decimalValue = DecimalPlaces;
//        }

//        // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
//        if (MagnitudePlaces === undefined || MagnitudePlaces == null) {
//            magnitudeValue = 0;
//        }
//        else {
//            magnitudeValue = MagnitudePlaces;
//        }
//        var isDollarAmout = false;
//        var isPercentAmout = false;
//        var isPostiveSign = false;
//        var isNegativeSign = false;
//        var CorrectBudgetVal = false;
//        if (budgetValue.indexOf('%') > -1) {
//            isPercentAmout = true;
//        }

//        var StrAfterRemExtrChar = [];
//        var SpChar = '';
//        StrAfterRemExtrChar = RemoveExtraCharactersFromString(budgetValue, symbolPreList);
//        budgetValue = StrAfterRemExtrChar[0];
//        SpChar = StrAfterRemExtrChar[1];
//        var remNumber = '';

//        var newremain = '';

//        if (budgetValue.indexOf(".") != -1) {
//            remNumber = budgetValue.substr(budgetValue.indexOf('.'));
//            if (remNumber.length > 2)
//                newremain = remNumber.substring(0, decimalValue + 1);
//        }
//        if (magnitudeValue != 0) {
//            if (budgetValue > 999 && !isNaN(budgetValue)) {
//                CorrectBudgetVal = true;
//                // Start - Added By Nandish Shah for Ticket #635 on 12/10/2015
//                var ActVal = GetAbberiviatedValue(budgetValue, decimalValue, magnitudeValue);
//                if (isPercentAmout) {
//                    $(cell).html(ActVal + ' %');
//                    if (remNumber == '' && DecimalPlaces == null) {
//                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), 0, '.', ',') + ' %');
//                    }
//                    else if (remNumber == '') {
//                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue, '.', ',') + ' %');
//                    }
//                    else {
//                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') + ' %');
//                    }
//                }
//                else {
//                    $(cell).html(SpChar == '' ? ActVal : SpChar + ' ' + ActVal);
//                    if (remNumber == '' && DecimalPlaces == null) {
//                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), 0, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), 0, '.', ','));
//                    }
//                    else if (remNumber == '') {
//                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue, '.', ','));
//                    }
//                    else {
//                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ','));
//                    }
//                }
//                $(cell).addClass('north');
//                $(cell).attr('data-toggle', 'popover');
//                $(cell).popover({
//                    trigger: "hover",
//                    placement: 'bottom',
//                    container: container,
//                    html: true,
//                });
//            }
//            else if (budgetValue < -999 && !isNaN(budgetValue)) {
//                CorrectBudgetVal = true;
//                var ActVal = GetAbberiviatedValue(budgetValue, decimalValue, magnitudeValue);
//                if (isPercentAmout) {
//                    $(cell).html(ActVal + ' %');
//                    if (remNumber == '' && DecimalPlaces == null) {
//                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), 0, '.', ',') + ' %');
//                    }
//                    else if (remNumber == '') {
//                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue, '.', ',') + ' %');
//                    }
//                    else {
//                        $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') + ' %');
//                    }
//                }
//                else {
//                    $(cell).html(SpChar == '' ? ActVal : SpChar + ' ' + ActVal);
//                    if (remNumber == '' && DecimalPlaces == null) {
//                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), 0, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), 0, '.', ','));
//                    }
//                    else if (remNumber == '') {
//                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue, '.', ','));
//                    }
//                    else {
//                        $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ','));
//                    }
//                }
//                $(cell).addClass('north');
//                $(cell).attr('data-toggle', 'popover');
//                $(cell).popover({
//                    trigger: "hover",
//                    placement: 'bottom',
//                    container: container,
//                    html: true
//                });
//            }
//            else {
//                if (!isNaN(budgetValue)) {
//                    CorrectBudgetVal = true;
//                    var ActVal = GetAbberiviatedValue(budgetValue, decimalValue, magnitudeValue);
//                    if (isPercentAmout) {
//                        $(cell).html(ActVal + ' %');
//                        if (remNumber == '' && DecimalPlaces == null) {
//                            $(cell).attr('data-original-title', number_format(budgetValue.toString(), 0, '.', ',') + ' %');
//                        }
//                        else if (remNumber == '') {
//                            $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue, '.', ',') + ' %');
//                        }
//                        else {
//                            $(cell).attr('data-original-title', number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') + ' %');
//                        }
//                    }
//                    else {
//                        $(cell).html(SpChar == '' ? ActVal : SpChar + ' ' + ActVal);
//                        if (remNumber == '' && DecimalPlaces == null) {
//                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), 0, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), 0, '.', ','));
//                        }
//                        else if (remNumber == '') {
//                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue, '.', ','));
//                        }
//                        else {
//                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ','));
//                        }
//                    }
//                    $(cell).addClass('north');
//                    $(cell).attr('data-toggle', 'popover');
//                    $(cell).popover({
//                        trigger: "hover",
//                        placement: 'bottom',
//                        container: container,
//                        html: true
//                    });
//                }
//                else {
//                    CorrectBudgetVal = false;
//                    $(cell).html(budgetValue);
//                    $(cell).attr('data-original-title', budgetValue);
//                    $(cell).addClass('north');
//                    $(cell).attr('data-toggle', 'popover');
//                    $(cell).popover({
//                        trigger: "hover",
//                        placement: 'bottom',
//                        container: container,
//                        html: true
//                    });
//                }
//            }
//            if (!CorrectBudgetVal && !isNaN(budgetValue)) {
//                if (isPercentAmout) {
//                    $(cell).html(budgetValue.replace(/ /g, '') + ' %');
//                    $(cell).attr('data-original-title', number_format(budgetValue.replace(/ /g, '').toString(), decimalValue, '.', ',') + ' %');
//                }
//                else {
//                    $(cell).html(SpChar == '' ? budgetValue.replace(/ /g, '') : SpChar + ' ' + budgetValue.replace(/ /g, ''));
//                    $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue.replace(/ /g, '').toString(), decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue.replace(/ /g, '').toString(), decimalValue, '.', ','));
//                }
//                $(cell).addClass('north');
//                $(cell).attr('data-toggle', 'popover');
//                $(cell).popover({
//                    trigger: "hover",
//                    placement: 'bottom',
//                    container: container,
//                    html: true
//                });
//            }
//        }
//        else {
//            if (isNaN(budgetValue)) {
//                if (isPercentAmout) {
//                    $(cell).html(budgetValue + ' %');
//                    $(cell).attr('data-original-title', budgetValue + ' %');
//                }
//                else {
//                    $(cell).html(SpChar == '' ? budgetValue : SpChar + ' ' + budgetValue);
//                    $(cell).attr('data-original-title', SpChar == '' ? budgetValue : SpChar + ' ' + budgetValue);
//                }
//            }
//            else {
//                if (isPercentAmout) {
//                    if (DecimalPlaces == null) {
//                        if (remNumber == '') {
//                            $(cell).html(number_format(budgetValue, 0, '.', ',') + ' %');
//                            $(cell).attr('data-original-title', number_format(budgetValue, 0, '.', ',') + ' %');
//                        }
//                        else {
//                            $(cell).html(number_format(budgetValue, decimalValue, '.', ',') + ' %');
//                            $(cell).attr('data-original-title', number_format(budgetValue, decimalValue, '.', ',') + ' %');
//                        }
//                    }
//                    else {
//                        $(cell).html(number_format(budgetValue, decimalValue, '.', ',') + ' %');
//                        $(cell).attr('data-original-title', number_format(budgetValue, decimalValue, '.', ',') + ' %');
//                    }
//                }
//                else {
//                    if (DecimalPlaces == null) {
//                        if (remNumber == '') {
//                            $(cell).html(SpChar == '' ? number_format(budgetValue, 0, '.', ',') : SpChar + ' ' + number_format(budgetValue, 0, '.', ','));
//                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue, 0, '.', ',') : SpChar + ' ' + number_format(budgetValue, 0, '.', ','));
//                        }
//                        else {
//                            $(cell).html(SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
//                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
//                        }
//                    }
//                    else {
//                        $(cell).html(SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
//                        if (remNumber == '') {
//                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
//                        }
//                        else {
//                            $(cell).attr('data-original-title', SpChar == '' ? number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + number_format(budgetValue, decimalValue, '.', ','));
//                        }
//                    }
//                }
//            }
//            $(cell).addClass('north');
//            $(cell).attr('data-toggle', 'popover');
//            $(cell).popover({
//                trigger: "hover",
//                placement: 'bottom',
//                container: container,
//                html: true
//            });
//        }
//    }
//}
//function SetLabelFormaterWithoutTipsy(obj) {
//    try {
//        var isAmount = false;
//        var budgetValue = $(obj).html();
//        if (budgetValue.indexOf("$") != -1) {
//            isAmount = true;
//        }

//        if (budgetValue) { //Check whether the number is empty or not
//            var StrAfterRemExtrChar = [];
//            StrAfterRemExtrChar = RemoveExtraCharactersFromString(budgetValue); //Function that remove the special char from the string 
//            budgetValue = StrAfterRemExtrChar[0];
//            if (budgetValue.length >= 4) {
//                SetFormatForLabelExtendedWithoutTipsy(obj);
//                //$(obj).html('$' + $(obj).html());
//                var remNumber = '';
//                if (budgetValue.indexOf(".") != -1) {
//                    remNumber = budgetValue.substr(budgetValue.indexOf('.'));
//                }
//            }
//        }
//    }
//    catch (err) {
//        if (arguments != null && arguments.callee != null && arguments.callee.trace)
//            logError(err, arguments.callee.trace());
//        //  hidePleaseWaitDialog();
//    }
//}
//function SetFormatForLabelExtendedWithoutTipsy(obj) {

//    try {
//        var isAmount = false;
//        var txtvalue = $(obj).text();
//        txtvalue = ConvertToNum(txtvalue);

//        if (txtvalue.indexOf("$") != -1) {
//            isAmount = true;
//        }
//        if (txtvalue) {
//            var StrAfterRemExtrChar = [];
//            StrAfterRemExtrChar = RemoveExtraCharactersFromString(txtvalue); //Function that remove the special char from the string 
//            txtvalue = StrAfterRemExtrChar[0];
//            if (txtvalue.indexOf('.') != -1) {
//                txtvalue = txtvalue.substring(0, txtvalue.indexOf('.'))
//            }
//            if (isAmount) {
//                $(obj).text('$' + GetAbberiviatedValue(txtvalue));
//                //$(obj).attr('title', '$' + txtvalue);
//            }
//            else {
//                $(obj).text(GetAbberiviatedValue(txtvalue));
//                //$(obj).attr('title', txtvalue);
//            }
//        }
//    }
//    catch (err) {
//        if (arguments != null && arguments.callee != null && arguments.callee.trace)
//            logError(err, arguments.callee.trace());
//        //hidePleaseWaitDialog();
//    }
//}
//function SetLabelFormaterWithTipsy(obj) {
//    try {
//        var isAmount = false;
//        var budgetValue = $(obj).html();
//        if (budgetValue.indexOf("$") != -1) {
//            isAmount = true;
//        }

//        if (budgetValue) { //Check whether the number is empty or not
//            var StrAfterRemExtrChar = [];
//            StrAfterRemExtrChar = RemoveExtraCharactersFromString(budgetValue); //Function that remove the special char from the string 
//            budgetValue = StrAfterRemExtrChar[0];
//            if (budgetValue.length >= 4) {
//                SetFormatForLabelExtended(obj);
//                //$(obj).html('$' + $(obj).html());
//                var remNumber = '';
//                if (budgetValue.indexOf(".") != -1) {
//                    remNumber = budgetValue.substr(budgetValue.indexOf('.'));
//                }

//                //Add tipsy for the current label.
//                $(obj).attr('title', budgetValue);
//                if (isAmount) {
//                    $(obj).prop('title', "$" + number_format(budgetValue.toString(), 0, '.', ',') + remNumber);
//                }
//                else {
//                    $(obj).prop('title', number_format(budgetValue.toString(), 0, '.', ',') + remNumber);
//                }

//                $(obj).addClass('north');
//                $('.north').tipsy({ gravity: 'n' });
//            }
//        }
//    }
//    catch (err) {
//        if (arguments != null && arguments.callee != null && arguments.callee.trace)
//            logError(err, arguments.callee.trace());
//        // hidePleaseWaitDialog();
//    }
//}
//function SetFormatForLabelExtended(obj) {
//    try {
//        var isAmount = false;
//        var txtvalue = $(obj).text();
//        if (txtvalue.indexOf("$") != -1) {
//            isAmount = true;
//        }
//        if (txtvalue) {
//            var StrAfterRemExtrChar = [];
//            StrAfterRemExtrChar = RemoveExtraCharactersFromString(txtvalue); //Function that remove the special char from the string 
//            txtvalue = StrAfterRemExtrChar[0];
//            if (txtvalue.indexOf('.') != -1) {
//                txtvalue = txtvalue.substring(0, txtvalue.indexOf('.'))
//            }
//            if (isAmount) {
//                $(obj).text('$' + GetAbberiviatedValue(txtvalue));
//                $(obj).attr('title', '$' + txtvalue);
//            }
//            else {
//                $(obj).text(GetAbberiviatedValue(txtvalue));
//                $(obj).attr('title', txtvalue);
//            }
//            $(obj).addClass('north');
//            //  $('.north').tipsy({ gravity: 'n' });
//        }
//    }
//    catch (err) {
//        if (arguments != null && arguments.callee != null && arguments.callee.trace)
//            logError(err, arguments.callee.trace());
//        //  hidePleaseWaitDialog();
//    }
//}
//function ReturnFormatValue(txtvalue) {
//    try {
//        var isAmount = false;
//        if (txtvalue.indexOf("$") != -1) {
//            isAmount = true;
//        }
//        if (txtvalue) {
//            var StrAfterRemExtrChar = [];
//            StrAfterRemExtrChar = RemoveExtraCharactersFromString(txtvalue); //Function that remove the special char from the string 
//            txtvalue = StrAfterRemExtrChar[0];
//            if (txtvalue.indexOf('.') != -1) {
//                txtvalue = txtvalue.substring(0, txtvalue.indexOf('.'))
//            }
//            if (isAmount) {
//                txtvalue = '$' + GetAbberiviatedValue(txtvalue);
//            }
//            else {
//                txtvalue = GetAbberiviatedValue(txtvalue);
//            }
//            //$(obj).addClass('north');
//            //$('.north').tipsy({ gravity: 'n' });
//        }
//        return txtvalue;
//    }
//    catch (err) {
//        if (arguments != null && arguments.callee != null && arguments.callee.trace)
//            logError(err, arguments.callee.trace());
//        // hidePleaseWaitDialog();
//    }
//}
//function RemoveExtraCharactersFromString(value, SpCharArr) {
//    var symbolPreList = [];
//    var SpChar = '';
//    if (SpCharArr === undefined || SpCharArr == null) {
//        symbolPreList = ['$', '₭', '%', 'NA', '£', '₹', 'Y', '¥'];
//    }
//    else {
//        symbolPreList = SpCharArr;
//    }
//    try {
//        for (var i = 0; i < symbolPreList.length; i++) {
//            if (value.indexOf(symbolPreList[i]) != -1) {
//                value = value.replace(symbolPreList[i], "");
//                SpChar = symbolPreList[i];
//                //break;
//            }
//        }
//        value = value.replace(/[\,]+/g, "");
//        return [value, SpChar];
//    }
//    catch (err) {
//        if (arguments != null && arguments.callee != null && arguments.callee.trace)
//            logError(err, arguments.callee.trace());
//        //hidePleaseWaitDialog();
//    }
//}
//function ConvertToNum(value) {
//    if (isNaN(value)) {
//        if (value != null) {
//            var str = value.toString();
//            if (str.indexOf(":") >= 0) {
//                str = str.substring(str.indexOf(":") + 1, str.length);
//                return str;
//            }
//        }
//    }
//    return value;
//}
//function FormatAllValues(className) {
//    try {
//        $('.' + className).each(function (itm, index) {
//            if ($(index).html().indexOf('%') == -1) {
//                SetLabelFormaterWithTipsy($(index));
//            }
//            else {
//                $(index).html($(index).html().replace('%', ''));
//                SetLabelFormaterWithTipsy($(index));
//                $(index).html($(index).html() + ' %');
//            }
//        });
//    }
//    catch (err) {
//        if (arguments != null && arguments.callee != null && arguments.callee.trace)
//            logError(err, arguments.callee.trace());
//        //hidePleaseWaitDialog();

//    }
//}
//function PDFExport(graph) {
//    var graphId = graph.renderTo.id;
//    var chart = $('#' + graphId).highcharts();
//    var ComparisonFlagEnabled = IsComparisonFlagEnabled.toLowerCase() == 'true' ? true : false;
//    var DashComparisonVisible = IsDashComparisonVisible.toLowerCase() == 'true' ? true : false;
//    if (ComparisonFlagEnabled && DashComparisonVisible) {
//        chart.options.title.text = 'Date Range : ' + DateRange + '<br /> Comparison : ' + ComparisionDate;
//    }
//    else {
//        chart.options.title.text = 'Date Range : ' + DateRange;
//    }
//    chart.exportChart({
//        url: ExportServerURL,
//        type: 'application/pdf',
//        filename: (chart.options.title.text == null || chart.options.title.text.indexOf('Date Range :') > -1) ? GetChartHeaderForExport(graph) : chart.options.title.text + ' ' + getCurrentDateTimeString()
//    });
//}
//function getCurrentDateTimeString() {
//    var currentdate = new Date();
//    var pad = '00';
//    var datetime = (pad + currentdate.getDate().toString()).slice(-pad.length)
//                    + (pad + (currentdate.getMonth() + 1).toString()).slice(-pad.length)
//                    + currentdate.getFullYear().toString() + "_"
//                    + (pad + currentdate.getHours().toString()).slice(-pad.length)
//                    + (pad + currentdate.getMinutes().toString()).slice(-pad.length)
//                    + (pad + currentdate.getSeconds().toString()).slice(-pad.length);
//    return datetime;
//}
function ME_GetAbberiviatedValue(value, DecimalPlaces, MagnitudePlaces) {
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
function ME_number_format(number, decimals, dec_point, thousands_sep) {
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
function ME_GetSymbolforValues(value, symbolType) {

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

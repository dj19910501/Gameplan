function ME_tooltipformatterBootStrap(budgetValue, cell, container, DecimalPlaces, MagnitudePlaces, symbolPreList) {
    if (budgetValue) {
        var decimalValue;
        var magnitudeValue;
        if (DecimalPlaces === undefined || DecimalPlaces == null) {
            decimalValue = 2;
        }
        else {
            decimalValue = DecimalPlaces;
        }

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
        StrAfterRemExtrChar = ME_RemoveExtraCharactersFromString(budgetValue, symbolPreList);
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
                var ActVal = ME_GetAbberiviatedValue(budgetValue, decimalValue, magnitudeValue);
                if (isPercentAmout) {
                    $(cell).html(ActVal + ' %');
                    if (remNumber == '' && DecimalPlaces == null) {
                        $(cell).attr('data-original-title', ME_number_format(budgetValue.toString(), 0, '.', ',') + ' %');
                    }
                    else if (remNumber == '') {
                        $(cell).attr('data-original-title', ME_number_format(budgetValue.toString(), decimalValue, '.', ',') + ' %');
                    }
                    else {
                        $(cell).attr('data-original-title', ME_number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') + ' %');
                    }
                }
                else {
                    $(cell).html(SpChar == '' ? ActVal : SpChar + ' ' + ActVal);
                    if (remNumber == '' && DecimalPlaces == null) {
                        $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.toString(), 0, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.toString(), 0, '.', ','));
                    }
                    else if (remNumber == '') {
                        $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.toString(), decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.toString(), decimalValue, '.', ','));
                    }
                    else {
                        $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ','));
                    }
                }
                //$(cell).addClass('north');
                //$(cell).attr('data-toggle', 'popover');
                //$(cell).popover({
                //    trigger: "hover",
                //    placement: 'bottom',
                //    container: container,
                //    html: true,
                //});
            }
            else if (budgetValue < -999 && !isNaN(budgetValue)) {
                CorrectBudgetVal = true;
                var ActVal = ME_GetAbberiviatedValue(budgetValue, decimalValue, magnitudeValue);
                if (isPercentAmout) {
                    $(cell).html(ActVal + ' %');
                    if (remNumber == '' && DecimalPlaces == null) {
                        $(cell).attr('data-original-title', ME_number_format(budgetValue.toString(), 0, '.', ',') + ' %');
                    }
                    else if (remNumber == '') {
                        $(cell).attr('data-original-title', ME_number_format(budgetValue.toString(), decimalValue, '.', ',') + ' %');
                    }
                    else {
                        $(cell).attr('data-original-title', ME_number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') + ' %');
                    }
                }
                else {
                    $(cell).html(SpChar == '' ? ActVal : SpChar + ' ' + ActVal);
                    if (remNumber == '' && DecimalPlaces == null) {
                        $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.toString(), 0, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.toString(), 0, '.', ','));
                    }
                    else if (remNumber == '') {
                        $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.toString(), decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.toString(), decimalValue, '.', ','));
                    }
                    else {
                        $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ','));
                    }
                }
                //$(cell).addClass('north');
                //$(cell).attr('data-toggle', 'popover');
                //$(cell).popover({
                //    trigger: "hover",
                //    placement: 'bottom',
                //    container: container,
                //    html: true
                //});
            }
            else {
                if (!isNaN(budgetValue)) {
                    CorrectBudgetVal = true;
                    var ActVal = ME_GetAbberiviatedValue(budgetValue, decimalValue, magnitudeValue);
                    if (isPercentAmout) {
                        $(cell).html(ActVal + ' %');
                        if (remNumber == '' && DecimalPlaces == null) {
                            $(cell).attr('data-original-title', ME_number_format(budgetValue.toString(), 0, '.', ',') + ' %');
                        }
                        else if (remNumber == '') {
                            $(cell).attr('data-original-title', ME_number_format(budgetValue.toString(), decimalValue, '.', ',') + ' %');
                        }
                        else {
                            $(cell).attr('data-original-title', ME_number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') + ' %');
                        }
                    }
                    else {
                        $(cell).html(SpChar == '' ? ActVal : SpChar + ' ' + ActVal);
                        if (remNumber == '' && DecimalPlaces == null) {
                            $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.toString(), 0, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.toString(), 0, '.', ','));
                        }
                        else if (remNumber == '') {
                            $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.toString(), decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.toString(), decimalValue, '.', ','));
                        }
                        else {
                            $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.toString(), decimalValue == 0 ? 2 : decimalValue, '.', ','));
                        }
                    }
                    //$(cell).addClass('north');
                    //$(cell).attr('data-toggle', 'popover');
                    //$(cell).popover({
                    //    trigger: "hover",
                    //    placement: 'bottom',
                    //    container: container,
                    //    html: true
                    //});
                }
                else {
                    CorrectBudgetVal = false;
                    $(cell).html(budgetValue);
                    $(cell).attr('data-original-title', budgetValue);
                    //$(cell).addClass('north');
                    //$(cell).attr('data-toggle', 'popover');
                    //$(cell).popover({
                    //    trigger: "hover",
                    //    placement: 'bottom',
                    //    container: container,
                    //    html: true
                    //});
                }
            }
            if (!CorrectBudgetVal && !isNaN(budgetValue)) {
                if (isPercentAmout) {
                    $(cell).html(budgetValue.replace(/ /g, '') + ' %');
                    $(cell).attr('data-original-title', ME_number_format(budgetValue.replace(/ /g, '').toString(), decimalValue, '.', ',') + ' %');
                }
                else {
                    $(cell).html(SpChar == '' ? budgetValue.replace(/ /g, '') : SpChar + ' ' + budgetValue.replace(/ /g, ''));
                    $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue.replace(/ /g, '').toString(), decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue.replace(/ /g, '').toString(), decimalValue, '.', ','));
                }
                //$(cell).addClass('north');
                //$(cell).attr('data-toggle', 'popover');
                //$(cell).popover({
                //    trigger: "hover",
                //    placement: 'bottom',
                //    container: container,
                //    html: true
                //});
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
                            $(cell).html(ME_number_format(budgetValue, 0, '.', ',') + ' %');
                            $(cell).attr('data-original-title', ME_number_format(budgetValue, 0, '.', ',') + ' %');
                        }
                        else {
                            $(cell).html(ME_number_format(budgetValue, decimalValue, '.', ',') + ' %');
                            $(cell).attr('data-original-title', ME_number_format(budgetValue, decimalValue, '.', ',') + ' %');
                        }
                    }
                    else {
                        $(cell).html(ME_number_format(budgetValue, decimalValue, '.', ',') + ' %');
                        $(cell).attr('data-original-title', ME_number_format(budgetValue, decimalValue, '.', ',') + ' %');
                    }
                }
                else {
                    if (DecimalPlaces == null) {
                        if (remNumber == '') {
                            $(cell).html(SpChar == '' ? ME_number_format(budgetValue, 0, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue, 0, '.', ','));
                            $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue, 0, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue, 0, '.', ','));
                        }
                        else {
                            $(cell).html(SpChar == '' ? ME_number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue, decimalValue, '.', ','));
                            $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue, decimalValue, '.', ','));
                        }
                    }
                    else {
                        $(cell).html(SpChar == '' ? ME_number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue, decimalValue, '.', ','));
                        if (remNumber == '') {
                            $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue, decimalValue, '.', ','));
                        }
                        else {
                            $(cell).attr('data-original-title', SpChar == '' ? ME_number_format(budgetValue, decimalValue, '.', ',') : SpChar + ' ' + ME_number_format(budgetValue, decimalValue, '.', ','));
                        }
                    }
                }
            }
            //$(cell).addClass('north');
            //$(cell).attr('data-toggle', 'popover');
            //$(cell).popover({
            //    trigger: "hover",
            //    placement: 'bottom',
            //    container: container,
            //    html: true
            //});
        }
    }
}
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
function ME_RemoveExtraCharactersFromString(value, SpCharArr) {
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
            }
        }
        value = value.replace(/[\,]+/g, "");
        return [value, SpChar];
    }
    catch (err) {
        if (arguments != null && arguments.callee != null && arguments.callee.trace)
            logError(err, arguments.callee.trace());
    }
}
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
                value = (value * 100) + "";
                if (value.indexOf('.') != -1) {
                    var arr = value.split('.');
                    if (arr[1].length > 2)
                        value = parseFloat(value).toFixed(0) + ""
                }
                value = value + "%"
            }
        }
        else {
            value = symbolType + value;
        }
    }


    return value;
}
//Added Following function to load graph,table and view data.
function LoadReport(obj, applicationCode, errorMsg, apiUrlNotConfigured, ApiUrl, dashId, dashPageId, msgNoDataAvailable, msgGraphNotConfigure,isViewData) {
   
    var IsGraph = true;
    var id = $(obj).attr('ReportGraphId');
    var DashboardContentId = $(obj).attr('dashboardcontentid');
    if (Number(id) == 0) {
        id = $(obj).attr('ReportTableId');
        IsGraph = false;
    }
    var ViewBy = $("#ddlViewBy").val();

    var StartDate = '';
    var EndDate = '';
    $('div[id=reportrange]').each(function () {
        StartDate = ($(this).data('daterangepicker').startDate.format("MM/DD/YYYY"));
        EndDate = ($(this).data('daterangepicker').endDate.format("MM/DD/YYYY"));
    });

    var SelectedDimensionValue = GetSelectedValues();
    var URL;
    if (ApiUrl == '') {
        $(obj).html(apiUrlNotConfigured);
        //Following method is used to Show/Hide viewAll icon.
        ManageViewAllIcon(obj, apiUrlNotConfigured);
    }
    else {
        var params = {};
        params.Id = id;
        params.DbName = "RPC";
        params.Container = $(obj).attr('id');
        params.SDV = SelectedDimensionValue;
        params.TopOnly = 'True';
        params.ViewBy = ViewBy;
        params.StartDate = StartDate;
        params.EndDate = EndDate;
        //Using Following Parameter we can check method is called from viewdata or not 
        params.IsViewData = isViewData;
        if (IsGraph) {
            URL = urlContent+"MeasureDashboard/GetChart/";
        }
        else {
            params.DashboardId = dashId;
            params.DashboardPageid = dashPageId;
            params.DashboardContentId = DashboardContentId;
            URL = urlContent + "MeasureDashboard/GetReportTable/";
        }

        $.ajax({
            url: URL,
            async: true,
            traditional: true,
            data: $.param(params, true),
            dataType: "json",
            success: function (data) {

                if (IsGraph) {
                    eval(JSON.parse(data.data));
                }
                else {
                    LoadReportTable(DashboardContentId, "divChart", data, "ReportTable", "_wrapper", true);
                }
                //Following method is used to Show/Hide viewAll icon.
                ManageViewAllIcon(obj, msgNoDataAvailable, msgGraphNotConfigure);
            },
            error: function (err) {
                //Following method is used to Show/Hide viewAll icon.
                ManageViewAllIcon(obj,err);
                $(obj).html(err);
            }
        });
    }
}
//Following method is used to manage(Show/Hide) viewAll icon.(Ex: if nessage will be "No Data Availble" or "Chart Type is not supported then" then ViewAll icon will not be appear)
function ManageViewAllIcon(obj,msgNoDataAvailable, msgGraphNotConfigure) {
  
    var id = $(obj).attr('ReportGraphId');

    if (Number(id) == 0) {
        id = $(obj).attr('ReportTableId');
    }
    if (obj != null || obj!= 'undefined') {
        if ($(obj).html().indexOf(msgNoDataAvailable) > 0 || $(obj).text().indexOf(msgGraphNotConfigure) > 0) {

            $("#" + id).hide();
        }
        else
            $("#" + id).show();
    }
}
var symbolPreListDefault = ['$', '₭', ' %', 'NA', '£', '₹', 'Y', '¥'];
//Following function is created to bind report table in to dhtmlx grid.
function LoadReportTable(DashboardContentId, divName, data, tableType, wrapperName, isReportTable) {
  
    $('#' + divName + DashboardContentId).html(data);
    var reportTableId = tableType + DashboardContentId;
    var defaultRows = $('#hdn_' + reportTableId).attr('defaultRows');
    var defaultSortColumn = $('#hdn_' + reportTableId).attr('defaultSortColumn');
    var defaultSortOrder = $('#hdn_' + reportTableId).attr('defaultSortOrder');
    if (defaultSortOrder != 'asc')
        defaultSortOrder = 'des';
    var ShowFooterRow = $('#hdn_' + reportTableId).attr('ShowFooterRow');
    var ReportSymbols = $('#hdn_' + reportTableId).attr('ReportSymbols');
    var symbolPreList = [];
    if (ReportSymbols != undefined && ReportSymbols != 'undefined' && ReportSymbols != "") {
        symbolPreList = ReportSymbols.split(',');
    }
    
    if (symbolPreList.length <= 1 && (symbolPreList[0] == "" || symbolPreList[0] == 'undefined')) {
        symbolPreList = symbolPreListDefault;
    }
    if ($.inArray('%', symbolPreList) == -1) {
        symbolPreList.push('%');
    }
    var TotalDecimalPlaces = $('#hdn_' + reportTableId).attr('TotalDecimalPlaces');
    var DecimalPreList = [];
    if (TotalDecimalPlaces != undefined && TotalDecimalPlaces != 'undefined' && TotalDecimalPlaces != "") {
        DecimalPreList = TotalDecimalPlaces.split(',');
    }

    $('#' + reportTableId + wrapperName).css('overflow-x', 'auto');
    $('#' + reportTableId + wrapperName).css('overflow-y', 'auto');
    $('#' + reportTableId + wrapperName).css('height', '100%');

    if (data.indexOf(reportTableId) > -1) {
        var mygrid = dhtmlXGridFromTable(reportTableId);

        $('#' + reportTableId + wrapperName).find('.objbox').find('table').attr('id', reportTableId + '_tbl');

        var tblTotal = $(mygrid.obj).find('.totalRow').clone();
        $(mygrid.obj).find('tbody').find('.totalRow').remove();

        if (ShowFooterRow == 'True') {
            var foot = $(mygrid.obj).find('tfoot');
            if (!foot.length) foot = $('<tfoot>').appendTo(mygrid.obj);
            foot.append($(tblTotal));
        }
        var columnCheckCount = 0;
        if (isReportTable)
            columnCheckCount = 1;
        $('#' + reportTableId).find('.objbox').find('table').find('tr').each(function (i, row) {
            $(row).find('td').each(function (j, cell) {
                if (j > columnCheckCount) {
                    try {
                        var originalValue = " " + $(cell).text();
                        if (Number(DecimalPreList[j]) > -1) {
                            ME_tooltipformatterBootStrap(originalValue, cell, 'body', DecimalPreList[j], null, symbolPreList);
                        }
                        else {
                            ME_tooltipformatterBootStrap(originalValue, cell, 'body', null, null, symbolPreList);
                        }
                        for (var i = 0; i < symbolPreList.length; i++) {
                            originalValue = originalValue.replace(symbolPreList[i], '');
                        }
                        $(cell).removeAttr('title');
                        $(cell).attr('data-sort', originalValue);

                        $(cell).addClass('north');
                        $(cell).attr('title', $(cell).attr('data-original-title'));
                        $(".north").tooltip({
                            trigger: "hover",
                            container: 'body',
                            placement: 'bottom',
                            html: true,
                        });
                    } catch (e) {
                    }

                }
            });
        });

        mygrid.setEditable(false);
        mygrid.enableAutoHeight(true);
        if (isReportTable)
            mygrid.setColumnHidden(1, true);

        $('#' + reportTableId).find('tfoot').find('.totalRow').find('td').each(function (j, cell) { if (j == 1) $(this).css('display', 'none'); });
        $('#' + reportTableId).find('tfoot').find('.goalRow').find('td').each(function (j, cell) {
            if (j == 1) {
                $(this).css('display', 'none');
            }
            else {
                if ($(cell).text() == "0" || $(cell).text().trim() == "") {
                    $(this).text('');
                    $(this).removeAttr('data-toggle');
                    $(this).removeAttr('data-original-title');
                    $(this).removeAttr('data-sort');
                }
            }
        });

        mygrid.enableAutoWidth(true);

        var count = mygrid.getColumnsNum();
        for (var j = 0; j < count; j++) {
            mygrid.adjustColumnSize(j);
        }
     
        $('#' + reportTableId).css('margin', '0 auto');
        $('#' + reportTableId).css('width', '100%');
        $('#' + reportTableId).find('.xhdr').css('width', '100%');
        $('#' + reportTableId).find('.hdr').css('width', '100%');

        var widthArray = [];
        $('#' + reportTableId).find('.xhdr table' + ' tr:first').find('th').each(function (key, data) {
            $('#' + reportTableId).find('.objbox table' + ' tr:first').find('th').each(function (key1, data1) {
                if (key == key1) {
                    var totalWt = $(data).width();
                    widthArray.push(totalWt);
                }
            });
        });
        $('#' + reportTableId).find('.objbox table' + ' tr:first').find('th').each(function (j, data1) {
            for (var i = 0; i < widthArray.length; i++) {
                if (j == i) {
                    mygrid.setColWidth(j, widthArray[i]);
                }
            }
        });

        mygrid.attachEvent("onMouseOver", function () { return false; });
        $('#' + reportTableId).find('tfoot').find('.totalRow').css('display', 'table-row');

        var parenttblHeight = $('#' + reportTableId).find('.objbox').find('table').height();
        var childtblHeight = $('#' + reportTableId).find('.xhdr').find('table').height();
        $('#' + reportTableId).height(parenttblHeight + childtblHeight);

        mygrid.attachEvent("onAfterSorting", function (index, type, direction) {
            $(mygrid.obj).find('tbody').find('.totalRow').remove();
            mygrid.setSortImgState(false);
            return true;
        });
    }
}
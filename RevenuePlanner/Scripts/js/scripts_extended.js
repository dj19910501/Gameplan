function number_format(number, decimals, dec_point, thousands_sep) {
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

// Check Date is in Valid Format Or Not
function isDate(txtDate) {
    var currVal = txtDate;
    if (currVal == '')
        return false;

    var rxDatePattern = /^(\d{1,2})(\/)(\d{1,2})(\/)(\d{4})$/; //Declare Regex
    var dtArray = currVal.match(rxDatePattern); // is format OK?
    if (dtArray == null)
        return false;
    //Checks for mm/dd/yyyy format.
    dtDay = dtArray[3];
    dtMonth = dtArray[1];
    dtYear = dtArray[5];
    if (dtDay < 1 || dtDay > 31)
        return false;
    else if (dtMonth < 1 || dtMonth > 12)
        return false;
    else if ((dtMonth == 4 || dtMonth == 6 || dtMonth == 9 || dtMonth == 11) && dtDay == 31)
        return false;
    else if (dtMonth == 2) {
        var isleap = (dtYear % 4 == 0 && (dtYear % 100 != 0 || dtYear % 400 == 0));
        if (dtDay > 29 || (dtDay == 29 && !isleap))
            return false;
    }
    return true;
}

function validateDateCompare(sdate, edate, msg) {
        sdate = sdate.split(" ")[0];
        edate = edate.split(" ")[0];
        date1 = new Date(sdate.split("/")[2], sdate.split("/")[0], sdate.split("/")[1]);
        date2 = new Date(edate.split("/")[2], edate.split("/")[0], edate.split("/")[1]);
        if (date1 > date2) {
            alert(msg);
            return false;
        }
        return true;
}

function FormatCurrency(amount, showDecimals) {
    /// Modified By Maninder Singh Wadhva PL Ticket#47
    if (isNaN(amount)) {
        return amount;
    }

    if (showDecimals == null)
        showDecimals = true;
    var i = parseFloat(amount);
    if (isNaN(i)) { i = 0.00; }
    var minus = false;
    if (i < 0) { minus = true; }
    i = Math.abs(i);
    i = parseInt((i + .005) * 100);
    i = i / 100;
    s = new String(i);
    if (showDecimals) {
        if (s.indexOf('.') < 0) { s += '.00'; }
        if (s.indexOf('.') == (s.length - 2)) { s += '0'; }
    }
    //s = minus + s;
    s = '$' + FormatCommas(s, showDecimals);
    if (minus)
        s = "(" + s + ")";
    return s;
}
function FormatCommas(amount, showDecimals) {
    if (showDecimals == null)
        showDecimals = true;
    var delimiter = ","; // replace comma if desired
    var a = amount.split('.', 2)
    var d = a[1];
    var i = parseInt(a[0]);
    if (isNaN(i)) { return ''; }
    var minus = '';
    if (i < 0) { minus = '-'; }
    i = Math.abs(i);
    var n = new String(i);
    var a = [];
    while (n.length > 3) {
        var nn = n.substr(n.length - 3);
        a.unshift(nn);
        n = n.substr(0, n.length - 3);
    }
    if (n.length > 0) { a.unshift(n); }
    n = a.join(delimiter);
    if (!showDecimals) {
        amount = n;
    }
    else {
        if (isNaN(d) || d.length < 1) { amount = n; }
        else { amount = n + '.' + d; }
    }
    amount = minus + amount;
    return amount;
}

function ReplaceCC(text) {
    return text.trim().replace(/,/g, '').replace('$', '');
}

function SetBudget(idName) {
    $(idName).html($(idName).html().replace("$", "").trim());
    $(idName).html("$ " + number_format($(idName).html(), 0, '.', ','));
    var budgetValue = $(idName).html();
    if (budgetValue.length >= 9) {
        $(idName).html($(idName).html().substring(0, 7) + "..");
        $(idName).prop('title', budgetValue);
        $(idName).addClass('north');
        $('.north').tipsy({ gravity: 'n' });
    }
    else {
        $(idName).removeAttr('original-title');
        $(idName).removeClass('north');
    }
}

function SetPriceValue(idName) {
    $(idName).html(number_format($(idName).html(), 0, '.', ','));
    var budgetValue = $(idName).html();
    if (budgetValue.length >= 10) {
        $(idName).html($(idName).html().substring(0, 8) + "..");
        $(idName).prop('title', budgetValue);
        $(idName).addClass('north');
        $('.north').tipsy({ gravity: 'n' });
    }
    else {
        $(idName).removeAttr('original-title');
        $(idName).removeClass('north');
    }
}

function getblurvalue(sender) {

    $(".nl-field-go").click();
}

function SetFormatForLabel(lableId, maxSize)
{
    var txtvalue = $(lableId).text();
    var lengthvalue = txtvalue.length;
    if (lengthvalue >= maxSize) {
        var firstString;
        var lastString;
        var SK = "k";
        var SM = "M";
        var SB = "B";
        switch (lengthvalue) {
            case 5:
                firstString = txtvalue.substring(0, 2);
                lastString = SK;
                break;
            case 6:
                firstString = txtvalue.substring(0, 3);
                lastString = SK;
                break;
            case 7:
                firstString = txtvalue.substring(0, 1);
                lastString = SM;
                break;
            case 8:
                firstString = txtvalue.substring(0, 2);
                lastString = SM;
                break;
            case 9:
                firstString = txtvalue.substring(0, 3);
                lastString = SM;
                break;
            case 10:
                firstString = txtvalue.substring(0, 1);
                lastString = SB;
                break;
            case 11:
                firstString = txtvalue.substring(0, 2);
                lastString = SB;
                break;
            case 12:
                firstString = txtvalue.substring(0, 3);
                lastString = SB;
                break;
        }
        $(lableId).text(firstString + lastString);
        $(lableId).attr('title', txtvalue);
        $(lableId).addClass('north');
        $('.north').tipsy({ gravity: 's' });
    }
    else {
        $(lableId).removeAttr('original-title');
        $(lableId).removeClass('north');
    }
}

function CheckDateYear(sdate, hdnYear, msg) {
    sdate = sdate.split(" ")[0];
    var sYear = sdate.split("/")[2];
    if (sYear != hdnYear) {
        alert(msg);
        return false;
    }
    return true;
}

function today() {
    var d = new Date();
    var curr_date = d.getDate();
    var curr_month = d.getMonth() + 1;
    var curr_year = d.getFullYear();
    document.write(curr_date + "/" + curr_month + "/" + curr_year);
}
function FormatPercent(amount, showDecimals) {
    amount = amount + '%';
    return amount;
}

//// Added By Maninder Singh 
//// Date 1/28/2014
//// Function to format number i.e. currency or percentage value.
function FormatNumber(value, isPercentage) {
    value = value == undefined ? 0 : value;

    if (isPercentage) {
        //// Modified By: Maninder Singh to address TFS Bug 149.
        value = numberWithCommas(Math.round(parseFloat(value) * 10) / 10);
        return value + '%'
    }
    else {
       
        return '$' + (value != 0 ? GetAbberiviatedValue(value) : 0);
    }
}

//// Added By Maninder Singh 
//// Date 1/28/2014
//// Function to append abberiviation i.e. K - for thousand, M- for million and B - for billion.
function GetAbberiviatedValue(value) {
    var absValue = Math.abs(value);
    absValue = absValue.toFixed();
    var isNegative = value < 0;
    if (absValue < 1000) {
        absValue = (Math.round(parseFloat(absValue) * 100) / 100);
        value = numberWithCommas(absValue);
    }
    else if (absValue < 1000000) {
        value = (Math.round(parseFloat(absValue / 1000) * 100) / 100);
        value = Number(value).toFixed(2);
        value = value.toString() + 'k';
    }
    else if (absValue < 1000000000) {
        value = (Math.round(parseFloat(absValue / 1000000) * 100) / 100);
        value = Number(value).toFixed(2);
        value = value.toString() + 'M';
    }
    else {
        value = (Math.round(parseFloat(absValue / 1000000000) * 100) / 100);
        value = Number(value).toFixed(2);
        value = value.toString() + 'B';
    }

    return (isNegative ? '-' + value : value);
}

//// Added By Maninder Singh 
//// Date 1/28/2014
//// Function to add commas to the value.
function numberWithCommas(value) {
    var splittedValues = value.toString().split(".");
    splittedValues[0] = splittedValues[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return splittedValues.join(".");
}

//// Added By Bhavesh Dobariya
//// Date 1/29/2014
//// Function to add Tooltip with format to the value.
function setLabelToolTip(lableId, value, maxSize, iscurrency) {
    var roundValue = (Math.round(parseFloat(value) * 100) / 100);
    var splitvalue = roundValue.toString().split(".");
    var lengthvalue = splitvalue[0].toString().length;
    if (lengthvalue > maxSize) {
        if (iscurrency) {
            $(lableId).text("$" + GetAbberiviatedValue(value));
            $(lableId).attr('title', "$" + numberWithCommas(roundValue));
        }
        else {
            $(lableId).text(GetAbberiviatedValue(value));
            $(lableId).attr('title', numberWithCommas(roundValue));
        }
        $(lableId).addClass('north');
        $('.north').tipsy({ gravity: 's' });
    }
    else {
        $(lableId).removeAttr('original-title');
        $(lableId).removeClass('north');
        if (iscurrency) {
            $(lableId).text("$" + numberWithCommas(roundValue));
        }
        else {
            $(lableId).text(numberWithCommas(roundValue));
        }
    }
}
//Start Manoj: 30Jan2014 - Bug 17:Should not be able to edit a published model
//Function added to disable all inputs
function MakeViewOnly(ContainerId) {
    var strContainer;
    if (ContainerId == null || ContainerId == '') {
        strContainer = 'body';
    }
    else {
        strContainer = '#' + ContainerId;
    }
    $(strContainer).find("input[type=text],textarea,select").each(function () {
        $(this).attr('disabled', 'disabled');
    });
}
//Function added to enable all inputs
function MakeEditable(ContainerId) {
    var strContainer;
    if (ContainerId == null || ContainerId == '') {
        strContainer = 'body';
    }
    else {
        strContainer = '#' + ContainerId;
    }
    $(strContainer).find("input[type=text],textarea,select").each(function () {
        $(this).removeAttr('disabled');
    });
}
//End Manoj: 30Jan2014 - Bug 17:Should not be able to edit a published model
function FormatWithRoundValue(value) {
    value = parseFloat(value);
    return (value != 0 ? GetAbberiviatedValue(value) : 0);
}

//Added By Bhavesh.
function FormatINQMQL(value, isAbbreriviation) {
    // do not apply format as K , M because it have dependency.
    if (value == "---" || value == "undefined" || value == "") {
        return value;
    }
    var Value = (Math.round(parseFloat(value) * 100) / 100);
    Value = Value.toFixed();
    return GetAbberiviatedValue(Value);
    //if (isAbbreriviation) {
    //    return GetAbberiviatedValue(Value);
    //}
    //else {
    //    return Value;
    //}
}

// Added by Juned - Bug# 244
function NumberFormatterTipsy(lableId, maxSize) {
    $(lableId).each(function () {
        var txtvalue = $(this).text();
        var lengthvalue = txtvalue.length;
        if (lengthvalue > maxSize) {
            $(this).text(txtvalue.substring(0, maxSize) + "...");
            $(this).attr('title', txtvalue);
            $(this).addClass('north');
            $('.north').tipsy({ gravity: 's' });
        }
        else {
            $(this).removeAttr('original-title');
            $(this).removeClass('north');
        }
    });
}

// Added by Juned - Bug# 244
function NumberFormatterTipsyTitle(lableId, maxSize) {
    $(lableId).each(function () {
        var txtvalue = $(this).text();
            var lengthvalue = txtvalue.length;
            if (lengthvalue > maxSize) {
                $(this).text(txtvalue.substring(0, maxSize) + "...");
            }
            $(this).attr('title', txtvalue);
          //  $(this).addClass('north');
          //  $('.north').tipsy({ gravity: 's' });
    });
}

//Added by Nirav Shah  on 10 feb 2014 for  Inspect screen : css changes as per new HTML
function modalPosition() {
    if ($(window).width() <= 921)
        $('#modal-container-186470').css('left', '0%');
    else
        $('#modal-container-186470').css('left', ($(window).width() - $('#modal-container-186470').width()) / 2)
    $('#modal-container-186470').css('top', $(document).scrollTop().toString() + 'px')
}

//Added by Juned on 27 feb to show tool tip on larger labels
function SetTitleToolTip(labelClass) {
    $(labelClass).each(function () {
        var labelText = $(this).text();
        $(this).attr('title', labelText);
    });
}

//// Added By: Maninder Singh Wadhva to calculate dynamic configuration for x or y axis.
function GetAxisConfiguration(dataset) {
    //// Array to hold chart points.
    var arrChartData = [];

    //// Pushing data into array.
    $.each(dataset, function (index, objChardData) {
        if (objChardData.Value != null) {
            arrChartData.push(parseInt(objChardData.Value));
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

    console.log(endValue);
    console.log(stepValue);

    return [{ "stepValue": stepValue, "endValue": endValue }];

}

function FormatForBoostStagesValue(value, number) {
    if (number == 1)
    {
        value = FormatNumber((Math.round(value * 100) / 100), true);
    }
    else if (number == 2) {
        value = (Math.round(value * 100)/100) + " Days";
    }
    else if (number == 3) {
        value = (Math.round(value * 100) / 100);
    }
    else if (number == 4) {
        value = (Math.round(value * 100) / 100);
    }
    return value;
}
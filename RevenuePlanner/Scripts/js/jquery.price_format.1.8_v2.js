/*

* Price Format jQuery Plugin
* Created By Eduardo Cuducos cuducos [at] gmail [dot] com
* Currently maintained by Flavio Silveira flavio [at] gmail [dot] com
* Version: 1.7
* Release: 2012-02-22

* original char limit by Flavio Silveira <http://flaviosilveira.com>
* original keydown event attachment by Kaihua Qi
* keydown fixes by Thasmo <http://thasmo.com>
* Clear Prefix on Blur suggest by Ricardo Mendes from PhonoWay
* original allow negative by Cagdas Ucar <http://carsinia.com>
* keypad fixes by Carlos Vinicius <http://www.kvinicius.com.br> and Rayron Victor
* original Suffix by Marlon Pires Junior
* CentsLimit set to zero fixed by Jereon de Jong
* original idea for the use of the plus sign

*/

(function ($) {

    /****************
	* Main Function *
	*****************/
    $.fn.priceFormat = function (options) {

        var defaults =
		{
		    prefix: 'US$ ',
		    suffix: '',
		    centsSeparator: '.',
		    thousandsSeparator: ',',
		    limit: false,
		    centsLimit: 2,
		    clearPrefix: false,
		    clearSufix: false,
		    allowNegative: false,
		    insertPlusSign: false,
		    isDouble: false,
		    isAllowNull: false
		};

        var options = $.extend(defaults, options);

        return this.each(function () {

            // pre defined options
            var obj = $(this);
            var is_number = /[0-9.]/;

            // load the pluggings settings
            var prefix = options.prefix;
            var suffix = options.suffix;
            var centsSeparator = options.centsSeparator;
            var thousandsSeparator = options.thousandsSeparator;
            var limit = options.limit;
            var centsLimit = options.centsLimit;
            var clearPrefix = options.clearPrefix;
            var clearSuffix = options.clearSuffix;
            var allowNegative = options.allowNegative;
            var insertPlusSign = options.insertPlusSign;
            var isDouble = options.isDouble;
            var isAllowNull = options.isAllowNull;

            var keyupFlag = false;

            // If insertPlusSign is on, it automatic turns on allowNegative, to work with Signs
            if (insertPlusSign) allowNegative = true;

            // skip everything that isn't a number
            // and also skip the left zeroes
            function to_numbers(str) {
                var formatted = '';
                for (var i = 0; i < (str.length) ; i++) {
                    char_ = str.charAt(i);
                    //if (formatted.length == 0 && char_ == 0) char_ = false;

                    if (char_ && char_.match(is_number)) {
                        if (limit) {
                            if (formatted.length < limit) formatted = formatted + char_;
                        }
                        else {
                            formatted = formatted + char_;
                        }
                    }
                }

                return formatted;
            }

            // Added by Arpita Soni on 01/17/2015 for Ticket #1071
            // skip everything that isn't a number
            // don't skip left zeros 
            function to_numbers_keyup(str) {
                var formatted = '';
                for (var i = 0; i < (str.length) ; i++) {
                    char_ = str.charAt(i);

                    if (char_ && char_.match(is_number)) {
                        if (limit) {
                            if (formatted.length < limit) formatted = formatted + char_;
                        }
                        else {
                            formatted = formatted + char_;
                        }
                    }
                }

                return formatted;
            }


            // format to fill with zeros to complete cents chars
            function fill_with_zeroes(str) {
                while (str.length < (centsLimit + 1)) str = '0' + str;
                return str;
            }

            // format as price
            function price_format(str) {
                // Start - Added by Sohel Pathan on 02/09/2014 for PL ticket #742
                
                var iszero = false;
                if (str == '0' || (prefix != '' && str == prefix + '0') || (suffix && str == '0' + suffix)) {
                    iszero = true;
                }
                // End - Added by Sohel Pathan on 02/09/2014 for PL ticket #742
                // formatting settings
                // Start - Modified by Arpita Soni on 01/17/2015 for Ticket #1071
                var formatted;
                if (!keyupFlag) {
                    formatted = fill_with_zeroes(to_numbers(str));
                }
                else {
                    formatted = fill_with_zeroes(to_numbers_keyup(str));
                    keyupFlag = false;
                }
                // End - Modified by Arpita Soni on 01/17/2015 for Ticket #1071
                var thousandsFormatted = '';
                var thousandsCount = 0;
                // Added by Rahul Shah on 04/11/2015 for PL#1729
                var cent = str.split('.')[1];
                if (cent > 0) {
                    centsLimit = cent.length + 1;
                }

                // Checking CentsLimit
                if (centsLimit == 0) {
                    centsSeparator = "";
                    centsVal = "";
                }

                // split integer from cents
                var centsVal = formatted.substr(formatted.length - centsLimit, centsLimit);
                var integerVal = formatted.substr(0, formatted.length - centsLimit);
                var doubleLast;
                var isDoubleExits = false;
                if (isDouble) {
                    if (integerVal.indexOf('.') == -1) {
                        isDoubleExits = false;
                    }
                    else {
                        isDoubleExits = true;
                        var orgValue = integerVal;
                        integerVal = orgValue.substr(0, orgValue.indexOf('.'));
                        doubleLast = (orgValue.substr(orgValue.indexOf('.') + 1));
                    }
                }

                // apply cents pontuation
                formatted = (centsLimit == 0) ? integerVal : integerVal + centsSeparator + centsVal;
                // apply thousands pontuation
                if (thousandsSeparator || $.trim(thousandsSeparator) != "") {
                    for (var j = integerVal.length; j > 0; j--) {
                        char_ = integerVal.substr(j - 1, 1);
                        thousandsCount++;
                        if (thousandsCount % 3 == 0) char_ = thousandsSeparator + char_;
                        thousandsFormatted = char_ + thousandsFormatted;
                    }

                    //
                    if (thousandsFormatted.substr(0, 1) == thousandsSeparator) thousandsFormatted = thousandsFormatted.substring(1, thousandsFormatted.length);
                    formatted = (centsLimit == 0) ? thousandsFormatted : thousandsFormatted + centsSeparator + centsVal;
                }

                // if the string contains a dash, it is negative - add it to the begining (except for zero)
                if (allowNegative && (integerVal != 0 || centsVal != 0)) {
                    if (str.indexOf('-') != -1 && str.indexOf('+') < str.indexOf('-')) {
                        formatted = '-' + formatted;
                    }
                    else {
                        if (!insertPlusSign)
                            formatted = '' + formatted;
                        else
                            formatted = '+' + formatted;
                    }
                }
                // Start - Added by Sohel Pathan on 02/09/2014 for PL ticket #742
                if (formatted == '0' && (doubleLast == '' || doubleLast == 'undefined' || doubleLast == null)) { // Made changes by Viral Kadiya on 09/14/2015 for PL ticket #1395.
                    if (isAllowNull) {
                        if (!iszero) formatted = '';
                    }
                }
                if (formatted != '') {
                    // End - Added by Sohel Pathan on 02/09/2014 for PL ticket #742
                    // apply the prefix
                    if (prefix) formatted = prefix + formatted;

                    // apply the suffix
                    if (isDoubleExits) formatted = formatted + "." + doubleLast;
                    if (suffix) formatted = formatted + suffix;

                }
                centsLimit = options.centsLimit;
                return formatted;
            }

            // filter what user type (only numbers and functional keys)
            function key_check(e) {
                //Start modified by Mtesh vaishnav for PL ticket #1071 issue
                // Allow: backspace, delete, tab, escape, enter and .
                if ($.inArray(e.keyCode, [8, 9, 27, 13, 46, 110, 190]) !== -1 ||
                    //        // Allow: Ctrl+A
                   (e.keyCode == 65 && e.ctrlKey === true) ||
                    //        // Allow: home, end, left, right
                   (e.keyCode >= 35 && e.keyCode <= 39)) {
                    //        // let it happen, don't do anything
                    return;

                    var obj = $(this);          // Added by Arpita Soni on 01/12/2015 for ticket #1071
                    var code = (e.keyCode ? e.keyCode : e.which);
                    var typed = String.fromCharCode(code);
                    var functional = false;
                    var str = obj.val();
                    var newValue = price_format(str + typed);
                    var issecondTime = false;
                    if (str.indexOf('.') > -1) {
                        if ((code == 190 || code == 110)) {
                            issecondTime = true;
                        }
                    }
                    // allow key numbers, 0 to 9
                    if ((code >= 48 && code <= 57) || (code >= 96 && code <= 105)) functional = true;

                    // check Backspace, Tab, Enter, Delete, and left/right arrows
                    if (code == 8) functional = true;
                    if (code == 9) functional = true;
                    if (code == 13) functional = true;
                    if (code == 46) functional = true;
                    if (code == 37) functional = true;
                    if (code == 39) functional = true;

                    // Minus Sign, Plus Sign
                    if (allowNegative && (code == 189 || code == 109)) functional = true;
                    if (insertPlusSign && (code == 187 || code == 107)) functional = true;
                    if (!issecondTime && isDouble && (code == 190 || code == 110)) functional = true;
                    if (!functional) {
                        e.preventDefault();
                        e.stopPropagation();
                        if (str != newValue) obj.val(newValue);
                    }
                }
                //    // Ensure that it is a number and stop the keypress
                if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                    e.preventDefault();
                }
                //End Modified by Mitesh Vaishnav for PL ticket #1071

            }

            // Added by Arpita Soni on 01/17/2015 for Ticket #1071
            // filter what user type on key press(only numbers and functional keys)
            function key_check_keypress(e) {
                var obj = $(this);
                var code = (e.keyCode ? e.keyCode : e.which);
                var typed = String.fromCharCode(code);
                var functional = false;
                var str = obj.val();
                var newValue = price_format(str + typed);
                var issecondTime = false;
                if (str.indexOf('.') > -1) {
                    if (code == 46) {
                        issecondTime = true;
                    }
                }
                // allow key numbers, 0 to 9
                if ((code >= 48 && code <= 57)) functional = true;

                // check Backspace, Tab, Enter, Delete, and left/right arrows
                if (code == 8) functional = true;
                if (code == 9) functional = true;
                if (code == 13) functional = true;
                if (code == 46) functional = true;
                if (e.keyCode == 37) functional = true;
                if (code == 39) functional = true;
                // Minus Sign, Plus Sign
                if (allowNegative && code == 45) functional = true;
                if (insertPlusSign && code == 43) functional = true;
                if (!issecondTime && isDouble && code == 46) functional = true;
                if (!functional) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (str != newValue) obj.val(newValue);
                }
            }
            // inster formatted price as a value of an input field
            function price_it() {
                var str = obj.val();
                var price = price_format(str);
                if (str != price) obj.val(price);
            }

            // Add prefix on focus
            function add_prefix() {
                var val = obj.val();
                obj.val(prefix + val);
            }

            function add_suffix() {
                var val = obj.val();
                obj.val(val + suffix);
            }

            // Clear prefix on blur if is set to true
            function clear_prefix() {
                if ($.trim(prefix) != '' && clearPrefix) {
                    var array = obj.val().split(prefix);
                    obj.val(array[1]);
                }
            }

            // Clear suffix on blur if is set to true
            function clear_suffix() {
                if ($.trim(suffix) != '' && clearSuffix) {
                    var array = obj.val().split(suffix);
                    obj.val(array[0]);
                }
            }

            // bind the actions
            $(this).bind('keydown.price_format', key_check);
            // Start - Modified by Arpita Soni on 01/17/2015 for Ticket #1071
            //$(this).bind('keypress.price_format', key_check_keypress);
            // $(this).bind('keyup.price_format', set_keyup_flag);
            $(this).bind('focusout.price_format', set_focusout_flag);
            // End - Modified by Arpita Soni on 01/17/2015 for Ticket #1071

            // Start - Added by Arpita Soni on 01/17/2015 for Ticket #1071
            //to set keyupFlag on keyup event
            function set_keyup_flag() {
                keyupFlag = true;
                price_it();
            }
            //to reset keyupFlag on keyup event
            function set_focusout_flag() {
                keyupFlag = false;
                price_it();
            }
            // End - Added by Arpita Soni on 01/17/2015 for Ticket #1071

            // Clear Prefix and Add Prefix
            if (clearPrefix) {
                $(this).bind('focusout.price_format', function () {
                    clear_prefix();
                });

                $(this).bind('focusin.price_format', function () {
                    add_prefix();
                });
            }

            // Clear Suffix and Add Suffix
            if (clearSuffix) {
                $(this).bind('focusout.price_format', function () {
                    clear_suffix();
                });

                $(this).bind('focusin.price_format', function () {
                    add_suffix();
                });
            }

            // If value has content
            if ($(this).val().length > 0) {
                price_it();
                clear_prefix();
                clear_suffix();
            }

        });

    };

    /**********************
    * Remove price format *
    ***********************/
    $.fn.unpriceFormat = function () {
        return $(this).unbind(".price_format");
    };

    /******************
    * Unmask Function *
    *******************/
    $.fn.unmask = function () {

        var field = $(this).val();
        var result = "";

        for (var f in field) {
            if (!isNaN(field[f]) || field[f] == "-") result += field[f];
        }

        return result;
    };

})(jQuery);
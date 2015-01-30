(function ($, undefined) {
    var multiselectID = 0;
    var $doc = $(document);
    var StageCodeOfWeight = 'weight';
    $.widget("ech.multiselectWeight", {
        options: {
        },
        _create: function () {
            this._namespaceID = this.eventNamespace || ('multiselectWeight' + multiselectID);
            this._bindEventsM();
            multiselectID++;
        },
        _bindEventsM: function () {
            var Button = this.element;
            var dropdownMenu = Button.parent().find('.dropdown-wrapper');
            Button.on('click', function (e) {
                $('.dropdown-wrapper').not(dropdownMenu).fadeOut();
                dropdownMenu.slideToggle("fast");
                e.stopPropagation();
            });
            this.element.parent().find('.dropdown-wrapper').each(function () {
                var menu = $(this);
                menu.on('click', function (e) {
                    e.stopPropagation();
                });
                $(this).find('.advance_a').on('click', function () {
                    if ($(this).text() == "Advanced Attribution >") {
                        menu.toggleClass('dropdown-block');
                        menu.find(".weight,.weight_header,.first_hide,.sus_header,.mql_header,.cw_header,.revenue_header,.cost_header").toggle();
                        $(this).text("Basic Attribution");
                        menu.find('input:checked').each(function () {
                            var optionId = $(this).val();
                            var allInputValue = $('#' + optionId + '_' + StageCodeOfWeight).val();
                            if ($('#' + optionId + '_' + StageCodeOfWeight).hasClass('error')) {
                                $(this).parents('tr').find('input[type=text]').addClass('error');
                            }
                            else {
                                $(this).parents('tr').find('input[type=text]').removeClass('error');
                            }
                            if (allInputValue == '') {
                                allInputValue = '0';
                            }
                            $(this).parents('tr').find('input[type=text]').val(allInputValue);
                        });
                    }
                    else {
                        menu.find('.innerpopup').css('display', 'block');
                    }
                });
                menu.find('.close_btn,.cncl_btn').on('click', function () {
                    menu.find('.innerpopup').css('display', 'none');
                });
                menu.find('.proceed_btn').on('click', function () {
                    menu.find('.innerpopup').css('display', 'none');
                    menu.toggleClass('dropdown-block');
                    menu.find(".weight,.weight_header,.first_hide,.sus_header,.mql_header,.cw_header,.revenue_header,.cost_header").toggle();
                    menu.find('.advance_a').text("Advanced Attribution >");

                    menu.find('input:checked').each(function () {
                        var total = 0;
                        var counter=0
                        $(this).parents('tr').find('input[type=text]').not('.text_blk_active').each(function () {
                            if ($(this).val() != '' && typeof $(this).val() != 'undefined') {
                                counter += 1;
                                total += parseInt($(this).val());
                            }
                        });
                        var avg = 0;
                        if (counter > 0) {
                            avg=  total / counter;
                        }
                        $(this).parents('tr').find('.text_blk_active').val(parseInt(avg).toString());
                    });
                    var validateTotalWeightage = 0;
                    menu.find('input:checked').each(function () {
                        validateTotalWeightage += parseInt($(this).parents('tr').find('.text_blk_active').val());
                    });
                    if (validateTotalWeightage != 100) {
                        menu.find('input:checked').each(function () {
                            $(this).parents('tr').find('.text_blk_active').addClass('error');
                        });
                    }
                    else {
                        menu.find('input:checked').each(function () {
                            $(this).parents('tr').find('.text_blk_active').removeClass('error');
                        });
                    }
                    menu.find('input[type=text]').not('.text_blk_active').val('');
                });
                menu.find('input[type=text]').on('keydown', function (e) {
                    // Allow: backspace, delete, tab, escape, enter and .
                    if ($.inArray(e.keyCode, [8, 9, 27, 13, 110]) !== -1 ||
                        // Allow: Ctrl+A
                        (e.keyCode == 65 && e.ctrlKey === true) ||
                        // Allow: home, end, left, right
                        (e.keyCode >= 35 && e.keyCode <= 39)) {
                        // let it happen, don't do anything
                        return;
                    }
                    // Ensure that it is a number and stop the keypress
                    if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                        e.preventDefault();
                    }
                });
                menu.find('input[type=text]').on('change', function (e) {
                    // Allow: backspace, delete, tab, escape, enter and .
                    if ($.inArray(e.keyCode, [8, 9, 27, 13, 110]) !== -1 ||
                        // Allow: Ctrl+A
                        (e.keyCode == 65 && e.ctrlKey === true) ||
                        // Allow: home, end, left, right
                        (e.keyCode >= 35 && e.keyCode <= 39)) {
                        // let it happen, don't do anything
                        return;
                    }
                    // Ensure that it is a number and stop the keypress
                    if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                        e.preventDefault();
                    }
                });
                menu.find('input[type=text]').on('keyup', function () {
                    var inputText = $(this);
                    var totalWeightage = 0;
                    var stageCode = inputText.attr('id').toString().split('_')[1].toString();
                    var isAllColumnInputBlank = true;
                    menu.find('input:checked').each(function () {
                        var optionId = $(this).val();
                        if (typeof $('#' + optionId + '_' + stageCode).val() != 'undefined' && $('#' + optionId + '_' + stageCode).val() != '') {
                            totalWeightage += parseInt($('#' + optionId + '_' + stageCode).val());
                            isAllColumnInputBlank = false;
                        }
                    });

                    menu.find('input:checked').each(function () {
                        var optionId = $(this).val();
                        if (totalWeightage != 100 && !isAllColumnInputBlank) {
                            $('#' + optionId + '_' + stageCode).addClass('error');
                        }
                        else {
                            if ($('#' + optionId + '_' + stageCode).hasClass('error')) {
                                $('#' + optionId + '_' + stageCode).removeClass('error')
                            }
                        }
                    });


                });
                menu.find('input[type=checkbox]').on('click', function () {

                    if ($(this).prop('checked') != true) {
                        $(this).parents('tr').find('input[type=text]').val('');
                        $(this).parents('tr').find('input[type=text]').keyup();
                        $(this).parents('tr').find('input[type=text]').removeClass('error');
                    }
                    else {
                        $(this).parents('tr').find('input[type=text]').keyup();
                    }
                    var title = "";
                    Button.find('p:first').text("");
                    //var checkedCheckbox = menu.find('input:checked').length;
                    //var inputValues = 100 / checkedCheckbox;
                    //var residual = 100 % checkedCheckbox;
                    menu.find('input:checked').each(function () {
                        title += $(this).parent().find('p').text() + ',';
                        //$(this).closest('tr').find('input[type=text]').each(function () {
                        //    $(this).val(inputValues.toString());
                        //});
                    });
                    if (title.indexOf(',') > 0) {
                        title = title.slice(0, -1);
                        Button.find('p:first').text(title);
                        Button.find('p:first').attr('title', title);
                    }
                    else {
                        Button.find('p:first').text('Please Select');
                    }
                });
            });
            $doc.click(function () {
                // all dropdowns
                $('.dropdown-wrapper').css('display', 'none');
            });
        },
        refresh: function () {
            var Menu = this.element.parent().find('.dropdown-wrapper');
            Menu.find('.table_drpdwn').each(function () {
                var j = 2;
                while (j > 0) {
                    if ($(this).find('thead tr').find('td').eq(j).text() != 'CW(%)') {
                        $(this).find('tbody tr').find('input[type=text]').on('keydown', function (e) {
                            // Allow: backspace, delete, tab, escape, enter and .
                            if ($.inArray(e.keyCode, [8, 9, 27, 13, 110]) !== -1 ||
                                // Allow: Ctrl+A
                                (e.keyCode == 65 && e.ctrlKey === true) ||
                                // Allow: home, end, left, right
                                (e.keyCode >= 35 && e.keyCode <= 39)) {
                                // let it happen, don't do anything
                                return;
                            }
                            // Ensure that it is a number and stop the keypress
                            if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                                e.preventDefault();
                            }
                        });
                        $(this).find('tbody tr').find('input[type=text]').on('change', function (e) {
                            // Allow: backspace, delete, tab, escape, enter and .
                            if ($.inArray(e.keyCode, [8, 9, 27, 13, 110]) !== -1 ||
                                // Allow: Ctrl+A
                                (e.keyCode == 65 && e.ctrlKey === true) ||
                                // Allow: home, end, left, right
                                (e.keyCode >= 35 && e.keyCode <= 39)) {
                                // let it happen, don't do anything
                                return;
                            }
                            // Ensure that it is a number and stop the keypress
                            if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                                e.preventDefault();
                            }
                        });
                        $(this).find('tbody tr').find('input[type=text]').on('keyup', function () {
                            var inputText = $(this);
                            var totalWeightage = 0;
                            var stageCode = inputText.attr('id').toString().split('_')[1].toString();
                            var isAllColumnInputBlank = true;
                            Menu.find('input:checked').each(function () {
                                var optionId = $(this).val();
                                if (typeof $('#' + optionId + '_' + stageCode).val() != 'undefined' && $('#' + optionId + '_' + stageCode).val() != '') {
                                    totalWeightage += parseInt($('#' + optionId + '_' + stageCode).val());
                                    isAllColumnInputBlank = false;
                                }
                            });

                            Menu.find('input:checked').each(function () {
                                var optionId = $(this).val();
                                if (totalWeightage != 100 && !isAllColumnInputBlank) {
                                    $('#' + optionId + '_' + stageCode).addClass('error');
                                }
                                else {
                                    if ($('#' + optionId + '_' + stageCode).hasClass('error')) {
                                        $('#' + optionId + '_' + stageCode).removeClass('error')
                                    }
                                }
                            });
                        });
                        j++;
                    }
                    else {
                        j = 0;
                    }
                }
            });
        }
    });

})(jQuery);
//function MultiselectWeight
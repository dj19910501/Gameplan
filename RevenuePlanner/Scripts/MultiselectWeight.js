(function ($, undefined) {
    var multiselectID = 0;
    var $doc = $(document);
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
            var dropdownMenu=Button.parent().find('.dropdown-wrapper');
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
                    menu.toggleClass('dropdown-block');
                    menu.find(".weight,.weight_header,.first_hide,.sus_header,.mql_header,.cw_header,.revenue_header,.cost_header").toggle();
                    
                            if ($(this).text() == "advance attribution >") {
                                $(this).text("Basic attribution");
                            }
                            else {
                                $(this).text("advance attribution >");
                            }
                });
                menu.find('input[type=text]').on('keyup',function () {
                    var inputText = $(this);
                        var totalWeightage = 0;
                        var stageCode = inputText.attr('id').toString().split('_')[1].toString();
                        menu.find('input:checked').each(function () {
                            var optionId = $(this).val();
                            if (typeof $('#' + optionId + '_' + stageCode).val() != 'undefined' && $('#' + optionId + '_' + stageCode).val() != '') {
                                totalWeightage += parseInt($('#' + optionId + '_' + stageCode).val());
                            }
                        });
                        
                            menu.find('input:checked').each(function () {
                                var optionId = $(this).val();
                                if (totalWeightage != 100) {
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
                    var title = "";
                    Button.find('span:first').text("");
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
                        title= title.slice(0, -1);
                        Button.find('p:first').text(title);
                    }
                });
            });
            $doc.click(function () {
                // all dropdowns
                $('.dropdown-wrapper').css('display','none');
            });
        },
        refresh: function () {
            var Menu = this.element.parent().find('.dropdown-wrapper');
            Menu.find('.table_drpdwn').each(function () {
                var j = 2;
                while (j > 0) {
                    if ($(this).find('thead tr').find('td').eq(j).text() != 'CW(%)') {
                        $(this).find('tbody tr').find('input[type=text]').on('keyup', function () {
                            var inputText = $(this);
                            var totalWeightage = 0;
                            var stageCode = inputText.attr('id').toString().split('_')[1].toString();
                            Menu.find('input:checked').each(function () {
                                var optionId = $(this).val();
                                if (typeof $('#' + optionId + '_' + stageCode).val() != 'undefined' && $('#' + optionId + '_' + stageCode).val() != '') {
                                    totalWeightage += parseInt($('#' + optionId + '_' + stageCode).val());
                                }
                            });

                            Menu.find('input:checked').each(function () {
                                var optionId = $(this).val();
                                if (totalWeightage != 100) {
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

        //getWeightArray: function () {
        //    return true;
        //    //debugger;
        //    //var checked = dropdownMenu.find('input').filter(':checked');
        //}
    });

})(jQuery);
//function MultiselectWeight
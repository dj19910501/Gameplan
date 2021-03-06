﻿(function ($, undefined) {
    var multiselectID = 0;
    var $doc = $(document);

    var StageCodeOfWeight = 'weight';
    $.widget("ech.multiselectWeight", {

        options: {
            errorDivId: 'noId',
            pageErrorDivId: 'noPageErrorId'
        },
        _create: function () {

            this._namespaceID = this.eventNamespace || ('multiselectWeight' + multiselectID);
            this._bindEventsM();
            multiselectID++;
        },
        _bindEventsM: function () {

            var o = this.options;
            var Button = this.element;
            var dropdownMenu = Button.parent().find('.dropdown-wrapper');
            var singleMode = 'Single';
            var multiMode = 'Multi';
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

                    if ($(this).attr('mode').toString() == singleMode) {
                        menu.toggleClass('dropdown-block minimum-width215');
                        menu.find('.text_ellipsis').toggleClass('minmax-width200');
                        menu.find(".weight,.weight_header,.first_hide,.revenue_header,.cost_header,.value_header,.top_head_attribute").toggle();
                        $('.first_hide').find('input').each(function () {
                            if ($(this).hasClass('multiselect-input-text-color-grey')) {
                                $(this).val("");
                            }
                        });
                        $(this).text("< Single-selection");
                        $(this).attr('mode', multiMode);
                        menu.find('input[type=checkbox]').toggle();
                        menu.find('p').removeClass('single-p');
                        menu.find('tr').removeClass('trdropdownhover setfocusTr');
                        menu.find('#aclose_tag').css('display', 'block');
                        //added by Rahul Shah on 05/11/2015 - when user click on multiMode of Custom dropdown then hide the "Please Select"
                        if ($(this).parent().parent().parent().parent().find('tbody').children(':first-child').find('p').text() == "Please Select") {
                            $(this).parent().parent().parent().parent().find('tbody').children(':first-child').hide();
                            $(this).parent().parent().parent().parent().find('tbody').children(':first-child').find('input[type=checkbox]').removeAttr('checked');
                        }

                        var title = DivideEqualInputValue(menu);
                    }
                    else {
                        //added by Rahul Shah on 05/11/2015 - when user click on SingleMode of Custom dropdown then Show the "Please Select"
                        if ($(this).parent().parent().parent().parent().find('tbody').children(':first-child').find('p').text() == "Please Select") {
                            $(this).parent().parent().parent().parent().find('tbody').children(':first-child').show();
                        }
                        if (o.errorDivId != 'noId') {
                            var errorDivId = o.errorDivId;
                            var isAllColumnInputBlank = true;
                            var totalWeightage = 0;
                            var checkCount = menu.find('input:checked').length;
                            var pageErrorDivId = '';
                            var label = menu.parent().find('a').attr('label').toString();
                            if (o.pageErrorDivId != 'noPageErrorId') {
                                pageErrorDivId = o.pageErrorDivId;
                            }
                            if (checkCount > 0) {
                                isAllColumnInputBlank = false;
                            }
                            if (!isAllColumnInputBlank) {
                                if (label != '' && typeof label != 'undefined') {

                                    $('#' + errorDivId).attr('proccedObject', label);
                                    $('#' + errorDivId).slideDown(400);
                                    $('#' + errorDivId + ' span').find('attributetext').text(label);
                                    if (pageErrorDivId != null && pageErrorDivId != 'undefined' && pageErrorDivId != '') {
                                        $('#' + pageErrorDivId).css('display', 'none');
                                    }
                                    window.location = '#' + errorDivId;//'#MultiSelectProcced';
                                    if ($(this).parent().parent().parent().parent().find('tbody').children(':first-child').find('p').text() == "Please Select") {
                                        $(this).parent().parent().parent().parent().find('tbody').children(':first-child').hide();
                                        $(this).parent().parent().parent().parent().find('tbody').children(':first-child').find('input[type=checkbox]').removeAttr('checked');
                                    }
                                }
                            }
                            else {
                                $('.dropdown_new_btn').each(function () {
                                    if ($(this).attr('label').toString() == label) {
                                        $('#' + errorDivId).slideUp(400);
                                        var menu = $(this).parent().find('.dropdown-wrapper');
                                        menu.toggleClass('dropdown-block minimum-width215');
                                        menu.find('.text_ellipsis').toggleClass('minmax-width200');
                                        menu.find(".weight,.weight_header,.first_hide,.revenue_header,.cost_header,.value_header,.top_head_attribute").toggle();
                                        menu.find('input[type=checkbox]').toggle();
                                        menu.find('.advance_a').text("> Multi-selection");
                                        menu.find('.advance_a').attr('mode', 'Single');
                                        $(this).find('p:first').text("Please Select");
                                        menu.find('input:checkbox').removeAttr('checked');
                                        menu.find('input[type=text]').val('');
                                        menu.find('p').addClass('single-p');
                                        menu.find('tr').addClass('trdropdownhover');
                                        menu.find('#aclose_tag').css('display', 'none');
                                    }
                                });
                            }

                        }
                        else {
                            menu.toggleClass('dropdown-block minimum-width215');
                            menu.find('.text_ellipsis').toggleClass('minmax-width200');
                            menu.find(".weight,.weight_header,.first_hide,.revenue_header,.cost_header,.value_header,.top_head_attribute").toggle();
                            menu.find('input[type=checkbox]').toggle();
                            $(this).text("> Multi-selection");
                            $(this).attr('mode', singleMode);
                            Button.find('p:first').text("Please Select");
                            menu.find('input:checkbox').removeAttr('checked');
                            menu.find('p').addClass('single-p');
                            menu.find('tr').addClass('trdropdownhover');
                            menu.find('#aclose_tag').css('display', 'none');
                        }
                    }
                });
                if (o.errorDivId != 'noId') {
                    $('#' + o.errorDivId).find('.close,.cncl_btn').on('click', function (e) {
                        $('#' + o.errorDivId).slideUp(400);
                        e.stopPropagation();
                    })
                }
                menu.find('.close_btn,.cncl_btn').on('click', function () {

                    menu.find('.innerpopup').css('display', 'none');
                });
                menu.find('.close_a').on('click', function () {

                    $('.dropdown-wrapper').css('display', 'none');
                });
                menu.find('input[type=text]').on('keydown', function (e) {
                    // Allow: backspace, delete, tab, escape, enter and .
                    if ($.inArray(e.keyCode, [8, 9, 27, 13, 46, 110]) !== -1 ||
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
                    if ($.inArray(e.keyCode, [8, 9, 27, 13, 46, 110]) !== -1 ||
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
                menu.find('.first_show').on('click', function (e) {

                    var dependancyFilters = {
                        parentCustomFieldsIds: [],
                        customFieldIds: [],
                        PlanIDs: [],
                        SelectedPlans: [],
                        PlanTitles: [],
                        OwnerIds: [],
                        TacticTypeids: [],
                        tempTacticTypeIds: [],
                        tempOwnerIds: [],
                        StatusIds: [],
                        SelectedYears: []

                    };
                    if (typeof filters != 'undefined' && filters != null) {
                        dependancyFilters = filters;
                    }
                    dependancyFilters.chekboxIds = [];
                    var checkbx = $(this).find('input:checkbox');
                    var ParentValue = checkbx.attr('name');
                    var title = "";
                    //Modified by Komal Rawal for #1435 Attribute dependency
                    if (menu.find('.advance_a').attr('mode') != singleMode) {
                        $(this).parent().parent().find("input[type=checkbox]").each(function () {
                            if ($(this).attr('checked') == 'checked') {
                                var chkid = $(this).val();
                                if (chkid != undefined && chkid != 'undefined') {
                                    dependancyFilters.chekboxIds.push(chkid);
                                }
                            }
                        });
                    }
                    else
                        dependancyFilters.chekboxIds.push(checkbx.val());

                    $('#CustomHtmlHelperfield').find('.span3').each(function () {
                        var InputType = $(this).find("input").attr('type');
                        var cnt = 0;
                        if (InputType == "text" && $(this).attr('parentid') != 'undefined' && $(this).attr('parentid') != '0' && $(this).attr('parentid') == ParentValue) {

                            var ParentOptionID = $(this).find("input").parent().attr('parentoptionid');
                            //Modified By Komal Rawal for #1864
                            var arrParentOptionId = ParentOptionID.split(",");
                            var i;
                            $(this).css("display", "none");
                            //if (arrParentOptionId.indexOf(filters.chekboxIds.toString()) > -1 && $(this).css("display") == "none") {
                            //    $(this).css("display", "inline-block");
                            //    cnt++;
                            //}

                            //Modified by Dashrath Prajapati for PL#1965
                            for (i = 0 ; i < dependancyFilters.chekboxIds.length; i++) {
                                if (arrParentOptionId.indexOf(dependancyFilters.chekboxIds[i]) > -1) {
                                    if ($(this).css("display") == "none") {
                                        $(this).css("display", "inline-block");
                                        cnt++;
                                    }
                                }
                            }
                            //up to here PL#1965

                            if (cnt == 0) {
                                $(this).find('input[type=text]').val('');
                            }
                        }
                        else {

                            if ($(this).attr('parentid') == ParentValue) {

                                var maindiv = $(this);
                                var isSelected = false;
                                var Selectedvalue = ""
                                $(this).css("display", "inline-block");
                                $(this).find('tbody tr').css("display", "none");
                                maindiv.find('tbody tr').each(function () {
                                    var checkbox = $(this).find('input[type=checkbox]');

                                    if ((checkbox.attr('checked') == 'checked' && $.inArray($(this).attr('parentid'), dependancyFilters.chekboxIds) > -1)) {
                                        Selectedvalue += $(this).find(' p:first').text() + ', ';
                                        isSelected = true;
                                    }
                                    var ParentOptionId = $(this).attr('parentid');
                                    var arrParentOptionId = ParentOptionId.split(",");
                                    var i;
                                    for (i = 0 ; i < dependancyFilters.chekboxIds.length; i++) {
                                        //Modified By Komal Rawal for #1864
                                        if (arrParentOptionId.indexOf(dependancyFilters.chekboxIds[i]) > -1 || $(this).find('.lable_inline').text() == "Please Select") {

                                            if ($(this).css("display") == "none") {
                                                $(this).css("display", "block");
                                                cnt++;
                                            }
                                        }
                                    }

                                });
                                if (!isSelected) {
                                    $(this).find('tbody tr').find('input:checkbox').removeAttr('checked');
                                    $(this).find('.dropdown_new_btn p:first').text('Please Select');

                                }
                                else {
                                    if (Selectedvalue.indexOf(',') > 0) {
                                        Selectedvalue = Selectedvalue.slice(0, -2);
                                    }
                                    $(this).find('.dropdown_new_btn p:first').text(Selectedvalue);
                                }

                                if (cnt == 1 || cnt == 0) {
                                    maindiv.css("display", "none");
                                    $(this).find('.dropdown_new_btn p:first').text('Please Select');
                                    $(this).find('tbody tr').find('input:checkbox').removeAttr('checked');
                                    $(this).find('input[type=checkbox]').css("display", "none");
                                    $(this).find('.advance_a').text("> Multi-selection");
                                    $(this).find('.advance_a').attr('mode', singleMode);
                                    $(this).find('input[type=text]').val('');
                                    $(this).find('p').addClass('single-p');
                                    $(this).find('tr').addClass('trdropdownhover');
                                    $(this).find('#aclose_tag').css('display', 'none');

                                }
                            }
                        }
                    });
                    //End
                    Button.find('p:first').text("");
                    if (menu.find('.advance_a').attr('mode') == singleMode) {
                        menu.find('input:checkbox').removeAttr('checked');
                        $(checkbx).attr('checked', 'checked');
                        title += $(checkbx).parent().find('p').text();
                        Button.find('p:first').text(title);
                        Button.find('p:first').attr('title', title);
                        menu.slideToggle("fast");
                        e.preventDefault();
                    }
                    else {
                        title = DivideEqualInputValue(menu);

                        if (title.indexOf(',') > 0) {
                            title = title.slice(0, -2);
                            Button.find('p:first').text(title);
                            Button.find('p:first').attr('title', title);
                        }
                        else {
                            Button.find('p:first').text('Please Select');
                        }
                    }
                    if ($(checkbx).prop('checked') != true) {
                        $(checkbx).parents('tr').find('input[type=text]').val('');
                        $(checkbx).parents('tr').find('input[type=text]').keyup();
                        $(checkbx).parents('tr').find('input[type=text]').removeClass('error');
                        $(checkbx).parents('tr').find('input[type=text]').addClass('multiselect-input-text-color-grey');
                    }
                    else {
                        $(checkbx).parents('tr').find('input[type=text]').keyup();
                        $(checkbx).parents('tr').find('input[type=text]').removeClass('multiselect-input-text-color-grey');
                    }
                });
            });
            $doc.click(function () {
                // all dropdowns
                $('.dropdown-wrapper').css('display', 'none');
            });
        }
    });

})(jQuery);
//function MultiselectWeight
function DivideEqualInputValue(menu) {
    var title = '';
    var checkedCheckbox = menu.find('input:checked').length;
    var inputValues = parseInt(100 / checkedCheckbox);
    var residual = parseInt(100 % checkedCheckbox);
    var checkedResidualDiff = checkedCheckbox - residual;
    var res_counter = 0;
    menu.find('input:checked').each(function () {
        res_counter += 1;
        title += $(this).parent().find('p').text() + ', ';
        if (res_counter <= checkedResidualDiff) {
            $(this).closest('tr').find('input[type=text]').each(function () {
                $(this).val(inputValues.toString());
            });
        }
        else {
            $(this).closest('tr').find('input[type=text]').each(function () {
                $(this).val((inputValues + 1).toString());
            });
        }
    });
    return title;
}

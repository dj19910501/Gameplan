var checkedYearcount = 0;
var checkedPlancount = 0;
var checkedOwnercount = 0;
var checkedTTcount = 0;
var checkedStatuscount = 0;
var TotalStatus = 0;
var FiltersNone = "None";
var FiltersAll = "All";
var selectedFilters = {
    yearIds: [],
    planName: [],
    ownerName: [],
    tacticTypeTitle: [],
    StatusIds: []
};
var filters = {
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
selectedFilters.StatusIds = [];
selectedFilters.ownerName = [];
selectedFilters.tacticTypeTitle = [];
selectedFilters.yearIds = [];
selectedFilters.planName = [];
$(document).ready(function () {
    $("#ulSelectedYear li input[type=checkbox]:checked").each(function () {
        selectedFilters.yearIds.push($(this).attr("yearvalue"));
    });
    $("#ulSelectedPlans li input[type=checkbox]:checked").each(function () {
        selectedFilters.planName.push($(this).parent().attr("title"));
    });
    checkedYearcount = $('#ulSelectedYear li input:checked').length;
    checkedPlancount = selectedFilters.planName.length;
    $('#cYearcount').text(checkedYearcount);
    $('#tPlancount').text("/" + $("#ulSelectedPlans li").length.toString());

    TotalStatus = $("#ulStatus li").length;
    $('#tStatuscount').text('/' + TotalStatus);
});

function OnYearChange(obj) {
    filters.SelectedYears = [];
    var id = $(obj).attr('id');
    if ($(obj).is(':checked')) {
        $("#" + id).addClass("close-list");
    }
    else {
        $("#" + id).removeClass("close-list");
    }
    checkedYearcount = $("#ulSelectedYear li input:checkbox:checked").length;
    $('#cYearcount').text(checkedYearcount);
    var listCheckbox = $("#ulSelectedYear").find("input[type=checkbox]");
    years = "";
    $.each(listCheckbox, function () {
        if ($(this).attr("checked")) {
            years += $(this).attr('yearValue') + ",";
        }
    });
    years = years.slice(0, -1);
    if (years != null && years != "") {
        filters.SelectedYears.push(years);
    }
    LoadPlanData(years);
}

var _item;
function LoadPlanData(years) {
    var strYear = years;
    var selectedPlanIds = [];
    var listofselectedplan = $("#accordion-element-Plan").find("ul").find("li").find("input[type='checkbox']:checked");
    $.each(listofselectedplan, function () {

        var pid = $(this).attr('id').split('_')[0];
        selectedPlanIds.push(pid);
    });
    var isListPlan = false;
    $.ajax({
        beforeSend: function (x) {
            myApp.hidePleaseWait();
        },
        type: 'POST',
        url: $('#PlanBudgetYearwise').val(),
        data: {
            Year: strYear,
            activemenu: $('#ActiveMenuHome').val()
        },
        success: function (data) {
            $('#ulSelectedPlans').html('');
            if (data != 'undefined' && data != null) {
                PlanCount = data.length;
                if (PlanCount > 0) {
                    $("#PlanAllModule").css("display", "block");
                    $("#NoPlanFound").hide();
                    $.each(data, function (index, item) {
                        _item = item.Value.replace(/'/g, "&#39;");
                        $('#ulSelectedPlans').append('<li class="accordion-inner" title="' + item.Text + '" id="liPlan' + item.Value + '"><span class="sidebarliwidth">' + item.Text + '</span><input type="checkbox" class="chkbxfilter" onchange="togglePlan(this)" id="' + _item + '"></input></li>');
                    });
                }
                else {
                    $("#PlanAllModule").css("display", "none");
                    $("#NoPlanFound").show();
                }
            }
            else {
                $("#PlanAllModule").css("display", "none");
                $("#NoPlanFound").show();
            }
            checkedPlancount = $("#ulSelectedPlans li input:checkbox:checked").length;
            var TotalPlancount = $("#ulSelectedPlans li").length;
            $('#tPlancount').text('/' + TotalPlancount);
            $('#cPlancount').text(checkedPlancount);
            var listofplan = $("#accordion-element-Plan").find("ul").find("li").find("input[type='checkbox']");
            $.each(listofplan, function () {

                var pid = $(this).attr('id').split('_')[0];
                var indexofplan = $.inArray(pid, selectedPlanIds);
                if (indexofplan > -1) {
                    $(this).attr("checked", "checked");
                    togglePlan($(this), true);
                }
            });
            var planids = [];
            $('#ulSelectedPlans').find("input[type=checkbox]").each(function () {
                if ($(this).attr('checked') == 'checked') {
                    var chkid = $(this).attr("id");
                    if (chkid != undefined && chkid != 'undefined') {
                        planids.push(chkid);
                    }
                }
            });
        }
    });
}

function togglePlan(obj, isLstPlan) {
    var IsLstPlan = false;
    if (isLstPlan != null && isLstPlan != 'undefined' && isLstPlan != '' && isLstPlan)
        IsLstPlan = true;
    GetLastview = false;
    var id = $(obj).attr('id');
    if ($(obj).is(':checked')) {
        $("#liPlan" + id).addClass("close-list");
    }
    else {
        $("#liPlan" + id).removeClass("close-list");
    }
    var planids = [];
    $('#ulSelectedPlans').find("input[type=checkbox]").each(function () {
        if ($(this).attr('checked') == 'checked') {
            var chkid = $(this).attr("id");
            if (chkid != undefined && chkid != 'undefined') {
                planids.push(chkid);
            }
        }
    });
    checkedPlancount = $("#ulSelectedPlans li input:checkbox:checked").length;
    $('#cPlancount').text(checkedPlancount);
}

function toggleStatus(obj) {
    GetLastview = false;
    var id = $(obj).attr('id');
    var liID = $(obj).parent().attr('id');
    if ($(obj).is(':checked')) {
        $('#' + liID).addClass("close-list");
    }
    else {
        $('#' + liID).removeClass("close-list");
    }
    filters.StatusIds = [];
    $("#ulStatus li input[type=checkbox]").each(function () {
        var chkid = $(this).attr("id");
        if ($(this).is(':checked')) {
            filters.StatusIds.push(chkid);
        }
    });
    checkedStatuscount = $("#ulStatus li input:checkbox:checked").length;
    $('#cStatuscount').text(checkedStatuscount);
}

function BulkStatusOperation(selection) {
    GetLastview = false;
    filters.StatusIds = [];
    if (selection) {
        $("#ulStatus li").each(function (i) {
            $(this).addClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).attr("checked", "checked");
            filters.StatusIds.push(chkid);
        });
    }
    else {
        $("#ulStatus li").each(function (i) {
            $(this).removeClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).removeAttr("checked");
        });
    }
    checkedStatuscount = $("#ulStatus li input:checkbox:checked").length;
    $('#cStatuscount').text(checkedStatuscount);
}

function toggleCustomFields(obj, CustFieldId, count) {
    var SelectedCustomFields = parseInt($("#SelectedCustomFilter_" + CustFieldId).text());
    GetLastview = false;
    var id = $(obj).attr('id');
    if ($(obj).is(':checked')) {
        $("#li_" + id).addClass("close-list");
        SelectedCustomFields++;
        $("#SelectedCustomFilter_" + CustFieldId).text(SelectedCustomFields);
    }
    else {
        $("#li_" + id).removeClass("close-list");
        SelectedCustomFields--;
        $("#SelectedCustomFilter_" + CustFieldId).text(SelectedCustomFields);
    }
}

function UpdateSelectedFilters() {
    $("#ulSelectedFilter").find("li #CustomFields .sidebarliwidthSelected-right").text("");
    var CheckedCounter = 0, AllCounter = 0, CurrentText = "", id = null;
    $("#divCustomFieldsFilter").find("div.accordion").each(function () {
        CheckedCounter = 0; AllCounter = 0; CurrentText = ""; id = null;
        if ($(this).find("input[type=checkbox]") != null || $(this).find("input[type=checkbox]") != "") {
            AllCounter = $(this).find("input[type=checkbox]").length;
            CheckedCounter = $(this).find("input[type=checkbox]:checked").length;
            $(this).find("input[type=checkbox]:checked").each(function () {

                CurrentText = CurrentText + ", " + $($(this).parent()).find("span.sidebarliwidth").text();
            });
        }
        id = this.id;
        if (id != null && id != "" && id.indexOf("-") > -1) {
            id = this.id.split("-")[1];
        }
        if (CheckedCounter == AllCounter) {
            $("#ulSelectedFilter").find("#lstCustomFieldSelected_" + id).text(FiltersAll);
            $("#SelectedCustomFilter_" + id).text(CheckedCounter);
        }
        else {
            if (CheckedCounter == 0) {
                $("#ulSelectedFilter").find("#lstCustomFieldSelected_" + id).text(FiltersNone);
                $("#SelectedCustomFilter_" + id).text("0");

            }
            else {
                CurrentText = CurrentText.substring(1);
                $("#ulSelectedFilter").find("#lstCustomFieldSelected_" + id).text(CurrentText);
                $("#SelectedCustomFilter_" + id).text(CheckedCounter);
            }
        }
    });
    selectedFilters.StatusIds = [];
    selectedFilters.ownerName = [];
    selectedFilters.tacticTypeTitle = [];
    selectedFilters.yearIds = [];
    selectedFilters.planName = [];

    $("#ulSelectedYear li input[type=checkbox]:checked").each(function () {
        selectedFilters.yearIds.push($(this).attr("yearvalue"));
    });
    if (selectedFilters.yearIds.length == 0) {
        $('#lstYearActive').text(FiltersNone);
    }
    else if ($("#ulSelectedYear li").length == selectedFilters.yearIds.length) {
        $('#lstYearActive').text(FiltersAll);
    }
    else {
        $('#lstYearActive').text(selectedFilters.yearIds.join(", "));
    }

    $("#ulSelectedPlans li input[type=checkbox]:checked").each(function () {
        selectedFilters.planName.push($(this).parent().attr("title"));
    });
    if (selectedFilters.planName.length == 0) {
        $('#lstPlanActive').text(FiltersNone);
    }
    else if ($("#ulSelectedPlans li").length == selectedFilters.planName.length) {
        $('#lstPlanActive').text(FiltersAll);
    }
    else {
        $('#lstPlanActive').text(selectedFilters.planName.join(", "));
    }
    checkedYearcount = selectedFilters.yearIds.length;
    $('#cYearcount').text(checkedYearcount);
    checkedPlancount = selectedFilters.planName.length;
    $('#cPlancount').text(checkedPlancount);
    $('#tPlancount').text("/" + $("#ulSelectedPlans li").length.toString());

    $("#ulStatus li input[type=checkbox]:checked").each(function () {
        selectedFilters.StatusIds.push($(this).attr('id'));
    });
    if (selectedFilters.StatusIds.length == 0) {
        $('#lstStatus').text(FiltersNone);
    }
    else if ($('#ulStatus li').length == selectedFilters.StatusIds.length) {
        $('#lstStatus').text(FiltersAll);
    }
    else {
        $('#lstStatus').text(selectedFilters.StatusIds.join(", "));
    }
    $("#ulSelectedOwner li input[type=checkbox]:checked").each(function () {
        selectedFilters.ownerName.push($(this).attr('ownertitle').toString());
    });
    if (selectedFilters.ownerName.length == 0) {
        $('#lstOwner').text(FiltersNone);
    }
    else if ($("#ulSelectedOwner li").length == selectedFilters.ownerName.length) {
        $('#lstOwner').text(FiltersAll);
    }
    else {
        $('#lstOwner').text(selectedFilters.ownerName.join(", "));
    }
    $("#ulTacticType li input[type=checkbox]:checked").each(function () {
        selectedFilters.tacticTypeTitle.push($(this).attr('tactictypetitle').toString());
    });
    if (selectedFilters.tacticTypeTitle.length == 0) {
        $('#lstTType').text(FiltersNone);
    }
    else if ($("#ulTacticType li").length == selectedFilters.tacticTypeTitle.length) {

        $('#lstTType').text(FiltersAll);
    }
    else {
        $('#lstTType').text(selectedFilters.tacticTypeTitle.join(", "));
    }

    checkedTTcount = selectedFilters.tacticTypeTitle.length;
    $('#cTTcount').text(checkedTTcount);

    checkedOwnercount = selectedFilters.ownerName.length;
    $('#cOwnercount').text(checkedOwnercount);


    checkedStatuscount = selectedFilters.StatusIds.length;
    $('#cStatuscount').text(checkedStatuscount);

    TotalStatus = $("#ulStatus li").length;
    $('#tStatuscount').text('/' + TotalStatus);
}

var PreviousListFilter = {
    OwnerIds: [],
    TacticTypeIds: []
}
function GetLastSetofViews(presetName) {
    $.ajax({
        type: 'POST',
        url: $('#LastSetOfViewsURL').val(),
        data: { PresetName: presetName },
        dataType: "json",
        async: false,
        success: function (data) {

            if (data.returnURL != 'undefined' && data.returnURL == '#') {
                window.location = $('#HomeIndexURL').val();
            }
            else {
                if (data.StatusNAmes != null) {
                    if (data.StatusNAmes.length != 0 && data.StatusNAmes.length != undefined) {
                        $("#ulStatus li input[type=checkbox]").removeAttr('checked');
                        $("#ulStatus li ").removeClass("close-list");
                        for (i = 0 ; i < data.StatusNAmes.length; i++) {
                            $("#ulStatus li input[type=checkbox]").each(function () {
                                var Value = $(this).attr("id");
                                if (Value == data.StatusNAmes[i]) {
                                    filters.StatusIds.push(Value);
                                    $(this).attr('checked', 'checked');
                                    $(this).parent().addClass("close-list");
                                }
                            });
                        }
                    }
                    else {
                        $('#ulStatus').find("input[type=checkbox]").each(function () {
                            $(this).attr('checked', 'checked');
                            $(this).parent().addClass("close-list");
                        });
                    }
                }
                else {
                    $("#ulStatus li").each(function (i) {
                        $(this).removeClass("close-list");
                        var chkid = $(this).find("input[type=checkbox]").attr("id");
                        $("#" + chkid).removeAttr("checked");
                    });
                }
                var AutoSelectedCustomField = 0;
                if (data.Customfields.length != 0 && data.Customfields.length != undefined) {
                    for (i = 0 ; i < data.Customfields.length; i++) {
                        var CustomFieldId = data.Customfields[i].ID.split('_')[1];
                        var CustomFieldValues = data.Customfields[i].Value;
                        $('#divCustomFieldsFilter').find("input[type=checkbox]").each(function () {
                            var chkid = $(this).attr("id");
                            var chkfieldid = $(this).attr("id").split('_')[0];
                            var chkOptionid = $(this).attr("id").split('_')[1];
                            if (chkfieldid == CustomFieldId) {
                                if (CustomFieldValues.indexOf(chkOptionid) != -1) {
                                    $(this).attr('checked', 'checked');
                                    AutoSelectedCustomField++;
                                    filters.customFieldIds.push(chkid);
                                    $(this).parent().addClass("close-list");
                                }
                                else {
                                    $(this).removeAttr('checked');
                                }

                            }
                        });
                        $("#SelectedCustomFilter_" + CustomFieldId).text(AutoSelectedCustomField);
                        AutoSelectedCustomField = 0;
                    }
                }
                else {
                    $('#divCustomFieldsFilter').find("input[type=checkbox]").each(function () {
                        $(this).attr('checked', 'checked');
                        $(this).parent().addClass("close-list");
                        AutoSelectedCustomField++;
                    });
                    $('#ulSelectedFilter').find("li #CustomFields .sidebarliwidthSelected-right").text(FiltersAll);
                }
                if (data.OwnerNames.length != 0 && data.OwnerNames.length != undefined && data.OwnerNames != "All") {
                    filters.OwnerIds = [];
                    $("#ulSelectedOwner li input[type=checkbox]").removeAttr('checked');
                    $("#ulSelectedOwner li ").removeClass("close-list");
                    for (i = 0 ; i < data.OwnerNames.length; i++) {
                        PreviousListFilter.OwnerIds.push(data.OwnerNames[i]);
                        $("#ulSelectedOwner li input[type=checkbox]").each(function () {
                            var Value = $(this).attr("id");
                            if (Value == data.OwnerNames[i]) {
                                filters.OwnerIds.push(Value);
                                $(this).attr('checked', 'checked');
                                $(this).parent().addClass("close-list");
                            }
                        });
                    }
                }
                else if (data.OwnerNames == "All") {
                    $("#ulSelectedOwner li input[type=checkbox]").each(function () {
                        var Value = $(this).attr("id");
                        filters.OwnerIds.push(Value);
                        $(this).attr('checked', 'checked');
                        $(this).parent().addClass("close-list");
                    });
                }
                if (data.TTList != null) {
                    if (data.TTList.length != 0 && data.TTList.length != undefined && data.TTList != "All") {
                        filters.TacticTypeids = [];
                        $("#ulTacticType li input[type=checkbox]").removeAttr('checked');
                        $("#ulTacticType li ").removeClass("close-list");
                        for (i = 0 ; i < data.TTList.length; i++) {
                            PreviousListFilter.TacticTypeIds.push(data.TTList[i]);
                            $("#ulTacticType li input[type=checkbox]").each(function () {
                                var Value = $(this).attr("id").replace("CbTT", "");
                                if (Value == data.TTList[i]) {
                                    filters.TacticTypeids.push(Value);
                                    $(this).attr('checked', 'checked');
                                    $(this).parent().addClass("close-list");
                                }
                            });
                        }
                    }
                    else if (data.TTList == "All") {
                        $("#ulTacticType li input[type=checkbox]").each(function () {
                            var Value = $(this).attr("id").replace("CbTT", "");
                            filters.TacticTypeids.push(Value);
                            $(this).attr('checked', 'checked');
                            $(this).parent().addClass("close-list");
                        });
                    }
                    else {
                        $('#ulTacticType').find("input[type=checkbox]").each(function () {
                            $(this).attr('checked', 'checked');
                            $(this).parent().addClass("close-list");
                        });
                    }
                }
                else {
                    $("#ulTacticType li").each(function (i) {
                        $(this).removeClass("close-list");
                        var chkid = $(this).find("input[type=checkbox]").attr("id");
                        $("#" + chkid).removeAttr("checked");
                    });
                }
                if (data.Years != null) {
                    if (data.Years.length != 0 && data.Years.length != undefined) {
                        $("#ulSelectedYear li input[type=checkbox]").removeAttr('checked');
                        $("#ulSelectedYear li ").removeClass("close-list");
                        for (i = 0 ; i < data.Years.length; i++) {
                            $("#ulSelectedYear li input[type=checkbox]").each(function () {
                                var Value = $(this).attr("yearvalue");
                                if (Value == data.Years[i]) {
                                    filters.SelectedYears.push(Value);
                                    $(this).attr('checked', 'checked');
                                    $(this).parent().addClass("close-list");
                                }
                            });
                        }
                    }
                }
                if (presetName != 'undefined' && presetName != null && presetName != "") {
                    SaveLastSetofViews();
                }
                UpdateSelectedFilters();
            }
        }
    });
}

var currentRequestforTactics = null;
function GetTacticTypelist(Planids, async, OnPlanChange) {
    var asyncval = false;
    //if (async != undefined && async != 'undefined' && async != '' && async != null) {
    //    asyncval = async;
    //}
    $('#ulTacticType').html('');
    filters.TacticTypeids = [];
    currentRequestforTactics = $.ajax({
        beforeSend: function (x) {
            if (!asyncval) {
                myApp.showPleaseWait();
            }
            if (currentRequestforTactics != null) {
                currentRequestforTactics.abort();
            }
        },
        type: "GET",
        cache: false,
        async: asyncval,
        url: $('#TacticTypeURL').val(),
        data: {
            PlanId: Planids.toString()
        },
        dataType: "json",
        success: function (data) {
            if (data.returnURL != 'undefined' && data.returnURL == '#') {
                window.location = $('#ServerUnavailableURL').val();
            }
            else {
                if (data.isSuccess == true) {
                    LoadTacticTypelist(data);
                }
                else {
                    $("#TacticTypeAllModule").css("display", "none");
                    $("#NoTTFound").show();
                }
                TactictypeCount = $("#ulTacticType li").length;
                checkedTTcount = $("#ulTacticType li input:checkbox:checked").length;
                $('#tTTcount').text('/' + TactictypeCount);
                $('#cTTcount').text(checkedTTcount);
                filters.tempTacticTypeIds = [];
                async = true;
                currentRequestforTactics = null;
            }
        }
    });
}

var ids = [];
var IsDeselectAll = false
var currentRequest = null;
function GetOwnerListForFilter(Planids, async, OnPlanChange) {
    var asyncval = false;
    //if (async != undefined && async != 'undefined' && async != '' && async != null) {
    //    asyncval = async;
    //}
    //else {
    //    asyncval = false;
    //}
    $('#ulSelectedOwner').html('');
    filters.OwnerIds = [];
    currentRequest = $.ajax({
        beforeSend: function (x) {
            if (!asyncval) {
                myApp.showPleaseWait();
            }
            if (currentRequest != null) {
                currentRequest.abort();
            }
        },
        type: "GET",
        cache: false,
        async: asyncval,
        url: $('#OwnerListURL').val(),
        data: {
            PlanId: Planids.toString(),
            ViewBy: $('#PlanGantType').val(),
            ActiveMenu: $('#ActiveMenu').val()
        },
        dataType: "json",
        success: function (data) {
            if (data.returnURL != 'undefined' && data.returnURL == '#') {
                window.location = $('#ServerUnavailableURL').val();
            }
            else {
                if (data.isSuccess == true) {
                    var LoggedInOwner = data.LoggedInUser.OwnerId;
                    if (data.AllowedOwner != 'undefined' && data.AllowedOwner != null) {
                        OwnerCount = data.AllowedOwner.length;
                        if (OwnerCount > 0) {
                            var IsOwnerPresent = false;
                            var checked = "";
                            var checkedclass = "";
                            $("#ulOwnerAllModule").css("display", "block");
                            $("#NoOwnerFound").hide();
                            $('#ulSelectedOwner li').remove();
                            $.each(data.AllowedOwner, function (i, OwnerItem) {
                                if (OwnerItem.OwnerId == LoggedInOwner) {
                                    IsOwnerPresent = true;
                                }
                                checked = "checked"
                                checkedclass = "close-list"
                                if (filters.tempOwnerIds.length > 0) {
                                    if (filters.tempOwnerIds.indexOf(OwnerItem.OwnerId.toString()) >= 0) {
                                        checked = "";
                                        checkedclass = "";
                                        $('#ulSelectedOwner').append('<li class="accordion-inner ' + checkedclass + '" title="' + OwnerItem.Title + '" id="liOwner' + OwnerItem.OwnerId + '"><span class="sidebarliwidth">' + OwnerItem.Title + '</span><input type="checkbox" class="chkbxfilter" OwnerTitle="' + OwnerItem.Title + '" onchange="toggleOwner(this)" id="' + OwnerItem.OwnerId + '"' + checked + '></input></li>');/*<span class="chkbx-icon"></span>*/
                                    }
                                    else {
                                        $('#ulSelectedOwner').append('<li class="accordion-inner ' + checkedclass + '" title="' + OwnerItem.Title + '" id="liOwner' + OwnerItem.OwnerId + '"><span class="sidebarliwidth">' + OwnerItem.Title + '</span><input type="checkbox" class="chkbxfilter" OwnerTitle="' + OwnerItem.Title + '" onchange="toggleOwner(this)" id="' + OwnerItem.OwnerId + '"' + checked + '></input></li>');/*<span class="chkbx-icon"></span>*/
                                    }
                                }
                                else {
                                    $('#ulSelectedOwner').append('<li class="accordion-inner ' + checkedclass + '" title="' + OwnerItem.Title + '" id="liOwner' + OwnerItem.OwnerId + '"><span class="sidebarliwidth">' + OwnerItem.Title + '</span><input type="checkbox" class="chkbxfilter" OwnerTitle="' + OwnerItem.Title + '" onchange="toggleOwner(this)" id="' + OwnerItem.OwnerId + '"' + checked + '></input></li>');/*<span class="chkbx-icon"></span>*/
                                }
                            });
                            if (IsOwnerPresent == false) {
                                checked = "checked"
                                checkedclass = "close-list"
                                if (filters.tempOwnerIds.indexOf(LoggedInOwner.toString()) >= 0) {
                                    checked = "";
                                    checkedclass = "";
                                    $('#ulSelectedOwner').append('<li class="accordion-inner ' + checkedclass + '" title="' + data.LoggedInUser.Title + '" id="liOwner' + data.LoggedInUser.OwnerId + '"><span class="sidebarliwidth">' + data.LoggedInUser.Title + '</span><input type="checkbox" class="chkbxfilter" OwnerTitle="' + data.LoggedInUser.Title + '" onchange="toggleOwner(this)" id="' + data.LoggedInUser.OwnerId + '"' + checked + '></input></li>');/*<span class="chkbx-icon"></span>*/
                                }
                                else {
                                    $('#ulSelectedOwner').append('<li class="accordion-inner ' + checkedclass + '" title="' + data.LoggedInUser.Title + '" id="liOwner' + data.LoggedInUser.OwnerId + '"><span class="sidebarliwidth">' + data.LoggedInUser.Title + '</span><input type="checkbox" class="chkbxfilter" OwnerTitle="' + data.LoggedInUser.Title + '" onchange="toggleOwner(this)" id="' + data.LoggedInUser.OwnerId + '"' + checked + '></input></li>');/*<span class="chkbx-icon"></span>*/
                                }

                            }
                        }
                        else {
                            $("#ulOwnerAllModule").css("display", "none");
                            $("#NoOwnerFound").show();
                        }
                    }
                    else {
                        $("#ulOwnerAllModule").css("display", "none");
                        $("#NoOwnerFound").show();
                    }
                    checkedOwnercount = $("#ulSelectedOwner li input:checkbox:checked").length;
                    TotalOwnercount = $("#ulSelectedOwner li").length;
                    $('#tOwnercount').text('/' + TotalOwnercount);
                    $('#cOwnercount').text(checkedOwnercount);
                }
                selectedFilters.ownerName = [];
                $("#ulSelectedOwner li input[type=checkbox]:checked").each(function () {
                    selectedFilters.ownerName.push($(this).attr('ownertitle').toString());

                });
                if (selectedFilters.ownerName.length == 0) {
                    $('#lstOwner').text(FiltersNone);
                }
                else if ($("#ulSelectedOwner li").length == selectedFilters.ownerName.length) {
                    $('#lstOwner').text(FiltersAll);
                }
                else {
                    $('#lstOwner').text(selectedFilters.ownerName.join(", "));
                }
                filters.tempOwnerIds = [];
            }
            async = true;
            currentRequest = null;
        }
    });
}

var GetLastview = true;
var pageIsScroll = false;
var state0 = 0;
function LoadTacticTypelist(data) {
    if (data.TacticTypelist != 'undefined' && data.TacticTypelist != null) {
        TactictypeCount = data.TacticTypelist.length;
        if (TactictypeCount > 0) {
            $("#TacticTypeAllModule").css("display", "block");
            $("#NoTTFound").hide();
            $('#ulTacticType li').remove();
            $.each(data.TacticTypelist, function (i, TactictypeItem) {
                var checkedclass = "";
                var checked = "";
                checked = "checked"
                checkedclass = "close-list"
                if (filters.tempTacticTypeIds.length > 0) {
                    if (filters.tempTacticTypeIds.indexOf(TactictypeItem.TacticTypeId.toString()) >= 0) {
                        checked = "";
                        checkedclass = "";
                        $('#ulTacticType').append('<li class="accordion-inner ' + checkedclass + '" title="' + TactictypeItem.Title + ' (' + TactictypeItem.Number + ')" id="liTT' + TactictypeItem.TacticTypeId + '"><span class="sidebarliwidth">' + TactictypeItem.Title + " " + '(<span class="font-size14 TacticNumber" Previousvalue= ' + TactictypeItem.Number + '>' + TactictypeItem.Number + '</span>) </span><input type="checkbox" class="chkbxfilter" TacticTypeTitle="' + TactictypeItem.Title + '" onchange="toggleTacticType(this)" id="CbTT' + TactictypeItem.TacticTypeId + '"' + checked + '></input></li>');
                    }
                    else {
                        $('#ulTacticType').append('<li class="accordion-inner ' + checkedclass + '" title="' + TactictypeItem.Title + ' (' + TactictypeItem.Number + ')" id="liTT' + TactictypeItem.TacticTypeId + '"><span class="sidebarliwidth">' + TactictypeItem.Title + " " + '(<span class="font-size14 TacticNumber" Previousvalue= ' + TactictypeItem.Number + '>' + TactictypeItem.Number + '</span>) </span><input type="checkbox" class="chkbxfilter" TacticTypeTitle="' + TactictypeItem.Title + '" onchange="toggleTacticType(this)" id="CbTT' + TactictypeItem.TacticTypeId + '"' + checked + '></input></li>');
                    }
                }
                else {
                    $('#ulTacticType').append('<li class="accordion-inner ' + checkedclass + '" title="' + TactictypeItem.Title + ' (' + TactictypeItem.Number + ')" id="liTT' + TactictypeItem.TacticTypeId + '"><span class="sidebarliwidth">' + TactictypeItem.Title + " " + '(<span class="font-size14 TacticNumber" Previousvalue= ' + TactictypeItem.Number + '>' + TactictypeItem.Number + '</span>) </span><input type="checkbox" class="chkbxfilter" TacticTypeTitle="' + TactictypeItem.Title + '" onchange="toggleTacticType(this)" id="CbTT' + TactictypeItem.TacticTypeId + '"' + checked + '></input></li>');
                }
            });
            selectedFilters.tacticTypeTitle = [];
            $("#ulTacticType li input[type=checkbox]:checked").each(function () {
                selectedFilters.tacticTypeTitle.push($(this).attr('tactictypetitle').toString());
            });
            if (selectedFilters.tacticTypeTitle.length == 0) {
                $('#lstTType').text(FiltersNone);
            }
            else if ($("#ulTacticType li").length == selectedFilters.tacticTypeTitle.length) {

                $('#lstTType').text(FiltersAll);
            }
            else {
                $('#lstTType').text(selectedFilters.tacticTypeTitle.join(", "));
            }
        }
        else {
            $("#TacticTypeAllModule").css("display", "none");
            $("#NoTTFound").show();
        }
    }
}

function BulkTTOperation(selection) {
    GetLastview = false;
    filters.TacticTypeids = [];
    if (selection) {
        $("#ulTacticType li").each(function (i) {
            $(this).addClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).attr("checked", "checked");
            filters.TacticTypeids.push(chkid.replace("CbTT", ""));
        });
    }
    else {
        $("#ulTacticType li").each(function (i) {
            $(this).removeClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).removeAttr("checked");
        });
    }
    checkedTTcount = $("#ulTacticType li input:checkbox:checked").length;
    $('#cTTcount').text(checkedTTcount);
}

function BulkOwnerOperation(selection) {
    GetLastview = false;
    filters.OwnerIds = [];
    if (selection) {
        $("#ulSelectedOwner li").each(function (i) {
            $(this).addClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).attr("checked", "checked");
            filters.OwnerIds.push(chkid);
        });
    }
    else {
        IsDeselectAll = true;
        $("#ulSelectedOwner li").each(function (i) {
            $(this).removeClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).removeAttr("checked");
        });
    }
    checkedOwnercount = $("#ulSelectedOwner li input:checkbox:checked").length;
    $('#cOwnercount').text(checkedOwnercount);
}

function BulkCustomFieldOperation(selection, customFieldId, TotalCount) {
    GetLastview = false;
    if (selection) {
        $('#ulSelected' + customFieldId + ' li').each(function (i) {
            $(this).addClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).attr("checked", "checked");
        });
        $("#SelectedCustomFilter_" + customFieldId).text(TotalCount);
    }
    else {
        $('#ulSelected' + customFieldId + ' li').each(function (i) {
            $(this).removeClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).removeAttr("checked");
        });
        $("#SelectedCustomFilter_" + customFieldId).text("0");
    }
}

function BulkPlanOperation(selection) {
    GetLastview = false;
    var planids = [];
    if (selection) {
        $("#ulSelectedPlans li").each(function (i) {
            $(this).addClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).attr("checked", "checked");
            planids.push(chkid);
        });
    }
    else {
        IsDeselectAll = true;
        $("#ulSelectedPlans li").each(function (i) {
            $(this).removeClass("close-list");
            var chkid = $(this).find("input[type=checkbox]").attr("id");
            $("#" + chkid).removeAttr("checked");
        });
    }
    checkedPlancount = $("#ulSelectedPlans li input:checkbox:checked").length;
    $('#cPlancount').text(checkedPlancount);
}

function CancelSavePreset() {
    $("#txtPresetName").val("");
    $("#errorMessageMainSidebar").css("display", "none");
    $("#successMessageSidebar").css("display", "none");
    $("#successMessageLoadPreset").css("display", "none");
    $("#content_SaveFilters").css("display", "none");
    $("#DefaultFilters").css("display", "none");
    $("#PlanModulesSection").css("display", "block");
    $("#Filtersidebar").css("display", "block");
}

function ClearAllPreset() {
    BulkTTOperation(false);
    BulkStatusOperation(false);
    BulkOwnerOperation(false);    
    LoadPlanData(null);        
    BulkPlanOperation(false);
    $('#divCustomFieldsFilter').find("li").each(function (i) {
        $(this).removeClass("close-list");
        var chkid = $(this).find("input[type=checkbox]").attr("id");
        $("#" + chkid).removeAttr("checked");
    });
    $("#accordion-Year li input:checkbox:checked").each(function () {
        $(this).removeAttr("checked");
        $(this).parent().removeClass("close-list");
    });
    UpdateSelectedFilters();
}

function LoadPreset() {
    var isLoadPreset = true;
    $.ajax({
        type: 'POST',
        url: $('#LastSetOfViewsURL').val(),
        data: {
            isLoadPreset: isLoadPreset,
        },
        async: false,
        success: function (data) {
            if (data != null) {
                $('#DefaultFilters').css('display', 'block');
                $('#DefaultFilters').html(data);
            }
            else {
                $('#DefaultFilters').css('display', 'none');
            }
        }
    });
    $("#successMessageSidebar").css("display", "none");
    $("#DefaultFilters").css("display", "block");
    $("#PlanModulesSection").css("display", "none");
    $("#Filtersidebar").css("display", "none");
}

function SavePreset() {
    var ViewName = $("#txtPresetName").val();
    if (ViewName.trim() == null || ViewName.trim() == "") {
        $("#successMessageSidebar").css("display", "none");
        $('div[id=errorMessageMainSidebar]').each(function () {
            if ($(this).parent().is(':visible')) {
                $(this).show();
                $(this).find("span[id=spanErrorMessageRole]").text($('#ProvidePresetName').val());
            }
        });
        $("#txtPresetName").val("");
    }
    else {
        SavePresetValue = true;
        SaveLastSetofViews(ViewName.trim());
        UpdateResult();
    }
}

var isFiltered = false;
var SavePresetValue = false;
function UpdatePlan() {
    IsUpdate = true;
    isRequest = false;

    var planids = [];
    $('#ulSelectedPlans').find("input[type=checkbox]:checked").each(function () {
        var chkid = $(this).attr("id");
        if (chkid != undefined && chkid != 'undefined') {
            planids.push(chkid);
        }
    });
    if (planids.length > 0) {
        $('#divCustomFieldsFilter').find("input[type=checkbox]").each(function () {
            $(this).attr('checked', 'checked');
            $(this).parent().addClass("close-list");
        });
        $('#accordion-Status').find("input[type=checkbox]").each(function () {
            $(this).attr('checked', 'checked');
            $(this).parent().addClass("close-list");
        });
    } else {
        $('#divCustomFieldsFilter').find("input[type=checkbox]:checked").each(function () {
            $(this).removeAttr('checked');
            $(this).parent().removeClass("close-list");
        });
        $('#accordion-Status').find("input[type=checkbox]:checked").each(function () {
            $(this).removeAttr('checked');
            $(this).parent().removeClass("close-list");
        });
    }
    GetTacticTypelist(planids, false);
    GetOwnerListForFilter(planids, false);
    UpdateResult();
    GetMultiplePlanNames();
    GetGoalValues(urlContent + 'Home/GetGoalValues', filters.PlanIDs);

}

var isFiltered = false;
var IsUpdate = false;
var SavePresetValue = false;
function UpdateResult() {
    IsUpdate = true;
    isRequest = false; 
    GetFilterIds();
    //if (activeMenu == '@Enums.ActiveMenu.Plan.ToString().ToLower()') {
    if ($('#IsGridView').val().toLowerCase() == "true") {
        filters.customFieldIds = [];
        $('#divCustomFieldsFilter').find("input[type=checkbox]").each(function () {
            if ($(this).attr('checked') == 'checked') {
                var chkid = $(this).attr("id");
                if (chkid != undefined && chkid != 'undefined') {
                    filters.customFieldIds.push(chkid);
                }
            }
        });
        var CheckedCounter = 0, AllCounter = 0, id = null, UncheckedCounter = 0;
        $("#divCustomFieldsFilter").find("div.accordion").each(function () {
            if ($(this).find("input[type=checkbox]") != null || $(this).find("input[type=checkbox]") != "") {
                AllCounter = $(this).find("input[type=checkbox]").length;
                CheckedCounter = $(this).find("input[type=checkbox]:checked").length;
                UncheckedCounter = AllCounter - CheckedCounter;
                if (AllCounter == UncheckedCounter) {
                    var Id = $(this).attr("id");
                    if (Id.indexOf("-") >= 0) {
                        Id = Id.split('-')[1];
                        var CustomId = Id + "_null";
                        filters.customFieldIds.push(CustomId);

                    }
                }
                else if (AllCounter == CheckedCounter) {
                    id = this.id;
                    if (id != null && id != "" && id.indexOf("-") > -1) {
                        id = this.id.split("-")[1];
                    }
                    var i = 0, customfieldid;
                    for (i = 0; i < filters.customFieldIds.length; i++) {
                        if (filters.customFieldIds[i].indexOf("_") > -1) {
                            customfieldid = filters.customFieldIds[i].split("_")[0];
                            if (id == customfieldid) {
                                filters.customFieldIds.splice(i, 1);
                                i--;
                            }
                        }
                    }
                }
            }
        });

        var CustomFieldId = filters.customFieldIds;
        var OwnerIds = filters.OwnerIds;
        var TacticTypeids = filters.TacticTypeids;
        var StatusIds = filters.StatusIds;
        LoadPlanGrid();
        $("#totalEntity").text(ExportSelectedIds.TaskID.length);

    }
    else {
        //Need to Implement
        BindPlanCalendar();
    }
    GetHeadsUpData(urlContent + 'Plan/GetHeaderforPlanByMultiplePlanIDs', urlContent + 'Home/GetActivityDistributionchart', secHome, SelectedTimeFrameOption)
    UpdateSelectedFilters();
    if (SavePresetValue == false) {
        SaveLastSetofViews();
    }
    SavePresetValue = false;

    BindViewSelections(urlContent + 'Home/GetViewBylistData');
}

function GetPlanIds() {
    filters.PlanIDs = [];
    $("#ulSelectedPlans li input[type=checkbox]:checked").each(function () {
        var chkid = $(this).attr("id");
        filters.PlanIDs.push(chkid);
    });
    return filters.PlanIDs;
}
function GetFilterIds() {
    filters.SelectedYears = [];
    filters.PlanIDs = [];
    filters.customFieldIds = [];
    filters.OwnerIds = [];
    filters.TacticTypeids = [];
    filters.StatusIds = [];

    $("#ulSelectedYear li input[type=checkbox]:checked").each(function () {
        filters.SelectedYears.push($(this).attr("yearvalue"));
    });
    $("#ulSelectedPlans li input[type=checkbox]:checked").each(function () {
        var chkid = $(this).attr("id");
        filters.PlanIDs.push(chkid);
    });
    $("#ulSelectedOwner li input[type=checkbox]:checked").each(function () {
        filters.OwnerIds.push($(this).attr('id').toString());
    });
    $("#ulTacticType li input[type=checkbox]:checked").each(function () {
        var Value = $(this).attr("id").replace("CbTT", "");
            filters.TacticTypeids.push(Value);        
    });
    $("#ulStatus li input[type=checkbox]:checked").each(function () {
        filters.StatusIds.push($(this).attr('id').toString());
    });

    $('#divCustomFieldsFilter').find("input[type=checkbox]").each(function () {
        if ($(this).attr('checked') == 'checked') {
            var chkid = $(this).attr("id");
            if (chkid != undefined && chkid != 'undefined') {
                filters.customFieldIds.push(chkid);
            }
        }
    });
    var CheckedCounter = 0, AllCounter = 0, id = null, UncheckedCounter = 0;
    $("#divCustomFieldsFilter").find("div.accordion").each(function () {
        if ($(this).find("input[type=checkbox]") != null || $(this).find("input[type=checkbox]") != "") {
            AllCounter = $(this).find("input[type=checkbox]").length;
            CheckedCounter = $(this).find("input[type=checkbox]:checked").length;
            UncheckedCounter = AllCounter - CheckedCounter;
            if (AllCounter == UncheckedCounter) {
                var Id = $(this).attr("id");
                if (Id.indexOf("-") >= 0) {
                    Id = Id.split('-')[1];
                    var CustomId = Id + "_null";
                    filters.customFieldIds.push(CustomId);
                }
            }
            else if (AllCounter == CheckedCounter) {
                id = this.id;
                if (id != null && id != "" && id.indexOf("-") > -1) {
                    id = this.id.split("-")[1];
                }
                var i = 0, customfieldid;
                for (i = 0; i < filters.customFieldIds.length; i++) {
                    if (filters.customFieldIds[i].indexOf("_") > -1) {
                        customfieldid = filters.customFieldIds[i].split("_")[0];
                        if (id == customfieldid) {
                            filters.customFieldIds.splice(i, 1);
                            i--;
                        }
                    }
                }
            }
        }
    });
    return filters;
}

function ApplyFilters() {
    $.ajax({
        url: $('#GetCacheValue').val(),
        async: false,
        data: {
        },
        success: function (result) {
        }
    });
    UpdateResult();
}

var IsPlanChange = false;
function SaveLastSetofViews(ViewName) {
    filters.customFieldIds = [];
    filters.TacticTypeids = [];
    filters.StatusIds = [];
    filters.OwnerIds = [];
    filters.parentCustomFieldsIds = [];
    $("#ulSelectedOwner li input:checkbox:checked").map(function () {
        filters.OwnerIds.push($(this).attr("id"));
    });
    $("#ulTacticType li input[type=checkbox]").each(function () {
        var chkid = $(this).attr("id");
        if ($(this).is(':checked')) {
            filters.TacticTypeids.push(chkid.replace("CbTT", ""));
        }
    });
    $('#divCustomFieldsFilter').children().each(function () {
        filters.parentCustomFieldsIds.push($(this).attr('id').split('-')[1])
    });
    $('#divCustomFieldsFilter').find("input[type=checkbox]").each(function () {
        if ($(this).attr('checked') == 'checked') {
            var chkid = $(this).attr("id");
            if (chkid != undefined && chkid != 'undefined') {
                filters.customFieldIds.push(chkid);
            }
        }
    });
    filters.SelectedPlans = [];
    $('#ulSelectedPlans').find("input[type=checkbox]").each(function () {
        if ($(this).attr('checked') == 'checked') {
            var chkid = $(this).attr("id");
            if (chkid != undefined && chkid != 'undefined') {
                filters.SelectedPlans.push(chkid);
            }
        }
    });
    $("#ulStatus li input[type=checkbox]").each(function () {
        var chkid = $(this).attr("id");
        if ($(this).is(':checked')) {
            filters.StatusIds.push(chkid);
        }
    });
    filters.SelectedYears = [];
    var listCheckbox = $("#ulSelectedYear").find("input[type=checkbox]");
    years = "";
    $.each(listCheckbox, function () {
        if ($(this).attr("checked")) {
            years += $(this).attr('yearValue') + ",";
        }
    });
    years = years.slice(0, -1);
    if (years != null && years != "") {
        filters.SelectedYears.push(years);
    }
    var Planid = filters.SelectedPlans;
    var CustomFieldId = filters.customFieldIds;
    var OwnerIds = filters.OwnerIds;
    var TacticTypeids = filters.TacticTypeids;
    var GetSelectedPlanNo = filters.SelectedPlans.length;
    var StatusIds = filters.StatusIds;
    var ParentCustomFieldsIds = filters.parentCustomFieldsIds;
    if (filters.TacticTypeids.length != 0 && $("#ulTacticType li input[type=checkbox]").length == filters.TacticTypeids.length) {
        TacticTypeids = 'All';
    }
    if (filters.OwnerIds.length != 0 && $("#ulSelectedOwner li input[type=checkbox]").length == filters.OwnerIds.length) {
        OwnerIds = 'All';
    }
    $.ajax({
        type: "POST",
        cache: false,
        async: false,
        url: $('#SaveLastSetofViews').val(),
        data: {
            PlanId: Planid.toString(),
            customFieldIds: CustomFieldId.toString(),
            ownerIds: OwnerIds.toString(),
            TacticTypeid: TacticTypeids.toString(),
            StatusIds: filters.StatusIds.toString(),
            ViewName: ViewName,
            SelectedYears: filters.SelectedYears.toString(),
            ParentCustomFieldsIds: filters.parentCustomFieldsIds.toString()

        },
        dataType: "json",
        success: function (data) {
            if (data.isSuccess == true) {
                if (data.ViewName != null && data.ViewName != "" && data.ViewName != undefined) {
                    $("#content_SaveFilters").css("display", "none");
                    $("#Filtersidebar").css("display", "block");
                    $("#errorMessageMainSidebar").css("display", "none");
                    $('div[id=successMessageSidebar]').each(function () {
                        if ($(this).parent().is(':visible')) {
                            $(this).show();
                            $(this).find("span[id=spanSuccessMessageRole]").text($('#SavePresetSuccess').val());
                        }
                    });
                    $("#txtPresetName").val('');
                }
            }
            else {
                $("#successMessageSidebar").css("display", "none");
                $('div[id=errorMessageMainSidebar]').each(function () {
                    if ($(this).parent().is(':visible')) {
                        $(this).show();
                        $(this).find("span[id=spanErrorMessageRole]").text(data.msg);
                    }
                });
            }
        }
    });
}

function SaveAsPreset() {
    var IsAnyPlanSelected = $('#ulSelectedPlans').find("li").hasClass("close-list");
    var IsAnyCustomFieldSelected = $('#divCustomFieldsFilter').find("li").hasClass("close-list");
    var IsAnyTTSelected = $('#ulTacticType').find("li").hasClass("close-list");
    var IsAnyStatusSelected = $('#ulStatus').find("li").hasClass("close-list");
    var IsAnyOwnerSelected = $('#ulSelectedOwner').find("li").hasClass("close-list");
    if (!IsAnyCustomFieldSelected && !IsAnyPlanSelected && !IsAnyTTSelected && !IsAnyStatusSelected && !IsAnyOwnerSelected) {
        alert('Please select at least one filter');
        return false;
    }
    else {
        $("#content_SaveFilters").css("display", "block");
        $("#PlanModulesSection").css("display", "none");
        $("#Filtersidebar").css("display", "none");
    }
}
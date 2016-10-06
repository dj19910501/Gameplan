var isCopyTacticHomeGrid = 0;
var isEditTacticHomeGrid = 0;

///Manage Calendar/PlanGrid/Budget Icon Click
$('#btngridcalendar').click(function () {
    IsBudgetGrid = false
    BindUpcomingActivites(filters.PlanIDs.toString())
    //cleare success msg as we want to hide import msg on click of grid or calendar
    $('#SuccessMsg').css('display', 'none');
    if ($('#errorMsg').css('display') == 'block') {
        $('#errorMsg').css('display', 'none');
    }
    // get scroll set of selected row from grid to calendar
    scrollstate = {
        y: HomeGrid.objBox.scrollTop,
        x: HomeGrid.objBox.scrollLeft,
    }

    $('#exp-serach').css('display', 'none');
    if ($('#btnbudget').hasClass('P-icon-active')) {
        isCalendarView = true;
        $('#IsGridView').val('false');
        BindPlanCalendar();
    }
    else {
        if ($('#IsGridView').val().toLowerCase() == "false") {
            scrollstate = gantt.getScrollState();
            isCalendarView = false;
            SetcookieforSaveState();
            LoadPlanGrid();
            $('#IsGridView').val('true');

        } else {
            isCalendarView = true;
            $('#IsGridView').val('false');
            HomeGrid.saveOpenStates("plangridState");
            BindPlanCalendar();
        }
    }
    RemoveAllHoneyCombData();
    RefershPlanHeaderCalc();
    ShowhideDataonGridCalendar();
    GlobalSearch();
});
//load context from calendar to grid:Context management #2677
function SetcookieforSaveState()
{
    var c = [];
    gantt.eachTask(function (task) {

        if (task.$open == true) {
            c.push(task.id)
        }
    });
    var d = "gridOpenplangridState=" + c.join("|") + "; ";
    document.cookie = d
}
function ShowhideDataonGridCalendar() {
    if ($('#IsGridView').val().toLowerCase() == "true") {
        $("#GridGanttContent").empty();
        $("#GridGanttContent").hide();
        $('#divupcomingact').hide();
        $("#divgridview").show();
        $('.export-dd').find('#ExportXls').show();
        $('.export-dd').find('#ExportPDf').hide();
        $('#ChangeView').hide();
        isCalendarView = false;
        $('#btngridcalendar').removeClass('P-icon-active');
    }
    else {
        $('.export-dd').find('#ExportXls').hide();
        $('.export-dd').find('#ExportPDf').show();
        $('#divupcomingact').show();
        $('#divgridview').empty();
        $("#GridGanttContent").show();
        $("#divgridview").hide();
        isCalendarView = true;
        $('#btngridcalendar').addClass('P-icon-active');
    }
    $('#divgridview').removeClass('budget-grid');
   // $('#txtGlobalSearch').val('');
    IsBudgetGrid = false;
    $('#ImportBtn').parent().removeClass('round-corner');
    $('#ImportBtn').hide();
    $('#btnbudget').removeClass('P-icon-active');    
}

$('#btnbudget').click(function () {
    IsBudgetGrid = true;
    BindUpcomingActivites(filters.PlanIDs.toString())
    //cleare success msg as we want to hide import msg on click of grid or calendar
    $('#SuccessMsg').css('display', 'none');
    if ($('#errorMsg').css('display') == 'block') {
        $('#errorMsg').css('display', 'none');
    }
    scrollstate = {
    y: HomeGrid.objBox.scrollTop,
        x: HomeGrid.objBox.scrollLeft,
    }
    isCalendarView = false;
    IsGridView = false;
    $('#IsGridView').val('false');
    $('#ChangeView').hide();
    $('#exp-serach').css('display', 'none');
    HomeGrid.saveOpenStates("plangridState");		 
    RemoveAllHoneyCombData();
    LoadBudgetGrid();
    ShowHideDataonBudgetScreen();
    GlobalSearch();

});

function ShowHideDataonBudgetScreen() {
    $('#ImportBtn').parent().addClass('round-corner');
   // $('#txtGlobalSearch').val('');
    IsBudgetGrid = true;
    $('#divupcomingact').show();
    $('#btngridcalendar').removeClass('P-icon-active');
    $('#btnbudget').addClass('P-icon-active');
    $("#GridGanttContent").empty();
    $("#GridGanttContent").hide();
    $('#divgridview').empty();
    $("#divgridview").show();
    $('#ImportBtn').show();
    $('.export-dd').find('#ExportXls').show();
    $('.export-dd').find('#ExportPDf').hide();
}

$('#ChangeView').click(function () {
    if (isCalendarView) {
        SetcookieforSaveState();
        scrollstate = gantt.getScrollState();
    }
    else {
        scrollstate = {
            y: HomeGrid.objBox.scrollTop,
            x: HomeGrid.objBox.scrollLeft,
        }
        HomeGrid.saveOpenStates("plangridState");
    }
    LoadPlanGrid();
    $('#IsGridView').val('true');
    IsBudgetGrid = false;
    if ($('#errorMsg').css('display') == 'block') {
        $('#errorMsg').css('display', 'none');
    }
    if ($('#ExpClose').css('display') == 'block') {
        $('#ExpClose').css('display', 'none');
        $('#ExpSearch').css('display', 'block');
    }
    BindUpcomingActivites(filters.PlanIDs.toString())
    RefershPlanHeaderCalc();
    ShowhideDataonGridCalendar();
});

//Added by Rahul Shah to call budget data 
function LoadBudgetGrid() {
    filters = GetFilterIds(); 
   // BindUpcomingActivites(filters.PlanIDs.toString())
 var selectedTimeFrame = $('#ddlUpComingActivites').val();
    var currentDate = new Date();
    if (selectedTimeFrame == null || selectedTimeFrame == 'undefined' || selectedTimeFrame == "") {
        selectedTimeFrame = currentDate.getFullYear().toString();
    }
    var viewBy = $('#ddlTabViewBy').val();
    $.ajax({
        url: urlContent + 'Plan/GetBudgetData/',
        data: {
            planIds: filters.PlanIDs.toString(),
            ownerIds: filters.OwnerIds.toString(),
            TacticTypeids: filters.TacticTypeids.toString(),
            StatusIds: filters.StatusIds.toString(),
            customFieldIds: filters.customFieldIds.toString() ,
            year: selectedTimeFrame.toString(),
            ViewBy:viewBy

        },
        success: function (result) {
            $('#exp-serach').css('display', 'block'); // To load dropdown after grid is loaded  ticket - 2596
            var gridhtml = '<div id="NodatawithfilterGrid" style="display:none;">' +
    '<span class="pull-left margin_t30 bold " style="margin-left: 20px;">No data exists. Please check the filters or grouping applied.</span>' +
'<br/></div>';
            gridhtml += result;
            $("#divgridview").html('');
            $("#divgridview").html(gridhtml);
            $("div[id^='LinkIcon']").each(function () {
                bootstrapetitle($(this), 'This tactic is linked to ' + "<U>" + htmlDecode($(this).attr('linkedplanname') + "</U>"), "tipsy-innerWhite");
            });
            $('#ChangeView').show();
        }
    });
}

//insertation start by kausha 21/09/2016 #2638/2592 Export to excel
var exportgridData;
var gridname;
//insertation end by kausha 21/09/2016 #2638/2592 Export to excel
//Function To Call HomeGrid Data for Selected Plan
function LoadPlanGrid() {
    filters = GetFilterIds();
    var viewBy = $('#ddlTabViewBy').val();
  
    $.ajax({
        url: urlContent + 'Plan/GetHomeGridData/',
        data: {
            planIds: filters.PlanIDs.toString(),
            ownerIds: filters.OwnerIds.toString(),
            TacticTypeid: filters.TacticTypeids.toString(),
            StatusIds: filters.StatusIds.toString(),
            customFieldIds: filters.customFieldIds.toString(),
            viewBy: viewBy
        },
        success: function (result) {
            $('#exp-serach').css('display', 'block'); // To load dropdown after grid is loaded  ticket - 2596
            var gridhtml = '<div id="NodatawithfilterGrid" style="display:none;">' +
    '<span class="pull-left margin_t30 bold " style="margin-left: 20px;">No data exists. Please check the filters or grouping applied.</span>' +
'<br/></div>';
            gridhtml += result;
            $("#divgridview").html('');
            $("#divgridview").html(gridhtml);
            $("div[id^='LinkIcon']").each(function () {
                bootstrapetitle($(this), 'This tactic is linked to ' + "<U>" + htmlDecode($(this).attr('linkedplanname') + "</U>"), "tipsy-innerWhite");
            });
        }
    });
}
//End

//Add Click Popup display  
//Start
function DisplayEditablePopup(id, type) {
    gridSearchFlag = 1;
    isEditTactic = id;
    isDataModified = false;
    if (type == "Plan") {
        ShowModels(inspectEdit, secPlan, id, 0, RequestedModule);
    }
    else if (type == "CP") {
        ShowModels(inspectEdit, secCampaign, id, 0, RequestedModule);
    }
    else if (type == "PP") {
        ShowModels(inspectEdit, secProgram, id, 0, RequestedModule);
    }
    else if (type == "TP") {
        ShowModels(inspectEdit, secTactic, id, 0, RequestedModule);
    }
    else if (type == "LP") {
        ShowModels(inspectEdit, secLineItem, id, 0, RequestedModule);
    }
}

function ShowModels(mode, section, id, ParentId, RequestedModule) {
    removeDefaultModalPopupBackgroungColor();
    modalFullPosition();
    $('.modal-backdrop').addClass('modalFull-backdrop');
    $('.modal-backdrop').attr("style", "display:none !important;");
    $("#successMessage").css("display", "none");
    $("#errorMessage").css("display", "none");
    $("#successMessageDuplicatePlan").css("display", "none");
    $("#modal-container-186470").modal('show');
    ParentId = typeof ParentId != 'undefined' ? ParentId : '0';
    RequestedModule = typeof RequestedModule != 'undefined' ? RequestedModule : '';

    $("#hdnRequestedModule").val(RequestedModule);
    var currentTab = "Setup";
    if (section == "campaign") {
        if (mode == "Add") {
            loadInspectPopup("0", "campaign", currentTab, "Add", ParentId, RequestedModule);
        }
        else {
            loadInspectPopup(id, "campaign", currentTab, "Edit", ParentId, RequestedModule);
        }
    }
    else if (section == "program") {
        if (mode == "Add") {
            loadInspectPopup("0", "program", currentTab, "Add", ParentId, RequestedModule);
        }
        else {
            loadInspectPopup(id, "program", currentTab, "Edit", ParentId, RequestedModule);
        }
    }
    else if (section == "tactic") {
        if (mode == "Add") {
            loadInspectPopup("0", "tactic", currentTab, "Add", ParentId, RequestedModule);
        }
        else {
            loadInspectPopup(id, "tactic", currentTab, "Edit", ParentId, RequestedModule);
        }
    }
    else if (section == "improvementtactic") {
        if (mode == "Add") {
            loadInspectPopup(0, "improvementtactic", currentTab, "Add", ParentId, RequestedModule);
        }
        else {
            loadInspectPopup(id, "improvementtactic", currentTab, "Edit", ParentId, RequestedModule);
        }
    }
    else if (section == "lineitem") {
        if (mode == "Add") {
            loadInspectPopup("0", "lineitem", currentTab, "Add", ParentId, RequestedModule);
        }
        else {
            loadInspectPopup(id, "lineitem", currentTab, "Edit", ParentId, RequestedModule);
        }
    }
    else if (section == "plan") {
        if (mode == "Add") {
            loadInspectPopup("0", "plan", currentTab, "Add", ParentId, RequestedModule);
        }
        else {
            loadInspectPopup(id, "plan", currentTab, "Edit", ParentId, RequestedModule);
        }
    }
};

function loadInspectPopup(id, section, tabValue, mode, parentId) {
       $('#modalMainContainer').show(); // Added to remove extra space Below Grid/Calendar Table
    logMixpanelTrack("Open " + section + " inspection window.");
    parentId = typeof parentId !== 'undefined' ? parentId : '0';
    var url = urlContent + 'Inspect/LoadInspectPopup/';
    $("#divPartial").empty();
    $("#divPartial").load(url + '?id=' + id + '&Section=' + section + '&TabValue=' + tabValue + '&InspectPopupMode=' + mode + '&parentId=' + parentId + '&RequestedModule=' + reaIndex, function (response, status, xhr) {
        if (response == '{"serviceUnavailable":"~/Login/ServiceUnavailable"}') {
            window.location = urlServiceUnavailibility;
        }
    });
}
///Added by Rahul Shah to open Inpection window on click ok ViewIcon for Plan Grid
function DisplayPopup(item) {
    inspectCloseFocus = $(item).position().top;
    var id = $(item).parent().prev().html();
    var type = $(item).attr('id');
    gridSearchFlag = 1;
    DisplayEditablePopup(id, type);
}

function DisplayPopUpMenu(obj, e) {
    gridSearchFlag = 1;
    var LinkTacticPermission = $(obj).attr('linktacticpermission');
    var LinkedTacticId = $(obj).attr('linkedtacticid');
    var type = $(obj).attr('id');
    var name = $(obj).attr('Name');
    var title = $(obj).attr('aria-label');
    var permission = $(obj).attr('Permission');
    var LineType = $(obj).attr('lineitemtype');
    if ($('#IsGridView').val().toLowerCase() == "true" || IsBudgetGrid) {

        LinkTacticPermission = $(obj).attr('linktacticper');
        LinkedTacticId = $(obj).attr('linkedtacticid');
        type = $(obj).attr('id');
        name = $(obj).attr('alt');
        title = $(obj).parent().prev().attr('title');
        permission = $(obj).attr('per');
        LineType = $(obj).attr('lt');

    }
    var name_Id = name.split("_");
    var PlanId;
    var CampaignId;
    var ProgramId;
    var TacticId;
    var ImpTacticId;
    var LineId;

    if (name_Id.length == 1) {
        PlanId = name_Id[0].replace("L", "");
    }
    else if (name_Id.length == 2) {
        var nameindex = name.indexOf("Z");
        if (nameindex >= 0) {
            PlanId = name_Id[1].replace("L", "");
            CampaignId = name_Id[1].replace("C", "");
        } else {
            PlanId = name_Id[0].replace("L", "");
            CampaignId = name_Id[1].replace("C", "");
        }
    }
    else if (name_Id.length == 3) {
        var nameindex = name.indexOf("Z");
        if (nameindex >= 0) {
            PlanId = name_Id[1].replace("L", "");
            CampaignId = name_Id[2].replace("C", "");
        } else {
            PlanId = name_Id[0].replace("L", "");
            CampaignId = name_Id[1].replace("C", "");
            ProgramId = name_Id[2].replace("P", "");
        }
    }
    else if (name_Id.length == 4) {
        var nameindex = name.indexOf("Z");
        if (nameindex >= 0) {
            PlanId = name_Id[1].replace("L", "");
            CampaignId = name_Id[2].replace("C", "");
            ProgramId = name_Id[3].replace("P", "");
        } else {
            PlanId = name_Id[0].replace("L", "");
            CampaignId = name_Id[1].replace("C", "");
            ProgramId = name_Id[2].replace("P", "");
            TacticId = name_Id[3].replace("T", "");
        }
    }
    else {
        var nameindex = name.indexOf("Z");
        if (nameindex >= 0) {
            PlanId = name_Id[1].replace("L", "");
            CampaignId = name_Id[2].replace("C", "");
            ProgramId = name_Id[3].replace("P", "");
            TacticId = name_Id[4].replace("T", "");
        } else {
            PlanId = name_Id[0].replace("L", "");
            CampaignId = name_Id[1].replace("C", "");
            ProgramId = name_Id[2].replace("P", "");
            TacticId = name_Id[3].replace("T", "");
            LineId = name_Id[4];
        }
    }
    var ul;
    if (type == "Plan") {
        if (permission == "true") {
            if (PlanId == "000") { // bind only new plan option for bank row
                ul = "<ul style='margin: 0;'>  <li class='new-prog' id='NewPlan'>New Plan </li></ul>";
            }
            else {
            if ('@IsPlanCreateAuthorized') {
                ul = "<ul style='margin: 0;'>  <li class='new-prog' id='NewPlan'>New Plan </li> <li class='new-prog' id='ClonePlan1'>Copy Plan</li>  <li class='new-prog' id='ChildCampaign'>New Campaign</li> </ul>";
            }
            else {
                ul = "<ul style='margin: 0;'> <li class='new-prog' id='ChildCampaign'>New Campaign</li> </ul>";
            }
        }
        }
        else {
            $('#popupType').css('display', 'none');
        }
    }
    else if (type == "Program") {
        if (permission == "true") {
            ul = "<ul style='margin: 0;'>  <li class='new-prog' id='NewProgram'>New Program </li> <li class='new-prog' id='CloneProgram1'>Copy Program </li>  <li class='new-prog' id='ChildTactic'>New Tactic</li> <li class='new-prog CopyEntity' sectionType='" + type + "' entityId='" + PlanId + "_" + ProgramId + "' id='lblCopyTo' popupType='Copying'  onclick='OpentCopyPopup(this)'>Copy To</li></ul>";
        }
        else {
            $('#popupType').css('display', 'none');
        }
    }
    else if (type == "Tactic") {
        if (permission == "true") {

            if (LinkTacticPermission.toLowerCase() == "true" && (LinkedTacticId == "null" || LinkedTacticId == "0")) {
                ul = "<ul style='margin: 0;'>  <li class='new-prog' id='NewTactic1'>New Tactic </li> <li class='new-prog' id='CloneTactic1'>Copy Tactic </li>  <li class='new-prog' id='ChildLineItem'>New Line Item </li> <li class='new-prog CopyEntity' sectionType='" + type + "' entityId='" + PlanId + "_" + TacticId + "' id='lblCopyTo' onclick='OpentCopyPopup(this)' popupType='Copying'>Copy To</li> <li class='link-icon' sectionType='" + type + "' entityId='" + PlanId + "_" + TacticId + "' id='lblLinkTo' onclick='OpentCopyPopup(this)'popupType='Linking'><i class='fa fa-link'></i>Link To</li></ul>";
            }
            else {
                ul = "<ul style='margin: 0;'>  <li class='new-prog' id='NewTactic1'>New Tactic </li> <li class='new-prog' id='CloneTactic1'>Copy Tactic </li>  <li class='new-prog' id='ChildLineItem'>New Line Item </li> <li class='new-prog CopyEntity' sectionType='" + type + "' entityId='" + PlanId + "_" + TacticId + "' id='lblCopyTo' popupType='Copying' onclick='OpentCopyPopup(this)'>Copy To</li></ul>";
            }
        }
        else {
            $('#popupType').css('display', 'none');
        }
    }

    else if (type == "Campaign") {
        if (permission == "true") {
            ul = "<ul style='margin: 0;'>  <li class='new-prog' id='NewCampaign'>New Campaign</li> <li class='new-prog' id='CloneCampaign1'>Copy Campaign</li>  <li class='new-prog' id='ChildProgram'>New Program</li> <li class='new-prog CopyEntity' sectionType='" + type + "' entityId='" + PlanId + "_" + CampaignId + "' id='lblCopyTo' onclick='OpentCopyPopup(this)' popupType='Copying'>Copy To</li></ul>";
        }
        else {
            $('#popupType').css('display', 'none');
        }
    }
    else if (type == "Line") {
        if (permission == "true") {
            if (LineType == "0") {
                ul = "<ul style='margin: 0;'>  <li class='new-prog' id='NewLineItem'>New Line Item</li> </ul>";
            }
            else {
                ul = "<ul style='margin: 0;'>  <li class='new-prog' id='NewLineItem'>New Line Item</li> <li class='new-prog' id='CloneLineItem'>Copy Line Item</li>  </ul>";

            }
        }
        else {
            $('#popupType').css('display', 'none');
        }
    }
    else {
        if (permission == "true") {
            ul = "<ul style='margin: 0;'>  <li class='new-prog' id='ImpsTactic'>New " + type + "</li> </ul>";
        }
        else {
            $('#popupType').css('display', 'none');
        }
        if (name_Id.length != 1 && name_Id.length != 0) {
            var nameindex = name.indexOf("Z");
            if (nameindex >= 0) {
                ImpTacticId = name_Id[2].replace("M", "");
            } else {
                ImpTacticId = name_Id[1].replace("M", "");
            }
        }
    }
    if (permission == "true") {
        $('#popupType').css('display', 'block');
    }
    $('#popupType').html(ul);

    var left = e.pageX;
    var target = $(e.target);
    var targetOffset = target.offset().top;
    var scrollPosition = $(window).scrollTop();

    if ($('#popupType').css('display') != 'none') {
        if (scrollPosition <= targetOffset) {
            $('#popupType').css({
                'top': targetOffset,
                'left': left,
            });
        }
        else {
            var targetHeight = target.height();
            var contentHeight = $('#popupType').outerHeight();
            var targetBottomOffset = targetOffset + targetHeight - contentHeight;
            $('#popupType').css({
                'top': targetBottomOffset,
                'left': left,
            });
        }
    }
    e.stopPropagation();
    $("#NewPlan").click(function (e) {
       //Open inspect popup for add new plan added by devanshi #2587
        ShowModels(inspectAdd, secPlan, " ", 0, RequestedModule);

    });

    $('#ClonePlan1').click(function () {
        var planId = PlanId;
        if (parseInt(planId) > 0) {
            $.ajax({
                type: 'POST',
                url: urlContent + 'Plan/Clone/',
                data: {
                    CloneType: planCloneType,
                    Id: PlanId,
                    title: ''
                },
                success: function (data) {
                    if (data.returnURL != 'undefined' && data.returnURL == '#') {
                        window.location = urlContent + '/Login/Index';
                    }
                    else {
                        if (data.redirect) {
                            $("#successMessage").css("display", "block");
                            $("#spanMessageSuccess").empty();
                            $("#spanMessageSuccess").text(data.msg);
                            logMixpanelTrack("Copy Plan:" + data.msg);
                            window.location = data.redirect;
                            return;
                        }
                        if (data.errormsg != '') {
                            $('#cErrorDuplicatePlan').html('<strong>Error! </strong> ' + $('#cErrorDuplicatePlan').html());
                            return;
                        }

                    }
                }
            });
        }
    });

    $('#ChildCampaign').click(function () {
        isCopyTactic = PlanId;
        var planId = PlanId;
        ShowModels(inspectAdd, secCampaign, 0, planId, '@   q');
        $('.taskpopup').css('display', 'none');
    });

    //new Campaign
    $('#NewCampaign').click(function () {
        isCopyTactic = PlanId;
        var planId = PlanId;
        ShowModels(inspectAdd, secCampaign, 0, planId, RequestedModule);
        $('.taskpopup').css('display', 'none');
    });

    ////Copy Campaign.
    $('#CloneCampaign1').click(function () {
        if ($('#IsGridView').val().toLowerCase() == "false") {
            scrollstate = gantt.getScrollState();
            bodyscrollpos = $(window).scrollTop();
        }
        isCopyTactic = CampaignId;
        var idPlanCamaign = CampaignId;
        var Title = $("<div/>").text(title).html();
        $.ajax({
            type: 'POST',
            url: urlContent + 'Inspect/Clone/',
            data: {
                CloneType: campaignCloneType,
                Id: idPlanCamaign,
                title: Title,
                CalledFromBudget: CalledFromBudget,
                RequsetedModule: RequestedModule,
                planId: PlanId
            },
            success: function (data) {
                if (data.IsSuccess != 'undefined' && data.IsSuccess == '#') {
                    window.location = urlContent + '/Login/Index';
                }
                else if (data.IsSuccess) {
                    RefershPlanHeaderCalc();
                    isCopyTactic = data.Id;
                    isCopyTacticHomeGrid = isCopyTactic;
                    if ($('#IsGridView').val().toLowerCase() == "false") {
                        BindPlanCalendar();
                    }
                    else {
                        LoadPlanGrid();
                    }
                    isError = false;
                    ShowMessage(isError, data.msg);
                    logMixpanelTrack("Copy Campaign:" + data.msg);
                    return;
                }
                else {
                    isError = true;
                    ShowMessage(isError, data.msg);
                    return false;
                }
            }
        });
    });
    $('#ChildProgram').click(function () {
        if ($('#IsGridView').val().toLowerCase() == "false") {
            scrollstate = gantt.getScrollState();
            bodyscrollpos = $(window).scrollTop();
        }
        isCopyTactic = CampaignId;
        ShowModels(inspectAdd, secProgram, 0, CampaignId, RequestedModule);
        $('.taskpopup').css('display', 'none');
    });

    //New Program
    $('#NewProgram').click(function () {
        isCopyTactic = CampaignId;
        ShowModels(inspectAdd, secProgram, 0, CampaignId, RequestedModule);
        $('.taskpopup').css('display', 'none');
    });

    //Copy Program
    $('#CloneProgram1').click(function () {
        if ($('#IsGridView').val().toLowerCase() == "false") {
            scrollstate = gantt.getScrollState();
            bodyscrollpos = $(window).scrollTop();
        }
        isCopyTactic = ProgramId;
        var idPlanProgram = ProgramId;
        var Title = $("<div/>").text(title).html();
        $.ajax({
            type: 'POST',
            url: urlContent + 'Inspect/Clone/',
            data: {
                CloneType: programCloneType,
                Id: idPlanProgram,
                title: Title,
                CalledFromBudget: CalledFromBudget,
                RequsetedModule: RequestedModule
            },
            success: function (data) {
                if (data.IsSuccess != 'undefined' && data.IsSuccess == '#') {
                    window.location = urlContent + '/Login/Index';
                }
                else if (data.IsSuccess) {
                    RefershPlanHeaderCalc();
                    isCopyTactic = data.Id;
                    isCopyTacticHomeGrid = isCopyTactic;
                    if ($('#IsGridView').val().toLowerCase() == "false") {
                        BindPlanCalendar();
                    }
                    else {
                        LoadPlanGrid();
                    }
                    isError = false;
                    ShowMessage(isError, data.msg);
                    logMixpanelTrack("Copy Program:" + data.msg);
                    return;
                }
                else {
                    isError = true;
                    ShowMessage(isError, data.msg);
                    return false;
                }
            }
        });
    });

    //Child Tactic
    $("#ChildTactic").on("click", function () {
        if ($('#IsGridView').val().toLowerCase() == "false") {
            scrollstate = gantt.getScrollState();
            bodyscrollpos = $(window).scrollTop();
        }
        isCopyTactic = ProgramId;
        ShowModels(inspectAdd, secTactic, 0, ProgramId, RequestedModule);
        $("#errorMessage").css("display", "none");
        $("#successMessage").css("display", "none");
        $('.taskpopup').css('display', 'none');
        return false;
    });

    //New Tactic
    $("#NewTactic1").on("click", function () {

        if ($('#IsGridView').val().toLowerCase() == "false") {
            scrollstate = gantt.getScrollState();
            bodyscrollpos = $(window).scrollTop();
        }
        isCopyTactic = ProgramId;
        ShowModels(inspectAdd, secTactic, 0, ProgramId, RequestedModule);
        $("#errorMessage").css("display", "none");
        $("#successMessage").css("display", "none");
        $('.taskpopup').css('display', 'none');
        return false;
    });

    //Copy Tactic
    $('#CloneTactic1').click(function () {

        if ($('#IsGridView').val().toLowerCase() == "false") {
            scrollstate = gantt.getScrollState();
            bodyscrollpos = $(window).scrollTop();
        }
        isCopyTactic = ProgramId;
        var idPlanTactic = TacticId;
        var Title = $("<div/>").text(title).html();
        $.ajax({
            type: 'POST',
            url: urlContent + 'Inspect/Clone/',
            data: {
                CloneType: tacticCloneType,
                Id: idPlanTactic,
                title: Title,
                CalledFromBudget: CalledFromBudget,
                RequsetedModule: RequestedModule
            },
            success: function (data) {
                if (data.IsSuccess != 'undefined' && data.IsSuccess == '#') {
                    window.location = urlContent + '/Login/Index';
                }
                else if (data.IsSuccess) {
                    RefershPlanHeaderCalc();
                    isCopyTactic = data.Id;
                    isCopyTacticHomeGrid = isCopyTactic;
                    if ($('#IsGridView').val().toLowerCase() == "false") {
                        BindPlanCalendar();
                    }
                    else {
                        LoadPlanGrid();
                    }

                    isError = false;
                    ShowMessage(isError, data.msg);
                    logMixpanelTrack("Copy Tactic:" + data.msg);
                    return;
                }
                else {
                    isError = true;
                    ShowMessage(isError, data.msg);
                    return false;
                }
            }
        });
    });

    //Child Line Item
    $('#ChildLineItem').click(function () {
        isCopyTactic = TacticId;
        ShowModels(inspectAdd, secLineItem, 0, TacticId, RequestedModule);
        $('.taskpopup').css('display', 'none');
    });


    // New Line Item
    $('#NewLineItem').click(function () {
        isCopyTactic = TacticId;
        isDataModified = false;
        var planId = PlanId;
        ShowModels(inspectAdd, secLineItem, 0, TacticId, RequestedModule);
        $('.taskpopup').css('display', 'none');
    });

    //Copy Line Item
    $('#CloneLineItem').click(function () {
        if ($('#IsGridView').val().toLowerCase() == "false") {
            scrollstate = gantt.getScrollState();
            bodyscrollpos = $(window).scrollTop();
        }
        isCopyTactic = TacticId;
        var idPlanLineItem = LineId;
        var Title = $("<div/>").text(title).html();
        $.ajax({
            type: 'POST',
            url: urlContent + 'Inspect/Clone/',
            data: {
                CloneType: lineItemCloneType,
                Id: idPlanLineItem,
                title: Title,
                CalledFromBudget: CalledFromBudget,
                RequsetedModule: RequestedModule
            },
            success: function (data) {
                if (data.IsSuccess != 'undefined' && data.IsSuccess == '#') {
                    window.location = urlContent + '/Login/Index';
                }
                else if (data.IsSuccess) {
                    isCopyTactic = data.Id;
                    isCopyTacticHomeGrid = isCopyTactic;
                    isError = false;
                    ShowMessage(isError, data.msg);
                    LoadPlanGrid();
                    return;
                }
                else {
                    isError = true;
                    ShowMessage(isError, data.msg);
                    return false;
                }
            }
        });
    });
}

function displayconfirm(strURL) {
    if (isDataChanged()) {
        $('#spanMessageError').html(" You have unsaved changes. Do you wish to leave this page and lose your work?&nbsp;&nbsp;&nbsp;&nbsp;<a style='color:gray;' href='" + strURL + "' class='btn-gray'>Continue</a>&nbsp;&nbsp;<a style='color:gray;' id='confirmClose' href='#' class='underline'>Cancel</a>");
        $("#errorMessage").slideDown(400);
    }
    else {
        window.location.href = strURL;
    }
}

function isDataChanged() {
    var changed = false;
    $('.accordion-actuals').find("input,input[type=text],textarea,select").each(function () {
        var iv = $(this).attr("myValue");
        if ($(this).val().toString().trim() != iv.toString().trim()) {
            changed = false;
        }
    });
    return changed;
}
//End

//Inspection window close icon click
//Start
function CloseIconClick() {
    $('.close-x-big-icon').click(function () {
        $('#modalMainContainer').hide();// Added to remove extra space Below Grid/Calendar Table
        logMixpanelTrack("Exit from inspection window.");
        RemoveAllMediaCodeData();
        $("#modal-container-186470").addClass("transition-close");
        $("#modal-container-186470").removeClass("transition_y");
        addDefaultModalPopupBackgroungColor();
        $('body').removeClass('bodyOverflow');
        if (isDataModified) {
            $('#txtGlobalSearch').val("");
            $('#ExpClose').css('display', 'none');
            $('#ExpSearch').css('display', 'block');
        }
        if (isDataModified) {
            if (gridSearchFlag == 1) {
                isCopyTacticHomeGrid = isCopyTactic;
                isEditTacticHomeGrid = isEditTactic;
            }
        }
        if ($('#IsGridView').val().toLowerCase() == "true") {
            if (isDataModified) {
                if (gridSearchFlag == 1) {
                    isCopyTacticHomeGrid = isCopyTactic;
                    isEditTacticHomeGrid = isEditTactic;
                    LoadFilter();
                    gridSearchFlag = 0;
                }
                //else if (isBoostAuthorized) {
                //    var url = "@Url.Content("~/Plan/LoadImprovementGrid")";
                //    $("#ImprovementGrid").load(url + '?id=' + CurrentPlanId);
                //}
            }
            if (typeof inspectCloseFocus != 'undefined' && inspectCloseFocus != '') {
                $("html, body").animate({ scrollTop: inspectCloseFocus }, 100);
            }
            return true;
        }
        else {
            if ($("#hdnPlanLineItemID").val() == '0' || $("#hdnPlanLineItemID").val() == undefined) {
                if (isDataModified) {
                    RefreshCurrentTab();
                }
                else {
                    gantt.refreshData();
                    GlobalSearch();
                }
            } else {
                if (IsBudgetGrid) {
                    LoadBudgetGrid();
                }
                $('#divPlanEditButtonHome').click();
            }
        }
        $(".datepicker.dropdown-menu").each(function () {
            $(this).remove();
        });
        if ($('#IsGridView').val().toLowerCase() == "true") {
            $(".gantt_last_cell").dblclick(function (e) {
                e.stopPropagation();
            });

            var $doc = $(document);
            $doc.click(function () {
                $('#popupType').css('display', 'none');
            });
            $(document).mouseup(function (e) {
                $('#popupType').css("display", "none");
            });
            $(".gantt_ver_scroll").scroll(function () {
                $('#popupType').css('display', 'none');
            });
        }
    });
}
//End

//////HoneComb related Methods 
////start

$("#honeycomb").popover({

    title: 'HoneyComb',
    html: true,
    placement: 'top',
    content: function () {
        CloseClick = true;
        $('#honeycomb_content').find(".hc-block").remove();
        var htmlstring = "";
        var htmlstringForAsset = "";
        var errMessageTopDisplay = "";
        errMessageTopDisplay = $('#honeycomb_content').find('#errorMsgHoneyComb').get(0).outerHTML;
        if (errMessageTopDisplay == undefined || errMessageTopDisplay == null || errMessageTopDisplay == '') {
            errMessageTopDisplay = '';
        }
        else {
            $('#honeycomb_content').find("#errorMsgHoneyComb").remove();
        }
        $('#honeycomb_content').find("#UnpackageHoneyComb").removeAttr('style');
        $('#honeycomb_content').find("#PackageHoneyComb").removeAttr('style');
        var ViewBy = $('#ddlTabViewBy').val();
        if ( ViewBy != null && ViewBy != undefined && ViewBy == ViewByROI) {
            $('#honeycomb_content').find("#PackageHoneyComb").attr('style', 'color:gray; cursor: default !important');
        }
        else {
            $('#honeycomb_content').find("#PackageHoneyComb").attr('style', 'cursor: pointer !important');
        }
        if (!IsPackageView) {
            $('#honeycomb_content').find("#UnpackageHoneyComb").attr('style', 'color:gray; cursor: default !important');
        }
        else {
            $('#honeycomb_content').find("#UnpackageHoneyComb").attr('style', 'cursor: pointer !important');
        }
        var hccontrolsdiv = $('#honeycomb_content').find("#hc-controlsdiv").parent().html();
        if (ExportSelectedIds.TaskID.length > 0) {
            $('#honeycomb_content').html("");
            for (i = 0; i < (ExportSelectedIds.TaskID.length) ; i++) {
                if (isCalendarView == false) {
                   if (ExportSelectedIds.ROITacticType[i] == AssetType) {
                        htmlstringForAsset += '<div class="hc-block asset-bg" anchortacticid="' + ExportSelectedIds.AnchorTacticId[i] + '" EntityTaskId="' + ExportSelectedIds.TaskID[i] + '"  dhtmlxrowid="' + ExportSelectedIds.TaskID[i] + '"  csvid="' + ExportSelectedIds.CsvId[i] + '" roitactictype="' + ExportSelectedIds.ROITacticType[i] + '"> <div class="row-fluid"> ' +
                            ' <div  class="span8 pophover-left"  style="border-left: 5px solid #' + ExportSelectedIds.ColorCode[i] + '"> ' +
                            ' <h5 title="' + htmlEncode(ExportSelectedIds.Title[i]) + '">' + htmlEncode(ExportSelectedIds.Title[i]) + '</h5> ' +
                            ' <p class="metadata"> ' +
                            ' <span>' + ExportSelectedIds.TacticType[i] + ' </span>|<span> ' + ExportSelectedIds.OwnerName[i] + '</span> ' +
                            ' </p></div> ' +
                            ' <div class="span1 text-right pull-right" title="Delete" > ' +
                            ' <i taskid = "' + ExportSelectedIds.TaskID[i] + '" id ="CloseIcon"  class="fa fa-times" onclick="CloseIcon(this)" ></i> ' +
                            ' </div></div></div>';
                        }
                        else {
                        htmlstring += '<div class="hc-block" anchortacticid="' + ExportSelectedIds.AnchorTacticId[i] + '" EntityTaskId="' + ExportSelectedIds.TaskID[i] + '"  dhtmlxrowid="' + ExportSelectedIds.TaskID[i] + '"  csvid="' + ExportSelectedIds.CsvId[i] + '" roitactictype="' + ExportSelectedIds.ROITacticType[i] + '"> <div class="row-fluid"> ' +
                                ' <div  class="span8 pophover-left"  style="border-left: 5px solid #' + ExportSelectedIds.ColorCode[i] + '"> ' +
                                ' <h5 title="' + htmlEncode(ExportSelectedIds.Title[i]) + '">' + htmlEncode(ExportSelectedIds.Title[i]) + '</h5> ' +
                                ' <p class="metadata"> ' +
                                ' <span>' + ExportSelectedIds.TacticType[i] + ' </span>|<span> ' + ExportSelectedIds.OwnerName[i] + '</span> ' +
                                ' </p></div> ' +
                                ' <div class="span1 text-right pull-right" title="Delete"> ' +
                                ' <i taskid = "' + ExportSelectedIds.TaskID[i] + '" id ="CloseIcon"  class="fa fa-times" onclick="CloseIcon(this)" ></i> ' +
                                ' </div></div></div>';
                        }
                    }
                    else {
                    if (ExportSelectedIds.ROITacticType[i] == AssetType) {
                        htmlstringForAsset += '<div class="hc-block asset-bg" anchortacticid="' + ExportSelectedIds.AnchorTacticId[i] + '"  EntityTaskId="' + ExportSelectedIds.TaskID[i] + '" csvid="' + ExportSelectedIds.CsvId[i] + '"  roitactictype="' + ExportSelectedIds.ROITacticType[i] + '"><div class="row-fluid"><div  class="span8 pophover-left"  style="border-left: 5px solid #' + ExportSelectedIds.ColorCode[i] + '"><h5 title="' + htmlEncode(ExportSelectedIds.Title[i]) + '">' + htmlEncode(ExportSelectedIds.Title[i]) + '</h5>  <p class="metadata"><span>' + ExportSelectedIds.TacticType[i] + ' </span>|<span> ' + ExportSelectedIds.OwnerName[i] + '</span></p></div><div class="span1 text-right pull-right" title="Delete"><i taskid = "' + ExportSelectedIds.TaskID[i] + '" id ="CloseIcon"  class="fa fa-times" onclick="CloseIcon(this)" ></i></div></div></div>';
                }
                else {
                        htmlstring += '<div class="hc-block" anchortacticid="' + ExportSelectedIds.AnchorTacticId[i] + '"  EntityTaskId="' + ExportSelectedIds.TaskID[i] + '" csvid="' + ExportSelectedIds.CsvId[i] + '"  roitactictype="' + ExportSelectedIds.ROITacticType[i] + '"><div class="row-fluid"><div  class="span8 pophover-left"  style="border-left: 5px solid #' + ExportSelectedIds.ColorCode[i] + '"><h5 title="' + htmlEncode(ExportSelectedIds.Title[i]) + '">' + htmlEncode(ExportSelectedIds.Title[i]) + '</h5>  <p class="metadata"><span>' + ExportSelectedIds.TacticType[i] + ' </span>|<span> ' + ExportSelectedIds.OwnerName[i] + '</span></p></div><div class="span1 text-right pull-right" title="Delete"><i taskid = "' + ExportSelectedIds.TaskID[i] + '" id ="CloseIcon"  class="fa fa-times" onclick="CloseIcon(this)" ></i></div></div></div>';
                    }
                }
            }
        }
        $('#honeycomb_content').append(errMessageTopDisplay + htmlstringForAsset + htmlstring + hccontrolsdiv);
        return $("#honeycomb_content").html();
    }
});

function RemoveAllHoneyCombData() {
    $('#honeycomb_content').find(".hc-block").remove();
    IsPackageView = false;
    PreventUnPackageClick = true;
    ExportSelectedIds = {
        TaskID: [],
        Title: [],
        ColorCode: [],
        OwnerName: [],
        TacticType: [],
        PlanFlag: [],
        CsvId: [],
        ROITacticType: [],
        AnchorTacticId: [],
        CalendarEntityType: [],
    };
        $("div[class*='honeycombbox-icon-gantt-Active']").removeClass("honeycombbox-icon-gantt-Active");
    $("#totalEntity").text(0);
    $(".popover ").hide();
    $(".honeycombbox").hide();
}

//// Add RemoveEntity Function to add/remove data into Array on click of honeycomb icon for Plangrid/Budget Grid
function AddRemoveEntity(item) {
    $(".popover").removeClass('in').addClass('out');
    if ($(item).attr('id') == 'Plan') {
        if ($(item).hasClass("honeycombbox-icon-gantt-Active")) {
            var index = ExportSelectedIds.TaskID.indexOf($(item).attr('altId'));
            if (index >= 0) {
                ExportSelectedIds.TaskID.splice(index, 1);
                ExportSelectedIds.Title.splice(index, 1);
                ExportSelectedIds.OwnerName.splice(index, 1);
                ExportSelectedIds.TacticType.splice(index, 1);
                ExportSelectedIds.ColorCode.splice(index, 1);
                ExportSelectedIds.PlanFlag.splice(index, 1);
                ExportSelectedIds.CsvId.splice(index, 1);
                ExportSelectedIds.ROITacticType.splice(index, 1);
                ExportSelectedIds.AnchorTacticId.splice(index, 1);
                ExportSelectedIds.CalendarEntityType.splice(index, 1);
            }
            $(item).removeClass("honeycombbox-icon-gantt-Active");
            $(item).addClass("honeycombbox-icon-gantt");
        }
        else {
            $(item).addClass("honeycombbox-icon-gantt-Active");
            ExportSelectedIds.TaskID.push($(item).attr('altId'));
            ExportSelectedIds.Title.push($(item).attr('taskname').replace('&amp', '&'));
            ExportSelectedIds.OwnerName.push($(item).attr('ownername'));
            ExportSelectedIds.TacticType.push($(item).attr('tactictype'));
            ExportSelectedIds.ColorCode.push($(item).attr('colorcode'));
            ExportSelectedIds.CsvId.push($(item).attr('csvid'));
            ExportSelectedIds.ROITacticType.push($(item).attr('roitactictype'));
            ExportSelectedIds.AnchorTacticId.push($(item).attr('anchortacticid'));
            if (isCalendarView == true) {
                ExportSelectedIds.PlanFlag.push('Calender');
                ExportSelectedIds.CalendarEntityType.push($(this).attr('id'));
            }
            else {
                                    ExportSelectedIds.PlanFlag.push('Grid');

            }
        }
    }
    if ($(item).attr('id') == 'Campaign') {

        if ($(item).hasClass("honeycombbox-icon-gantt-Active")) {
            var index = ExportSelectedIds.TaskID.indexOf($(item).attr('altId'));
                                    if (index >= 0) {
                ExportSelectedIds.TaskID.splice(index, 1);
                ExportSelectedIds.Title.splice(index, 1);
                ExportSelectedIds.OwnerName.splice(index, 1);
                ExportSelectedIds.TacticType.splice(index, 1);
                ExportSelectedIds.ColorCode.splice(index, 1);
                ExportSelectedIds.PlanFlag.splice(index, 1);
                ExportSelectedIds.CsvId.splice(index, 1);
                ExportSelectedIds.ROITacticType.splice(index, 1);
                ExportSelectedIds.AnchorTacticId.splice(index, 1);
                ExportSelectedIds.CalendarEntityType.splice(index, 1);
            }
            $(item).removeClass("honeycombbox-icon-gantt-Active");
            $(item).addClass("honeycombbox-icon-gantt");
        }
        else {
            $(item).addClass("honeycombbox-icon-gantt-Active");
            ExportSelectedIds.TaskID.push($(item).attr('altId'));
            ExportSelectedIds.Title.push(($(item).attr('taskname')).replace('&amp', '&'));
            ExportSelectedIds.OwnerName.push($(item).attr('ownername'));
            ExportSelectedIds.TacticType.push($(item).attr('tactictype'));
            ExportSelectedIds.ColorCode.push($(item).attr('colorcode'));
            ExportSelectedIds.CsvId.push($(item).attr('csvid'));
            ExportSelectedIds.ROITacticType.push($(item).attr('roitactictype'));
            ExportSelectedIds.AnchorTacticId.push($(item).attr('anchortacticid'));
            if (isCalendarView == true) {
                                    ExportSelectedIds.PlanFlag.push('Calender');
                ExportSelectedIds.CalendarEntityType.push($(this).attr('id'));
        }
        else {
                ExportSelectedIds.PlanFlag.push('Grid');
            }
        }
    }
    if ($(item).attr('id') == 'Program') {

        if ($(item).hasClass("honeycombbox-icon-gantt-Active")) {
            $(item).removeClass("honeycombbox-icon-gantt-Active");
            var index = ExportSelectedIds.TaskID.indexOf($(item).attr('altId'));
        if (index >= 0) {
            ExportSelectedIds.TaskID.splice(index, 1);
            ExportSelectedIds.Title.splice(index, 1);
            ExportSelectedIds.OwnerName.splice(index, 1);
            ExportSelectedIds.TacticType.splice(index, 1);
            ExportSelectedIds.ColorCode.splice(index, 1);
                ExportSelectedIds.PlanFlag.splice(index, 1);
                ExportSelectedIds.CsvId.splice(index, 1);
            ExportSelectedIds.ROITacticType.splice(index, 1);
            ExportSelectedIds.AnchorTacticId.splice(index, 1);
                ExportSelectedIds.CalendarEntityType.splice(index, 1);
            }
            $(item).addClass("honeycombbox-icon-gantt");

    }
    else {
            $(item).addClass("honeycombbox-icon-gantt-Active");
            ExportSelectedIds.TaskID.push($(item).attr('altId'));
            ExportSelectedIds.Title.push($(item).attr('taskname').replace('&amp', '&'));
            ExportSelectedIds.OwnerName.push($(item).attr('ownername'));
            ExportSelectedIds.TacticType.push($(item).attr('tactictype'));
            ExportSelectedIds.ColorCode.push($(item).attr('colorcode'));
            ExportSelectedIds.CsvId.push($(item).attr('csvid'));
            ExportSelectedIds.ROITacticType.push($(item).attr('roitactictype'));
            ExportSelectedIds.AnchorTacticId.push($(item).attr('anchortacticid'));
            if (isCalendarView == true) {
                ExportSelectedIds.PlanFlag.push('Calender');
                ExportSelectedIds.CalendarEntityType.push($(this).attr('id'));
        }
        else {
                ExportSelectedIds.PlanFlag.push('Grid');
            }
        }
    }

    if ($(item).attr('id') == 'Tactic') {
        if ($(item).hasClass("honeycombbox-icon-gantt-Active")) {
            var IsAssetTactic = $(item).attr('roitactictype');
            var AssetTacId = $(item).attr('anchortacticid');
            var EntityId = $(item).attr('taskid');
            if (IsPackageView && PackageAnchorId == EntityId && IsAssetTactic == AssetTypeAsset && AssetTacId == EntityId) {
                ShowMessage(true, DeselectAssetFromPackage, 3000);
                $('html,body').scrollTop(0);
                return false;
            }
            $(item).removeClass("honeycombbox-icon-gantt-Active");
            var index = ExportSelectedIds.TaskID.indexOf($(item).attr('altId'));
        if (index >= 0) {
            ExportSelectedIds.TaskID.splice(index, 1);
            ExportSelectedIds.Title.splice(index, 1);
            ExportSelectedIds.OwnerName.splice(index, 1);
            ExportSelectedIds.TacticType.splice(index, 1);
            ExportSelectedIds.ColorCode.splice(index, 1);
                ExportSelectedIds.PlanFlag.splice(index, 1);
                ExportSelectedIds.CsvId.splice(index, 1);
            ExportSelectedIds.ROITacticType.splice(index, 1);
            ExportSelectedIds.AnchorTacticId.splice(index, 1);
                ExportSelectedIds.CalendarEntityType.splice(index, 1);
            }
            $(item).addClass("honeycombbox-icon-gantt");
        }
        else {
            $(item).addClass("honeycombbox-icon-gantt-Active");
            ExportSelectedIds.TaskID.push($(item).attr('altId'));
            ExportSelectedIds.Title.push($(item).attr('taskname').replace('&amp', '&'));
            ExportSelectedIds.OwnerName.push($(item).attr('ownername'));
            ExportSelectedIds.TacticType.push($(item).attr('tactictype'));
            ExportSelectedIds.ColorCode.push($(item).attr('colorcode'));
            ExportSelectedIds.CsvId.push($(item).attr('csvid'));
            ExportSelectedIds.ROITacticType.push($(item).attr('roitactictype'));
            ExportSelectedIds.AnchorTacticId.push($(item).attr('anchortacticid'));
            if (isCalendarView == true) {
                ExportSelectedIds.PlanFlag.push('Calender');
                ExportSelectedIds.CalendarEntityType.push($(this).attr('id'));
            }
            else {
                ExportSelectedIds.PlanFlag.push('Grid');
            }
        }
    }

    if ($('.honeycombbox-icon-gantt-Active').length == 0) {
        $(".honeycombbox").hide();
    }
    else {
                $("#totalEntity").text(ExportSelectedIds.TaskID.length);
                $(".honeycombbox").show();
               //Added following condition to hide show export to pdf and xls option as per grid and calendar view
                if (isCalendarView) {
                    $('.dropdown-menu').find('a#ExportCSVHoneyComb').css("display", "none");
                    $('.dropdown-menu').find('a#ExportPDFVHoneyComb').css("display", "block");
                }
                else {
                    $('.dropdown-menu').find('a#ExportCSVHoneyComb').css("display", "block");
                    $('.dropdown-menu').find('a#ExportPDFVHoneyComb').css("display", "none");
                }
    }
}
//End

//End


///Copy to and Link To
//start
var isLoadcopied = false;
var cloneObj = "";
function OpentCopyPopup(obj, isProceed) {
    cloneObj = obj;
    var redirectType = $(obj).attr('redirecttype');
    if (redirectType == "" || redirectType == undefined) {
        if ($('#IsGridView').val().toLowerCase() == "false") {
            scrollstate = gantt.getScrollState();
        }
        else {
            linkItemId = $(obj).attr('entityid');
            if (linkItemId.indexOf("_") >= 0) {
                linkItemId = linkItemId.split('_')[1];
            }
            else {
                linkItemId = $(obj).attr('entityid').replace("_", "");
            }
            var selectedcell = HomeGrid.findCell(linkItemId, colSplitId, true);
            var id = selectedcell[0];
            var rowid = id[0];
            ItemIndex = HomeGrid.getRowIndex(rowid);
            state0 = ItemIndex;
        }
    }
    var sectionType = $(obj).attr('sectionType');
    var entityId = $(obj).attr('entityId');
    var popuptype = $(obj).attr('popupType');
    var returnpara = true;
    var activeTab = $("#InspectTab li.active a").text();
    if (activeTab != null && activeTab != undefined && activeTab != '' && redirectType != undefined) {
        if (sectionType.toLowerCase() == secTactic) {

            if ((checkChangeValue(".resubmission", true)) && ($("#hdnIsResubmitableStatus").val().toString().toLowerCase() == "true") && isProceed == undefined) {
                var iserror = false;
                $('form').find('input[type=text], select, textarea').each(function () {
                    if ($(this).attr('require') == 'true') {
                        if ($(this).val().toString().trim() == '' && $(this).parent().css("display") != "none") {
                            $(this).addClass("error");
                            iserror = true;
                        }
                        else {
                            $(this).removeClass("error");
                        }
                    }
                });
                $('.dropdown_new_btn').each(function () {
                    if (typeof $(this).attr('require') != 'undefined' && $(this).attr('require') == 'true' && $(this).find('p').text() == 'Please Select' && $(this).parent().parent().css("display") != "none") {
                        $(this).addClass("error");
                        iserror = true;
                    }
                    else {
                        $(this).removeClass("error");
                    }
                });
                if (iserror) {
                    ShowError(ValidateForEmptyField);
                    return false;
                }
                url = urlContent + 'Inspect/LoadResubmission/'
                $("#divResubmission").load(url + '?redirectionType=' + secTactic + '&labelValues=' + _resubmissionLabelValues);
                isLoadcopied = true;
            }
            else {
                returnpara = SaveAllData(activeTab);
                $('#spanMessageSuccess').empty();
                $("#successMessage").css("display", "none");
            }
        }
        else {
            returnpara = SaveAllData(activeTab);
            $('#spanMessageSuccess').empty();
            $("#successMessage").css("display", "none");
        }
    }
    if (returnpara && !isLoadcopied) {
        $("#dvCopyEntity").empty();
        var url = urlContent + 'Plan/LoadCopyEntityPopup/'
        //var url = '@Url.Content("~/Plan/LoadCopyEntityPopup")';
        $("#dvCopyEntity").load(url + "?entityId=" + entityId + "&section=" + sectionType + "&PopupType=" + popuptype + "&RedirectType=" + redirectType); // section parameter added to open share tactic popup
    }
    else {
        $("#SuccessMsg").css("display", "none");
        $("#successMessage").css("display", "none");
        $('.btn-dropdwn').hide();
    }
}

function OpentCopyPopuponProceed(obj) {
    var redirectType = $(obj).attr('redirecttype');
    var sectionType = $(obj).attr('sectionType');
    var entityId = $(obj).attr('entityId');
    var popuptype = $(obj).attr('popupType');
    var planTacticId = entityId.split('_')[1];
    loadInspectPopup(planTacticId, secTactic, "Setup", inspectEdit, 0, RequestedModule);
    $("#dvCopyEntity").empty();
    var url = '@Url.Content("~/Plan/LoadCopyEntityPopup")';
    $("#dvCopyEntity").load(url + "?entityId=" + entityId + "&section=" + sectionType + "&PopupType=" + popuptype + "&RedirectType=" + redirectType); // section parameter added to open share tactic popup
    $("#SuccessMsg").css("display", "none");
    $('.btn-dropdwn').hide();
}

//End

function RemoveAllMediaCodeData() {
    $('#honeycomb_contentMediaCode').find(".hc-block").remove();
    $('#divHoneyCombBoxMediaCode .popover-content').html("");
    ExportSelectedIdsMediaCode = {
        MediacodeSelectedRowId: [],
        Mediacodes: [],
        MediacodeID: [],
        AltId: []
    };
    $('#MediaCodeGrid').find('tr').find('td').find("div[class*='honeycombbox-icon-gantt-Active']").removeClass("honeycombbox-icon-gantt-Active");
    $("#totalEntityMediacode").text(ExportSelectedIdsMediaCode.AltId.length);
    $("#divHoneyCombBoxMediaCode").hide();
    $('#MediaCodeSelectAll').removeAttr('checked');
}

/// End

//Common Functions
//Start
function RefershPlanHeaderCalc() {
 var TimeFrame=$('#ddlUpComingActivites').val();
    if (TimeFrame == null)
    {
        TimeFrame = "thisquarter";
    }
    GetHeadsUpData(urlContent + 'Plan/GetHeaderforPlanByMultiplePlanIDs/', urlContent + 'Home/GetActivityDistributionchart/', secHome, TimeFrame);
}
//End
///Function for After Link to reload the Grid/Calendar data with New Link Icon 
function ConfirmLinkTactic() {
    $("#modal-container-186470").addClass("transition-close");
    $("#modal-container-186470").removeClass("transition_y");
    addDefaultModalPopupBackgroungColor();
    $('body').removeClass('bodyOverflow');
    if ($('#IsGridView').val().toLowerCase() == "true") {
        if (isDataModified) {
            if (gridSearchFlag == 1) {
                isCopyTacticHomeGrid = isCopyTactic;
                isEditTacticHomeGrid = isEditTactic;
                LoadPlanGrid();
                gridSearchFlag = 0;
            }
        }
        if (typeof inspectCloseFocus != 'undefined' && inspectCloseFocus != '') {
            $("html, body").animate({ scrollTop: inspectCloseFocus }, 100);
        }

        $(".gantt_last_cell").dblclick(function (e) {
            e.stopPropagation();
        });
        var $doc = $(document);
        $doc.click(function () {
            $('#popupType').css('display', 'none');
        });

        $(document).mouseup(function (e) {
            $('#popupType').css("display", "none");
        });
        $(".gantt_ver_scroll").scroll(function () {
            $('#popupType').css('display', 'none');
        });
        return true;

    }
    else {
        if (isDataModified) {
            BindPlanCalendar();
        }
        else {
            gantt.refreshData();
        }
    }

    $(".datepicker.dropdown-menu").each(function () {
        $(this).remove();
    });

    $("div[id^='LinkIcon']").each(function () {
        bootstrapetitle($(this), 'This tactic is linked to ' + "<U>" + htmlDecode($(this).attr('linkedplanname') + "</U>"), "tipsy-innerWhite");
    });
};
////End

//set value of dropdown in search criteria
$(".searchDropdown li a").click(function () {
    var searchColumn = $(this).text();
    $("#txtGlobalSearch").val("");
    $("#searchCriteria").text($(this).text()[0]);
    $("#searchCriteria").val(searchColumn.replace(" ",''));
    $("#txtGlobalSearch").attr('Placeholder', $(this).text());
    if ($('#errorMsg').css('display') == 'block') {
        $('#errorMsg').css('display', 'none');
    }
    if ($('#ExpClose').css('display') == 'block') {
        $('#ExpSearch').css('display', 'block');
        $('#ExpClose').css('display', 'none');
    }
    if ($('#btnbudget').hasClass('P-icon-active')) {
        LoadBudgetGrid();
    }
    else if($('#btngridcalendar').addClass('P-icon-active'))
    {
        BindPlanCalendar();
    }
    else{
    BindHomeGrid();
}
});

//Search button click
$('#ExpSearch').click(function () {
    if ($('#txtGlobalSearch').val().trim() != undefined && $('#txtGlobalSearch').val().trim() != null) {
        GlobalSearch();
        if ($('#txtGlobalSearch').val().trim() != "") {
            $('#ExpSearch').css('display', 'none');
            $('#ExpClose').css('display', 'block');
        }
        else {
            $('#ExpSearch').css('display', 'block');
            $('#ExpClose').css('display', 'none');
        }
    }
});

//Close button click 
$('#ExpClose').click(function () {
    $('#txtGlobalSearch').val("");
    $('#ExpClose').css('display', 'none');
    GlobalSearch();
    $('#ExpSearch').css('display', 'block');
    if ($('#errorMsg').css('display') == 'block') {
        $('#errorMsg').css('display', 'none');
    }

});

////Handle enter key in search textbox 
$('#txtGlobalSearch').on('keyup', function (event) {
    if (event.which == 13 || $('#txtGlobalSearch').val().trim() == "") {
        $('#ExpSearch').click();
    }
    else {
        $('#ExpClose').css('display', 'none');
        $('#ExpSearch').css('display', 'block');
    }
});
//Added by Dhvani to maintain dop-down for Global search
function OpenDropdown() {
    IsManageDropdown = true; // Handle sorting on dropdown click 
}
var SearchTextforcal = ""
function GlobalSearch() {
    var SearchText = $('#txtGlobalSearch').val();
   // if (SearchText != "" && SearchText != 'undefined') {
    if ($('#IsGridView').val() == 'True' || IsBudgetGrid) {

        var SearchDDLValue = $('#searchCriteria').val().replace(" ", "");
        GlobalSearchonGrid(SearchText, SearchDDLValue);
    }
    else {
        gantt.refreshData();
        gantt.render();
        function contains(haystack, needle) {
            var a = (haystack || "").toLowerCase(),
                b = (needle || "").toLowerCase();

            return !!(a.indexOf(b) > -1);
        }
        function hasValue(parent, value, searchcriteria) {
            if (value == "") {
                return true;
            }
            if (searchcriteria == glblsrchExternalName) {
                if (contains(htmlDecode(gantt.getTask(parent).machineName), value))
                    return true;
            }
            if (searchcriteria == glblsrchActivityName) {
                if (contains(htmlDecode(gantt.getTask(parent).text), value))
                    return true;
            }

            var child = gantt.getChildren(parent);
            for (var i = 0; i < child.length; i++) {
                if (hasValue(child[i], value, searchcriteria))
                    return true;
            }
            return false;
        }
        gantt.attachEvent("onBeforeTaskDisplay", function (id, task) {

            if (hasValue(id, $('#txtGlobalSearch').val().trim(), $('#searchCriteria').val().replace(" ", "").toUpperCase().toString())) {
                return true;
            } else {
                return false;
            }
        });
        if ($(gantt.$grid_data).find('.gantt_row').length <= 0) {

            if ($('#txtGlobalSearch').val().length > 0) {
                SearchTextforcal = $('#txtGlobalSearch').val().trim();
                $('#txtGlobalSearch').val("");
                GlobalSearch();
                $('#txtGlobalSearch').val(SearchTextforcal);
                $('#SuccessMsg').css('display', 'none');
                $("#spanMsgSuccess").empty();
                $("#errorMsg").css("display", "block");
                $("#spanMsgError").empty();
                $("#spanMsgError").text("No data found! Please check the filter and make correct Plan and Attributes selections");
            }
        }
        gantt.eachTask(function (task) {
            task.$open = true;
        });
        gantt.render(); // To expand in gantt
        }
    //}
}

function GlobalSearchonGrid(node, columnName) {
    var colindex = 0;
    var text = node;
    HomeGrid.setFiltrationLevel(-2);
    if (columnName.toUpperCase().toString() == glblsrchActivityName) {
        colindex = HomeGrid.getColIndexById(TaskNameId);
    }
    else if (columnName.toUpperCase().toString() == glblsrchExternalName) {
        colindex = HomeGrid.getColIndexById(MachineNameId);
    }
    HomeGrid.filterTreeBy(colindex, function (data) {
        return htmlDecode(data).toLowerCase().toString().indexOf(text.toLowerCase()) != -1;
    });
    if (HomeGrid.rowsBuffer.length <= 0) {
        $('#txtGlobalSearch').val("");
        //BindHomeGrid();
        $('#txtGlobalSearch').val(node.trim());
        $("#errorMsg").css("display", "block");
        $("#spanMsgError").empty();
        $("#spanMsgError").text("No data found! Please check the filter and make correct Plan and Attributes selections");
        $(window).scrollTop(0);
    }
    else {
        if ($('#txtGlobalSearch').val().trim() != undefined && $('#txtGlobalSearch').val().trim() != "" && $('#txtGlobalSearch').val().trim() != null) {
            HomeGrid.expandAll();
        }
        $("#errorMsg").css("display", "none");
        $("#spanMsgError").empty();
    }
}
//insertation start Added following method for open pop up to import file.
//insertation start Added following method for open pop up to import file.
function LoadFileInputModelBox() {

    $('.fileinput-upload-button').hide();
    // Clear file input on modal hide.
    $('#ImportModal').on('hidden.bs.modal', function () {
        $('#input-43').fileinput('clear');
    });
    $('#input-43').on('fileclear', function (event) {
        $('.fileinput-upload-button').hide();
    });

    //Initializing file input plugin

    $("#input-43").fileinput({
        type: 'POST',
        showPreview: false,
        uploadUrl: urlContent + 'Plan/ExcelFileUpload/',
        allowedFileExtensions: ["xls", "xlsx"],
        msgInvalidFileExtension: 'Incorrect file format. Please export the file and use that file to upload your changes',
        elErrorContainer: "#errorBlock"

    });
    //howing progress bar while importing file.
    $('#input-43').on('filepreupload', function (event, data, previewId, index) {
        myApp.showPleaseWait();
    });
    // Refresh budget grid data after file is uploaded.
    $('#input-43').on('fileuploaded', function (event, data, previewId, index) {

        //Showing message for conflicting data.
        if (data.response.conflict == true) {
            ShowMessage(false, data.response.message);
        }
        else if (data.response.conflict == false) {
            ShowMessage(false, data.response.message);
        }
        $('#ImportModal').modal('hide');
        var selectedTimeFrame = $('#ddlUpComingActivites').val();
        var currentDate = new Date();
        if (selectedTimeFrame == null || selectedTimeFrame == 'undefined' || selectedTimeFrame == "") {
            selectedTimeFrame = currentDate.getFullYear().toString();
        }
        $('#divgridview').load(urlContent + 'Plan/GetBudgetData' + '?PlanIds=' + filters.PlanIDs.toString() + '&OwnerIds=' + filters.OwnerIds.toString() + '&TactictypeIds=' + filters.TacticTypeids.toString() + '&StatusIds=' + filters.StatusIds.toString() + '&CustomFieldIds=' + filters.customFieldIds.toString() + '&year='+selectedTimeFrame.toString());

    });

    $('#input-43').on('fileloaded', function (event, file, previewId, index, reader) {
        $('.fileinput-upload-button').show();
    });
    $('#input-43').on('fileuploaderror', function (event, data, msg) {
        $('.fileinput-upload-button').hide();
    });
}
///End


// Bind View Dropdown list.
function BindViewSelections(strUrl) {
    var $dropdown = $("#ddlTabViewBy");
    $dropdown.html('');
    var $html = '';
    var type = '';
    $.ajax({
        type: 'POST',
        url: strUrl,
        data: { planids: filters.PlanIDs.toString() },
        success: function (data) {
            if (data != null && data != 'undefined') {
                if (data.length > 0) {
                    $.each(data, function (index, time) {

                        if (type == time.Value) {
                            $html += '<option value="' + time.Value + '" selected="selected">' + time.Text + '</option>';
                        }
                        else {
                            $html += '<option value="' + time.Value + '">' + time.Text + '</option>';
                        }

                    });
                }
                $dropdown.append($html);
                $("#ddlTabViewBy").multiselect('refresh');
                //$('#ddlTabViewBy').val(data.ViewBy.toString());
            }
        }
    });
}

// ViewBy Change Event.
$("#ddlTabViewBy").change(function () {
    RemoveAllHoneyCombData();
    if ($('#IsGridView').val().toLowerCase() == "true") {
        LoadPlanGrid();
        $("#GridGanttContent").hide();
        $('#divupcomingact').hide();
        $("#divgridview").show();
        $('.export-dd').find('#ExportXls').show();
        $('.export-dd').find('#ExportPDf').hide();
    }
    else if (IsBudgetGrid)
    {
        isCalendarView = false;
        $('#ChangeView').hide();
        $('#exp-serach').css('display', 'none');
        LoadBudgetGrid();
        ShowHideDataonBudgetScreen();
    }
    else {
        BindPlanCalendar();
        $('.export-dd').find('#ExportXls').hide();
        $('.export-dd').find('#ExportPDf').show();
        $('#divupcomingact').show();
        $("#GridGanttContent").show();
        $("#divgridview").hide();
    }
});

//For Task Name Edit
function SetFilterData(EntityId, nValue) {
    $('#ulSelectedPlans li[id=liPlan' + EntityId + ']').find('span').text(nValue);
    $('#ulSelectedPlans li[id=liPlan' + EntityId + ']').attr('title', nValue);
    $('#ulSelectedPlans li[id=liPlan' + EntityId + ']').find('input[type=checkbox]').attr('plantitle', nValue);
    selectedFilters.planName = [];
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
    GetMultiplePlanNames();
} 



//To Handle roi package/unpackage
var AnchorTaskIdsList = {
    Id: [],
    Value: []
};
function CallPackageHoneyComb() {
    var ViewBy = $('#ddlTabViewBy').val();
    if (ViewBy != null && ViewBy != undefined && ViewBy == ViewByROI) {
        return false;
    }
    var countElements = $('.popover-content').find(".hc-block").length;
    var hasAnchorTactic = 0;
    var isOnlyTactics = true;
    var anchorTacticId = "";
    var promotionTacticIds = [];
    var AnchorTaskId = 0;
    var dhtmlxrowid = "";
    var IsAlreadyPackaged = [];
    var PlanIds = [];

    if (countElements > 0) {
        $('.popover-content').find(".hc-block").each(function () {
                var ROITacticType = $(this).attr('roitactictype');
                var ExistingAnchorTacticId = $(this).attr('anchortacticid');
                var Csvid = $(this).attr('csvid').split('_');
                var EntityType = Csvid[0];

                if (ROITacticType.toLowerCase() == AssetType.toLowerCase()) {
                    anchorTacticId = Csvid[1];
                if (isCalendarView == false) {
                        dhtmlxrowid = $(this).attr('dhtmlxrowid');
                    }
                else {
                        AnchorTaskId = $(this).attr('EntityTaskId');
                        var index = AnchorTaskIdsList.Id.indexOf(AnchorTaskId);
                        if (index >= 0) {
                            AnchorTaskIdsList.Id.splice(index, 1);
                            AnchorTaskIdsList.Value.splice(index, 1);
                        }
                        AnchorTaskIdsList.Id.push(AnchorTaskId);
                    }
                    
                    hasAnchorTactic++;
                }
            if (EntityType.toLowerCase() == EntityTypeTactic.toLowerCase()) {
                var TaskId = $(this).attr('EntityTaskId');
                var ListIds = TaskId.split('_');
                $.each(ListIds, function (index, Id) {
                    if (Id.indexOf('L') >= 0) {
                        var TacticPlanId = Id.replace("L", "");
                        var PlanIndex = PlanIds.indexOf(TacticPlanId);
                        if (PlanIndex < 0) {
                            PlanIds.push(TacticPlanId);
                        }

                    }

                });
            }
                if (EntityType.toLowerCase() != EntityTypeTactic.toLowerCase()) {
                    isOnlyTactics = false;
                }
                else {
                    promotionTacticIds.push(Csvid[1]);
                }

                if ((!IsPackageView && ExistingAnchorTacticId != "0" && ExistingAnchorTacticId != "null") || (IsPackageView && ExistingAnchorTacticId != "0" && ExistingAnchorTacticId != anchorTacticId && ExistingAnchorTacticId != "null")) {
                    IsAlreadyPackaged.push($(this).find('h5').text());
                }
            });

            AnchorTaskIdsList.Value.push(promotionTacticIds.toString());
    }
    if (hasAnchorTactic != 1 || !isOnlyTactics || IsAlreadyPackaged.length > 0 || PlanIds.length > 1) {
        // All errors as per validation
        isError = true;
        AnchorTaskIdsList = {
            Id: [],
            Value: []
        };
        if (!isOnlyTactics) {
            ShowMessageHoneyComb(isError, OnlyTacticsMessage, 400);
        }
        else if (PlanIds.length > 1) {
            ShowMessageHoneyComb(isError, OnlySinglePlanTacticMessage, 400);
        }
        else if (hasAnchorTactic == 0) {
            ShowMessageHoneyComb(isError, AtleastOneAssetMessage, 400);
        }
        else if (hasAnchorTactic > 1) {
            ShowMessageHoneyComb(isError, MorethanOneAssetMessage, 400);
        }
        else if (IsAlreadyPackaged.length > 0) {
            ShowMessageHoneyComb(isError, "Tactics(" + IsAlreadyPackaged.join(',') + ") can belong to only one package. Please remove above tactic(s).", 400);
        }

    }
    else {
        // Create new package
        $.ajax({
            type: 'POST',
            url:   urlContent + 'Home/AddROIPackageDetails/',
            data: { AnchorTacticId: anchorTacticId, PromotionTacticIds: promotionTacticIds.toString() },
            success: function (data) {
                if (isCalendarView == true) {
                    var TaskHtml = $("div[task_id='" + AnchorTaskId + "']");
                    if (TaskHtml.length > 0) {
                        TaskHtml = $("div[task_id='" + AnchorTaskId + "']")[0];
                        var length = TaskHtml.childNodes.length;
                    }
                    if (TaskHtml != 'undefined' && TaskHtml != undefined && TaskHtml != null && length > 1) {
                        var LastChildNode = TaskHtml.getElementsByClassName("gantt_tree_content").item(0).outerHTML;
                        var FileNode = TaskHtml.getElementsByClassName("gantt_file").item(0);
                        var BlankDiv = TaskHtml.getElementsByClassName("gantt_blank").item(0);
                        if (FileNode != null && FileNode != 'undefined' && FileNode != undefined) {
                            FileNode = TaskHtml.getElementsByClassName("gantt_file").item(0).outerHTML;
                        }
                        var AllCHildNodes = TaskHtml.childNodes[1].innerHTML;
                        var IsPAckageExists = TaskHtml.getElementsByClassName("ROIPackage").item(0);
                        if (IsPAckageExists != null && IsPAckageExists != undefined && IsPAckageExists != 'undefined') {
                            var PackageNode = TaskHtml.getElementsByClassName("ROIPackage").item(0).outerHTML;
                            AllCHildNodes = AllCHildNodes.replace(PackageNode, "");
                        }
                        if (BlankDiv != null && BlankDiv != 'undefined' && BlankDiv != undefined) {

                            AllCHildNodes = AllCHildNodes.replace(BlankDiv.outerHTML, "<div class='unlink-icon ROIPackage' onclick='OpenHoneyComb(this)' style='cursor:pointer' pkgtacids='" + promotionTacticIds + "' ><i class='fa fa-object-group'></i></div>");
                        }
                        else {
                            AllCHildNodes = AllCHildNodes.replace(LastChildNode, "");
                            AllCHildNodes = AllCHildNodes.replace(FileNode, "");
                            AllCHildNodes = AllCHildNodes.concat("<div class='unlink-icon ROIPackage' onclick='OpenHoneyComb(this)' style='cursor:pointer' pkgtacids='" + promotionTacticIds + "' ><i class='fa fa-object-group'></i></div>" + FileNode + LastChildNode);
                        }
                        TaskHtml.childNodes[1].innerHTML = AllCHildNodes;
                    }


                    // Bind anchortacticid of the new package to honeycomb icons in the grid
                    $.each(promotionTacticIds, function (i) {
                        var ItemID = anchorTacticId + "_" + promotionTacticIds[i];
                        AddRemovePackageItems.AddItemId.push(ItemID);
                        $('.honeycombbox-icon-gantt[taskid=' + promotionTacticIds[i] + ']').attr('anchortacticid', anchorTacticId);
                    });
                }
                else {
                    var rowId = dhtmlxrowid;
                    var getvalue = HomeGrid.cells(rowId, TaskNameColIndex).getValue();
                    var Index = getvalue.indexOf("pkgIcon");
                    if (Index <= -1) {
                        // Add package icon on the grid when new package is created
                        var PkgIconDiv = "<div id=pkgIcon class='package-icon package-icon-grid' style='cursor:pointer' onclick=OpenHoneyComb(this) pkgtacids='" + promotionTacticIds.toString() + "'><i class='fa fa-object-group'></i></div>";
                        HomeGrid.cells(rowId, TaskNameColIndex).setValue(PkgIconDiv + getvalue);
                    }
                    else if (Index > -1) {
                        // Bind updated items to package icon into grid
                        var newValue = "<div id=pkgIcon class='package-icon package-icon-grid' style='cursor:pointer'  onclick=OpenHoneyComb(this) pkgtacids='" + promotionTacticIds.toString() + "'><i class='fa fa-object-group'></i></div>";
                        var oldValue = $(getvalue).get(0).outerHTML;
                        getvalue = getvalue.replace(oldValue, newValue);
                        HomeGrid.cells(rowId, TaskNameColIndex).setValue(getvalue);
                    }
                    // Bind anchortacticid of the new package to honeycomb icons in the grid
                    $.each(promotionTacticIds, function (i) {
                        $('.honeycombbox-icon-gantt[csvid=Tactic_' + promotionTacticIds[i] + ']').attr('anchortacticid', anchorTacticId);
                    });

                }
                // Unselect all items
                RemoveAllHoneyCombData();
                if (data.IsUpdatePackage != undefined && data.IsUpdatePackage == true) {
                    ShowMessage(false, PackageUpdated, 3000);
                }
                else {
                    ShowMessage(false, PackageCreated, 3000);
                }
            }

        });
    }

}

function CallUnPackageHoneyComb() {
    if (PreventUnPackageClick == false) {
        var anchorTacticId = 0;
         
            var objAssetElement = $('.popover-content').find(".hc-block[roitactictype=" + AssetType + "]");
            if (objAssetElement != undefined && objAssetElement != null && objAssetElement.length > 0) {
                anchorTacticId = $(objAssetElement).attr('csvid').split('_')[1];
                if (isCalendarView == true) {
                    var CalendarTaskID = $('.popover-content').find(".hc-block[roitactictype=" + AssetType + "]").attr('entitytaskid');
                    $('.popover-content').find(".hc-block[anchortacticid= " + anchorTacticId + " ]").each(function () {
                        var TaskID = $(this).attr('entitytaskid');
                        AddRemovePackageItems.RemoveId.push(TaskID);
                    });
                }
            }
            else {
                ShowMessageHoneyComb(true, DeselectAssetFromPackage, 400);
                return false;
            }
           

        $.ajax({
            url: urlContent + 'Home/UnpackageTactics/',
            data: {
                AnchorTacticId: anchorTacticId
            },
            success: function (data) {
                if (isCalendarView == false) {
                    // Remove package icon from the grid
                    var rowObject = $('.honeycombbox-icon-gantt[taskid=' + anchorTacticId + ']');
                    var rowId = $(rowObject).attr('altid');
                    var getvalue = HomeGrid.cells(rowId, TaskNameColIndex).getValue();

                    var Index = getvalue.indexOf("pkgIcon");
                    if (Index > -1) {
                        var newValue = $(getvalue).get(0).outerHTML;
                        getvalue = getvalue.replace(newValue, '');
                        HomeGrid.cells(rowId, TaskNameColIndex).setValue(getvalue);
                    }
                }
                else {
                    var TaskHtml = $("div[task_id='" + CalendarTaskID + "']");
                    if (TaskHtml.length > 0) {
                        TaskHtml = $("div[task_id='" + CalendarTaskID + "']")[0];
                        var length = TaskHtml.childNodes.length;
                    }
                    if (TaskHtml != 'undefined' && TaskHtml != undefined && TaskHtml != null && length > 1) {
                        var FileNode = TaskHtml.getElementsByClassName("ROIPackage").item(0);
                        var BlankDiv = TaskHtml.getElementsByClassName("gantt_blank").item(0);
                        var IsLinkIcon = TaskHtml.getElementsByClassName("unlink-icon");
                        if (FileNode != null && FileNode != 'undefined' && FileNode != undefined) {
                            FileNode = TaskHtml.getElementsByClassName("ROIPackage").item(0).outerHTML
                        }
                        var AllCHildNodes = TaskHtml.childNodes[1].innerHTML;
                        if (BlankDiv != null && BlankDiv != 'undefined' && BlankDiv != undefined) {
                            AllCHildNodes = AllCHildNodes.replace(FileNode, "");

                        }
                        else {
                            if (IsLinkIcon != null && IsLinkIcon != 'undefined' && IsLinkIcon != undefined && IsLinkIcon.length <= 1) {
                                AllCHildNodes = AllCHildNodes.replace(FileNode, "<div class='gantt_tree_icon gantt_blank'></div>");
                            }
                            else {
                                AllCHildNodes = AllCHildNodes.replace(FileNode, "");
                            }

                        }
                        TaskHtml.childNodes[1].innerHTML = AllCHildNodes;
                    }
                    if (AnchorTaskIdsList.Id.length != 0) {
                        $.each(AnchorTaskIdsList.Id, function (index, AnchorID) {
                            if (AnchorID == CalendarTaskID) {
                                var index = AnchorTaskIdsList.Id.indexOf(AnchorID);
                                if (index >= 0) {
                                    AnchorTaskIdsList.Id.splice(index, 1);
                                    AnchorTaskIdsList.Value.splice(index, 1);
                                }
                            }
                        });
                    }
                    else {
                        AnchorTaskIdsList.Id.push(CalendarTaskID);
                    }
                    // Added by Arpita Soni for Ticket #2357 on 07/15/2016
                    var ViewBy = $('#ddlTabViewBy').val();
                    if (ViewBy != null && ViewBy != undefined && ViewBy == ViewByROI) {
                        var removeEntirePkg = CalendarTaskID.split('_')[0];
                        gantt.deleteTask(removeEntirePkg);
                        gantt.refreshData();
                        // If there is no data then hide grid and display message
                        if ($(gantt.$grid_data).find('.gantt_row').length <= 0) {
                            $("#gantt_here").hide();
                            $(".pull-right.toggle-status.source-sans-proregular").hide();
                            $("#improvements").hide();
                            $('#lblMarkringActivitiestitle').hide();
                            $('#divCollaborator').hide();
                            $('#divCollaboratorCount').hide(); // Added for #2112
                            $("#NodatawithfilterCalendar").show();
                        }

                        RefershPlanHeaderCalc();
                    }
                }

                // Unbind anchortacticid of the deleted package to honeycomb icons in the grid

                $('.honeycombbox-icon-gantt[anchortacticid=' + anchorTacticId + ']').each(function () {
                    $(this).attr('anchortacticid', '0');
                });
                // Unselect all items from the grid
                RemoveAllHoneyCombData();

                ShowMessage(false, PackageUnsuccessful, 3000);
            }
        });
    }
    else {
        return false;
    }
}

function ShowMessageHoneyComb(IsError, Message, time) {
    $(".popover-content > #errorMsgHoneyComb").css("display", "block");
    $(".popover-content > #errorMsgHoneyComb > #spanMsgErrorHoneyComb").empty();
    $(".popover-content > #errorMsgHoneyComb > #spanMsgErrorHoneyComb").text(Message);
    if (time != 'undefined') {
        $(".popover-content > #errorMsgHoneyComb").slideDown(time);
    }
    $("#errorMsgHoneyComb").find(".close").click(function (e) {
        $(this).closest(".alert").slideUp(400);
    });
}

function CloseIcon(item) {
    CloseClick = true;
    var taskId = item.attributes.taskid.value;
    var Totallength = $("#totalEntity").text();
    var Height = $(".popover").css('top').replace("px", "");
    var Blockheight = $('.popover-content').find(".hc-block").css('height').replace("px", "");
    var Margin = $('.popover-content').find(".hc-block").css('margin').split(" ")[0].replace("px", "");
    var Sum = parseInt(Number(Blockheight) + Number(Margin));
    var removeFromHoneyComb = true;
    if (isCalendarView == false) {
        if (IsPackageView && currentPkgIds.indexOf($(item).parents('.hc-block').attr('anchortacticid')) > -1) {
            var result = DeleteTacticFromPackageOnClickCloseIcon(item, taskId, Totallength, Height, Sum);
            if (!result) {
                return false;
            }
        }
        else {
            $("div[altid='" + taskId + "']").removeClass("honeycombbox-icon-gantt-Active");
            Totallength = Totallength - 1;
            Height = Number(Height) + (Sum);
            $("#totalEntity").text(Totallength);
            $(".popover").css('top', Height + "px");
        }
        var index = ExportSelectedIds.TaskID.indexOf(taskId);
        if (index >= 0) {
            ExportSelectedIds.TaskID.splice(index, 1);
            ExportSelectedIds.Title.splice(index, 1);
            ExportSelectedIds.OwnerName.splice(index, 1);
            ExportSelectedIds.TacticType.splice(index, 1);
            ExportSelectedIds.ColorCode.splice(index, 1);
            ExportSelectedIds.PlanFlag.splice(index, 1);
            ExportSelectedIds.CsvId.splice(index, 1);
            ExportSelectedIds.ROITacticType.splice(index, 1);
            ExportSelectedIds.AnchorTacticId.splice(index, 1);
            ExportSelectedIds.CalendarEntityType.splice(index, 1);
        }
    }
    else {
        if (IsPackageView && currentPkgIds.indexOf($(item).parents('.hc-block').attr('anchortacticid')) > -1) {
            var result = DeleteTacticFromPackageCalendarOnClickCloseIcon(item, taskId, Totallength, Height, Sum);
            if (!result) {
                return false;
            }
        }
        else {
            $("div[altId='" + taskId + "']").removeClass("honeycombbox-icon-gantt-Active");
            Totallength = Totallength - 1;
            Height = Number(Height) + (Sum);
            $("#totalEntity").text(Totallength);
            $(".popover").css('top', Height + "px");
        }
        var index = ExportSelectedIds.TaskID.indexOf(taskId);
        if (index >= 0) {
            ExportSelectedIds.TaskID.splice(index, 1);
            ExportSelectedIds.Title.splice(index, 1);
            ExportSelectedIds.OwnerName.splice(index, 1);
            ExportSelectedIds.TacticType.splice(index, 1);
            ExportSelectedIds.ColorCode.splice(index, 1);
            ExportSelectedIds.PlanFlag.splice(index, 1);
            ExportSelectedIds.CsvId.splice(index, 1);
            ExportSelectedIds.ROITacticType.splice(index, 1);
            ExportSelectedIds.AnchorTacticId.splice(index, 1);
            ExportSelectedIds.CalendarEntityType.splice(index, 1);
        }

    }
    item.parentNode.parentNode.parentNode.remove();
    var HoneyContent = $('.popover-content').find(".hc-block").length;
    if (HoneyContent == 0) {
        PreventUnPackageClick = true;
        IsPackageView = false;
        $(".popover ").hide();
        $(".honeycombbox").hide();
    }
};

function DeleteTacticFromPackageOnClickCloseIcon(item, taskId, Totallength, Height, Sum) {
    // If package is selected then apply validation as per selected asset or promotion
    var ROITacticType = $(item).parents('.hc-block').attr('roitactictype');
    var planTacticId = $(item).parents('.hc-block').attr('csvid').split('_')[1];
    var dhtmlxrowid = $(item).parents('.popover-content').find(".hc-block[roitactictype=" + AssetType + "]").attr('dhtmlxrowid');
    var IsPromotion = true;

    // If Asset tactic then provide confirmation to delete entire package
    if (ROITacticType.toLowerCase() == AssetType.toLowerCase()) {
        var deleteAsset = confirm('Deleting this Tactic will delete the entire package.\n Do you wish to continue?');
        IsPromotion = false;
        if (!deleteAsset) {
            removeFromHoneyComb = false;
            return false;
        }
    }

    // Delete tactics from database
    $.ajax({
        type: 'POST',
        beforeSend: function (x) {
            if (ROITacticType.toLowerCase() != AssetType.toLowerCase()) {
                myApp.hidePleaseWait();
            }
        },
        url:urlContent + 'Home/UnpackageTactics/',
        data: { AnchorTacticId: planTacticId, IsPromotion: IsPromotion },
        success: function (data) {
            var getvalue = HomeGrid.cells(dhtmlxrowid, TaskNameColIndex).getValue();
            var Index = getvalue.indexOf("pkgIcon");

            if (IsPromotion) {
                if (Index > -1) {
                    // Bind updated items to package icon into grid
                    var newValue = "<div id=pkgIcon class='package-icon package-icon-grid' style='cursor:pointer'  onclick=OpenHoneyComb(this) pkgtacids='" + data.remainItems.toString() + "'><i class='fa fa-object-group'></i></div>";
                    var oldValue = $(getvalue).get(0).outerHTML;
                    getvalue = getvalue.replace(oldValue, newValue);
                    HomeGrid.cells(dhtmlxrowid, TaskNameColIndex).setValue(getvalue);
                }
                // Unbind anchortacticid of the deleted package to honeycomb icons in the grid
                $('.honeycombbox-icon-gantt[csvid=Tactic_' + planTacticId + ']').each(function () {
                    $(this).attr('anchortacticid', '0');
                });
                // Update count on Honeycomb icon
                $("div[altid='" + taskId + "']").removeClass("honeycombbox-icon-gantt-Active");
                Totallength = Totallength - 1;
                Height = Number(Height) + (Sum);
            }
            else {
                // As package is deleted, remove package icon from the grid
                if (Index > -1) {
                    var newValue = $(getvalue).get(0).outerHTML;
                    getvalue = getvalue.replace(newValue, '');
                    HomeGrid.cells(dhtmlxrowid, TaskNameColIndex).setValue(getvalue);
                }
                // Unbind anchortacticid of the deleted package to honeycomb icons in the grid
                $('.honeycombbox-icon-gantt[anchortacticid=' + planTacticId + ']').each(function () {
                    $(this).attr('anchortacticid', '0');
                });

                // If asset tactic is deleted then entire package deleted so remove all data from honeycomb
                RemoveAllHoneyCombData();
                ShowMessage(false, PackageUnsuccessful, 3000);
            }

            $("#totalEntity").text(Totallength);
            $(".popover").css('top', Height + "px");
        }
    });
    return true;
}


function DeleteTacticFromPackageCalendarOnClickCloseIcon(item, taskId, Totallength, Height, Sum) {
    // If package is selected then apply validation as per selected asset or promotion
    var ROITacticType = $(item).parents('.hc-block').attr('roitactictype');
    var CalendarTaskID = $('.popover-content').find(".hc-block[roitactictype=" + AssetType + "]").attr('entitytaskid');
    var planTacticId = $(item).parents('.hc-block').attr('csvid').split('_')[1];
    var IsPromotion = true;
    var PromoID = 0;

    // If Asset tactic then provide confirmation to delete entire package
    if (ROITacticType.toLowerCase() == AssetType.toLowerCase() ) {
        var deleteAsset = confirm('Deleting this Tactic will delete the entire package.\n Do you wish to continue?');
        IsPromotion = false;
        if (!deleteAsset) {
            removeFromHoneyComb = false;
            return false;
        }
        else {
            $('.popover-content').find(".hc-block[anchortacticid= " + planTacticId + " ]").each(function () {
                var TaskID = $(this).attr('entitytaskid');
                AddRemovePackageItems.RemoveId.push(TaskID);
            });
        }
    }

    // Delete tactics from database
    $.ajax({
        beforeSend: function (x) {
            if (ROITacticType.toLowerCase() != AssetType.toLowerCase()) {
                myApp.hidePleaseWait();
            }
        },
        type: 'POST',
        url:urlContent + 'Home/UnpackageTactics/',
        data: { AnchorTacticId: planTacticId, IsPromotion: IsPromotion },
        success: function (data) {
            var TaskHtml = $("div[task_id='" + CalendarTaskID + "']");
            if (TaskHtml.length > 0) {
                TaskHtml = $("div[task_id='" + CalendarTaskID + "']")[0];
                var length = TaskHtml.childNodes.length;
            }
            if (TaskHtml != 'undefined' && TaskHtml != undefined && TaskHtml != null && TaskHtml.length != 0) {
                var FileNode = TaskHtml.getElementsByClassName("ROIPackage").item(0);
                if (FileNode != null && FileNode != 'undefined' && FileNode != undefined) {
                    FileNode = TaskHtml.getElementsByClassName("ROIPackage").item(0).outerHTML;
                }
            }
            var ViewBy = $('#ddlTabViewBy').val();

            if (IsPromotion) {
                AddRemovePackageItems.RemoveId.push(taskId);
                if (TaskHtml != 'undefined' && TaskHtml != undefined && TaskHtml != null && TaskHtml.length != 0) {
                    TaskHtml.getElementsByClassName("ROIPackage").item(0).setAttribute("pkgtacids", data.remainItems.toString());
                }

                if (AnchorTaskIdsList.Id.length != 0) {
                    $.each(AnchorTaskIdsList.Id, function (index, AnchorID) {
                        if (AnchorID == CalendarTaskID) {
                            AnchorTaskIdsList.Value[index] = data.remainItems.toString()
                        }
                    });
                }
                else {

                    AnchorTaskIdsList.Id.push(CalendarTaskID);
                    AnchorTaskIdsList.Value.push(data.remainItems.toString());
                }

                if (ViewBy != null && ViewBy != undefined && ViewBy == ViewByROI) {
                    var taskToDelete = $("div[altid='" + taskId + "']");
                    if (taskToDelete != undefined && taskToDelete.length > 0) {
                        gantt.deleteTask(taskId);
                    }
                }
                // Update count on Honeycomb icon
                // Conditions for Calender
                $("div[altid='" + taskId + "']").removeClass("honeycombbox-icon-gantt-Active");
                Totallength = Totallength - 1;
                Height = Number(Height) + (Sum);

                // Unbind anchortacticid of the deleted package to honeycomb icons in the grid
                $('.honeycombbox-icon-gantt[altid =' + taskId + ']').each(function () {
                    $(this).attr('anchortacticid', '0');
                });
            }
            else {

                // As package is deleted, remove package icon from the grid
                if (TaskHtml != 'undefined' && TaskHtml != undefined && TaskHtml != null && length > 1) {
                    var BlankDiv = TaskHtml.getElementsByClassName("gantt_blank").item(0);
                    var IsLinkIcon = TaskHtml.getElementsByClassName("unlink-icon");
                    var AllCHildNodes = TaskHtml.childNodes[1].innerHTML;
                    if (BlankDiv != null && BlankDiv != 'undefined' && BlankDiv != undefined) {
                        AllCHildNodes = AllCHildNodes.replace(FileNode, "");

                    }
                    else {
                        if (IsLinkIcon != null && IsLinkIcon != 'undefined' && IsLinkIcon != undefined && IsLinkIcon.length <= 1) {
                            AllCHildNodes = AllCHildNodes.replace(FileNode, "<div class='gantt_tree_icon gantt_blank'></div>");
                        }
                        else {
                            AllCHildNodes = AllCHildNodes.replace(FileNode, "");
                        }
                    }

                    TaskHtml.childNodes[1].innerHTML = AllCHildNodes;
                }

                $.each(AnchorTaskIdsList.Id, function (index, AnchorID) {
                    if (AnchorID == CalendarTaskID) {
                        var index = AnchorTaskIdsList.Id.indexOf(AnchorID);
                        if (index >= 0) {
                            AnchorTaskIdsList.Id.splice(index, 1);
                            AnchorTaskIdsList.Value.splice(index, 1);
                        }

                    }

                });
                AnchorTaskIdsList.Id.push(CalendarTaskID);
                if ( ViewBy != null && ViewBy != undefined && ViewBy == ViewByROI) {
                    var removeEntirePkg = taskId.split('_')[0];
                    gantt.deleteTask(removeEntirePkg);
                    gantt.refreshData();
                }
                // If there is no data then hide grid and display message
                if ($(gantt.$grid_data).find('.gantt_row').length <= 0) {
                    $("#gantt_here").hide();
                    $(".pull-right.toggle-status.source-sans-proregular").hide();
                    $("#improvements").hide();
                    $('#lblMarkringActivitiestitle').hide();
                    $('#divCollaborator').hide();
                    $('#divCollaboratorCount').hide(); // Added for #2112
                    $("#NodatawithfilterCalendar").show();
                }

                // If asset tactic is deleted then entire package deleted so remove all data from honeycomb
                RemoveAllHoneyCombData();

                $('.honeycombbox-icon-gantt[anchortacticid=' + planTacticId + ']').each(function () {
                    $(this).attr('anchortacticid', '0');
                });

                ShowMessage(false, PackageUnsuccessful, 3000);
            }
            $("#totalEntity").text(Totallength);
            $(".popover").css('top', Height + "px");
        }
    });

    return true;
}

function OpenHoneyComb(obj) {
    RemoveAllHoneyCombData();
    IsPackageView = true;
    PreventUnPackageClick = false;
    currentPkgIds = $(obj).attr('pkgtacids').replace(/ /g, '').split(',');
    $('.popover-content > #UnpackageHoneyComb').removeAttr('style');
    $('.popover-content > #UnpackageHoneyComb').attr('style', 'cursor: pointer !important');
    var ViewBy = $('#ddlTabViewBy').val();
    if (ViewBy != null && ViewBy != undefined && ViewBy == ViewByROI) {
        $('.popover-content > #PackageHoneyComb').removeAttr('style');
        $('.popover-content > #PackageHoneyComb').attr('style', 'color:gray; cursor: default !important');
    }
    var IdsNotinList = [];
    var TacticColorCode;
    for (i = 0; i < currentPkgIds.length; i++) {
        var rowObject = $('.honeycombbox-icon-gantt[taskid=' + currentPkgIds[i] + ']');
        if (rowObject.length > 1) {
            $('.honeycombbox-icon-gantt[taskid=' + currentPkgIds[i] + ']').each(function () {
                if ($(this).is(':visible') == true) {
                    rowObject = $(this);
                }
            });
        }
        var id = $(rowObject).attr('taskid');
        if (rowObject.is(':visible') == true && id == $(rowObject).attr('anchortacticid')) {
            PackageAnchorId = id;
            TacticColorCode = $(rowObject).attr('colorcode');
        }

        if (IsUpdate == true || rowObject.is(':visible') == false || (id == undefined || id == 'undefined' || id == null || id == "")) {
            IdsNotinList.push(currentPkgIds[i])
        }
        if (currentPkgIds.indexOf(id) >= 0) {
            $(rowObject).trigger('click');
        }
    }
    if (IdsNotinList.length != 0) {
        $.ajax({
            beforeSend: function (x) {
                myApp.hidePleaseWait();
            },
            type: 'POST',
            url: urlContent + 'Home/GetPackageTacticDetails/',
            data: {
                viewBy:$('#ddlTabViewBy').val(),
                TacticIds: IdsNotinList.toString(),
                TacticTaskColor:TacticColorCode,
                IsGridView: isCalendarView == false ? true : false
            },
            success: function (data) {
                var ListData = data.Listofdata;
                if (isCalendarView == false) {
                    $.each(IdsNotinList, function (index, TacticId) {
                        $.each(ListData, function (index, Data) {
                            if (Data.TacticId.toString() == TacticId) {
                                var index = ExportSelectedIds.TaskID.indexOf(Data.TaskId);
                                if (index < 0) {
                                    ExportSelectedIds.TaskID.push(Data.TaskId);
                                    ExportSelectedIds.Title.push(htmlDecode(Data.Title));
                                    ExportSelectedIds.OwnerName.push(Data.OwnerName);
                                    ExportSelectedIds.TacticType.push(Data.TacticTypeValue);
                                    ExportSelectedIds.ColorCode.push(Data.ColorCode);
                                    ExportSelectedIds.PlanFlag.push('Grid');
                                    ExportSelectedIds.CsvId.push(Data.CsvId);
                                    ExportSelectedIds.ROITacticType.push(Data.ROITacticType);
                                    ExportSelectedIds.AnchorTacticId.push(Data.AnchorTacticId);
                                    if (Data.TaskId != undefined && Data.TaskId != null && Data.TaskId != "") {
                                        ExpandTacticsForSelectedPackage(Data.TaskId);
                                    }
                                    $("div[altid='" + Data.TaskId + "']").addClass("honeycombbox-icon-gantt-Active");
                                }
                            }

                        });
                    });
                }
                else {
                    $.each(IdsNotinList, function (index, TacticId) {
                        $.each(ListData, function (index, Data) {
                            if (Data.TacticId.toString() == TacticId) {
                                var ValueAlreadyExists = false
                                $("div[altId='" + Data.TaskId + "']").addClass("honeycombbox-icon-gantt-Active");
                                $.each(ExportSelectedIds.TaskID, function (index, TaskId) {
                                    var Ids = TaskId.split('_');
                                    var index = Ids.indexOf("T" + TacticId);
                                    if (index >= 0) {
                                        ValueAlreadyExists = true;
                                    }

                                });
                                var Dataindex = ExportSelectedIds.TaskID.indexOf(Data.TaskId);
                                if (Dataindex < 0 && ValueAlreadyExists == false) {
                                    ExportSelectedIds.TaskID.push(Data.TaskId);
                                    ExportSelectedIds.Title.push(htmlDecode(Data.Title));
                                    ExportSelectedIds.OwnerName.push(Data.OwnerName);
                                    ExportSelectedIds.TacticType.push(Data.TacticTypeValue);
                                    ExportSelectedIds.ColorCode.push(Data.ColorCode);
                                    ExportSelectedIds.PlanFlag.push('Calender');
                                    ExportSelectedIds.CsvId.push(Data.CsvId);
                                    ExportSelectedIds.ROITacticType.push(Data.ROITacticType);
                                    ExportSelectedIds.CalendarEntityType.push(Data.CalendarEntityType);
                                    ExportSelectedIds.AnchorTacticId.push(Data.AnchorTacticId);
                                }

                            }

                        });
                    });
                }
                $("#totalEntity").text(ExportSelectedIds.TaskID.length);
                $(".honeycombbox").show();
            }
        });
    }
}

//to expand tactics for selected package
function ExpandTacticsForSelectedPackage(PCPTId) {
    var arrEntityIds = PCPTId.split('_');
    var CampaignId = "", ProgramId = "", TacticId = "";
    if (arrEntityIds != null && arrEntityIds.length > 2) {
        CampaignId = arrEntityIds[1].replace('C', '');
        ProgramId = arrEntityIds[2].replace('P', '');
        var divCamp = $('div[taskid="' + CampaignId + '"]').attr('altId');
        HomeGrid.openItem(divCamp);
        var divProg = $('div[taskid="' + ProgramId + '"]').attr('altId');
        HomeGrid.openItem(divProg);
    }
}
// Method to refresh calender after popup close-- #2587
function RefreshCurrentTab() {
    LoadFilter();
    if (!IsBudgetGrid) {
    BindPlanCalendar();
    }
    else if (IsBudgetGrid) {
        LoadBudgetGrid();
    }

}

///Added by dhvani to open inspect popup for URL -- PL #2534
function ShowInspectForPlanTacticId() {
    var hdnIsImprovement = $('#hdnIsImprovement');
    ShowModel(null, true);
    $('#hdnshowInspectForPlanTacticId').val(0);
    $('#hdnIsImprovement').val(false);

}

function ShowInspectForPlanCampaignId() {
    ShowModel(null, true);
    $('#hdnshowInspectForPlanCampaignId').val(0);
}

function ShowInspectForPlanProgramId() {
    ShowModel(null, true);
    $('#hdnshowInspectForPlanProgramId').val(0);
}

function ShowInspectForPlanLineItemId() {
    ShowModel(null, true);
    $('#hdnShowInspectForPlanLineItemId').val(0);
    }

function ShowInspectForPlanId() {
    var hdnShowInspectForPlanId = $("#CurrentPlanId")
    if (hdnShowInspectForPlanId.val() != 0) {
        ShowModel(null, true);
        $('#hdnShowInspectForPlanId').val(0);
    }
}
function SetselectedRow()
{
    var idcoluIndex = '';
    if ($('#IsGridView').val().toLowerCase()=='true')
        idcoluIndex = HomeGrid.getColIndexById('id');
    else
        idcoluIndex = HomeGrid.getColIndexById('ActivityId');
    if (isCopyTacticHomeGrid != 0) {
        var selectedcell = HomeGrid.findCell(isCopyTacticHomeGrid, idcoluIndex, true);
        var id = selectedcell[0];
        var rowid;
        if (id != undefined && id != 'undefined') {
            rowid = id[0];
            HomeGrid.openItem(rowid);
            ItemIndex = HomeGrid.getRowIndex(rowid);
            state0 = ItemIndex;
            HomeGrid.selectRow(HomeGrid.getRowIndex(rowid), true, true, true);
        }
        selectedTaskID = rowid;
        //isCopyTacticHomeGrid = 0;
        //isCopyTactic = 0;
    }
    else if (isEditTacticHomeGrid != 0) {
        var selectedcell = HomeGrid.findCell(isEditTacticHomeGrid, idcoluIndex, true);
        var id = selectedcell[0];
        var rowid;
        if (id != undefined && id != 'undefined') {
            rowid = id[0];
            HomeGrid.openItem(HomeGrid.getParentId(rowid));
            HomeGrid.selectRow(HomeGrid.getRowIndex(rowid), true, true, true);
            ItemIndex = HomeGrid.getRowIndex(rowid);
            state0 = ItemIndex;
        }
        selectedTaskID = rowid;
        //isEditTacticHomeGrid = 0;
        //isEditTactic = 0;
    }
}
//Following function is used to export to pdf from honeComb
function CallPdfHoneyComb() {
   
    var isQuater = $("#ddlUpComingActivites").val();
    PdfFilters.SelectedPlans = [];
    $.each(filters.PlanIDs, function () {
        PdfFilters.SelectedPlans.push(this);
    });
    PdfFilters.OwnerIds = [];
    $.each(filters.OwnerIds, function () {
        PdfFilters.OwnerIds.push(this);
    });
    PdfFilters.TacticTypeids = [];
    $.each(filters.TacticTypeids, function () {
        PdfFilters.TacticTypeids.push(this);
    });
    PdfFilters.StatusIds = [];
    $.each(filters.StatusIds, function () {
        PdfFilters.StatusIds.push(this);
    });
    //PdfFilters.SelectedPlans = filters.SelectedPlans;
    PdfFilters.customFieldIds = filters.customFieldIds;
    var isAvailablerecord = false;
    // Modified By Nishant Sheth
    // Desc :: To resolve the #1981 code review points to resolve click on clear filters and
    if (PdfFilters.SelectedPlans.toString().trim().length > 0 && activeMenu == 'home') {
        isAvailablerecord = true;
    }
    else {
        if ($("#CurrentPlanId").val() > 0 && activeMenu != 'home') {
            isAvailablerecord = true;
        }
    }
    if (isAvailablerecord) {
        var GanttTaskDataforHoney = {
            data: new Array()
        };// Added By Rahul Shah For HoneyComp Pdf
        var GanttTaskDataNew = GanttTaskData;
        var defaultParentArr = {
            index: [],
            parent: [],
            tacticId: [],

        }
        if (ExportSelectedIds.TaskID.length <= 0) {
            ExportSelectedIds.TaskID = ExportSelectedIdsafterSearch.TaskID;
        }
        var Status = "";
      
        var planyear = GanttTaskDataNew.data[0].start_date.getFullYear();
        for (var i = 0; i < GanttTaskDataNew.data.length; i++) {
            if (GanttTaskDataNew.data[i].type != undefined) {
                if (GanttTaskDataNew.data[i].type.toUpperCase().toString() == "PLAN") {

                    Status = GanttTaskDataNew.data[i].Status;
                }
            }
            for (var j = 0; j < ExportSelectedIds.TaskID.length; j++) {
                if (GanttTaskDataNew.data[i].id == ExportSelectedIds.TaskID[j]) {

                    GetParentIdRecursive(GanttTaskDataNew.data[i].id);

                    //defaultParentArr.index.push(i);

                    //defaultParentArr.parent.push(GanttTaskDataNew.data[i].parent);
                    //GanttTaskDataforHoney.data[GanttTaskDataforHoney.data.length - 1].parent = 0;
                    //GanttTaskDataforHoney.data[GanttTaskDataforHoney.data.length - 1].color = '#' + GanttTaskDataforHoney.data[GanttTaskDataforHoney.data.length - 1].colorcode;
                    if (GanttTaskDataNew.data[i].type != undefined) {
                        if (GanttTaskDataNew.data[i].type.toUpperCase().toString() == "TACTIC") {

                            //TotalTact = TotalTact + 1;
                            //TotalCost = TotalCost + GanttTaskDataNew.data[i].cost;
                            // Add BY Nishant Sheth
                            //Desc:: resolve the issue with view by dropdown
                            var name_Id = GanttTaskDataNew.data[i].id.split('_');
                            var name = name_Id[0];
                            var PlanId;
                            var CampaignId;
                            var ProgramId;
                            var TacticId;
                            var ImpTacticId;
                            var LineId;
                            if (name_Id.length == 1) {
                                PlanId = name_Id[0].replace("L", "");
                            }
                            else if (name_Id.length == 2) {
                                // Chaneg By Nishant Sheth
                                // Desc:: To resolve issue with change view by dropdown
                                var nameindex = name.indexOf("Z");
                                if (nameindex >= 0) {
                                    PlanId = name_Id[1].replace("L", "");
                                    CampaignId = name_Id[1].replace("C", "");
                                } else {
                                    PlanId = name_Id[0].replace("L", "");
                                    CampaignId = name_Id[1].replace("C", "");
                                }
                            }
                            else if (name_Id.length == 3) {
                                // Chaneg By Nishant Sheth
                                // Desc:: To resolve issue with change view by dropdown
                                var nameindex = name.indexOf("Z");
                                if (nameindex >= 0) {
                                    PlanId = name_Id[1].replace("L", "");
                                    CampaignId = name_Id[2].replace("C", "");
                                    //ProgramId = name_Id[2].replace("P", "");
                                } else {
                                    PlanId = name_Id[0].replace("L", "");
                                    CampaignId = name_Id[1].replace("C", "");
                                    ProgramId = name_Id[2].replace("P", "");
                                }
                            }
                            else if (name_Id.length == 4) {
                                // Chaneg By Nishant Sheth
                                // Desc:: To resolve issue with change view by dropdown
                                var nameindex = name.indexOf("Z");
                                if (nameindex >= 0) {
                                    PlanId = name_Id[1].replace("L", "");
                                    CampaignId = name_Id[2].replace("C", "");
                                    ProgramId = name_Id[3].replace("P", "");
                                    //TacticId = name_Id[3].replace("T", "");
                                } else {
                                    PlanId = name_Id[0].replace("L", "");
                                    CampaignId = name_Id[1].replace("C", "");
                                    ProgramId = name_Id[2].replace("P", "");
                                    TacticId = name_Id[3].replace("T", "");
                                }
                            }
                            else {

                                // Chaneg By Nishant Sheth
                                // Desc:: To resolve issue with change view by dropdown
                                var nameindex = name.indexOf("Z");
                                if (nameindex >= 0) {
                                    PlanId = name_Id[1].replace("L", "");
                                    CampaignId = name_Id[2].replace("C", "");
                                    ProgramId = name_Id[3].replace("P", "");
                                    TacticId = name_Id[4].replace("T", "");
                                    //LineId = name_Id[4];
                                } else {
                                    PlanId = name_Id[0].replace("L", "");
                                    CampaignId = name_Id[1].replace("C", "");
                                    ProgramId = name_Id[2].replace("P", "");
                                    TacticId = name_Id[3].replace("T", "");
                                    LineId = name_Id[4];
                                }
                            }

                            defaultParentArr.tacticId.push(TacticId);
                        }
                    }
                }
            }
        }

        for (var p = 0; p < selectedIds.length; p++) {

            var taskdata = gantt.getTask(selectedIds[p]);
            GanttTaskDataforHoney.data.push(taskdata);
            GanttTaskDataforHoney.data[GanttTaskDataforHoney.data.length - 1].color = '#' + GanttTaskDataforHoney.data[GanttTaskDataforHoney.data.length - 1].colorcode;
        }

        var tacticids = defaultParentArr.tacticId;
        gantt.clearAll();
        gantt.init("gantt_honey");
        gantt.parse(GanttTaskDataforHoney);
        gantt.refreshData();
        var htmltestdiv = '';
        htmltestdiv += '<h2 style="color: #040707; margin: 0 0 10px;">Marketing Calendar</h2>';
        htmltestdiv += '<p style=" line-height: 24px;margin: 0;">';
        htmltestdiv += '<label style="color:#050708;font-weight: 600;">Time Frame: </label>';
        htmltestdiv += '<span style="color: #9C9C9C; font-weight: 600; font-size: 14px;">';
        htmltestdiv += isQuater + '</span>';
        htmltestdiv += '</p>';
     
        var innerfiltervalueplan = "";
        var isselectedplan = true;
        var DropDownPlan = true;
        $("#ulSelectedPlans").find("input[type=checkbox]").each(function () {
            if ($(this).attr('checked') == 'checked') {
                isselectedplan = true;
            }
        });
        if (PdfFilters.SelectedPlans.toString().trim().length > 0) {
            $.each(PdfFilters.SelectedPlans, function () {
                // Export pdf issue for plan name display undefined
                innerfiltervalueplan += " " + $("#" + this).parent().parent("#ulSelectedPlans").children().children("#" + this).parent().attr('title') + ",";
            });
        }

        if (PdfFilters.SelectedPlans.toLocaleString().trim().length > 0) {
            DropDownPlan = false;
        }

        if (innerfiltervalueplan != undefined && innerfiltervalueplan != 'undefined' && innerfiltervalueplan != '') {
            htmltestdiv += '<p style=" line-height: 24px;margin: 0;">';
            htmltestdiv += '<label style="color:#050708;font-weight: 600;">Plan: </label>';
            htmltestdiv += '<span style="color: #9C9C9C; font-weight: 600; font-size: 14px;">';
            innerfiltervalueplan = DropDownPlan == true ? innerfiltervalueplan : innerfiltervalueplan.substring(0, innerfiltervalueplan.length - 1);
            htmltestdiv += innerfiltervalueplan + '</span>';
            htmltestdiv += '</p>';
        }
        var Parentid = "";

        $.each($('#divCustomFieldsFilter').find('.dropdown-section'), function () {

            var headertitle = $(this).find("h2 span").text();
            var innerfiltervalue = "";

            Parentid = this.id;
            var sl = Parentid.lastIndexOf('-');
            Parentid = Parentid.substring(sl + 1, sl.length).toString();

            if (PdfFilters.customFieldIds.length > 0) {

                var a = PdfFilters.customFieldIds;
                $.each(a, function () {
                    if (this != null && this != undefined && this != 'undefined') {

                        var liId = this.toString();
                        var splitIds = liId.split('_');
                        if (Parentid == splitIds[0].toString()) {
                            var actualId = "#li_" + Parentid + "_" + splitIds[1];
                            var chkid = $(actualId).attr("title");
                            innerfiltervalue += " " + chkid + ",";
                        }

                    }
                });
                if (innerfiltervalue != undefined && innerfiltervalue != 'undefined' && innerfiltervalue != '') {
                    //Added following for #2592 feedback point, idf any value will not be selected in custom field then it will not be display as per other filter.
                    if (innerfiltervalue.indexOf("undefined") <= 0) {
                        htmltestdiv += '<p style=" line-height: 24px;margin: 0;">';
                        htmltestdiv += '<label style="color:#050708;font-weight: 600;">' + headertitle + ': </label>';
                        htmltestdiv += '<span style="color: #9C9C9C; font-weight: 600; font-size: 14px;">';
                        innerfiltervalue = innerfiltervalue.substring(0, innerfiltervalue.length - 1);
                        htmltestdiv += innerfiltervalue + '</span>';
                        htmltestdiv += '</p>';
                    }
                }
            }

        });

        var innerfiltervalueOwner = "";
        var isselectedOwner = true;
        if (PdfFilters.OwnerIds.toString().trim().length > 0) {
            $.each(PdfFilters.OwnerIds, function () {
                innerfiltervalueOwner += " " + $("#ulSelectedOwner").find('#liOwner' + this).attr("title") + ",";

            });
        }

        if (innerfiltervalueOwner != undefined && innerfiltervalueOwner != 'undefined' && innerfiltervalueOwner != '') {
            htmltestdiv += '<p style=" line-height: 24px;margin: 0;">';
            htmltestdiv += '<label style="color:#050708;font-weight: 600;">Owner: </label>';
            htmltestdiv += '<span style="color: #9C9C9C; font-weight: 600; font-size: 14px;">';
            innerfiltervalueOwner = innerfiltervalueOwner.substring(0, innerfiltervalueOwner.length - 1);
            htmltestdiv += innerfiltervalueOwner + '</span>';
            htmltestdiv += '</p>';
        }

        var innerfiltervalueTacticType = "";
        var isselectedTacticType = true;
        if (PdfFilters.TacticTypeids.length > 0) {
            $.each(PdfFilters.TacticTypeids, function () {
                innerfiltervalueTacticType += " " + $("#ulTacticType").find('#liTT' + this).attr("title") + ",";

            });
        }

        if (innerfiltervalueTacticType != undefined && innerfiltervalueTacticType != 'undefined' && innerfiltervalueTacticType != '') {
            htmltestdiv += '<p style=" line-height: 24px;margin: 0;">';
            htmltestdiv += '<label style="color:#050708;font-weight: 600;">Tactic Type: </label>';
            htmltestdiv += '<span style="color: #9C9C9C; font-weight: 600; font-size: 14px;">';
            innerfiltervalueTacticType = innerfiltervalueTacticType.substring(0, innerfiltervalueTacticType.length - 1);
            htmltestdiv += innerfiltervalueTacticType + '</span>';
            htmltestdiv += '</p>';
        }

        var innerfiltervalueStatus = "";
        var isselectedStatus = true;
        if (PdfFilters.StatusIds.length > 0) {
            $.each(PdfFilters.StatusIds, function () {
                innerfiltervalueStatus += " " + $("#ulStatus").find('#' + this).parent().find("span").text() + ",";

            });
        }

        if (innerfiltervalueStatus != undefined && innerfiltervalueStatus != 'undefined' && innerfiltervalueStatus != '') {
            htmltestdiv += '<p style=" line-height: 24px;margin: 0;">';
            htmltestdiv += '<label style="color:#050708;font-weight: 600;">Status: </label>';
            htmltestdiv += '<span style="color: #9C9C9C; font-weight: 600; font-size: 14px;">';
            innerfiltervalueStatus = innerfiltervalueStatus.substring(0, innerfiltervalueStatus.length - 1);
            htmltestdiv += innerfiltervalueStatus + '</span>';
            htmltestdiv += '</p>';
        }
        $("#filterdiv").html('');
        $("#filterdiv").append(htmltestdiv);

        var originurl = document.location.origin;
        var mypathurl = document.location.pathname.split('/')[1];
        var urlpdf = originurl + "/" + mypathurl + "/"; // This line use for stage servers.

        //$("#imglogopdf").attr("src", urlpdf + "Content/images/game-plan-logo.png")
        $("#DivBullhornbg").parent().find('.bullhorn-bg').each(function () {
            $("#mqlmaindiv").css("background-image", "url('" + urlpdf + "Content/images/bullhorn-bg.png')");

            $("#mqlmaindiv").css("background-position", "right bottom");
            $("#mqlmaindiv").css("background-repeat", "no-repeat");
        });

        $("#DivBullhornbg").parent().find('.bullhorn-bg-disabled').each(function () {
            $("#mqlmaindiv").css("background-image", "url('" + urlpdf + "Content/images/bullhorn-bg-disabled.png')");

            $("#mqlmaindiv").css("background-position", "right bottom");
            $("#mqlmaindiv").css("background-repeat", "no-repeat");
        });

        $("#DivBullhornbg").parent().find('.bulldoller-bg').each(function () {

            $("#budgetmaindiv").css("background-image", "url('" + urlpdf + "Content/images/icon-dollar.png')");

            $("#budgetmaindiv").css("background-position", "right bottom");
            $("#budgetmaindiv").css("background-repeat", "no-repeat");
        });
        var TotalTact = 0;
        var TotalCost = 0;
        var TotalMql = "";
        var ChartHtml = "";
        var Canvas = "";
        var stryear = isQuater;

        $.ajax({
            type: 'POST',
            async: false,
            url: urlContent + 'Home/GetHeaderDataforHoneycombPDF/',
            data: {
                TactIds: tacticids.toString(),
                //strParam: planyear.toString()
                strParam: stryear.toString()
            },
            success: function (data) {

                $("#pdfchart").find('.dhx_canvas_text').remove();
                $("#pdfchart").find("canvas").remove();
                setpdfgraphdata(data.lstchart);
                TotalMql = data.TotalMql;
                TotalCost = data.TotalCost;
                TotalTact = data.TotalCount;
                ChartHtml = $("#chartCanvas").html();
                Canvas = $("#chartpdf").find(".dhx_canvas_text");
            },
            error: function () {
            }
        });
        $("#mqllabeltest").text($("#pmqlLabel").text());
        $("#budgetlabeltest").text($("#pcostLabel").text());

        if (Status != undefined && Status != null) {
            if (Status.toUpperCase().toString() == "DRAFT") {
                $("#mqlspanvalue").text($("#pMQLs").text());
                $("#budgetspanvalue").text($("#pbudget").text());
            }
            else {
                $("#HoneyMQLs").text(TotalMql);
                SetPriceValue("#HoneyMQLs")
                $("#mqlspanvalue").text($("#HoneyMQLs").text());
                $("#honeybudget").text(TotalCost);
                SetBudget("#honeybudget")
                $("#budgetspanvalue").text($("#honeybudget").text());
            }
        }
        $("#ptacticcounttest").text(TotalTact);

        if (isQuater.split('-').length > 1) {

            $("#AttachChartCanvas").html('');
            $("#legendchart").show();
            var ChartHtml = $("#chartCanvasmultiyear").html();
            var Chartlegend = $("#legendchart").html();
            var Canvas = $("#chartpdf").find(".dhx_canvas_text");
            var i = 0;
            var AttachChartHtml = '';
            AttachChartHtml = ChartHtml;
            var leAlignment = 15;
            var z = 20;
            var colorchart = "#C633C9;";
            $.each(Canvas, function () {
                if (i > 7) {

                    var left = $(this).css('left');
                    var leftvalue = "";
                    var top = $(this).css('top');
                    var text = $(this).text();
                    var BarHeight = 55 - parseInt(top);
                    if (parseInt(left) > 60) {
                        colorchart = "#407B22;";
                    }
                    AttachChartHtml += "<div style='margin-left:" + left + "; margin-top:" + top + ";overflow: hidden;position: absolute;text-align: center;white-space: nowrap;font-size: 6px;font-weight: 200;'>";
                    AttachChartHtml += text;
                    AttachChartHtml += "<div style='width:5px;background-color:" + colorchart + " height:" + BarHeight + "px;border-top-left-radius: 10px;border-top-right-radius: 10px;'></div>";
                    //AttachChartHtml += "<div id='legend'></div>"
                    AttachChartHtml += "</div>";
                }

                i++;
            });

            $("#firstyear").text(isQuater.split('-')[0]);
            $("#secondyear").text(isQuater.split('-')[1]);
            //AttachChartHtml += Chartlegend;
            $("#AttachChartCanvasmultiyear").html('');
            $("#AttachChartCanvasmultiyear").html(AttachChartHtml);
            //$("#AttachChartCanvasmultiyear").css('margin-left', '60px');
        }
        else {
            $("#legendchart").hide();
            $("#AttachChartCanvasmultiyear").html('');
            ChartHtml = $("#chartCanvas").html();
            Canvas = $("#chartpdf").find(".dhx_canvas_text");
            var i = 0;
            var AttachChartHtml = ChartHtml;
            $.each(Canvas, function () {

                if (i > 4) {
                    var left = $(this).css('left');
                    var top = $(this).css('top');
                    var text = $(this).text();
                    var BarHeight = 55 - parseInt(top);
                    AttachChartHtml += "<div style='margin-left:" + left + "; margin-top:" + top + ";overflow: hidden;position: absolute;text-align: center;white-space: nowrap;font-size: 6px;font-weight: 200;'>";
                    AttachChartHtml += text;
                    AttachChartHtml += "<div style='width:5px;background-color: #C633C9;height:" + BarHeight + "px;border-top-left-radius: 10px;border-top-right-radius: 10px;'></div>";
                    AttachChartHtml += "</div>";
                }
                i++;
            });

            $("#AttachChartCanvas").html('');
            $("#AttachChartCanvas").html(AttachChartHtml);
        }
        $("#testdiv #mqlspanpercentage").hide();
        $('#AttachChartCanvas').css('margin-left', '55px');
        var headerhtml = $("#testdiv").html();

        var style = "<style> div[task_id='L10874']:last-of-type {background-color:rgb(202, 60, 206)} </style>";

        var datestart;
        var dateend;

        if ($.isNumeric(isQuater)) {
            datestart = "01-01-" + isQuater;
            dateend = "01-01-" + (parseInt(isQuater) + 1).toString();
        }
        else if (isQuater == 'thismonth') {
            var date = new Date(), m = date.getMonth();
            datestart = "01-" + (m + 1).toString() + "-" + date.getFullYear().toString();
            dateend = "01-" + (m + 2).toString() + "-" + date.getFullYear().toString();
            $('#mainpdfdiv').css('width', '1000px');
            var headerhtml = $("#testdiv").html();
        }
        else if (isQuater == 'thisquarter') {
            var startDate = getQarterStartDate();
            var m = startDate.getMonth();
            datestart = "01-" + (m + 1).toString() + "-" + startDate.getFullYear().toString();
            dateend = "01-" + (m + 4).toString() + "-" + startDate.getFullYear().toString();
            $('#mainpdfdiv').css('width', '1000px');
            var headerhtml = $("#testdiv").html();
        }
        else {
            //added by Rahul Shah on 18/12/2015 for PL #1766
            var PlanYears = isQuater.split("-");
            var yearDiffrence = (parseInt(PlanYears[1]) - parseInt(PlanYears[0])) + 1;
            datestart = "01-01-" + isQuater.split("-")[0];
            dateend = "01-01-" + (parseInt(isQuater) + yearDiffrence).toString();
            //Added by Rahul Shah for PL #1813

            // $('#AttachChartCanvas').css('margin-left', '160px');
            $('#mainpdfdiv').css('width', '');
            var headerhtml = $("#testdiv").html();
        }
        gantt.getGridColumn("colorcode").hide = true; 
        gantt.exportToPDF({
            header: headerhtml,
            start: datestart,
            end: dateend

        });

        //for (var m = 0; m < defaultParentArr.index.length; m++) {
        //    var indexdef = defaultParentArr.index[m];
        //    var parentdef = defaultParentArr.parent[m];
        //    GanttTaskData.data[indexdef].parent = parentdef;
        //}
        selectedIds = [];
    
        GanttTaskDataforHoney.data = new Array();
        gantt.clearAll();
        gantt.init("gantt_here");
        gantt.parse(GanttTaskData);
        gantt.getGridColumn("colorcode").hide = false;
        gantt.refreshData();

    }
    else {
        $('#cErrorInspectPopup').html('No plan selected to export the data in .pdf'); //PL #1595 - Dashrath Prajapati
        $('#errorMessageInspectPopup').css("display", "block");
        $('#errorMessageInspectPopup').removeClass('message-position');
    }
}
var selectedIds =[];
function GetParentIdRecursive(id) {
    
    var checkindexselected = selectedIds.indexOf(id);
    if (!(checkindexselected >= 0)) {
        selectedIds.push(id);
        var getparentid = gantt.getParent(id);
        if (getparentid != 0) {
            return GetParentIdRecursive(getparentid);
        }
    }

}
function setpdfgraphdata(lstchart) {
    
    //$(".dhx_chart_legend").html('');
    // $(".dhx_chart_legend").hide();
    var legendvalue = "";
    var activityyear = $('select#ddlUpComingActivites option:selected').val();
    if (activityyear.split('-').length > 1) {
        legendvalue = [{ text: activityyear.split('-')[0], color: "#c633c9" }, { text: activityyear.split('-')[1], color: "#407B22" }];
    }
    else {
        legendvalue = [];
    }
    var barChart2 = new dhtmlXChart({
        view: "bar",
        container: "chartpdf",
        value: "#NoOfActivity#",
        label: "#NoOfActivity#",
        color: "#Color#",
        radius: 3,
        padding: {
            top: 25,
            bottom: 16,
            right: 00,
            left: 00

        },
        xAxis: {
            template: "#Month#"
        },
        legend: {
            width: 3,
            align: "right",
            valign: "middle",
            marker: {
                type: "round",
                width: 8
            },
            values: legendvalue
        },
    });
    barChart2.parse(lstchart, "json");
    $('.dhx_chart_legend').css({ 'left': '100px', 'top': '21px' });

    var dhtml_length = $('.dhx_chart_legend').length;
    if (activityyear.split('-').length > 1) {
        var i = 0;
        var leftcss;
        $(".dhx_canvas_text").each(function () {
            i++;
            if (i > 8) {
                leftcss = parseInt($(this).css('left'));
                $(this).css('left', (leftcss - 1));
            }
            else {
                leftcss = parseInt($(this).css('left'));
                $(this).css('left', (leftcss + 7));
            }

        });
    }
}
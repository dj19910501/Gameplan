//To Load PlanGrid
//Start
function LoadPlanGrid() {
    $.ajax({
        url: urlContent + 'Plan/LoadHomeGrid_NewDesign/',
        data: {
            planId: filters.PlanIDs.toString(),
            ownerIds: filters.OwnerIds.toString(),
            TacticTypeid: filters.TacticTypeids.toString(),
            StatusIds: filters.StatusIds.toString(),
            customFieldIds: filters.customFieldIds.toString()
           
        },
        success: function (result) {
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
        ShowModels(inspectEdit, secPlan, id, 0, '@RequestedModule');
    }
    else if (type == "CP") {
        ShowModels(inspectEdit, secCampaign, id, 0, '@RequestedModule');
    }
    else if (type == "PP") {
        ShowModels(inspectEdit, secProgram, id, 0, '@RequestedModule');
    }
    else if (type == "TP") {
        ShowModels(inspectEdit, secTactic, id, 0, '@RequestedModule');
    }
    else if (type == "LP") {
        ShowModels(inspectEdit, secLineItem, id, 0, '@RequestedModule');
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

function DisplayPopUpMenu(obj, e) {
    gridSearchFlag = 1;
    var LinkTacticPermission = $(obj).attr('linktacticpermission');
    var LinkedTacticId = $(obj).attr('linkedtacticid');
    var type = $(obj).attr('id');
    var name = $(obj).attr('Name');
    var title = $(obj).attr('aria-label');
    var permission = $(obj).attr('Permission');
    var LineType = $(obj).attr('lineitemtype');
    if ($('#IsGridView').val().toLowerCase() == "true") {

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
            if ('@IsPlanCreateAuthorized') {
                ul = "<ul style='margin: 0;'>  <li class='new-prog' id='NewPlan'>New Plan </li> <li class='new-prog' id='ClonePlan1'>Copy Plan</li>  <li class='new-prog' id='ChildCampaign'>New Campaign</li> </ul>";
            }
            else {
                ul = "<ul style='margin: 0;'> <li class='new-prog' id='ChildCampaign'>New Campaign</li> </ul>";
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
        var url = urlContent + 'Plan/CreatePlan/';
        //var url = '@Url.Action("CreatePlan", "Plan")';
        displayconfirm(url);
    });

    $('#ClonePlan1').click(function () {
        var planId = PlanId;
        if (parseInt(planId) > 0) {
            $.ajax({
                type: 'POST',
                //url: '@Url.Content("~/Plan/Clone")',
                url: urlContent + 'Plan/Clone/',
                data: {
                    CloneType: planCloneType,
                    Id: PlanId,
                    title: ''
                },
                success: function (data) {
                    if (data.returnURL != 'undefined' && data.returnURL == '#') {
                        //window.location = '@Url.Content("~/Login/Index")';
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
        ShowModels(inspectAdd, secCampaign, 0, planId, '@RequestedModule');
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
                CalledFromBudget: '@CalledFromBudget',
                RequsetedModule: '@RequestedModule',
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
                        RefreshCurrentTab();
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
        ShowModels(inspectAdd, secProgram, 0, CampaignId, '@RequestedModule');
        $('.taskpopup').css('display', 'none');
    });

    //New Program
    $('#NewProgram').click(function () {
        isCopyTactic = CampaignId;
        ShowModels(inspectAdd, secProgram, 0, CampaignId, '@RequestedModule');
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
                CalledFromBudget: '@CalledFromBudget',
                RequsetedModule: '@RequestedModule'
            },
            success: function (data) {
                if (data.IsSuccess != 'undefined' && data.IsSuccess == '#') {
                    //window.location = '@Url.Content("~/Login/Index")';
                    window.location = urlContent + '/Login/Index';
                }
                else if (data.IsSuccess) {
                    RefershPlanHeaderCalc();
                    isCopyTactic = data.Id;
                    isCopyTacticHomeGrid = isCopyTactic;
                    if ($('#IsGridView').val().toLowerCase() == "false") {
                        RefreshCurrentTab();
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
        ShowModels(inspectAdd, secTactic, 0, ProgramId, '@RequestedModule');
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
        ShowModels(inspectAdd, secTactic, 0, ProgramId, '@RequestedModule');
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
            //url: '@Url.Content("~/Inspect/Clone")',
            url: urlContent + 'Inspect/Clone/',
            data: {
                CloneType: tacticCloneType,
                Id: idPlanTactic,
                title: Title,
                CalledFromBudget: '@CalledFromBudget',
                RequsetedModule: '@RequestedModule'
            },
            success: function (data) {
                if (data.IsSuccess != 'undefined' && data.IsSuccess == '#') {
                    //window.location = '@Url.Content("~/Login/Index")';
                    window.location = urlContent + '/Login/Index';
                }
                else if (data.IsSuccess) {
                    RefershPlanHeaderCalc();
                    isCopyTactic = data.Id;
                    isCopyTacticHomeGrid = isCopyTactic;
                    if ($('#IsGridView').val().toLowerCase() == "false") {
                        RefreshCurrentTab();
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
        ShowModels(inspectAdd, secLineItem, 0, TacticId, '@RequestedModule');
        $('.taskpopup').css('display', 'none');
    });


    // New Line Item
    $('#NewLineItem').click(function () {
        isCopyTactic = TacticId;
        isDataModified = false;
        var planId = PlanId;
        ShowModels(inspectAdd, secLineItem, 0, TacticId, '@RequestedModule');
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
                CalledFromBudget: '@CalledFromBudget',
                RequsetedModule: '@RequestedModule'
            },
            success: function (data) {
                if (data.IsSuccess != 'undefined' && data.IsSuccess == '#') {
                    //window.location = '@Url.Content("~/Login/Index")';
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

        if ($('#IsGridView').val().toLowerCase() == "true") {
            if (isDataModified) {
                if (gridSearchFlag == 1) {
                    isCopyTacticHomeGrid = isCopyTactic;
                    isEditTacticHomeGrid = isEditTactic;
                    LoadPlanGrid();
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
//$(".popover-content").live('click', function (e) {
//    if (ExportToCsv) {
//        CloseClick = false;
//    }
//    else {
//        CloseClick = true;
//    }
//    ExportToCsv = false;

//});

$("#honeycomb").popover({

    title: 'HoneyComb',
    html: true,
    placement: 'top',
    content: function () {
        var IsPackageView = false; //temp change
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
        if ($('#IsGridView').val().toLowerCase() == "false" && ViewBy != null && ViewBy != undefined && ViewBy == '@Enums.DictPlanGanttTypes[Convert.ToString(PlanGanttTypes.ROIPackage)]') {
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
        var Removefromlist = [];
        if (ExportSelectedIds.TaskID.length > 0) {
            $('#honeycomb_content').html("");
            for (i = 0; i < (ExportSelectedIds.TaskID.length) ; i++) {
                var HasActiveClass = false;
                if ($('#IsGridView').val().toLowerCase() == "true") {
                    HasActiveClass = $("div[altid='" + ExportSelectedIds.TaskID[i] + "']").hasClass("honeycombbox-icon-gantt-Active");
                    if (HasActiveClass == true || IsPackageView == true) {
                        if (ExportSelectedIds.ROITacticType[i] == '@Enums.AssetType.Asset.ToString()') {
                            htmlstringForAsset += '<div class="hc-block asset-bg" anchortacticid="' + ExportSelectedIds.AnchorTacticId[i] + '" dhtmlxrowid="' + ExportSelectedIds.DhtmlxRowId[i] + '"  csvid="' + ExportSelectedIds.CsvId[i] + '" roitactictype="' + ExportSelectedIds.ROITacticType[i] + '"> <div class="row-fluid"> ' +
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
                            htmlstring += '<div class="hc-block" anchortacticid="' + ExportSelectedIds.AnchorTacticId[i] + '" dhtmlxrowid="' + ExportSelectedIds.DhtmlxRowId[i] + '"  csvid="' + ExportSelectedIds.CsvId[i] + '" roitactictype="' + ExportSelectedIds.ROITacticType[i] + '"> <div class="row-fluid"> ' +
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
                        Removefromlist.push(ExportSelectedIds.TaskID[i]);
                    }
                }
                else {
                    if (ExportSelectedIds.ROITacticType[i] == '@Enums.AssetType.Asset.ToString()') {
                        htmlstringForAsset += '<div class="hc-block asset-bg" anchortacticid="' + ExportSelectedIds.AnchorTacticId[i] + '"  EntityTaskId="' + ExportSelectedIds.TaskID[i] + '"  EntityType="' + ExportSelectedIds.CalendarEntityType[i] + '" roitactictype="' + ExportSelectedIds.ROITacticType[i] + '"><div class="row-fluid"><div  class="span8 pophover-left"  style="border-left: 5px solid #' + ExportSelectedIds.ColorCode[i] + '"><h5 title="' + htmlEncode(ExportSelectedIds.Title[i]) + '">' + htmlEncode(ExportSelectedIds.Title[i]) + '</h5>  <p class="metadata"><span>' + ExportSelectedIds.TacticType[i] + ' </span>|<span> ' + ExportSelectedIds.OwnerName[i] + '</span></p></div><div class="span1 text-right pull-right" title="Delete"><i taskid = "' + ExportSelectedIds.TaskID[i] + '" id ="CloseIcon"  class="fa fa-times" onclick="CloseIcon(this)" ></i></div></div></div>';
                    }
                    else {
                        htmlstring += '<div class="hc-block" anchortacticid="' + ExportSelectedIds.AnchorTacticId[i] + '"  EntityTaskId="' + ExportSelectedIds.TaskID[i] + '"  EntityType="' + ExportSelectedIds.CalendarEntityType[i] + '" roitactictype="' + ExportSelectedIds.ROITacticType[i] + '"><div class="row-fluid"><div  class="span8 pophover-left"  style="border-left: 5px solid #' + ExportSelectedIds.ColorCode[i] + '"><h5 title="' + htmlEncode(ExportSelectedIds.Title[i]) + '">' + htmlEncode(ExportSelectedIds.Title[i]) + '</h5>  <p class="metadata"><span>' + ExportSelectedIds.TacticType[i] + ' </span>|<span> ' + ExportSelectedIds.OwnerName[i] + '</span></p></div><div class="span1 text-right pull-right" title="Delete"><i taskid = "' + ExportSelectedIds.TaskID[i] + '" id ="CloseIcon"  class="fa fa-times" onclick="CloseIcon(this)" ></i></div></div></div>';
                    }
                }
            }
        }
        $('#honeycomb_content').append(errMessageTopDisplay + htmlstringForAsset + htmlstring + hccontrolsdiv);
        for (i = 0; i < Removefromlist.length; i++) {
            var index = ExportSelectedIds.TaskID.indexOf(Removefromlist[i]);
            if (index >= 0) {
                ExportSelectedIds.TaskID.splice(index, 1);
                ExportSelectedIds.Title.splice(index, 1);
                ExportSelectedIds.OwnerName.splice(index, 1);
                ExportSelectedIds.TacticType.splice(index, 1);
                ExportSelectedIds.ColorCode.splice(index, 1);
                ExportSelectedIds.ROITacticType.splice(index, 1);
                ExportSelectedIds.DhtmlxRowId.splice(index, 1);
                ExportSelectedIds.AnchorTacticId.splice(index, 1);
                ExportSelectedIds.CalendarEntityType.splice(index, 1);
                ExportSelectedIds.AnchorTacticId.splice(index, 1);
            }
        }
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
        DhtmlxRowId: [],
        AnchorTacticId: [],
        CalendarEntityType: [],
    };
    if ($('#IsGridView').val().toLowerCase() == "true") {
        $("div[class*='honeycombbox-icon-gantt-Active']").removeClass("honeycombbox-icon-gantt-Active")
    }
    else {
        $("div[class*='honeycombbox-icon-gantt-Active']").removeClass("honeycombbox-icon-gantt-Active");
    }
    $("#totalEntity").text(0);
    $(".popover ").hide();
    $(".honeycombbox").hide();
}

function OpenHoneyComb(obj) {
    RemoveAllHoneyCombData();
    IsPackageView = true;

    PreventUnPackageClick = false;
    currentPkgIds = $(obj).attr('pkgtacids').split(',');
    $('.popover-content > #UnpackageHoneyComb').removeAttr('style');
    $('.popover-content > #UnpackageHoneyComb').attr('style', 'cursor: pointer !important');
    var ViewBy = $('#ddlTabViewBy').val();
    if ($('#IsGridView').val().toLowerCase() == "false" && ViewBy != null && ViewBy != undefined && ViewBy == '@Enums.DictPlanGanttTypes[Convert.ToString(PlanGanttTypes.ROIPackage)]') {
        $('.popover-content > #PackageHoneyComb').removeAttr('style');
        $('.popover-content > #PackageHoneyComb').attr('style', 'color:gray; cursor: default !important');
    }
    var IdsNotinList = [];
    for (i = 0; i < currentPkgIds.length; i++) {
        var rowObject = $('.honeycombbox-icon-gantt[taskid=' + currentPkgIds[i] + ']');
        var id = $(rowObject).attr('taskid');

        if (rowObject.is(':visible') == true && id == $(rowObject).attr('anchortacticid')) {
            PackageAnchorId = id;
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
            url: '@Url.Content("~/Home/GetPackageTacticDetails")',
            data: {
                viewBy: isRequest ? '@PlanGanttTypes.Request.ToString()' : type,
                TacticIds: IdsNotinList.toString(),
                TacticTaskColor: '@TacticTaskColor',
                IsGridView: $('#IsGridView').val().toLowerCase()
            },
            success: function (data) {
                var ListData = data.Listofdata;
                if ($('#IsGridView').val().toLowerCase() == "true") {
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
                                    if (Data.PCPTId != undefined && Data.PCPTId != null && Data.PCPTId != "") {
                                        ExpandTacticsForSelectedPackage(Data.PCPTId);
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
                                $("div[name1='" + Data.TaskId + "']").addClass("honeycombbox-icon-gantt-Active");
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

function CloseIcon(item) {
    CloseClick = true;
    var IsPackageView = false; //temp change
    var taskId = item.attributes.taskid.value;
    var Totallength = $("#totalEntity").text();
    var Height = $(".popover").css('top').replace("px", "");
    var Blockheight = $('.popover-content').find(".hc-block").css('height').replace("px", "");
    var Margin = $('.popover-content').find(".hc-block").css('margin').split(" ")[0].replace("px", "");
    var Sum = parseInt(Number(Blockheight) + Number(Margin));
    var removeFromHoneyComb = true;
    if ($('#IsGridView').val().toLowerCase() == "true") {
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
            ExportSelectedIds.ROITacticType.splice(index, 1);
            ExportSelectedIds.CalendarEntityType.splice(index, 1);
            ExportSelectedIds.DhtmlxRowId.splice(index, 1);
            ExportSelectedIds.AnchorTacticId.splice(index, 1);
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
            $("div[name1='" + taskId + "']").removeClass("honeycombbox-icon-gantt-Active");
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
            ExportSelectedIds.ROITacticType.splice(index, 1);
            ExportSelectedIds.CalendarEntityType.splice(index, 1);
            ExportSelectedIds.AnchorTacticId.splice(index, 1);
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
            var selectedcell = HomeGrid.findCell(linkItemId, 3, true);
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
    loadInspectPopup(planTacticId, secTactic, "Setup", inspectEdit, 0, '@RequestedModule');
    $("#dvCopyEntity").empty();
    var url = '@Url.Content("~/Plan/LoadCopyEntityPopup")';
    $("#dvCopyEntity").load(url + "?entityId=" + entityId + "&section=" + sectionType + "&PopupType=" + popuptype + "&RedirectType=" + redirectType); // section parameter added to open share tactic popup
    $("#SuccessMsg").css("display", "none");
    $('.btn-dropdwn').hide();
}

//End

//Global Search Related Functions.
//Start
var SearchTextforcal = ""
function GlobalSearch() {
    if (isCopyTactic > 0) {
        $('#txtGlobalSearch').val("");
        $('#ExpClose').css('display', 'none');
        $('#ExpSearch').css('display', 'block');
        isCopyTactic = 0;
    }
    if ($('#divPlanButton').children().hasClass("request-btn-active") || $('#RequestTab').children().hasClass("request-btn-active")) {
        gantt.refreshData();
        function contains(haystack, needle) {
            var a = (haystack || "").toLowerCase(),
                b = (needle || "").toLowerCase();

            return !!(a.indexOf(b) > -1);

        }
        function hasValue(parent, value, searchcriteria) {
            if (value == "") {
                return true;
            }
            if (searchcriteria == ExternalName) {
                if (contains(htmlDecode(gantt.getTask(parent).machineName), value))
                    return true;
            }
            if (searchcriteria == ActivityName) {
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
        else {
            $("#errorMsg").css("display", "none");
            $("#spanMsgError").empty();
        }
    }
    else if ($('#divPlanEditButtonHome').children().hasClass("request-btn-active")) {
        var SearchDDLValue = $('#searchCriteria').val().replace(" ", "");
        var SearchText = $('#txtGlobalSearch').val();
        GlobalSearchonGrid(SearchText, SearchDDLValue);
    }
}

function GlobalSearchonGrid(node, columnName) {
    var colindex = 0;
    var text = node;
    HomeGrid.setFiltrationLevel(-2);
    if (columnName.toLowerCase().toString() == ActivityName) {
        colindex = HomeGrid.getColIndexById('taskname');
    }
    else if (columnName.toLowerCase().toString() == ExternalName) {
        colindex = HomeGrid.getColIndexById('machinename');
    }
    HomeGrid.filterTreeBy(colindex, function (data) {
        return htmlDecode(data).toLowerCase().toString().indexOf(text.toLowerCase()) != -1;
    });
    if (HomeGrid.rowsBuffer.length <= 0) {
        $('#txtGlobalSearch').val("");
        BindHomeGrid();
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
    var Currenttime = $('#ddlUpComingActivites').val();
    var SelectedPlanIds = [];
    var SelectedOwners = [];
    var SelectedCF = [];
    var SelectedTacticType = [];
    var SelectedStatus = [];
    $('#ulSelectedPlans').find("input[type=checkbox]").each(function () {
        if ($(this).attr('checked') == 'checked') {
            var chkid = $(this).attr("id");
            if (chkid != undefined && chkid != 'undefined') {
                SelectedPlanIds.push(chkid);
            }
        }
    });

    if (SelectedPlanIds.length == 0 || SelectedPlanIds == null) {
        SelectedPlanIds = $('#hdnSelectedPlanIds').val()
    }

    $("#ulSelectedOwner li input:checkbox:checked").map(function () {
        SelectedOwners.push($(this).attr("id"));
    });

    $("#ulTacticType li input[type=checkbox]:checked").each(function () {

        var chkid = $(this).attr("id");
        SelectedTacticType.push(chkid.replace("CbTT", ""));

    });

    $('#divCustomFieldsFilter').find("input[type=checkbox]:checked").each(function () {
        var chkid = $(this).attr("id");
        if (chkid != undefined && chkid != 'undefined') {
            SelectedCF.push(chkid);
        }
    });
    var CheckedCounter = 0, AllCounter = 0, id = null, UncheckedCounter = 0;;
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
                    SelectedCF.push(CustomId);

                }
            }
            else if (AllCounter == CheckedCounter) {
                id = this.id;
                if (id != null && id != "" && id.indexOf("-") > -1) {
                    id = this.id.split("-")[1];
                }
                var i = 0, customfieldid;
                for (i = 0; i < SelectedCF.length; i++) {
                    if (SelectedCF[i].indexOf("_") > -1) {
                        customfieldid = SelectedCF[i].split("_")[0];
                        if (id == customfieldid) {
                            SelectedCF.splice(i, 1);
                            i--;
                        }
                    }
                }
            }
        }
    });
    $("#ulStatus li input[type=checkbox]:checked").each(function () {
        var chkid = $(this).attr("id");
        SelectedStatus.push(chkid);
    });

    if ($('#IsGridView').val().toLowerCase() == "true") {
        GetHeadsUpData(urlContent + 'Plan/GetHeaderforPlanByMultiplePlanIDs/', urlContent + 'Home/GetActivityDistributionchart/', secHome, SelectedTimeFrameOption);
    }

}
//End


///Calendar Functions
///Start
function BindPlanCalendar() {    
    var strURL = urlContent + 'Home/LoadPlanCalendar/';
    $.ajax({
        url: strURL,
        type: 'post',
        dataType: 'html',
        async: false,
        //data: {
        //    activeMenu: '@Enums.ActiveMenu.Home',
        //    CurrPlanIds: '@Sessions.PlanId'
        //},
        success: function (data) {
            if (data != 'undefined' && data != null) {
                $('#GridGanttContent').html(data);
            }
            //var staticData = '[{"id":"L20220","text":"Plan_Usability_V","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"color":"","colorcode":"333333","planid":20220,"type":"Plan","TacticType":"--","Status":"Published","OwnerName":"Admin admin","Permission":true},{"id":"L20220_C27294","text":"C1","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220","color":"","colorcode":"4798ba","plancampaignid":27294,"Status":"Created","type":"Campaign","TacticType":"--","OwnerName":"Admin admin","Permission":true},{"id":"L20220_C27294_P33913_T128321_Y31104","text":"T1","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294_P33913","color":"","colorcode":"317232","isSubmitted":false,"isDeclined":false,"projectedStageValue":"0","mqls":"0","cost":100000,"cws":0,"plantacticid":128321,"Status":"Created","type":"Tactic","TacticType":"Email","OwnerName":"Admin admin","ROITacticType":"Promotion","IsAnchorTacticId":0,"PlanTacticId":128321,"CalendarHoneycombpackageIDs":"","Permission":true,"LinkTacticPermission":false,"LinkedTacticId":null,"LinkedPlanName":null},{"id":"L20220_C27294_P33913_T129873_Y31104","text":"T2","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294_P33913","color":"","colorcode":"317232","isSubmitted":false,"isDeclined":false,"projectedStageValue":"0","mqls":"0","cost":100000,"cws":0,"plantacticid":129873,"Status":"Created","type":"Tactic","TacticType":"Email","OwnerName":"Admin admin","ROITacticType":"Promotion","IsAnchorTacticId":0,"PlanTacticId":129873,"CalendarHoneycombpackageIDs":"","Permission":true,"LinkTacticPermission":false,"LinkedTacticId":null,"LinkedPlanName":null},{"id":"L20220_C27294_P33913","text":"P1","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294","color":"","colorcode":"c6ebf3","planprogramid":33913,"Status":"Created","type":"Program","TacticType":"--","OwnerName":"Admin admin","Permission":true}]';
            ConfigureGanttwithdefaultSettings();
            SetGanttData();
        }
    });
}

var AnchorTaskIdsList = {
    Id: [],
    Value: []
};

var AddRemovePackageItems = {
    RemoveId: [],
    AddItemId: [],
}


var ExportSelectedIdsafterSearch = {
    TaskID: [],
}
var arrClosedTask = [];
var GanttTaskData;
var showTacticStatus = false;

var scrollstate = 0;
var bodyscrollpos = 0;
var isCopyTactic = 0;

var type = 'Tactic';    //TODO: Set Fix value. //Need to talk 


function ConfigureGanttwithdefaultSettings() {
    gantt.config.grid_width = 364;
    gantt.config.readonly = true;
    gantt.config.autofit = true;
    gantt.config.drag_links = false;
    gantt.config.drag_resize = true;
    gantt.config.drag_progress = false;
    gantt.config.smart_rendering = true;
}

//// Function to render gantt i.e. for this year or this quarter.
function SetGanttData() {
    var isRender;
    var taskData = JSON.parse('[{"id":"L20220","text":"Plan_Usability_V","ColourName":"ColourName","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":1,"open":false,"color":"","colorcode":"333333","planid":20220,"type":"Plan","TacticType":"--","Status":"Published","OwnerName":"Admin admin","Permission":true},{"id":"L20220_C27294","text":"C1","ColourName":"ColourName","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":2,"open":false,"parent":"L20220","color":"","colorcode":"4798ba","plancampaignid":27294,"Status":"Created","type":"Campaign","TacticType":"--","OwnerName":"Admin admin","Permission":true},{"id":"L20220_C27294_P33913_T128321_Y31104","text":"T1","ColourName":"ColourName","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294_P33913","color":"","colorcode":"317232","isSubmitted":false,"isDeclined":false,"projectedStageValue":"0","mqls":"0","cost":100000,"cws":0,"plantacticid":128321,"Status":"Created","type":"Tactic","TacticType":"Email","OwnerName":"Admin admin","ROITacticType":"Promotion","IsAnchorTacticId":0,"PlanTacticId":128321,"CalendarHoneycombpackageIDs":"","Permission":true,"LinkTacticPermission":false,"LinkedTacticId":null,"LinkedPlanName":null},{"id":"L20220_C27294_P33913_T129873_Y31104","text":"T2","ColourName":"ColourName","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294_P33913","color":"","colorcode":"317232","isSubmitted":false,"isDeclined":false,"projectedStageValue":"0","mqls":"0","cost":100000,"cws":0,"plantacticid":129873,"Status":"Created","type":"Tactic","TacticType":"Email","OwnerName":"Admin admin","ROITacticType":"Promotion","IsAnchorTacticId":0,"PlanTacticId":129873,"CalendarHoneycombpackageIDs":"","Permission":true,"LinkTacticPermission":false,"LinkedTacticId":null,"LinkedPlanName":null},{"id":"L20220_C27294_P33913","text":"P1","ColourName":"ColourName","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294","color":"","colorcode":"c6ebf3","planprogramid":33913,"Status":"Created","type":"Program","TacticType":"--","OwnerName":"Admin admin","Permission":true}]');
    var isQuater = '2016';  //TODO: Set Fix value.
    var planYear = 2016;    //TODO: Set Fix value.


    var tasks = {
        data: taskData
    };

    GanttTaskData = tasks;
    $.each(taskData, function (index, objData) {
        objData.start_date = new Date(objData.start_date);
    });

    //// Instead of iterating each task and delete then it, there is a better way to use .clearAll() method that removes all the task from gantt.
    if ($('.gantt_task_row').length > 0) {
        gantt.clearAll();
    }

    gantt.config.select_task = false;
    if (isQuater == "thisquarter") {
        //// Setting scale.
        gantt.config.scale_unit = "month";
        gantt.config.step = 12;
        gantt.config.date_scale = " %Y";

        gantt.config.scale_height = 60;
        gantt.config.columns = [{ name: "text", label: "Task name", tree: true, width: '*', resize: true }, { name: "ColourName", label: "ColourName", tree: true, resize: true, width: '*', resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 70 }];
        gantt.config.subscales = [{ unit: "month", step: 1, date: "%M" }];

        //// Getting quarter
        var startDate = getQarterStartDate();
        var endDate = new Date(startDate.getFullYear(), startDate.getMonth() + 3, 1);
        gantt.config.start_date = startDate;
        gantt.config.end_date = endDate;
        //// filtering quater data
        var filteredData = $.grep(tasks.data,
                                   function (value) {
                                       return (value.start_date >= startDate && value.start_date <= endDate)
                                   }
                                 );
        var filteredTasks = { data: filteredData };
        gantt.init("gantt_here");
        gantt.config.branch_loading = true;

        var json_length = filteredTasks.data.length;
        var gantt_div = document.getElementById("gantt_here");

        // To resolve extra space
        CalGanntHeight();

        //// Function to add class to each task bar.
        gantt.templates.task_class = function (start, end, task) {
            return getCSSForTask(task);
        };
        gantt.parse(filteredTasks);

        //// display popup based on type and name and title.
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
    else if (isQuater == "thismonth") {
        gantt.config.scale_unit = "month";
        gantt.config.step = 12;
        gantt.config.date_scale = " %Y";
        gantt.config.scale_height = 60;
        gantt.config.columns = [{ name: "text", label: "Task name", tree: true, width: '*', resize: true }, { name: "ColourName", label: "ColourName", tree: true, resize: true, width: '*', resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 70 }];
        gantt.config.subscales = [{ unit: "month", step: 1, date: "%M" }];

        var date = new Date(), m = date.getMonth();
        gantt.config.start_date = new Date(date.getFullYear(), m, 01);
        gantt.config.end_date = new Date(date.getFullYear(), m + 1, 01);
        /*Freezing of Application with Please Wait Dialog Box (This Month Option)*/
        var startDate = new Date(date.getFullYear(), m, 01);
        var endDate = new Date(date.getFullYear(), m + 1, 01);
        var filteredData = $.grep(tasks.data,
                                  function (value) {
                                      return (value.start_date >= startDate && value.start_date <= endDate)
                                  }
                                );
        var filteredTasks = { data: filteredData };

        gantt.init("gantt_here");
        var json_length = tasks.data.length;
        var gantt_div = document.getElementById("gantt_here");

        // To resolve extra space
        CalGanntHeight();

        //// Function to add class to each task bar.
        gantt.templates.task_class = function (start, end, task) {
            return getCSSForTask(task);
        };

        gantt.parse(filteredTasks);//Freezing of Application with Please Wait Dialog Box (This Month Option)


        //// display popup based on type and name and title.
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
    else if ($.isNumeric(isQuater)) {

        var PlanYears = isQuater.split("-");
        var yearDiffrence = 1;
        if (PlanYears.length > 1) {
            yearDiffrence = (parseInt(PlanYears[1]) - parseInt(PlanYears[0])) + 1;
        }
        gantt.config.scale_unit = "year";
        gantt.config.step = 1;
        gantt.config.date_scale = "%Y";
        gantt.config.scale_height = 60;
        gantt.config.columns = [{ name: "text", label: "Task name", tree: true, width: '*', resize: true }, { name: "ColourName", label: "ColourName", tree: true, resize: true, width: '*', resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 70 }];
        gantt.config.subscales = [{ unit: "month", step: 1, date: "%M" }];

        var date = new Date();
        gantt.config.start_date = new Date(planYear, 00, 01);
        gantt.config.end_date = new Date(planYear + yearDiffrence, 00, 01);

        gantt.init("gantt_here");

        var json_length = tasks.data.length;
        var gantt_div = document.getElementById("gantt_here");

        // To resolve extra space
        CalGanntHeight();

        //// Function to add class to each task bar.
        gantt.templates.task_class = function (start, end, task) {
            return getCSSForTask(task);
        };

        gantt.parse(tasks);
    }
    else {

        // Desc :: For 24 months
        var PlanYears = isQuater.split("-");
        var yearDiffrence = 1;
        if (PlanYears.length > 1) {
            yearDiffrence = (parseInt(PlanYears[1]) - parseInt(PlanYears[0])) + 1;
        }
        gantt.config.scale_unit = "year";
        gantt.config.step = 1;
        gantt.config.date_scale = "%Y";
        gantt.config.scale_height = 60;
        gantt.config.columns = [{ name: "text", label: "Task name", tree: true, width: '*', resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 70 }];
        gantt.config.subscales = [{ unit: "month", step: 1, date: "%M" }];

        var date = new Date();
        gantt.config.start_date = new Date(planYear, 00, 01);
        gantt.config.end_date = new Date(planYear + yearDiffrence, 00, 01);
        gantt.init("gantt_here");

        var json_length = tasks.data.length;
        var gantt_div = document.getElementById("gantt_here");

        // To resolve extra space
        CalGanntHeight();

        //// Function to add class to each task bar.
        gantt.templates.task_class = function (start, end, task) {
            return getCSSForTask(task);
        };

        gantt.parse(tasks);
    }

    //// Hide/Show Tabulated data shown below gantt of request tab.
    if ('Request' == type) {

        var gantt_div1 = document.getElementById("gantt_here");

        // To resolve extra space
        CalGanntHeight();

        ////
        gantt.config.min_column_width = function (data) {
            return 10;
        };
    }

    isRender = true;
    gantt.eachTask(function (task) {
        // Planning Window doesnt remember previous view
        if (arrClosedTask.indexOf(task.id) > -1) {
            gantt.getTask(task.id).$open = false;
        }
        else {
            gantt.getTask(task.id).$open = true;
        }
        //CountTacticForRenderChart(task.id, '');
    });

    isRender = false;

    hoverRows();
    AttachEventToTactic();
    AttachEventToonTaskRowClick();
    //GlobalSearch();

    setTimeout(function () {
        var _scrollY = scrollstate.y
        gantt.scrollTo(0, _scrollY);
    }, 250);

}
//To resolve extra space
function CalGanntHeight() {
    var contentWraper = $('#content_wraper').parent().height();
    var positionGannt = $('.gantt-padding').position();
    $('.gantt-padding').height(contentWraper - positionGannt.top);
}

function getCSSForTask(task) {
    var isImprovement = false;
    var cssClass = task.color + ' no_drag ';

    //// To Show status of tactics on Home and Plan screen
    if (showTacticStatus == true) {
        if (task.Status == "Created")
            cssClass += " tactic-created";
        else if (task.Status == "Submitted")
            cssClass += " tactic-submitted ";
        else if (task.Status == "Approved" || task.Status == "Complete" || task.Status == "In-Progress")
            cssClass += " tactic-approved";
        else if (task.Status == "Rejected" || task.Status == "Declined")
            cssClass += " tactic-rejected";
    }
    return cssClass;
}

gantt.attachEvent("onTaskOpened", function (id) {
    onGanttTaskOpen(id);
});

function onGanttTaskOpen(id) {

    hoverRows();
    arrClosedTask = jQuery.grep(arrClosedTask, function (value) {
        return value != id;
    });

    // To display tooltip on camp., prog., tactic name in calendar.
    $(".gantt_container .gantt_tree_content").each(function (index, element) {
        $(element).attr("title", htmlDecode(element.innerHTML));
    });


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
    GlobalSearch();

}

function hoverRows() {
    /*change hover envent on gantt rows to yellow banckgorund*/
    $('.gantt_row').hover(function () {
        $(this).css("background-color", "#fff3a1");
        var task_id = $(this).attr('task_id');
        $('.gantt_task_row').each(function () {
            if ($(this).attr('task_id') == task_id) {
                $(this).css("background-color", "#fff3a1");
            }
        });
    }, function () {
        $(this).css("background-color", "#ffffff");
        var task_id = $(this).attr('task_id');
        $('.gantt_task_row').each(function () {
            if ($(this).attr('task_id') == task_id) {
                $(this).css("background-color", "#ffffff");
            }
        });
    });
    /*change hover envent on gantt rows to yellow banckgorund*/
    $('.gantt_task_row').hover(function () {
        $(this).css("background-color", "#fff3a1");
        var task_id = $(this).attr('task_id');
        $('.gantt_row').each(function () {
            if ($(this).attr('task_id') == task_id) {
                $(this).css("background-color", "#fff3a1");
            }
        });
    }, function () {
        $(this).css("background-color", "#ffffff");
        var task_id = $(this).attr('task_id');
        $('.gantt_row').each(function () {
            if ($(this).attr('task_id') == task_id) {
                $(this).css("background-color", "#ffffff");
            }
        });
    });
}

function AttachEventToTactic() {

    var eventTaskClick; //// Variable to hold double click event.

    //// Detaching double click event.
    if (eventTaskClick != undefined) {
        gantt.detachEvent(eventTaskClick);
    }

    //// Attaching single click event
    eventTaskClick = gantt.attachEvent("onTaskClick", function (taskId, e) {
        var where = e.target.getAttribute('class');
        if (where == 'gantt_task_content' || where == 'gantt_tree_content') {
            scrollstate = gantt.getScrollState();
            bodyscrollpos = $(window).scrollTop();

            ShowModel(taskId, null);
            return true;
        } else {
            return false;
        }
    });

}

function AttachEventToonTaskRowClick() {
    var eventTaskDoubleClickevent;
    //// Detaching double click event.
    if (eventTaskDoubleClickevent != undefined) {
        gantt.detachEvent(eventTaskDoubleClickevent);
    }

    //// Attaching double click event
    eventTaskDoubleClickevent = gantt.attachEvent("onTaskRowClick", function (taskId, trg) {
        scrollstate = gantt.getScrollState();
        bodyscrollpos = $(window).scrollTop();
    });
}

function ShowModel(taskId, isShowInspect) {
    removeDefaultModalPopupBackgroungColor();
    var task, planTacticId, planProgramId, planCampaignId, planId, planLineItemId;
    if (taskId != null) {
        task = gantt.getTask(taskId);
        planCampaignId = task.plancampaignid;
        planProgramId = task.planprogramid;
        planId = task.planid;
        planTacticId = task.plantacticid;
    } else if (isShowInspect) {
        planTacticId = $("#hdnshowInspectForPlanTacticId").val();
        planCampaignId = $("#hdnshowInspectForPlanCampaignId").val();
        planProgramId = $("#hdnshowInspectForPlanProgramId").val();
        planLineItemId = $("#hdnShowInspectForPlanLineItemId").val();
    }
        //// Checking whether current task is tactic or not.
    else if (typeof planTacticId != 'undefined' && planTacticId != 0) {
        modalFullPosition(); // Added by Kapil Antala on 17 Sep 2014 for #732 - new popup design
        $('.modal-backdrop').addClass('modalFull-backdrop');
        $('.modal-backdrop').attr("style", "display:none !important;");
        $("#successMessage").css("display", "none");
        $("#errorMessage").css("display", "none");
        $("#modal-container-186470").modal('show');
        loadInspectPopup(planTacticId, secTactic, "Setup", inspectEdit, 0, inspectRequestIndex);
    }

        //// Checking whether current task is tactic or not.
    else if (typeof planProgramId != 'undefined' && planProgramId != 0) {
        modalFullPosition(); // Added by Kapil Antala on 17 Sep 2014 for #732 - new popup design
        $('.modal-backdrop').addClass('modalFull-backdrop');
        $('.modal-backdrop').attr("style", "display:none !important;");
        $("#successMessage").css("display", "none");
        $("#errorMessage").css("display", "none");
        $("#modal-container-186470").modal('show');
        loadInspectPopup(planProgramId, secProgram, "Setup", inspectEdit, 0, inspectRequestIndex);//Modified by Komal rawal For #1325 - To get the default mode as read only
    }
        //// Checking whether current task is tactic or not.
    else if (typeof planCampaignId != 'undefined' && planCampaignId != 0) {
        modalFullPosition(); // Added by Kapil Antala on 17 Sep 2014 for #732 - new popup design
        $('.modal-backdrop').addClass('modalFull-backdrop');
        $('.modal-backdrop').attr("style", "display:none !important;");
        $("#successMessage").css("display", "none");
        $("#errorMessage").css("display", "none");
        $("#modal-container-186470").modal('show');
        loadInspectPopup(planCampaignId, secCampaign, "Setup", inspectEdit, 0, inspectRequestIndex); //Modified by Komal rawal For #1324 - To get the default mode as read only
    }
        //// Checking whether current task is tactic or not.
    else if (typeof planLineItemId != 'undefined' && planLineItemId != 0) {
        modalFullPosition(); // Added by Kapil Antala on 17 Sep 2014 for #732 - new popup design
        $('.modal-backdrop').addClass('modalFull-backdrop');
        $('.modal-backdrop').attr("style", "display:none !important;");
        $("#successMessage").css("display", "none");
        $("#errorMessage").css("display", "none");
        $("#modal-container-186470").modal('show');
        loadInspectPopup(planLineItemId, secLineItem, "Setup", inspectEdit, 0, inspectRequestIndex);
    }
    else if (typeof planId != 'undefined' && planId != 0) {
        modalFullPosition();
        $('.modal-backdrop').addClass('modalFull-backdrop');
        $('.modal-backdrop').attr("style", "display:none !important;");
        $("#successMessage").css("display", "none");
        $("#errorMessage").css("display", "none");
        $("#modal-container-186470").modal('show');
        loadInspectPopup(planId, secPlan, "Setup", inspectEdit);
    }
}
///End

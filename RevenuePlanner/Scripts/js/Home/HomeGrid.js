var eventiddrag = 0;
var eventidonedit = 0;
var eventidonbeforedrag = 0;
var eventidonscroll = 0;
var editidonOpenEnd = 0;

var progid = 0;
var campid = 0;
var planid = 0;
var tactid = 0;
var progActVal = 0;
var CampActVal = 0;
var PlanActVal = 0;
var TactActVal = 0;
var diff = 0;
var newProgVal = 0;
var newCampVal = 0;
var newPlanVal = 0;
var newTactVal = 0;
var value;
var TacticName;
var ExportToCsv = false;
function SetTooltip() {
    $(".grid_Search").tooltip({
        'container': 'body',
        'placement': 'bottom'
    });
    $(".grid_add").tooltip({
        'container': 'body',
        'placement': 'bottom'
    });
    $(".honeycombbox-icon-gantt").tooltip({
        'container': 'body',
        'placement': 'bottom'
    });
}

var $doc = $(document);
$doc.click(function () {
    $('#popupType').css('display', 'none');
    $('#dhx_combo_select').css('display', 'none');
});

$(document).mouseup(function (e) {
    $('#popupType').css("display", "none");
    $('#dhx_combo_select').css('display', 'none');
});
$(".grid_ver_scroll").scroll(function () {
    $('#popupType').css('display', 'none');
});

///Get GridColumn Index and Hide Column
function GridHideColumn() {  
    TaskNameColIndex = HomeGrid.getColIndexById("taskname");
    PlannedCostColIndex = HomeGrid.getColIndexById(plannedCostColId);
    AssetTypeColIndex = HomeGrid.getColIndexById("roitactictype");
    TypeColIndex = HomeGrid.getColIndexById("tactictype");
    OwnerColIndex = HomeGrid.getColIndexById("owner");
    TargetStageGoalColIndex = HomeGrid.getColIndexById("inq");
    MQLColIndex = HomeGrid.getColIndexById("mql");
    RevenueColIndex = HomeGrid.getColIndexById("revenue");
    GridHiddenId = HomeGrid.getColIndexById('id');
    ActivitypeHidden = HomeGrid.getColIndexById('activitytype');
    MachineNameHidden = HomeGrid.getColIndexById('machinename');
    HomeGrid.setColumnHidden(GridHiddenId, true);
    HomeGrid.setColumnHidden(ActivitypeHidden, true);
    HomeGrid.setColumnHidden(MachineNameHidden, true);
    HomeGrid.setColumnMinWidth("300", TaskNameColIndex);
}
///

////Move column functionality
function MoveColumn() {
    HomeGrid.attachEvent("onAfterCMove", function (cInd, posInd) {
        var ColumnCount = HomeGrid.getColumnCount();
        var ColumnDetail = [];
        for (var i = 0; i < ColumnCount; i++) {
            var ColWidth = HomeGrid.getColWidth(i);
            var customcolId = HomeGrid.getColumnId(i).toString();
            if (customcolId.indexOf("custom_") >= 0) {
                customcolId = customcolId.replace("custom_", "");
            }
            if (ColWidth != 0) {
                ColumnDetail.push({
                    AttributeId: customcolId,
                    AttributeType: " ",
                    ColumnOrder: parseInt(i)
                });
            }
        }
        if (ColumnDetail != null && ColumnDetail.length > 0 && ColumnDetail != undefined) {
            ColumnDetail = JSON.stringify(ColumnDetail);

            $.ajax({
                url: urlContent + 'ColumnView/SaveColumnView', // we are calling json method
                type: 'post',
                dataType: 'json',
                contentType: 'application/json',
                data: "{'AttributeDetail':" + (ColumnDetail) + "}",
                success: function (data) {
                }
            });
        }
    });
}
///End

//Related to Honeycomb
//Start
//// Add RemoveEntity Function to add/remove data into Array on click of honeycomb icon for Plangrid/Budget Grid
function AddRemoveEntity(item) {
    $(".popover").removeClass('in').addClass('out');
    if ($(item).attr('id') == 'PlanAdd') {
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
                ExportSelectedIds.DhtmlxRowId.splice(index, 1);
                ExportSelectedIds.AnchorTacticId.splice(index, 1);
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
            ExportSelectedIds.PlanFlag.push('Grid');
            ExportSelectedIds.CsvId.push($(item).attr('csvid'));
            ExportSelectedIds.ROITacticType.push($(item).attr('roitactictype'));
            ExportSelectedIds.DhtmlxRowId.push($(item).attr('dhtmlxrowid'));
            ExportSelectedIds.AnchorTacticId.push($(item).attr('anchortacticid'));
        }
    }
    if ($(item).attr('id') == 'CampaignAdd') {

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
                ExportSelectedIds.DhtmlxRowId.splice(index, 1);
                ExportSelectedIds.AnchorTacticId.splice(index, 1);
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
            ExportSelectedIds.PlanFlag.push('Grid');
            ExportSelectedIds.CsvId.push($(item).attr('csvid'));
            ExportSelectedIds.ROITacticType.push($(item).attr('roitactictype'));
            ExportSelectedIds.DhtmlxRowId.push($(item).attr('dhtmlxrowid'));
            ExportSelectedIds.AnchorTacticId.push($(item).attr('anchortacticid'));
        }
    }
    if ($(item).attr('id') == 'ProgramAdd') {

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
                ExportSelectedIds.DhtmlxRowId.splice(index, 1);
                ExportSelectedIds.AnchorTacticId.splice(index, 1);
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
            ExportSelectedIds.PlanFlag.push('Grid');
            ExportSelectedIds.CsvId.push($(item).attr('csvid'));
            ExportSelectedIds.ROITacticType.push($(item).attr('roitactictype'));
            ExportSelectedIds.DhtmlxRowId.push($(item).attr('dhtmlxrowid'));
            ExportSelectedIds.AnchorTacticId.push($(item).attr('anchortacticid'));
        }
    }

    if ($(item).attr('id') == 'TacticAdd') {
        if ($(item).hasClass("honeycombbox-icon-gantt-Active")) {
            var IsAssetTactic = $(item).attr('roitactictype');
            var AssetTacId = $(item).attr('anchortacticid');
            var EntityId = $(item).attr('taskid');
            if (IsPackageView && PackageAnchorId == EntityId && IsAssetTactic == AssetTypeAsset && AssetTacId == EntityId) {
                ShowMessage(true, '@RevenuePlanner.Helpers.Common.objCached.DeselectAssetFromPackage', 3000);
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
                ExportSelectedIds.DhtmlxRowId.splice(index, 1);
                ExportSelectedIds.AnchorTacticId.splice(index, 1);
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
            ExportSelectedIds.PlanFlag.push('Grid');
            ExportSelectedIds.CsvId.push($(item).attr('csvid'));
            ExportSelectedIds.ROITacticType.push($(item).attr('roitactictype'));
            ExportSelectedIds.DhtmlxRowId.push($(item).attr('dhtmlxrowid'));
            ExportSelectedIds.AnchorTacticId.push($(item).attr('anchortacticid'));
        }
    }

    if ($('.honeycombbox-icon-gantt-Active').length == 0) {
        $(".honeycombbox").hide();
    }
    else {
        $("#totalEntity").text(ExportSelectedIds.TaskID.length);
        $(".honeycombbox").show();
        $('.dropdown-menu').find('a#ExportCSVHoneyComb').css("display", "block");
        $('.dropdown-menu').find('a#ExportPDFVHoneyComb').css("display", "none");
    }
}
//End

//Related to Bind Grid
//start

function LoadAfterParsing() {
    if (eventidonedit != 0) {
        HomeGrid.detachEvent(eventidonedit);
    }
    eventidonedit = HomeGrid.attachEvent("onEditCell", doOnEditCell);
    if (eventiddrag != 0) {
        HomeGrid.detachEvent(eventiddrag);
    }
    eventiddrag = HomeGrid.attachEvent("onDrag", doOnDrag);
    if (eventidonscroll != 0) {
        HomeGrid.detachEvent(eventidonscroll);
    }
    eventidonscroll = HomeGrid.attachEvent("onScroll", function (sLeft, sTop) {
        $(".dhx_combo_select").css("display", "none");
        $(".dhtmlxcalendar_dhx_skyblue").css("display", "none");
        $("#popupType").css("display", "none");
    });
    if (eventidonbeforedrag != 0) {
        HomeGrid.detachEvent(eventidonbeforedrag);
    }
    eventidonbeforedrag = HomeGrid.attachEvent("onBeforeDrag", function (id) {
        if (id.split(".")[0].toString() != "tact") return false;
        var locked = HomeGrid.cells(id, TaskNameColIndex).getAttribute("locked");
        if ((locked != null && locked != "") && locked == "1")
            return false;
        return true;
    });

    $(".grid_Search").off("click");
    $(".grid_Search").click(function (e) {
        inspectCloseFocus = $(this).position().top;
        var id = $(this).parent().next().html();
        var type = $(this).attr('id');
        gridSearchFlag = 1;
        DisplayEditablePopup(id, type);
    });

    if (isCopyTacticHomeGrid != 0) {
        $('#txtGlobalSearch').val("");
        $('#ExpClose').css('display', 'none');
        $('#ExpSearch').css('display', 'block');
        var selectedcell = HomeGrid.findCell(isCopyTacticHomeGrid, 3, true);
        var id = selectedcell[0];
        var rowid;
        if (id != undefined && id != 'undefined') {
            rowid = id[0];

            var SourcePtype = (rowid).split(".")[0];
            HomeGrid.openItem(rowid);

            ItemIndex = HomeGrid.getRowIndex(rowid);
            state0 = ItemIndex;
            HomeGrid.selectRow(HomeGrid.getRowIndex(rowid), true, true, true);
        }
        isCopyTacticHomeGrid = 0;
        isCopyTactic = 0;
    }
    else if (isEditTacticHomeGrid != 0) {
        var selectedcell = HomeGrid.findCell(isEditTacticHomeGrid, 3, true);
        var id = selectedcell[0];
        var rowid;
        if (id != undefined && id != 'undefined') {
            rowid = id[0];
            HomeGrid.openItem(HomeGrid.getParentId(rowid));
            HomeGrid.selectRow(HomeGrid.getRowIndex(rowid), true, true, true);
            ItemIndex = HomeGrid.getRowIndex(rowid);
            state0 = ItemIndex;
        }
        isEditTacticHomeGrid = 0;
        isEditTactic = 0;
    }

    if (editidonOpenEnd != 0) {
        HomeGrid.detachEvent(editidonOpenEnd);
    }
    editidonOpenEnd = HomeGrid.attachEvent("onOpenEnd", function (rowid) {
        SetTooltip();
        setTimeout(function () {
            HomeGrid.saveOpenStates();
        }, 1000);
        $(".grid_Search").off("click");
        $(".grid_Search").click(function (e) {
            inspectCloseFocus = $(this).position().top;
            var id = $(this).parent().next().html();
            var type = $(this).attr('id');
            gridSearchFlag = 1;
            DisplayEditablePopup(id, type);

        });
        var childItems = HomeGrid.getAllSubItems(rowid);
        if (childItems != undefined && childItems != null && childItems != "") {
            childItems = childItems.split(',').filter(function (tac) {
                return tac.indexOf('tact') > -1;
            });
            $.each(childItems, function (item) {
                var objHoneyComb = $(HomeGrid.getRowById(childItems[item])).find('div[id=TacticAdd]');
                var altIdForTac = objHoneyComb.attr('altid');
                var index = ExportSelectedIds.TaskID.indexOf(altIdForTac);
                if (index < 0) {
                    objHoneyComb.removeClass('honeycombbox-icon-gantt-Active');
                }
                else {
                    objHoneyComb.addClass('honeycombbox-icon-gantt-Active');
                }
            });
        }
    });
}

function sort_Owner(a, b, ord, a_id, b_id) {
    a = HomeGrid.cells(a_id, OwnerColIndex).getText();
    b = HomeGrid.cells(b_id, OwnerColIndex).getText();
    return ord == "asc" ? (a > b ? 1 : -1) : (a > b ? -1 : 1);
};

function sort_TacticType(a, b, ord, a_id, b_id) {
    var atype = a_id.split(".")[0].toString();
    var btype = b_id.split(".")[0].toString();
    if (atype == "tact" && btype == "tact") {
        a = HomeGrid.cells(a_id, TypeColIndex).getText();
        b = HomeGrid.cells(b_id, TypeColIndex).getText();
        return ord == "asc" ? (a > b ? 1 : -1) : (a > b ? -1 : 1);
    } else return 0;
};

function convertNumber(num) {
    var finlval = 0;
    var base = parseFloat(num.replace(CurrencySybmol, ''));
    if (num.toLowerCase().match(/k/)) {
        return finlval = Math.round(base * 1000);
    }
    else if (num.toLowerCase().match(/m/)) {
        return finlval = Math.round(base * 1000000);
    }
    else if (num.toLowerCase().match(/b/)) {
        return finlval = Math.round(base * 1000000000);
    }
    else
        return finlval = numb.replace(CurrencySybmol, '');
}

function ResizeGrid(wid) {
    $("#gridbox").attr("width", wid);
    HomeGrid.setSizes();
    LoadAfterParsing();
}

function doOnDrag(sid, tid) {
    var dragSourcePtype = HomeGrid.getParentId(sid).split(".")[0];
    var dragSourcetype = sid.split(".")[0].toString()
    var dragTargettype = tid.split(".")[0];
    if (dragSourcetype == "tact") {
        if (dragSourcePtype == dragTargettype) {
            var splanid = HomeGrid.cells("plan." + sid.split(".")[1], HomeGrid.getColIndexById('id')).getValue();
            var dplanid = HomeGrid.cells("plan." + tid.split(".")[1], HomeGrid.getColIndexById('id')).getValue();
            var parentid = HomeGrid.getParentId(sid);
            if (dplanid == splanid) {
                if (parentid != tid) {
                    var DestinationMember = new Array();
                    DestinationMember = HomeGrid.getAllSubItems(tid).split(',');

                    var sourseid = HomeGrid.cells(sid, HomeGrid.getColIndexById('id')).getValue();
                    var destinatinId = HomeGrid.cells(tid, HomeGrid.getColIndexById('id')).getValue();
                    var tacticname = HomeGrid.cells(sid, TaskNameColIndex).getValue();
                    var dtactictitle = "";

                    for (a in DestinationMember) {
                        if (DestinationMember[a].toString() != "" && DestinationMember[a].toString() != null) {
                            dtactictitle = HomeGrid.cells(DestinationMember[a].toString(), TaskNameColIndex).getValue();
                            if (dtactictitle == tacticname) {
                                alert("Tactic with same title already exist in Targeted Program.");
                                return false;
                            }
                        }
                    }
                    ProgarmName = HomeGrid.cells(tid, TaskNameColIndex).getValue();
                    $("#lipname").html(ProgarmName);
                    $("#hdnsourceid").val(sourseid);
                    $("#hdndestid").val(destinatinId);
                    $("#divMovetacticPopup").modal('show');
                    RemoveAllHoneyCombData();
                }
                else {
                    ProgarmName = HomeGrid.cells(tid, TaskNameColIndex).getValue();
                    alert("Tactic is already in " + ProgarmName + ".");
                }
            }
            else
                alert("Tactic can move only to same plan program."); return false;
        }
        else {
            var stype = GetItemType(sid.split(".")[0].toString());
            var dtype = GetItemType(dragTargettype.toString());
            alert(stype + " can not move to " + dtype); return false;
        }
    }
    else {
        alert("Only tactic can Move.");
        return false;
    }
}

function GetItemType(val) {
    var itemType = "";
    if (val.toString() == "tact")
        itemType = "Tactic";
    else if (val.toString() == "prog")
        itemType = "Program";
    else if (val.toString() == "camp")
        itemType = "Campaign";
    else if (val.toString() == "plan")
        itemType = "Plan";
    else if (val.toString() == "line")
        itemType = "LineItem";
    return itemType;
}

function SaveMoveTactic() {
    var sourseid = $("#hdnsourceid").val();
    var destinatinId = $("#hdndestid").val();
    $.ajax({
        type: 'POST',
        url: urlContent + 'Plan/SaveGridDetail',
        data: { UpdateType: "tactic", UpdateColumn: "ParentID", UpdateVal: destinatinId, Id: parseInt(sourseid) },
        dataType: 'json',
        success: function (states) {
            LoadPlanGrid();
        }
    });
}

function formatDate(d) {
    d = new Date(d);
    function addZero(n) {
        return n < 10 ? '0' + n : '' + n;
    }
    return addZero(d.getMonth() + 1) + "/" + addZero(d.getDate()) + "/" + d.getFullYear();
}

function SetColumUpdatedValue(CellInd, diff) {
    progActVal = HomeGrid.cells(progid, CellInd).getAttribute("actval");
    CampActVal = HomeGrid.cells(campid, CellInd).getAttribute("actval");
    PlanActVal = HomeGrid.cells(planid, CellInd).getAttribute("actval");
    newProgVal = parseInt(progActVal) + parseInt(diff);
    newCampVal = parseInt(CampActVal) + parseInt(diff);
    newPlanVal = parseInt(PlanActVal) + parseInt(diff);
    HomeGrid.cells(progid, CellInd).setAttribute("actval", newProgVal);
    HomeGrid.cells(campid, CellInd).setAttribute("actval", newCampVal);
    HomeGrid.cells(planid, CellInd).setAttribute("actval", newPlanVal);
    if (tactid != 0) {
        TactActVal = HomeGrid.cells(tactid, CellInd).getAttribute("actval");
        newTactVal = parseInt(TactActVal) + parseInt(diff);
        HomeGrid.cells(tactid, CellInd).setAttribute("actval", newTactVal);
    }
}

///Export Functionality
///Start
function ExportToCsvSp() {
    HomeGrid.setColumnHidden(2, true);
    HomeGrid.expandAll();
    HomeGrid.toExcel("https://dhtmlxgrid.appspot.com/export/excel");
    HomeGrid.collapseAll();
    HomeGrid.setColumnHidden(2, false);
}

function ExportCSVHoneyCombSp() {
    HomeGrid.setColumnHidden(2, true);
    var rowIdArray = [];
    HomeGrid.forEachRow(function (id) {
        var d = HomeGrid.cells(id, 2).getValue();
        if (d.indexOf('honeycombbox-icon-gantt-Active') <= -1) {
            HomeGrid.setRowHidden(id, true);
            rowIdArray.push(id);
        }
    });
    HomeGrid.toExcel("https://dhtmlxgrid.appspot.com/export/excel");
    if (rowIdArray != undefined) {
        $.each(rowIdArray, function (key) {
            HomeGrid.setRowHidden(rowIdArray[key], false);
        });
    }
    HomeGrid.setColumnHidden(2, false);
}

function doOnEditCell(stage, rowId, cellInd, nValue, oValue) {
    var updatetype = rowId.split(".")[0];
    var Id;
    var UpdateColumn;
    var UpdateVal;
    var Colind = this.cell.cellIndex;
    var lineItemFlag = 0;
    $(".popover").removeClass('in').addClass('out'); //Close Honey comb popup on edit cell. to display edited data in honeycomb
    if (stage == 0) {
        var newvalue = HomeGrid.cells(rowId, cellInd).getValue();
        if (newvalue.indexOf("</div>") > -1) {
            if (newvalue.split("</div>").length > 2) {
                value = newvalue.split("</div>")[0] + '</div>' + newvalue.split("</div>")[1];
                TacticName = newvalue.split("</div>")[2];
            }
            else {
                value = newvalue.split("</div>")[0];
                TacticName = newvalue.split("</div>")[1];
            }
        }
    }
    else {
        if (nValue != undefined) {
            TacticName = nValue;
        }
    }
    UpdateColumn = HomeGrid.getColLabel(Colind, 0);
    if (stage == 0) {
        ///TODO : Uncomment After bunding Tactic/Line Item type Drop-down list
        //if (Colind == TypeColIndex) {
        //    if (updatetype == "line") {
        //        var actval = HomeGrid.cells(rowId, cellInd).getAttribute("actval");
        //        if (actval == "") {
        //            return false;
        //        }
        //        var combo = HomeGrid.getCombo(cellInd);
        //        var lineitemtype = lineitemtype;
        //        combo.clear();
        //        $.each(lineitemtype, function (i, item) {
        //            combo.put(item.LineItemTypeId, item.Title);
        //        });
        //    }
        //    else {
        //        var combo = HomeGrid.getCombo(cellInd);
        //        var tacticTypelist = tacticTypelist;
        //        combo.clear();
        //        $.each(tacticTypelist, function (i, item) {
        //            combo.put(item.TacticTypeId, item.Title);
        //        });
        //    }
        //}
        var locked = HomeGrid.cells(rowId, cellInd).getAttribute("locked");
        if ((locked != null && locked != "") && locked == "1")
            return false;
    }
    if (stage == 1) {
        if (updatetype == "line") {
            var oldval = HomeGrid.cells(rowId, cellInd).getValue();
            var actval = HomeGrid.cells(rowId, cellInd).getAttribute("actval");
            if (cellInd != 1) {
                if (oldval == "")
                    $('.dhx_combo_select option[value="' + oldval + '"]').remove();
                else {
                    var v1 = parseInt(oldval);
                    if (isNaN(v1)) {
                        $('.dhx_combo_select option[value="' + oldval + '"]').remove();
                        $('.dhx_combo_select').val(actval);
                    }
                    else
                        $('.dhx_combo_select').val(actval);
                }
            }
        }
        $(".dhx_combo_edit").off("keydown");
        if (UpdateColumn == "Planned Cost" || UpdateColumn == "Target Stage Goal") {

            $(".dhx_combo_edit").on('keydown', (function (e) { GridPriceFormatKeydown(e); }));
            HomeGrid.editor.obj.onkeypress = function (e) {
                e = e || window.event;
                if ((e.keyCode >= 47) || (e.keyCode == 0)) {
                    var text = this.value;
                    if (text.length > 10) {
                        return false;
                    }
                }
            }
        }
        if (UpdateColumn == "Target Stage Goal") {
            var psv = HomeGrid.cells(rowId, TargetStageGoalColIndex).getValue().split(" ");
            this.editor.obj.value = (psv[0].replace(/,/g, ""));
        }
    }
    if (stage == 2) {
        if (nValue != null && nValue != "") {
            var NewValue = htmlDecode(nValue);
            var TaskID = HomeGrid.cells(rowId, 3).getValue();
            var oldAssetType = HomeGrid.cells(rowId, AssetTypeColIndex).getValue();
            if (UpdateColumn == "" || UpdateColumn == null)
                UpdateColumn = HomeGrid.getColLabel(Colind, 0);
            if(UpdateColumn == "Task Name"){
                if (CheckHtmlTag(nValue) == false) {
                    alert(TitleContainHTMLString);
                    return false;
                }

                updatePlanNameDrp(TaskID,NewValue);
            }
            if (cellInd == 1) {
                $("div[taskId='" + TaskID + "']").attr('taskname', NewValue);
            }
            if (ExportSelectedIds.TaskID.length > 0) {
                var TasknameIndex = ExportSelectedIds.Title.indexOf(oValue);
                if (TasknameIndex >= 0) {
                    ExportSelectedIds.Title[TasknameIndex] = NewValue;

                }

            }
            var sdateindex = HomeGrid.getColIndexById('startdate');
            var edateindex = HomeGrid.getColIndexById('enddate');
            var idindex = HomeGrid.getColIndexById('id');
            var costindex = HomeGrid.getColIndexById('plannedcost');
            var stageindex = HomeGrid.getColIndexById('inq');
            Id = HomeGrid.cells(rowId, idindex).getValue();
            if (UpdateColumn == "Start Date") {
                var startyear = new Date(HomeGrid.cells("plan." + rowId.split(".")[1], sdateindex).getValue()).getFullYear();
                var edate = HomeGrid.cells(rowId, edateindex).getValue();
                if (!CheckDateYear(nValue, hdnYear, StartDateCurrentYear)) return false; 
                if (!validateDateCompare(nValue, edate, DateComapreValidation)) return false;

                if (updatetype == "prog") {
                    var tsdate = HomeGrid.getUserData(rowId, "tsdate");
                    if (!validateDateCompare(nValue, tsdate, TacticStartDateCompareWithParentStartDate)) return false;
                }
                if (updatetype == "camp") {
                    var psdate = HomeGrid.getUserData(rowId, "psdate");
                    var tsdate = HomeGrid.getUserData(rowId, "tsdate");
                    if (!validateDateCompare(nValue, psdate, ProgramStartDateCompareWithParentStartDate)) {
                        return false;
                    }
                    if (!validateDateCompare(nValue, tsdate, TacticStartDateCompareWithParentStartDate)) {
                        return false;
                    }
                }
                nValue = formatDate(nValue);
                oValue = formatDate(oValue);
            }
            if (UpdateColumn == "End Date") {
                var endyear = new Date(HomeGrid.cells("plan." + rowId.split(".")[1], edateindex).getValue()).getFullYear();
                var sdate = HomeGrid.cells(rowId, sdateindex).getValue();

                if (!CheckDateYear(nValue, hdnYear, EndDateCurrentYear)) return false;
                if (!validateDateCompare(sdate, nValue, DateComapreValidation)) return false;

                if (updatetype == "prog") {
                    var tedate = HomeGrid.getUserData(rowId, "tedate");
                    if (!validateDateCompare(tedate, nValue, TacticEndDateCompareWithParentEndDate)) return false;
                }
                if (updatetype == "camp") {
                    var pedate = HomeGrid.getUserData(rowId, "pedate");
                    var tedate = HomeGrid.getUserData(rowId, "tedate");
                    if (!validateDateCompare(pedate, nValue, ProgramEndDateCompareWithParentEndDate)) {
                        return false;
                    }
                    if (!validateDateCompare(tedate, nValue, TacticEndDateCompareWithParentEndDate)) {
                        return false;
                    }
                }
                nValue = formatDate(nValue);
                oValue = formatDate(oValue);
            }
            if (UpdateColumn.toString().trim() == "Target Stage Goal") {
                var splitoval = oValue.split(" ");
                if (nValue != splitoval[0].replace(/,/g, "")) {
                    var tactictypeindex = HomeGrid.getColIndexById('tactictype');
                    var tacticTypeId = HomeGrid.getUserData(rowId, "tactictype");
                    GetConversionRate(Id, tacticTypeId, UpdateColumn, nValue, rowId, nValue, null);
                    return true;
                }
                else
                    return false;
            }
            if (UpdateColumn == "Type" && updatetype == "tact") {
                var tacticTypeId = nValue;
                var objHoneyComb = $(HomeGrid.getRowById(rowId)).find('div[id=TacticAdd]');
                //var arrTacTypes = JSON.parse('@Html.Raw(Json.Encode(lstTacticType))'); ///TODO : Uncomment After bunding Tactic/Line Item type Drop-down list
                var newAssetType = arrTacTypes.filter(function (v) {
                    return v.TacticTypeId == tacticTypeId;
                });
                if (newAssetType != null && newAssetType.length > 0) {
                    newAssetType = newAssetType[0].AssetType;
                }
                if (objHoneyComb != undefined && objHoneyComb != null) {
                    var anchorTacId = objHoneyComb.attr('anchortacticid');
                    if (anchorTacId != null && anchorTacId != "0") {
                        if (oldAssetType != null && oldAssetType != "" && oldAssetType != newAssetType && oldAssetType.toLowerCase() == AssetTypeAsset.ToLower()) {
                            var retValue = confirm('Package associated to this tactic will be deleted. Do you wish to continue?');
                            if (!retValue) {
                                return false;
                            }
                        }
                    }
                }
                if (IsMediaCodePermission == 'true' && newAssetType != null && newAssetType != "" && oldAssetType != newAssetType && newAssetType.toLowerCase() == AssetTypeAsset.ToLower()) {
                    var retValue = confirm('Media code associated to this tactic will be deleted. Do you wish to continue?');
                    if (!retValue) {
                        return false;
                    }
                }
                if (nValue != oValue) {
                    $.ajax({
                        type: 'POST',
                        url: urlContent + 'Plan/LoadTacticTypeValue',
                        data: { tacticTypeId: tacticTypeId },
                        success: function (data) {
                            var TaskID = HomeGrid.cells(rowId, 3).getValue();
                            if (ExportSelectedIds.TaskID.length > 0) {
                                var OldValue = $("div[taskId='" + TaskID + "']").attr('TacticType')
                                var TacticTypeIndex = ExportSelectedIds.TacticType.indexOf(OldValue);
                                if (TacticTypeIndex >= 0 && data.TacticTypeName != "") {
                                    ExportSelectedIds.TacticType[TacticTypeIndex] = data.TacticTypeName;

                                }
                            }
                            if (data.TacticTypeName != "" && data.TacticTypeName != null) {
                                $("div[taskId='" + TaskID + "']").attr('tactictype', data.TacticTypeName);
                            }
                            pcost = data.revenue;
                            var stagetitle = data.stageTitle;
                            var projectedStageValue = data.projectedStageValue;
                            if (parseFloat(projectedStageValue) > 0)
                                HomeGrid.cells(rowId, TargetStageGoalColIndex).setValue(FormatCommas(projectedStageValue.toString(), false) + " " + stagetitle);
                            else
                                HomeGrid.cells(rowId, TargetStageGoalColIndex).setValue(projectedStageValue + " " + stagetitle);
                            HomeGrid.setUserData(rowId, "stage", stagetitle);
                            HomeGrid.setUserData(rowId, "tactictype", tacticTypeId);
                            GetConversionRate(Id, tacticTypeId, UpdateColumn, projectedStageValue, rowId, nValue, data.stageId);
                        }
                    });
                    return true;
                }
            }
            if (updatetype == "line") {
                var actval = HomeGrid.cells(rowId, cellInd).getAttribute("actval");
                if (actval == null || actval == "")
                    actval = oValue;
                if (nValue != oValue && nValue != actval) {

                    UpdateVal = nValue;
                    tactid = HomeGrid.getParentId(rowId);
                    progid = HomeGrid.getParentId(tactid);
                    campid = HomeGrid.getParentId(progid);
                    planid = HomeGrid.getParentId(campid);
                    var TotalRowIds = HomeGrid.getAllSubItems(tactid);
                    $.ajax({
                        type: 'POST',
                        url: urlContent + 'Plan/SaveGridDetail',
                        data: { UpdateType: GetItemType(updatetype), UpdateColumn: UpdateColumn.trim(), UpdateVal: UpdateVal, Id: parseInt(Id) },
                        dataType: 'json',
                        success: function (states) {
                            if (states.errormsg != null && states.errormsg.trim() != "") {
                                alert(states.errormsg.trim());
                                HomeGrid.cells(rowId, cellInd).setValue(oValue);
                                return false;
                            }
                            else if (UpdateColumn == "Planned Cost") {
                                diff = parseInt(nValue) - parseInt(oValue);
                                diffLineAndTactic = states.lineItemCost - states.tacticCost
                                if (states.lineItemCost > states.tacticCost) {

                                    SetColumUpdatedValue(PlannedCostColIndex, diffLineAndTactic);
                                    HomeGrid.cells(progid, PlannedCostColIndex).setValue((newProgVal));
                                    HomeGrid.cells(campid, PlannedCostColIndex).setValue((newCampVal));
                                    HomeGrid.cells(planid, PlannedCostColIndex).setValue((newPlanVal));
                                    HomeGrid.cells(tactid, PlannedCostColIndex).setValue((newTactVal));
                                    for (var i = 0; i < TotalRowIds.split(',').length; i++) {
                                        if (HomeGrid.getUserData(TotalRowIds.split(',')[i], "IsOther") == "True") {
                                            HomeGrid.cells(TotalRowIds.split(',')[i], PlannedCostColIndex).setValue((states.otherLineItemCost));
                                            HomeGrid.deleteRow(TotalRowIds.split(',')[i]);
                                        }
                                    }
                                }
                                else if (states.lineItemCost == states.tacticCost) {
                                    for (var i = 0; i < TotalRowIds.split(',').length; i++) {
                                        if (HomeGrid.getUserData(TotalRowIds.split(',')[i], "IsOther") == "True") {
                                            HomeGrid.deleteRow(TotalRowIds.split(',')[i]);
                                        }
                                    }
                                }
                                else {
                                    for (var i = 0; i < TotalRowIds.split(',').length; i++) {
                                        if (HomeGrid.getUserData(TotalRowIds.split(',')[i], "IsOther") == "True") {
                                            HomeGrid.cells(TotalRowIds.split(',')[i], PlannedCostColIndex).setValue((states.otherLineItemCost));
                                        }
                                    }
                                }
                                ChangeTabView('liGrid');
                                ItemIndex = HomeGrid.getRowIndex(tactid);
                                state0 = ItemIndex;
                                HomeGrid.cells(rowId, PlannedCostColIndex).setValue((nValue));
                            }
                            else if (UpdateColumn == "Type")
                                HomeGrid.cells(rowId, cellInd).setAttribute("actval", nValue);
                        }
                    });
                }
                return true;
            }
            if (htmlDecode(nValue) != oValue) {
                if (UpdateColumn != "Type" && UpdateColumn.toString().trim() != "Target Stage Goal") {
                    progid = HomeGrid.getParentId(rowId);
                    campid = HomeGrid.getParentId(progid);
                    planid = HomeGrid.getParentId(campid);
                    var TotalRowIds = HomeGrid.getAllSubItems(rowId);
                    UpdateVal = nValue;
                    $.ajax({
                        type: 'POST',
                        url: urlContent + 'Plan/SaveGridDetail',
                        data: { UpdateType: GetItemType(updatetype), UpdateColumn: UpdateColumn.trim(), UpdateVal: UpdateVal, Id: parseInt(Id) },
                        dataType: 'json',
                        success: function (states) {
                            var TaskID = HomeGrid.cells(rowId, 3).getValue();
                            var OldValue = $("div[taskId='" + TaskID + "']").attr('OwnerName');

                            if (ExportSelectedIds.TaskID.length > 0) {
                                var OwnerNameIndex = ExportSelectedIds.OwnerName.indexOf(OldValue);
                                if (OwnerNameIndex >= 0 && states.OwnerName != "") {
                                    ExportSelectedIds.OwnerName[OwnerNameIndex] = states.OwnerName;

                                }
                            }
                            if (states.OwnerName != "" && states.OwnerName != null) {
                                $("div[taskId='" + TaskID + "']").attr('ownername', states.OwnerName);
                            }
                            if ((OldValue.toString() != states.OwnerName.toString()) && states.OwnerName != "" && states.OwnerName != null) {
                                if (planid != 0 && planid != null && planid != undefined) {
                                    var PlanID = HomeGrid.cells(planid, 3).getValue();
                                }
                                else if (campid != 0 && campid != null && campid != undefined) {
                                    var PlanID = HomeGrid.cells(campid, 3).getValue();
                                }
                                else {
                                    if (progid != 0 && progid != null && progid != undefined) {
                                        var PlanID = HomeGrid.cells(progid, 3).getValue();
                                    }
                                    else {
                                        var PlanID = HomeGrid.cells(rowId, 3).getValue();
                                    }
                                }
                                $("#ulSelectedOwner li input[type=checkbox]").each(function () {
                                    if ($(this).attr('checked') != 'checked') {
                                        filters.tempOwnerIds.push($(this).attr("id"));
                                    }
                                });
                                GetTacticTypelist(PlanID, false);
                                GetOwnerListForFilter(PlanID, false);
                                SaveLastSetofViews();
                            }
                            if (states.errormsg != null && states.errormsg.trim() != "") {
                                alert(states.errormsg.trim());
                                HomeGrid.cells(rowId, cellInd).setValue(oValue);
                                return false;
                            }
                            if (UpdateColumn == "Start Date") {
                                if (states.IsExtended) {
                                    alert("Since the Tactic is link to another Plan, it cannot be extended");
                                    HomeGrid.cells(rowId, cellInd).setValue(oValue);
                                    return false;
                                }
                                var EndDate = HomeGrid.cells(rowId, 5).getValue();
                                var EndYear = EndDate.split('/')[2];
                                var StartYear = nValue.split('/')[2];
                                var YearDiff = EndYear - StartYear;
                                var oldvalueofEndYear = oValue.split('/')[2];
                                var _yrDiff = oldvalueofEndYear - StartYear;

                                if (YearDiff > 0) {
                                    var getvalue = HomeGrid.cells(rowId, 1).getValue();
                                    var Index = getvalue.indexOf("unlink-icon");
                                    if (Index <= -1) {
                                        var UnLinkIconDiv = "<div class='unlink-icon unlink-icon-grid'><i class='fa fa-chain-broken'></i></div>";
                                        HomeGrid.cells(rowId, TaskNameColIndex).setValue(UnLinkIconDiv + getvalue);
                                        $("div[tacticaddId='" + TaskID + "']").attr("linktacticper", "True");
                                    }

                                }
                                else {
                                    if (_yrDiff == 0) {
                                        return false;
                                    }
                                    var getvalue = HomeGrid.cells(rowId, TaskNameColIndex).getValue();
                                    var Index = getvalue.indexOf("</div>");
                                    if (Index > -1) {
                                        HomeGrid.cells(rowId, TaskNameColIndex).setValue(getvalue.split("</div>")[1]);
                                        $("div[tacticaddId='" + TaskID + "']").attr("linktacticper", "False")
                                    }
                                }
                                ComapreDate(updatetype, rowId, sdateindex, nValue, UpdateColumn);
                            }

                            if (UpdateColumn == "End Date") {

                                if (states.IsExtended) {
                                    alert("Since the Tactic is link to another Plan, it cannot be extended");
                                    HomeGrid.cells(rowId, cellInd).setValue(oValue);
                                    return false;
                                }
                                var StartDate = HomeGrid.cells(rowId, 4).getValue();
                                var StartYear = StartDate.split('/')[2];
                                var EndYear = nValue.split('/')[2];
                                var YearDiff = EndYear - StartYear;
                                var oldvalueofEndYear = oValue.split('/')[2];
                                var _yrDiff = oldvalueofEndYear - StartYear;
                                if (YearDiff > 0) {
                                    var getvalue = HomeGrid.cells(rowId, TaskNameColIndex).getValue();
                                    var Index = getvalue.indexOf("unlink-icon");
                                    if (Index <= -1) {
                                        var UnLinkIconDiv = "<div class='unlink-icon unlink-icon-grid'><i class='fa fa-chain-broken'></i></div>";
                                        HomeGrid.cells(rowId, TaskNameColIndex).setValue(UnLinkIconDiv + getvalue);
                                        $("div[tacticaddId='" + TaskID + "']").attr("linktacticper", "True")
                                    }
                                }
                                else {
                                    if (_yrDiff == 0) {
                                        return false;
                                    }
                                    var getvalue = HomeGrid.cells(rowId, TaskNameColIndex).getValue();
                                    var Index = getvalue.indexOf("</div>");
                                    if (Index > -1) {
                                        HomeGrid.cells(rowId, TaskNameColIndex).setValue(getvalue.split("</div>")[1]);
                                        $("div[tacticaddId='" + TaskID + "']").attr("linktacticper", "False")
                                    }
                                }
                                ComapreDate(updatetype, rowId, edateindex, nValue, UpdateColumn);
                            }
                            if (UpdateColumn == "Planned Cost") {
                                if (nValue < states.lineItemCost) {
                                    HomeGrid.cells(rowId, PlannedCostColIndex).setValue((oValue));
                                }
                                else if (nValue == states.lineItemCost) {
                                    if (TotalRowIds != "") {
                                        for (var i = 0; i < TotalRowIds.split(',').length; i++) {

                                            if (HomeGrid.getUserData(TotalRowIds.split(',')[i], "IsOther") == "True") {
                                                HomeGrid.deleteRow(TotalRowIds.split(',')[i]);
                                            }
                                        }
                                    }
                                }
                                else {
                                    diff = parseInt(nValue) - parseInt(oValue);
                                    SetColumUpdatedValue(PlannedCostColIndex, diff);
                                    HomeGrid.cells(progid, PlannedCostColIndex).setValue((newProgVal));
                                    HomeGrid.cells(campid, PlannedCostColIndex).setValue((newCampVal));
                                    HomeGrid.cells(planid, PlannedCostColIndex).setValue((newPlanVal));
                                    HomeGrid.cells(rowId, PlannedCostColIndex).setValue((nValue));
                                    if (TotalRowIds != "") {
                                        for (var i = 0; i < TotalRowIds.split(',').length; i++) {
                                            if (HomeGrid.getUserData(TotalRowIds.split(',')[i], "IsOther") == "True") {
                                                HomeGrid.cells(TotalRowIds.split(',')[i], PlannedCostColIndex).setValue((states.OtherLineItemCost));
                                            }
                                        }
                                    }
                                }
                                ChangeTabView('liGrid');
                                ItemIndex = HomeGrid.getRowIndex(rowId);
                                state0 = ItemIndex;
                            }
                            if (UpdateColumn == "Owner") {
                                CheckPermissionByOwner(rowId, nValue, updatetype, parseInt(Id))

                            }
                            if (UpdateColumn == "Task Name") {
                                $('#txtGlobalSearch').val("");
                                $('#ExpClose').css('display', 'none');
                                $('#ExpSearch').css('display', 'block');
                                GlobalSearch();
                            }

                        }
                    });
                }
                if (cellInd == 1) {
                    if (value != undefined && value != "undefined" && value != null && value.trim() != '') {
                        HomeGrid.cells(rowId, cellInd).setValue(value + "</div>" + TacticName);
                    }
                    else {
                        HomeGrid.cells(rowId, cellInd).setValue(TacticName);
                    }
                }
                value = "";
                $("div[id^='LinkIcon']").each(function () {
                    bootstrapetitle($(this), 'This tactic is linked to ' + "<U>" + htmlDecode($(this).attr('linkedplanname') + "</U>"), "tipsy-innerWhite");
                });
                return true;
            }
            if (cellInd == 1) {
                if (value != undefined && value != "undefined" && value != null) {
                    HomeGrid.cells(rowId, cellInd).setValue(value + "</div>" + TacticName);
                }
                else {
                    HomeGrid.cells(rowId, cellInd).setValue(TacticName);
                }
            }
            value = "";
            $("div[id^='LinkIcon']").each(function () {
                bootstrapetitle($(this), 'This tactic is linked to ' + "<U>" + htmlDecode($(this).attr('linkedplanname') + "</U>"), "tipsy-innerWhite");
            });
            return true;
        }
    }
}

function updatePlanNameDrp(_planid, newpplanname) {
    $('#nl-form2 > div[class="nl-field nl-dd"]').find('li').each(function (e) {
        var planid = $(this).val().toString();
        if (planid == _planid) {
            var updatedPlantext = newpplanname;
            $(this).attr('originalvalue', updatedPlantext);
            $(this).text('');
            $(this).text(updatedPlantext);
            $(this).parent().parent().find('a').text(updatedPlantext)
        }
    });
}

function CheckPermissionByOwner(rowId, NewOwner, updatetype, updateid) {
    $.ajax({
        type: 'POST',
        url: urlContent + 'Plan/CheckPermissionByOwner',
        data: { NewOwnerID: NewOwner, UpdateType: GetItemType(updatetype), updatedid: parseInt(updateid) },
        dataType: 'json',
        success: function (data) {
            if (data.IsLocked == "1") {
                HomeGrid.cells(rowId, TaskNameColIndex).setAttribute("locked", data.IsLocked);
                HomeGrid.cells(rowId, 4).setAttribute("locked", data.IsLocked);
                HomeGrid.cells(rowId, 5).setAttribute("locked", data.IsLocked);
                HomeGrid.cells(rowId, PlannedCostColIndex).setAttribute("locked", data.IsLocked);
                HomeGrid.cells(rowId, AssetTypeColIndex).setAttribute("locked", data.IsLocked);
                HomeGrid.cells(rowId, TypeColIndex).setAttribute("locked", data.IsLocked);
                HomeGrid.cells(rowId, OwnerColIndex).setAttribute("locked", data.IsLocked);
                HomeGrid.cells(rowId, TargetStageGoalColIndex).setAttribute("locked", data.IsLocked);
                HomeGrid.setCellTextStyle(rowId, TaskNameColIndex, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, 2, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, 4, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, 5, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, PlannedCostColIndex, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, AssetTypeColIndex, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, TypeColIndex, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, OwnerColIndex, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, TargetStageGoalColIndex, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, MQLColIndex, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, RevenueColIndex, data.cellTextColor);
            }
        },
        error: function (ts) { }
    });
}

function GetConversionRate(TacticID, TacticTypeID, UpdateColumn, projectedStageValue, rowid, UpdateVal, stageid) {
    var pmql = 0;
    var pcost = 0;
    var isAssortment = true;
    var StageValue = 0;
    var revenue = 0;
    progid = HomeGrid.getParentId(rowid);
    campid = HomeGrid.getParentId(progid);
    planid = HomeGrid.getParentId(campid);
    StageValue = projectedStageValue;
    $.ajax({
        type: 'POST',
        url: urlContent + 'Plan/CalculateMQL',
        data: {
            tactictid: parseInt(TacticID), TacticTypeId: parseInt(TacticTypeID), projectedStageValue: StageValue, RedirectType: isAssortment, isTacticTypeChange: true, StageID: stageid
        },
        success: function (data) {
            var tactActMqlVal = HomeGrid.cells(rowid, MQLColIndex).getAttribute("actval");
            var mqlConversion = 0;
            if (data.revenue != null)
                revenue = data.revenue;
            if (data.mql == 'N/A') {
                HomeGrid.setCellExcellType(rowid, MQLColIndex, "ro");
                HomeGrid.cells(rowid, MQLColIndex).setValue(data.mql);
                diff = parseInt(-tactActMqlVal);
                SetColumUpdatedValue(MQLColIndex, diff);
            }
            else {
                if (data.mql != null) {
                    mqlConversion = data.mql;
                }
                var mqlValue = GetAbberiviatedValue(mqlConversion.toString(), false);
                HomeGrid.cells(rowid, MQLColIndex).setValue(mqlValue);
                diff = parseInt(mqlConversion) - parseInt(tactActMqlVal);
                HomeGrid.cells(rowid, MQLColIndex).setAttribute("actval", mqlConversion);
                SetColumUpdatedValue(MQLColIndex, diff);
            }
            HomeGrid.cells(progid, MQLColIndex).setValue(GetAbberiviatedValue(newProgVal), false);
            HomeGrid.cells(campid, MQLColIndex).setValue(GetAbberiviatedValue(newCampVal), false);
            HomeGrid.cells(planid, MQLColIndex).setValue(GetAbberiviatedValue(newPlanVal), false);
            HomeGrid.cells(rowid, RevenueColIndex).setValue(FormatNumber(revenue));
            var tactActRevenuVal = HomeGrid.cells(rowid, RevenueColIndex).getAttribute("actval");
            diff = parseInt(revenue) - parseInt(tactActRevenuVal);
            SetColumUpdatedValue(RevenueColIndex, diff);
            HomeGrid.cells(progid, RevenueColIndex).setValue((FormatNumber(newProgVal)));
            HomeGrid.cells(campid, RevenueColIndex).setValue((FormatNumber(newCampVal)));
            HomeGrid.cells(planid, RevenueColIndex).setValue((FormatNumber(newPlanVal)));
            HomeGrid.cells(rowid, RevenueColIndex).setAttribute("actval", revenue);
            $.ajax({
                type: 'POST',
                url: urlContent + 'Plan/SaveGridDetail',
                data: { UpdateType: GetItemType("tact"), UpdateColumn: UpdateColumn.trim(), UpdateVal: UpdateVal, Id: parseInt(TacticID) },
                dataType: 'json',

                success: function (states) {
                    if (UpdateColumn == "Target Stage Goal") {
                        var psv = HomeGrid.getUserData(rowid, "stage");
                        HomeGrid.cells(rowid, TargetStageGoalColIndex).setValue(FormatCommas(UpdateVal.toString()) + " " + psv);
                    }

                    if (UpdateColumn == "Type") {
                        var PlanIds = HomeGrid.cells(planid, 3).getValue()
                        $("#ulTacticType li input[type=checkbox]").each(function () {
                            var chkid = $(this).attr("id");
                            if ($(this).attr('checked') != 'checked') {
                                filters.tempTacticTypeIds.push(chkid.replace("CbTT", ""));
                            }

                        });
                        GetTacticTypelist(PlanIds, false);
                        SaveLastSetofViews();
                        var tacCost = 0;
                        if (states.TacticCost != null && states.TacticCost != 'undefined') {
                            tacCost = states.TacticCost;
                        }
                        PlannedCostColIndex = HomeGrid.getColIndexById(plannedCostColId);
                        var oldPlanCost = HomeGrid.cells(rowid, PlannedCostColIndex).getValue();
                        HomeGrid.cells(rowid, PlannedCostColIndex).setValue(tacCost);
                        var TotalchildRowIds = HomeGrid.getAllSubItems(rowid);
                        PlannedCostColIndex = HomeGrid.getColIndexById(plannedCostColId);
                        if (tacCost < states.lineItemCost) {
                            HomeGrid.cells(rowid, PlannedCostColIndex).setValue((oldPlanCost));
                        }
                        else if (tacCost == states.lineItemCost) {
                            if (TotalchildRowIds != "") {
                                for (var i = 0; i < TotalchildRowIds.split(',').length; i++) {

                                    if (HomeGrid.getUserData(TotalchildRowIds.split(',')[i], "IsOther") == "True") {
                                        HomeGrid.deleteRow(TotalchildRowIds.split(',')[i]);
                                    }
                                }
                            }
                            diff = parseInt(tacCost) - parseInt(oldPlanCost);
                            SetColumUpdatedValue(PlannedCostColIndex, diff);
                            HomeGrid.cells(progid, PlannedCostColIndex).setValue((newProgVal));
                            HomeGrid.cells(campid, PlannedCostColIndex).setValue((newCampVal));
                            HomeGrid.cells(planid, PlannedCostColIndex).setValue((newPlanVal));
                            HomeGrid.cells(rowid, PlannedCostColIndex).setValue((tacCost));
                        }
                        else {
                            diff = parseInt(tacCost) - parseInt(oldPlanCost);
                            SetColumUpdatedValue(PlannedCostColIndex, diff);
                            HomeGrid.cells(progid, PlannedCostColIndex).setValue((newProgVal));
                            HomeGrid.cells(campid, PlannedCostColIndex).setValue((newCampVal));
                            HomeGrid.cells(planid, PlannedCostColIndex).setValue((newPlanVal));
                            HomeGrid.cells(rowid, PlannedCostColIndex).setValue((tacCost));
                            if (TotalchildRowIds != "") {
                                for (var i = 0; i < TotalchildRowIds.split(',').length; i++) {
                                    if (HomeGrid.getUserData(TotalchildRowIds.split(',')[i], "IsOther") == "True") {
                                        HomeGrid.cells(TotalchildRowIds.split(',')[i], PlannedCostColIndex).setValue((states.OtherLineItemCost));
                                    }
                                }
                            }
                        }
                        ChangeTabView('liGrid');
                        ItemIndex = HomeGrid.getRowIndex(rowid);
                        state0 = ItemIndex;
                    }
                }
            });
        }
    });
}

function ComapreDate(updatetype, rowId, dateindex, nValue, Updatecolumn) {
    var newDate = new Date(formatDate(nValue));
    if (updatetype == "tact") {
        progid = HomeGrid.getParentId(rowId);
        campid = HomeGrid.getParentId(progid);
        planid = HomeGrid.getParentId(campid);
        var programid = HomeGrid.cells(progid, 3).getValue();
        var campaignid = HomeGrid.cells(campid, 3).getValue();
        var ProgstartDate = new Date(formatDate(HomeGrid.cells(progid, dateindex).getValue()));
        var Campstartdate = new Date(formatDate(HomeGrid.cells(campid, dateindex).getValue()));
        var Planstartdate = new Date(formatDate(HomeGrid.cells(planid, dateindex).getValue()));

        if (Updatecolumn == "Start Date") {
            if (ProgstartDate > newDate)
                HomeGrid.cells(progid, dateindex).setValue(formatDate(nValue));
            if (Campstartdate > newDate) {
                HomeGrid.cells(campid, dateindex).setValue(formatDate(nValue));
            }
            if (Planstartdate > newDate) {
                HomeGrid.cells(planid, dateindex).setValue(formatDate(nValue));
            }
            var tactActMinDate = HomeGrid.getUserData(progid, "tsdate");
            var progMinDate = HomeGrid.getUserData(campid, "psdate");
            $.ajax({
                type: 'POST',
                url: urlContent + 'GetMinMaxDate',
                data: { Parentid: parseInt(campaignid), UpdateType: "Tactic", updatedid: parseInt(programid) },
                dataType: 'json',
                success: function (data) {
                    if (formatDate(data.TactMinDate) != formatDate(tactActMinDate))
                        HomeGrid.setUserData(progid, "tsdate", formatDate(data.TactMinDate));
                    if (formatDate(data.TactMinDate) != formatDate(tactActMinDate))
                        HomeGrid.setUserData(campid, "tsdate", formatDate(data.TactMinDate));
                    if (formatDate(data.ProgMinDate) != formatDate(progMinDate))
                        HomeGrid.setUserData(campid, "psdate", formatDate(data.ProgMinDate));
                },
                error: function (ts) { }
            });

        }
        else if (Updatecolumn == "End Date") {
            if (ProgstartDate < newDate)
                HomeGrid.cells(progid, dateindex).setValue(formatDate(nValue));
            if (Campstartdate < newDate) {
                HomeGrid.cells(campid, dateindex).setValue(formatDate(nValue));
            }
            if (Planstartdate < newDate) {
                HomeGrid.cells(planid, dateindex).setValue(formatDate(nValue));
            }
            var tactActMaxDate = HomeGrid.getUserData(progid, "tedate");
            var progMaxDate = HomeGrid.getUserData(campid, "pedate");
            $.ajax({
                type: 'POST',
                url: urlContent + 'Plan/GetMinMaxDate',
                data: { Parentid: parseInt(campaignid), UpdateType: "Tactic", updatedid: parseInt(programid) },
                dataType: 'json',
                success: function (data) {
                    if (formatDate(data.TactMaxDate) != formatDate(tactActMaxDate))
                        HomeGrid.setUserData(progid, "tedate", formatDate(data.TactMaxDate));
                    if (formatDate(data.TactMaxDate) != formatDate(tactActMaxDate))
                        HomeGrid.setUserData(campid, "tedate", formatDate(data.TactMaxDate));
                    if (formatDate(data.ProgMaxDate) != formatDate(progMaxDate))
                        HomeGrid.setUserData(campid, "psdate", formatDate(data.ProgMaxDate));
                }
            });
        }
    }
    else if (updatetype == "prog") {
        campid = HomeGrid.getParentId(rowId);
        planid = HomeGrid.getParentId(campid);
        var programid = HomeGrid.cells(rowId, 3).getValue();
        var campaignid = HomeGrid.cells(campid, 3).getValue();
        var Campstartdate = new Date(formatDate(HomeGrid.cells(campid, dateindex).getValue()));
        var Planstartdate = new Date(formatDate(HomeGrid.cells(planid, dateindex).getValue()));
        if (Updatecolumn == "Start Date") {
            if (Campstartdate > newDate) {
                HomeGrid.cells(campid, dateindex).setValue(formatDate(nValue));
            }
            if (Planstartdate > newDate) {
                HomeGrid.cells(planid, dateindex).setValue(formatDate(nValue));
            }
            var progMinDate = HomeGrid.getUserData(campid, "psdate");
            var tactMinDate = HomeGrid.getUserData(campid, "tsdate");
            $.ajax({
                type: 'POST',
                url: urlContent + 'Plan/GetMinMaxDate',
                data: { Parentid: parseInt(campaignid), UpdateType: "Program", updatedid: parseInt(programid) },
                dataType: 'json',
                success: function (data) {
                    if (formatDate(data.TactMinDate) != formatDate(tactMinDate))
                        HomeGrid.setUserData(campid, "tsdate", formatDate(data.TactMinDate));
                    if (formatDate(data.ProgMinDate) != formatDate(progMinDate))
                        HomeGrid.setUserData(campid, "psdate", formatDate(data.ProgMinDate));
                },
                error: function (ts) { }
            });
        }
        else if (Updatecolumn == "End Date") {
            if (Campstartdate < newDate) {
                HomeGrid.cells(campid, dateindex).setValue(formatDate(nValue));
            }
            if (Planstartdate < newDate) {
                HomeGrid.cells(planid, dateindex).setValue(formatDate(nValue));
            }
            var progMaxDate = HomeGrid.getUserData(campid, "pedate");
            var tactMaxDate = HomeGrid.getUserData(campid, "tedate");
            $.ajax({
                type: 'POST',
                url: urlContent + 'Plan/GetMinMaxDate',
                data: { Parentid: parseInt(campaignid), UpdateType: "Program", updatedid: parseInt(programid) },
                dataType: 'json',
                success: function (data) {
                    if (formatDate(data.TactMaxDate) != formatDate(tactMaxDate))
                        HomeGrid.setUserData(campid, "tedate", formatDate(data.TactMaxDate));
                    if (formatDate(data.ProgMaxDate) != formatDate(progMaxDate))
                        HomeGrid.setUserData(campid, "pedate", formatDate(data.ProgMaxDate));
                },
                error: function (ts) { }
            });
        }
    }
    else if (updatetype == "camp") {
        planid = HomeGrid.getParentId(rowId);
        var Planstartdate = new Date(formatDate(HomeGrid.cells(planid, dateindex).getValue()));
        if (Updatecolumn == "Start Date") {
            if (Planstartdate > newDate) {
                HomeGrid.cells(planid, dateindex).setValue(formatDate(nValue));
            }
        }
        else if (Updatecolumn == "End Date") {
            if (Planstartdate < newDate) {
                HomeGrid.cells(planid, dateindex).setValue(formatDate(nValue));
            }
        }
    }
}

//insertation start by kausha 21/09/2016 #2638/2592 Export to excel homegrid,budget,homegrid honeycomb
function ExportToExcel(isHoneyComb) {
    //start  
    var rowIdArray = [];
    if (gridname.toLowerCase() == "home") {
      
        if (isHoneyComb) {
            HomeGrid.forEachRow(function (id) {             
                var d = HomeGrid.cells(id, 2).getValue();
                if (d.indexOf('honeycombbox-icon-gantt-Active') <= -1) {
                    HomeGrid.setRowHidden(id, true);
                    rowIdArray.push(id);
                }
            });
        }
        HomeGrid.saveOpenStates();
        HomeGrid.expandAll();
        HomeGrid.setColumnHidden(2, true);
        HomeGrid.toExcel("http://dhtmlxgrid.appspot.com/export/excel");
        HomeGrid.collapseAll();
        HomeGrid.loadOpenStates();
        HomeGrid.setColumnHidden(2, false);
        if (rowIdArray != undefined) {
            $.each(rowIdArray, function (key) {
                HomeGrid.setRowHidden(rowIdArray[key], false);
            });
        }

    }
    else if (gridname.toLowerCase() == "budget") {
        
        var exportGrid = new dhtmlXGridObject('gridExport');
        var JsonExportModel = exportgridData;
        exportGrid.setImagePath(imgPath);
        exportGrid.setImageSize(1, 1);
        exportGrid.setColAlign("right,left,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center,center");
        exportGrid.enableAutoHeight(true);
        exportGrid.enableAutoWidth(false);
        exportGrid.setColumnIds(ColumnIds);      

        if (gridname != undefined) {
            if (gridname.toLowerCase() == 'budget') {
                if (gridheader != undefined)
                    exportGrid.setHeader(gridheader);
                if (colType != undefined)
                    exportGrid.setColTypes(colType);
                if (colSorting != undefined)
                    exportGrid.setColSorting(colSorting);
                if (budgetWidth != undefined)
                    exportGrid.setInitWidths(budgetWidth);
                if (attachHeader != undefined)
                    exportGrid.attachHeader(attachHeader);
               
            }          
        }     
        exportGrid.init();
        setTimeout(function () {
            exportGrid.setSizes();
        }, 200);
     
      
        var mainGridData = JsonExportModel;
        mainGridData = $('<textarea/>').html(mainGridData.toString().replace(/[\\]/g, "\\\\")).text(); // Decode Html content.
        var GridDataHomeGrid = (mainGridData.toString().replace(/&amp;/g, '&'));
        exportGrid.parse(GridDataHomeGrid, "json");
        if (isHoneyComb) {
            HomeGrid.forEachRow(function (id) {                
                var d = HomeGrid.cells(id, 2).getValue();
                if (d.indexOf('honeycombbox-icon-gantt-Active') <= -1) {
                    exportGrid.setRowHidden(id, true);
                    rowIdArray.push(id);
                }
            });
        }
        exportGrid.setColumnHidden(2, true);
        exportGrid.setColumnHidden(0, true);
        exportGrid.expandAll();        
        exportGrid.toExcel("http://dhtmlxgrid.appspot.com/export/excel");
        if (rowIdArray != undefined) {
            $.each(rowIdArray, function (key) {
                exportGrid.setRowHidden(rowIdArray[key], false);
            });
        }
    }
    //end

}

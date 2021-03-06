﻿var eventiddrag = 0;
var eventidonedit = 0;
var eventidonbeforedrag = 0;
var eventidonscroll = 0;
var updatetype = 0;
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
var _customFieldValues = [];
var IsDependentTextBox = false;
var NodatawithfilterGrid = '<div id="NodatawithfilterGrid" style="display:none;">' +
    '<span class="pull-left margin_t30 bold " style="margin-left: 20px;">No data exists. Please check the filters or grouping applied.</span>' + '<br/></div>';
var SetTooltipFirstTime = true;
function SetTooltip() {
    // Use the delegation options.  makes things MUCH FASTER
    if (SetTooltipFirstTime) {
        SetTooltipFirstTime = false;
        $(document.body).tooltip({
            'container': 'body',
            'placement': 'bottom',
            'selector': '.grid_Search,.grid_add,.honeycombbox-icon-gantt'
        });
    }
}

SetTooltip();

var $doc = $(document);
$doc.click(function () {
    $('#popupType').css('display', 'none');
    $('#dhx_combo_select').css('display', 'none');
    $('.dhx_clist').css('display', 'none');
});

$(document).mouseup(function (e) {
    $('#popupType').css("display", "none");
    $('#dhx_combo_select').css('display', 'none');
});
$(".grid_ver_scroll").scroll(function () {
    $('#popupType').css('display', 'none');
    $('.dhx_clist').css('display', 'none');
});

///Get GridColumn Index and Hide Column
function GridHideColumn() {
    StartDateColIndex = HomeGrid.getColIndexById(StartDateId);
    EndDateColIndex = HomeGrid.getColIndexById(EndDateId);
    TaskNameColIndex = HomeGrid.getColIndexById(TaskNameId);
    PlannedCostColIndex = HomeGrid.getColIndexById(PlannedCostId);
    AssetTypeColIndex = HomeGrid.getColIndexById(AssetTypeId);
    TypeColIndex = HomeGrid.getColIndexById(TacticTypeId);
    OwnerColIndex = HomeGrid.getColIndexById(OwnerId);
    TargetStageGoalColIndex = HomeGrid.getColIndexById(TargetStageGoalId);
    MQLColIndex = HomeGrid.getColIndexById(MQLId);
    RevenueColIndex = HomeGrid.getColIndexById(RevenueId);
    GridHiddenId = HomeGrid.getColIndexById('id');
    ActivitypeHidden = HomeGrid.getColIndexById(ActivityTypeId);
    MachineNameHidden = HomeGrid.getColIndexById(MachineNameId);
    HomeGrid.setColumnHidden(GridHiddenId, true);
    HomeGrid.setColumnHidden(ActivitypeHidden, true);
    HomeGrid.setColumnHidden(MachineNameHidden, true);
}

////Move column functionality
function MoveColumn() {
    HomeGrid.attachEvent("onAfterCMove", function (cInd, posInd) {
        var ColumnCount = HomeGrid.getColumnCount();
        var ColumnDetail = [];
        var AttrType = 'Common'; // For as default column or customfield column with which kind of entity custom fields
        for (var i = 0; i < ColumnCount; i++) {
            AttrType = 'Common';
            var ColWidth = HomeGrid.getColWidth(i);
            var customcolId = HomeGrid.getColumnId(i).toString();
            if (ColWidth != 0) {
                ColumnDetail.push({
                    AttributeId: customcolId,
                    AttributeType: AttrType,
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
        $('.dhx_clist').css('display', 'none');
    });
    if (eventidonbeforedrag != 0) {
        HomeGrid.detachEvent(eventidonbeforedrag);
    }
    eventidonbeforedrag = HomeGrid.attachEvent("onBeforeDrag", function (id) {
        if (id != "" && id != undefined) {
            var drag_id = HomeGrid.cells(id, GridHiddenId).getValue();
            var drag_Type = HomeGrid.cells(id, ActivitypeHidden).getValue();
            if (drag_id.length > 0) {
                if (drag_Type.toLowerCase() != secTactic) return false;
                var locked = HomeGrid.cells(id, TaskNameColIndex).getAttribute("lo");
                if ((locked != null && locked != "") && locked == "1")
                    return false;
                return true;
            }
        }
    });

    SetselectedRow();
}

function sort_Owner(a, b, ord, a_id, b_id) {
    a = HomeGrid.cells(a_id, OwnerColIndex).getText();
    b = HomeGrid.cells(b_id, OwnerColIndex).getText();
    return ord == "asc" ? (a > b ? 1 : -1) : (a > b ? -1 : 1);
};

function sort_TacticType(a, b, ord, a_id, b_id) {
    var atype = HomeGrid.cells(a_id, ActivitypeHidden).getValue();
    var btype = HomeGrid.cells(b_id, ActivitypeHidden).getValue();
    if (atype.toLowerCase() == secTactic.toLowerCase() && btype.toLowerCase() == secTactic.toLowerCase()) {
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
    var dragSourcePtype = ''
    var dragSourcetype = ''
    var dragTargettype = ''
    if (sid != "" && tid != "" && sid != undefined && tid != undefined) {
        var sourceId = HomeGrid.cells(sid, GridHiddenId).getValue();
        var targetedId = HomeGrid.cells(tid, GridHiddenId).getValue();
        var sourceProwId = HomeGrid.getParentId(sid);
        var sourcePId = HomeGrid.cells(sourceProwId, GridHiddenId).getValue();
        if (sourcePId.length > 0 && sourceId.length > 0 && targetedId.length > 0) {
            dragSourcePtype = HomeGrid.cells(sourceProwId, ActivitypeHidden).getValue();
            dragSourcetype = HomeGrid.cells(sid, ActivitypeHidden).getValue();
            dragTargettype = HomeGrid.cells(tid, ActivitypeHidden).getValue();
            if (dragSourcetype.toLowerCase() == secTactic) {
                if (dragSourcePtype == dragTargettype) {
                    var splanid = HomeGrid.getParentId(HomeGrid.getParentId(HomeGrid.getParentId(sid)));
                    var dplanid = HomeGrid.getParentId(HomeGrid.getParentId(tid));
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
                    alert(dragSourcetype + " can not move to " + dragTargettype); return false;
                }
            }
            else {
                alert("Only tactic can Move.");
                return false;
            }
        }
    }
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
            HomeGrid.saveOpenStates("plangridState");
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
   
    progActVal = HomeGrid.cells(progid, CellInd).getValue();
    CampActVal = HomeGrid.cells(campid, CellInd).getValue();
    PlanActVal = HomeGrid.cells(planid, CellInd).getValue();
    newProgVal = parseFloat(ReplaceCC(progActVal.toString())) + parseFloat(diff);
    newCampVal = parseFloat(ReplaceCC(CampActVal.toString())) + parseFloat(diff);
    newPlanVal = parseFloat(ReplaceCC(PlanActVal.toString())) + parseFloat(diff);
}


function doOnEditCell(stage, rowId, cellInd, nValue, oValue) {

    updatetype = HomeGrid.cells(rowId, ActivitypeHidden).getValue();
    var Id;
    var UpdateColumn;
    var UpdateVal;
    var Colind = this.cell.cellIndex;
    var lineItemFlag = 0;
    var type = HomeGrid.getColType(cellInd);
    $(".popover").removeClass('in').addClass('out'); //Close Honey comb popup on edit cell. to display edited data in honeycomb
    //Added by Preet Shah on 10/12/2016. If use just enter minus sign, value has been reset to old value. For #2850.
    if (nValue == '-') {
        nValue = oValue;
        HomeGrid.cells(rowId, cellInd).setValue(nValue);
    }
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
    AssignParentIds(rowId);
    UpdateColumn = HomeGrid.getColumnId(Colind, 0);
    var type = HomeGrid.getColType(cellInd);
    var _planid = HomeGrid.cells(planid, GridHiddenId).getValue();
    if (stage == 0) {
        var locked = HomeGrid.cells(rowId, cellInd).getAttribute("lo"); //to checked cell is locked or not
        if ((locked != null && locked != "") && locked == "1")
            return false;
        if (rowId == "newRow_0")
            return false;
        if (Colind == TypeColIndex) {
            if (updatetype.toLowerCase() == secLineItem) {
                var combo = HomeGrid.getCombo(cellInd);
                combo.clear();
                if (lineItemTypefieldOptionList.length > 0) {
                    $.each(lineItemTypefieldOptionList, function (i, item) {
                        if (item.PlanId == _planid) {
                            combo.put(item.id, item.value);
                        }
                    });
                }
            }
            else {
                var combo = HomeGrid.getCombo(cellInd);
                //var tacticTypelist1 = TacticTypelistDataforGrid;
                combo.clear();
                if (tacticTypefieldOptionList.length > 0) {
                    $.each(tacticTypefieldOptionList, function (i, item) {
                        if (item.PlanId == _planid) {
                            combo.put(item.id, item.value);
                        }
                    });
                }
            }
        }

        //added by devanshi #2598
        var customcolId = HomeGrid.getColumnId(cellInd);
        if (customcolId.indexOf("custom_") >= 0) {
            var iddetail = customcolId.replace("custom_", "");
            var id = iddetail.split(':')[0];
            var clistitem = [];

            var entityid = HomeGrid.cells(rowId, GridHiddenId).getValue();
           // if (type == "clist") {

            GetCustomfieldOptionlist(id, entityid, cellInd, type);
            if (IsDependentTextBox)
                return false;
          // }
        }
        opencombobox();
    }
    if (stage == 1) {
        if (type == "clist" && $(".dhx_clist input").length == 1) {
            $(".dhx_clist").css("display", "none");
            return false;
        }
        if (IsDependentTextBox)
            return false;
        if (updatetype.toLowerCase() == secLineItem.toLowerCase() || updatetype.toLowerCase() == secTactic.toLowerCase()) {
            var oldval = HomeGrid.cells(rowId, cellInd).getValue();
            var actval = HomeGrid.cells(rowId, cellInd).getAttribute("actval");
            if (isNaN(parseInt(oldval))) {
                var optionlist;
                if (updatetype.toLowerCase() == secLineItem) {
                    optionlist = lineItemTypefieldOptionList;
                }
                else {
                    optionlist = tacticTypefieldOptionList;
                }
                var oldTacticTypeid = optionlist.filter(function (item) {
                    if (item.PlanId == _planid && item.value.trim().toLowerCase().toString() == oldval.trim().toLowerCase().toString()) {
                        return item.id;
                    }
                });
                if (oldTacticTypeid.length > 0) {
                    actval = oldTacticTypeid[0].id;
                }
            }
            else {
                actval = oldval;
            }
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
        //if (UpdateColumn == PlannedCostId || UpdateColumn == TargetStageGoalId) {
        //    debugger;
        //    $(".dhx_combo_edit").on('keydown', (function (e) { GridPriceFormatKeydown(e); }));
        //    HomeGrid.editor.obj.onkeypress = function (e) {
        //        e = e || window.event;
        //        if ((e.keyCode >= 47) || (e.keyCode == 0)) {
        //            var text = this.value;
        //            if (text.length > 10) {
        //                return false;
        //            }
        //        }
        //    }
        //}
        if (UpdateColumn == TargetStageGoalId) {
            $(".dhx_combo_edit").on('keydown', (function (e) { GridPriceFormatKeydown(e,this); }));
            HomeGrid.editor.obj.onkeypress = function (e) {
                e = e || window.event;
                if ((e.keyCode >= 47) || (e.keyCode == 0)) {
                    var text = this.value;
                    if (text.length > 10) {
                        return false;
                    }
                }
            }
            var psv = HomeGrid.cells(rowId, TargetStageGoalColIndex).getValue().split(" ");
            //this.editor.obj.value = (psv[0].replace(/,/g, ""));
            this.editor.obj.value = (ReplaceCC(psv[0].toString()));
        }

        if (UpdateColumn == PlannedCostId) {
            $(".dhx_combo_edit").on('keydown', (function (e) { GridPriceFormatKeydown(e,this,true); }));
            HomeGrid.editor.obj.onkeypress = function (e) {
                e = e || window.event;
                if ((e.keyCode >= 47) || (e.keyCode == 0)) {
                    var text = this.value;
                    if (text.length > 10) {
                        return false;
                    }
                }
            }
            var actualcost = HomeGrid.cells(rowId, PlannedCostColIndex).getValue().replace(CurrencySybmol,'');
            //this.editor.obj.value = (psv[0].replace(/,/g, ""));
            this.editor.obj.value = (ReplaceCC(actualcost.toString()));
        }
    }
    if (stage == 2) {
        if (nValue.trim() != null && nValue.trim() != "" || UpdateColumn.toString().trim().indexOf("custom_") >= 0) {
            var oldAssetType = '';
            var NewValue = htmlDecode(nValue);
            var TaskID = HomeGrid.cells(rowId, GridHiddenId).getValue();
            if (AssetTypeColIndex != undefined) {
                oldAssetType = HomeGrid.cells(rowId, AssetTypeColIndex).getValue();
            }
            if (UpdateColumn == "" || UpdateColumn == null)
                UpdateColumn = HomeGrid.getColumnId(Colind, 0);
            if (UpdateColumn == TaskNameId) {
                if (CheckHtmlTag(nValue) == false) {
                    alert(TitleContainHTMLString);
                    return false;
                }

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
            Id = HomeGrid.cells(rowId, GridHiddenId).getValue();
            if (UpdateColumn == StartDateId) {
                var startyear = new Date(HomeGrid.cells(planid, StartDateColIndex).getValue()).getFullYear();
                var edate = HomeGrid.cells(rowId, EndDateColIndex).getValue();
                if (!CheckDateYear(nValue, startyear, StartDateCurrentYear)) return false;
                if (!validateDateCompare(nValue, edate, DateComapreValidation)) return false;

                if (updatetype.toLowerCase() == secProgram.toLowerCase()) {
                    var tsdate = HomeGrid.getUserData(rowId, "tsdate");
                    if (!validateDateCompare(nValue, tsdate, TacticStartDateCompareWithParentStartDate)) return false;
                }
                if (updatetype.toLowerCase() == secCampaign.toLowerCase()) {
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
            if (UpdateColumn == PlannedCostId) {
                nValue = nValue.replace(CurrencySybmol, '');
                oValue = oValue.replace(CurrencySybmol, '');
                nValue = parseFloat(nValue);
            }
            if (UpdateColumn == EndDateId) {
                var endyear = new Date(HomeGrid.cells(planid, StartDateColIndex).getValue()).getFullYear();
                var sdate = HomeGrid.cells(rowId, StartDateColIndex).getValue();

                if (!CheckDateYear(nValue, endyear, EndDateCurrentYear)) return false;
                if (!validateDateCompare(sdate, nValue, DateComapreValidation)) return false;

                if (updatetype.toLowerCase() == secProgram.toLowerCase()) {
                    var tedate = HomeGrid.getUserData(rowId, "tedate");
                    if (!validateDateCompare(tedate, nValue, TacticEndDateCompareWithParentEndDate)) return false;
                }
                if (updatetype.toLowerCase() == secCampaign.toLowerCase()) {
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
            if (UpdateColumn.toString().trim() == TargetStageGoalId) {
                var splitoval = oValue.split(" ");
                //if (nValue != splitoval[0].replace(/,/g, ""))
                if (nValue != ReplaceCC(splitoval[0].toString())) {
                    var tactictypeindex = HomeGrid.getColIndexById('tactictype');
                    var tacticTypeId = HomeGrid.getUserData(rowId, "tactictype");
                    GetConversionRate(Id, tacticTypeId, UpdateColumn, nValue, rowId, nValue, null);
                    return true;
                }
                else
                    return false;
            }
            if (UpdateColumn.toString().trim().indexOf("custom_") >= 0) {
                var customcolId = UpdateColumn;
                var iddetail = customcolId.replace("custom_", "");
                var id = iddetail.split(':')[0];
                var weightage = 100;
                if (nValue != null && nValue != undefined && nValue != oValue) {
                    var customvalue = nValue.split(',');
                    _customFieldValues = [];
                    if (customvalue.length > 0) {
                        var TotalSelectedData = customvalue.length;
                        var inputValues = parseInt(100 / TotalSelectedData);
                        var residual = parseInt(100 % TotalSelectedData);
                        var checkedResidualDiff = TotalSelectedData - residual;
                        var res_counter = 0;
                        $.each(customvalue, function (key) {
                            res_counter += 1;
                            if (res_counter <= checkedResidualDiff && checkedResidualDiff != _customFieldValues.length - 1) {
                                weight = inputValues;
                            }
                            else {
                                weight = inputValues + 1;
                            }
                            _customFieldValues.push({
                                customFieldId: id,
                                Value: htmlEncode(customvalue[key]),
                                Weight: weight,
                                CostWeight: weight
                            });
                        });
                    }
                    _customFieldValues = JSON.stringify(_customFieldValues);
                }
                else {
                    return false;
                }
            }
            if (UpdateColumn == TacticTypeId && updatetype.toLowerCase() == secTactic.toLowerCase()) {
                //here oValue is assign id according to its value
                var oldTacticTypeid = tacticTypefieldOptionList.filter(function (item) {
                    if (item.PlanId == _planid && item.value.trim().toLowerCase().toString() == oValue.trim().toLowerCase().toString()) {
                        return item.id;
                    }
                });
                if (oldTacticTypeid.length > 0) {
                    oValue = oldTacticTypeid[0].id;
                }
                var tacticTypeId = nValue;
                var objHoneyComb = $(HomeGrid.getRowById(rowId)).find('div.honeycombbox-icon-gantt');
                var arrTacTypes = tacticTypefieldOptionList;
                var newAssetType = arrTacTypes.filter(function (v) {
                    return v.id == tacticTypeId;
                });
                if (newAssetType != null && newAssetType.length > 0) {
                    newAssetType = newAssetType[0].Type;
                }
                if (objHoneyComb != undefined && objHoneyComb != null) {
                    var anchorTacId = objHoneyComb.attr('anchortacticid');
                    if (anchorTacId != null && anchorTacId != "0") {
                        if (oldAssetType != null && oldAssetType != "" && oldAssetType != newAssetType && oldAssetType.toLowerCase() == AssetTypeAsset.toLowerCase()) {
                            var retValue = confirm('Package associated to this tactic will be deleted. Do you wish to continue?');
                            if (!retValue) {
                                return false;
                            }
                        }
                    }
                }
                if (IsMediaCodePermission == 'true' && newAssetType != null && newAssetType != "" && oldAssetType != newAssetType && newAssetType.toLowerCase() == AssetTypeAsset.toLowerCase()) {
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
                            var TaskID = HomeGrid.cells(rowId, GridHiddenId).getValue();
                            if (ExportSelectedIds.TaskID.length > 0) {
                                var OldValue = $("div[taskId='" + TaskID + "']").attr('TacticType')
                                var TacticTypeIndex = ExportSelectedIds.TacticType.indexOf(OldValue);
                                if (TacticTypeIndex >= 0 && data.TacticTypeName != "") {
                                    ExportSelectedIds.TacticType[TacticTypeIndex] = data.TacticTypeName;

                                }
                            }
                            if (data.TacticTypeName != "" && data.TacticTypeName != null) {
                                $("div[taskId='" + TaskID + "']").attr('tactictype', data.TacticTypeName);
                                $("div[taskId='" + TaskID + "']").attr('roitactictype', newAssetType);

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
            if (updatetype.toLowerCase() == secLineItem.toLowerCase()) {
                //here oValue is assign id according to its value
                if (UpdateColumn == TacticTypeId) {
                    var oldLineItemTypeid = lineItemTypefieldOptionList.filter(function (item) {
                        if (item.PlanId == _planid && item.value.trim().toLowerCase().toString() == oValue.trim().toLowerCase().toString()) {
                            return item.id;
                        }
                    });
                    if (oldLineItemTypeid.length > 0) {
                        oValue = oldLineItemTypeid[0].id;
                    }
                }
                var actval = HomeGrid.cells(rowId, cellInd).getAttribute("actval");
                if (actval == null || actval == "")
                    actval = oValue;
                if (nValue != oValue && nValue != actval) {
                    UpdateVal = nValue;
                    var TotalRowIds = HomeGrid.getAllSubItems(tactid);
                    $.ajax({
                        type: 'POST',
                        url: urlContent + 'Plan/SaveGridDetail',
                        data: { UpdateType: updatetype, UpdateColumn: UpdateColumn.trim(), UpdateVal: UpdateVal, Id: parseInt(Id), CustomFieldInput: _customFieldValues, ColumnType: type.toString(), oValue: oValue.toString() },
                        dataType: 'json',
                        success: function (states) {
                            HomeGrid.saveOpenStates("plangridState");

                            if (states.errormsg != null && states.errormsg.trim() != "") {
                                alert(states.errormsg.trim());
                                HomeGrid.cells(rowId, cellInd).setValue(oValue);
                                return false;
                            }
                            else if (UpdateColumn == PlannedCostId) {
                                diff = parseInt(states.lineItemCost) - parseInt(ReplaceCC(oValue.toString()));
                                for (var i = 0; i < TotalRowIds.split(',').length; i++) {
                                    if (HomeGrid.getUserData(TotalRowIds.split(',')[i], "IsOther") != "False") {
                                        HomeGrid.cells(TotalRowIds.split(',')[i], PlannedCostColIndex).setValue(CurrencySybmol + number_format((states.tacticCost - states.lineItemCost), 2, '.', ','));
                                    }
                                }
                                RefershPlanHeaderCalc();
                                ItemIndex = HomeGrid.getRowIndex(tactid);
                                state0 = ItemIndex;
                                HomeGrid.cells(rowId, PlannedCostColIndex).setValue((CurrencySybmol + number_format(nValue, 2, '.', ',')));
                            }
                            if (UpdateColumn == PlannedCostId || UpdateColumn == TaskNameId) {
                                if (states.linkTacticId > 0) {
                                    LoadPlanGrid();
                                }
                            }
                            if (UpdateColumn.toString().trim().indexOf("custom_") >= 0) {
                                var ids = states.DependentCustomfield;
                                if (ids != null && ids != undefined) {
                                    for (var i = 0; i < ids.length; i++) {
                                        var colIndex = HomeGrid.getColIndexById(ids[i].CustomFieldId);
                                        if (colIndex != undefined && colIndex != '')
                                            HomeGrid.cells(rowId, colIndex).setValue(ids[i].OptionValue);
                                    }
                                }
                            }
                        }
                    });
                }
                else {
                    if (UpdateColumn == PlannedCostId) {
                        HomeGrid.cells(rowId, PlannedCostColIndex).setValue((CurrencySybmol + number_format(nValue, 2, '.', ',')));
                    }
                }
                return true;
            }
            if (htmlDecode(nValue) != oValue) {
                if (UpdateColumn != TacticTypeId && UpdateColumn.toString().trim() != TargetStageGoalId) {
                    progid = HomeGrid.getParentId(rowId);
                    campid = HomeGrid.getParentId(progid);
                    planid = HomeGrid.getParentId(campid);
                    var TotalRowIds = HomeGrid.getAllSubItems(rowId);
                    UpdateVal = nValue;
                    $.ajax({
                        type: 'POST',
                        url: urlContent + 'Plan/SaveGridDetail',
                        data: {
                            UpdateType: updatetype, UpdateColumn: UpdateColumn.trim(), UpdateVal: UpdateVal, Id: parseInt(Id), CustomFieldInput: _customFieldValues, ColumnType: type.toString(), oValue: oValue.toString()
                        },
                        dataType: 'json',
                        success: function (states) {
                            HomeGrid.saveOpenStates("plangridState");
                            var TaskID = HomeGrid.cells(rowId, GridHiddenId).getValue();
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

                            if (states.errormsg != null && states.errormsg.trim() != "") {
                                alert(states.errormsg.trim());
                                HomeGrid.cells(rowId, cellInd).setValue(oValue);
                                return false;
                            }
                            if (UpdateColumn == StartDateId) {
                                if (states.IsExtended) {
                                    alert("Since the Tactic is linked to another Plan, it cannot be extended");
                                    HomeGrid.cells(rowId, cellInd).setValue(oValue);
                                    return false;
                                }
                                var EndDate = HomeGrid.cells(rowId, EndDateColIndex).getValue();
                                var EndYear = new Date(EndDate).getFullYear();
                                var StartYear = new Date(nValue).getFullYear();
                                var YearDiff = EndYear - StartYear;
                                var oldvalueofEndYear = new Date(oValue).getFullYear(); //oValue.split('/')[2];
                                var _yrDiff = oldvalueofEndYear - StartYear;

                                if (YearDiff > 0) {
                                    var getvalue = HomeGrid.cells(rowId, TaskNameColIndex).getValue();
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
                                ComapreDate(updatetype, rowId, StartDateColIndex, nValue, UpdateColumn);
                            }

                            if (UpdateColumn == EndDateId) {
                                var StartDate = HomeGrid.cells(rowId, StartDateColIndex).getValue();
                                var StartYear = new Date(StartDate).getFullYear(); //StartDate.split('/')[2];
                                var EndYear = new Date(nValue).getFullYear();  //nValue.split('/')[2];
                                var YearDiff = EndYear - StartYear;
                                var oldvalueofEndYear = new Date(oValue).getFullYear(); //oValue.split('/')[2];
                                var _yrDiff = oldvalueofEndYear - StartYear;
                                if (YearDiff > 0) {
                                    debugger;
                                    var getvalue = HomeGrid.cells(rowId, TaskNameColIndex).getValue();
                                    var Index = getvalue.indexOf("</div>");
                                    if (Index > -1) {
                                        getvalue = getvalue.split("</div>")[1];
                                    }
                                    var UnLinkIconDiv = "<div class='unlink-icon unlink-icon-grid'><i class='fa fa-chain-broken'></i></div>";
                                    HomeGrid.cells(rowId, TaskNameColIndex).setValue(UnLinkIconDiv + getvalue);
                                    $("div[tacticaddId='" + TaskID + "']").attr("linktacticper", "True")
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
                                ComapreDate(updatetype, rowId, EndDateColIndex, nValue, UpdateColumn);
                            }
                            if (UpdateColumn == PlannedCostId) {
                                var newcostValue = parseFloat(ReplaceCC(nValue.toString()).replace(CurrencySybmol, ''));
                                var oldcostValue = parseFloat(ReplaceCC(oValue.toString()).replace(CurrencySybmol, ''));
                                diff = newcostValue - oldcostValue;
                                SetColumUpdatedValue(PlannedCostColIndex, diff);
                                HomeGrid.cells(progid, PlannedCostColIndex).setValue((CurrencySybmol + number_format(newProgVal, 2, '.', ',')));
                                HomeGrid.cells(campid, PlannedCostColIndex).setValue((CurrencySybmol + number_format(newCampVal, 2, '.', ',')));
                                HomeGrid.cells(planid, PlannedCostColIndex).setValue((CurrencySybmol + number_format(newPlanVal, 2, '.', ',')));
                                HomeGrid.cells(rowId, PlannedCostColIndex).setValue((CurrencySybmol + number_format(newcostValue, 2, '.', ',')));
                                if (TotalRowIds != null && TotalRowIds != "" && TotalRowIds.length > 0) {

                                    for (var i = 0; i < TotalRowIds.split(',').length; i++) {
                                        if (HomeGrid.getUserData(TotalRowIds.split(',')[i], "IsOther") != "False") {
                                            HomeGrid.cells(TotalRowIds.split(',')[i], PlannedCostColIndex).setValue(CurrencySybmol + number_format(newcostValue - states.lineItemCost, 2, '.', ','));
                                        }
                                    }
                                }

                            }
                            if (UpdateColumn == PlannedCostId || UpdateColumn == TaskNameId) {
                                if (states.linkTacticId > 0) {
                                    LoadPlanGrid();
                                }
                                RefershPlanHeaderCalc();
                                ItemIndex = HomeGrid.getRowIndex(rowId);
                                state0 = ItemIndex;
                            }
                            if (UpdateColumn == OwnerId) {
                                if ((OldValue.toString() != states.OwnerName.toString()) && states.OwnerName != "" && states.OwnerName != null) {
                                    if (planid != 0 && planid != null && planid != undefined) {
                                        GetTacticTypelist(filters.PlanIDs);
                                        GetOwnerListForFilter(filters.PlanIDs);
                                        SaveLastSetofViews();
                                    }
                                }
                                CheckPermissionByOwner(rowId, nValue, updatetype, parseInt(Id))

                            }
                            if (UpdateColumn == TaskNameId) {
                                $('#txtGlobalSearch').val("");
                                $('#ExpClose').css('display', 'none');
                                $('#ExpSearch').css('display', 'block');
                                GlobalSearch();
                            }
                            if (UpdateColumn.toString().trim().indexOf("custom_") >= 0) {
                                var ids = states.DependentCustomfield;
                                if (ids != null && ids != undefined) {
                                    for (var i = 0; i < ids.length; i++) {
                                        var colIndex = HomeGrid.getColIndexById(ids[i].CustomFieldId);
                                        if (colIndex != undefined && colIndex != '')
                                            HomeGrid.cells(rowId, colIndex).setValue(ids[i].OptionValue);
                                    }
                                }
                            }
                         
                        }
                    });
                }
                if (cellInd == TaskNameColIndex) {
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
            else
            {
                if (UpdateColumn == PlannedCostId)
                    HomeGrid.cells(rowId, cellInd).setValue((CurrencySybmol + number_format(nValue, 2, '.', ',')));

}
            if (cellInd == TaskNameColIndex) {
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

function AssignParentIds(rowId) {
    if (updatetype.toLowerCase() == secTactic) {
        tactid = rowId;
        progid = HomeGrid.getParentId(rowId);
        campid = HomeGrid.getParentId(progid);
        planid = HomeGrid.getParentId(campid);
    }
    else if (updatetype.toLowerCase() == secLineItem) {
        tactid = HomeGrid.getParentId(rowId);
        progid = HomeGrid.getParentId(tactid);
        campid = HomeGrid.getParentId(progid);
        planid = HomeGrid.getParentId(campid);
    }
    else if (updatetype.toLowerCase() == secProgram) {
        campid = HomeGrid.getParentId(rowId);
        planid = HomeGrid.getParentId(campid);
    }
    else if (updatetype.toLowerCase() == secCampaign) {
        planid = HomeGrid.getParentId(rowId);
    }
    else if (updatetype.toLowerCase() == secPlan) {
        planid = rowId;
    }
}
function CheckPermissionByOwner(rowId, NewOwner, updatetype, updateid) {
    $.ajax({
        type: 'POST',
        url: urlContent + 'Plan/CheckPermissionByOwner',
        data: { NewOwnerID: NewOwner, UpdateType: updatetype, updatedid: parseInt(updateid) },
        dataType: 'json',
        success: function (data) {
            if (data.IsLocked == "1") {
                HomeGrid.cells(rowId, TaskNameColIndex).setAttribute("lo", data.IsLocked);
                HomeGrid.cells(rowId, StartDateColIndex).setAttribute("lo", data.IsLocked);
                HomeGrid.cells(rowId, EndDateColIndex).setAttribute("lo", data.IsLocked);
                HomeGrid.cells(rowId, PlannedCostColIndex).setAttribute("lo", data.IsLocked);
                HomeGrid.cells(rowId, AssetTypeColIndex).setAttribute("lo", data.IsLocked);
                HomeGrid.cells(rowId, TypeColIndex).setAttribute("lo", data.IsLocked);
                HomeGrid.cells(rowId, OwnerColIndex).setAttribute("lo", data.IsLocked);
                HomeGrid.cells(rowId, TargetStageGoalColIndex).setAttribute("lo", data.IsLocked);
                HomeGrid.setCellTextStyle(rowId, TaskNameColIndex, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, StartDateColIndex, data.cellTextColor);
                HomeGrid.setCellTextStyle(rowId, EndDateColIndex, data.cellTextColor);
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
            if (MQLColIndex != undefined && MQLColIndex != null) {                
                var tactActMqlVal = HomeGrid.cells(rowid, MQLColIndex).getValue();
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
                    var mqlValue = mqlConversion.toString();
                    HomeGrid.cells(rowid, MQLColIndex).setValue(numberWithCommas(mqlValue));
                    diff = parseInt(mqlConversion) - parseInt(ReplaceCC(tactActMqlVal.toString()));
                    
                    SetColumUpdatedValue(MQLColIndex, diff);
                }
                HomeGrid.cells(progid, MQLColIndex).setValue(numberWithCommas(newProgVal), false);
                HomeGrid.cells(campid, MQLColIndex).setValue(numberWithCommas(newCampVal), false);
                HomeGrid.cells(planid, MQLColIndex).setValue(numberWithCommas(newPlanVal), false);
                
                var tactActRevenuVal = HomeGrid.cells(rowid, RevenueColIndex).getValue();
                HomeGrid.cells(rowid, RevenueColIndex).setValue(CurrencySybmol + numberWithCommas(parseInt(revenue).toString()));
                diff = parseInt(revenue) - parseInt(ReplaceCC(tactActRevenuVal.toString()).replace(CurrencySybmol, ''));
                SetColumUpdatedValue(RevenueColIndex, diff);
                HomeGrid.cells(progid, RevenueColIndex).setValue(CurrencySybmol + numberWithCommas(newProgVal));
                HomeGrid.cells(campid, RevenueColIndex).setValue(CurrencySybmol + numberWithCommas(newCampVal));
                HomeGrid.cells(planid, RevenueColIndex).setValue(CurrencySybmol + numberWithCommas(newPlanVal));               
            }
            $.ajax({
                type: 'POST',
                url: urlContent + 'Plan/SaveGridDetail',
                data: { UpdateType: 'Tactic', UpdateColumn: UpdateColumn.trim(), UpdateVal: UpdateVal, Id: parseInt(TacticID) },
                dataType: 'json',

                success: function (states) {
                    HomeGrid.saveOpenStates("plangridState");
                    if (UpdateColumn == TargetStageGoalId) {
                        var psv = HomeGrid.getUserData(rowid, "stage");
                        HomeGrid.cells(rowid, TargetStageGoalColIndex).setValue(FormatCommas(UpdateVal.toString()) + " " + psv);
                        RefershPlanHeaderCalc();
                    }

                    if (UpdateColumn == TacticTypeId) {
                        var PlanIds = HomeGrid.cells(planid, GridHiddenId).getValue()
                        $("#ulTacticType li input[type=checkbox]").each(function () {
                            var chkid = $(this).attr("id");
                            if ($(this).attr('checked') != 'checked') {
                                filters.tempTacticTypeIds.push(chkid.replace("CbTT", ""));
                            }

                        });
                        GetTacticTypelist(filters.PlanIDs, false);
                        SaveLastSetofViews();
                        var TotalchildRowIds = HomeGrid.getAllSubItems(rowid);
                        var tacCost = 0;
                        if (states.TacticCost != null && states.TacticCost != 'undefined') {
                            tacCost = states.TacticCost;
                        }
                        PlannedCostColIndex = HomeGrid.getColIndexById(PlannedCostId);
                        var oldTacticCost = HomeGrid.cells(rowid, PlannedCostColIndex).getValue();
                        diff = parseInt(tacCost) - parseInt(ReplaceCC(oldTacticCost.toString()));
                        SetColumUpdatedValue(PlannedCostColIndex, diff);
                        HomeGrid.cells(progid, PlannedCostColIndex).setValue((CurrencySybmol + numberWithCommas(newProgVal)));
                        HomeGrid.cells(campid, PlannedCostColIndex).setValue((CurrencySybmol + numberWithCommas(newCampVal)));
                        HomeGrid.cells(planid, PlannedCostColIndex).setValue((CurrencySybmol + numberWithCommas(newPlanVal)));
                        HomeGrid.cells(rowid, PlannedCostColIndex).setValue((CurrencySybmol + numberWithCommas(tacCost)));
                        if (TotalchildRowIds != null && TotalchildRowIds!="" && TotalchildRowIds.length > 0) {
                            for (var i = 0; i < TotalchildRowIds.split(',').length; i++) {
                                if (HomeGrid.getUserData(TotalchildRowIds.split(',')[i], "IsOther") != "False") {
                                    HomeGrid.cells(TotalchildRowIds.split(',')[i], PlannedCostColIndex).setValue(CurrencySybmol + (tacCost - states.lineItemCost));
                                }
                            }
                        }
                        if (states.linkTacticId > 0) {
                            LoadPlanGrid();
                        }
                        RefershPlanHeaderCalc();
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
    if (updatetype.toLowerCase() == secTactic) {
        progid = HomeGrid.getParentId(rowId);
        campid = HomeGrid.getParentId(progid);
        planid = HomeGrid.getParentId(campid);
        var programid = HomeGrid.cells(progid, GridHiddenId).getValue();
        var campaignid = HomeGrid.cells(campid, GridHiddenId).getValue();
        var ProgstartDate = new Date(formatDate(HomeGrid.cells(progid, dateindex).getValue()));
        var Campstartdate = new Date(formatDate(HomeGrid.cells(campid, dateindex).getValue()));
        var Planstartdate = new Date(formatDate(HomeGrid.cells(planid, dateindex).getValue()));

        if (Updatecolumn == StartDateId) {
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
        else if (Updatecolumn == EndDateId) {
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
    else if (updatetype.ToLower() == secProgram) {
        campid = HomeGrid.getParentId(rowId);
        planid = HomeGrid.getParentId(campid);
        var programid = HomeGrid.cells(rowId, 3).getValue();
        var campaignid = HomeGrid.cells(campid, 3).getValue();
        var Campstartdate = new Date(formatDate(HomeGrid.cells(campid, dateindex).getValue()));
        var Planstartdate = new Date(formatDate(HomeGrid.cells(planid, dateindex).getValue()));
        if (Updatecolumn == StartDateId) {
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
        else if (Updatecolumn == EndDateId) {
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
    else if (updatetype.toLowerCase() == secCampaign) {
        planid = HomeGrid.getParentId(rowId);
        var Planstartdate = new Date(formatDate(HomeGrid.cells(planid, dateindex).getValue()));
        if (Updatecolumn == StartDateId) {
            if (Planstartdate > newDate) {
                HomeGrid.cells(planid, dateindex).setValue(formatDate(nValue));
            }
        }
        else if (Updatecolumn == EndDateId) {
            if (Planstartdate < newDate) {
                HomeGrid.cells(planid, dateindex).setValue(formatDate(nValue));
            }
        }
    }
}

function ExportToCsvSp() {
    if ($('#txtGlobalSearch').val().trim() != undefined && $('#txtGlobalSearch').val().trim() != "") {
        var ExportdataId = new Array();
        $(HomeGrid.rowsBuffer).find('td div.honeycombbox-icon-gantt').each(function () {
            ExportdataId.push($(this).attr('csvid'));
        });

        var Honeycombids = ExportdataId.toString();
        $.ajax({
            type: 'POST',
            url: urlContent + 'Plan/ExportToCsv/',
            data: { HoneycombIds: Honeycombids, PlanId: filters.PlanIDs.toString() },
            success: function (data) {
                window.location = urlContent + 'Plan/ExportCsvDataTable/';
            }
        });
    }
    else {
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
        var OwnerIds = filters.OwnerIds.toString();
        var TacticTypeids = filters.TacticTypeids.toString();
        var StatusIds = filters.StatusIds.toString();
        var CustomFieldId = filters.customFieldIds.toString();
        $.ajax({
            type: 'POST',
            url: urlContent + 'Plan/ExportToCsv/',
            data: { ownerIds: OwnerIds, TacticTypeid: TacticTypeids, StatusIds: StatusIds, customFieldIds: CustomFieldId, PlanId: filters.PlanIDs.toString() },
            success: function (data) {
                window.location = urlContent + 'Plan/ExportCsvDataTable/';
            }

        });
    }
}

var IsExportToCsv = false;
function ExportCSVHoneyCombSp() {
    var Honeycombids = ExportSelectedIds.CsvId.toString();
    $.ajax({
        type: 'POST',
        url: urlContent + 'Plan/ExportToCsv/',
        data: { HoneycombIds: Honeycombids, PlanId: filters.PlanIDs.toString() },
        success: function (data) {
            window.location = urlContent + 'Plan/ExportCsvDataTable/';
        }

    });
}

function ExportToExcel(isHoneyComb) {
    if (gridname.toLowerCase() == "home") {
        if (isHoneyComb) {
            var TaskId = [];
            for (var i = 0; i < ExportSelectedIds.TaskID.length; i++) {
                TaskId.push(ExportSelectedIds.TaskID[i].split(/_(.+)?/)[0])
            }
            var arrTaskId = TaskId.filter(function (itm, i, TaskId) {
                return i == TaskId.indexOf(itm);
            });
            if (arrTaskId.length > 1) {
                alert('You can export only one plan at a time. Please select the correct plan and try to export again');
            }
            else {
                ExportCSVHoneyCombSp();
            }            
        }
        else {
            if (filters.PlanIDs.length > 1) {
                alert('You can export only one plan at a time. Please select the correct plan and try to export again');
            }
            else {
                ExportToCsvSp();
            }            
        }
    }
    else {
        var rowIdArray = [];
        var HoneyCombSelectedArray = [];

        var machineNameIndex = HomeGrid.getColIndexById("MachineName");
        var TypeIndex = HomeGrid.getColIndexById("Type");
        var ActivityIdindex = HomeGrid.getColIndexById("ActivityId");
        if (gridname.toLowerCase() == "budget") {
            iconColumnIndex = HomeGrid.getColIndexById("Buttons");
            colourCodeIndex = HomeGrid.getColIndexById("colourcode");
        }
        if (isHoneyComb) {
            HomeGrid.forEachRow(function (id) {

                var d = HomeGrid.cells(id, iconColumnIndex).getValue();
                if (d.indexOf('honeycombbox-icon-gantt-Active') <= -1) {
                    HomeGrid.setRowHidden(id, true);
                    rowIdArray.push(id);
                }
                else {
                    HoneyCombSelectedArray.push(id);
                }
            });
        }
        HomeGrid.saveOpenStates("plangridState");
        HomeGrid.expandAll();

        var ActivityIdIndex = HomeGrid.getColIndexById("ActivityId");

        HomeGrid.setColumnHidden(ActivityIdindex, false);
        HomeGrid.setColumnHidden(TypeIndex, false);
        var columnIdArray = ColumnIds.split(',');
        var serializeStr = '';
        for (i = 0; i < columnIdArray.length; ++i) {
            if (i == 0) {
                if (columnIdArray[i] == 'MachineName' || columnIdArray[i] == 'LineItemTypeId' || columnIdArray[i] == 'Buttons' || columnIdArray[i] == 'colourcode')
                    serializeStr = serializeStr + "false"
                else
                    serializeStr = serializeStr + "true"
            }
            else {
                if (columnIdArray[i] == 'MachineName' || columnIdArray[i] == 'LineItemTypeId' || columnIdArray[i] == 'Buttons' || columnIdArray[i] == 'colourcode')
                    serializeStr = serializeStr + ",false"
                else
                    serializeStr = serializeStr + ",true"
            }


        }
        HomeGrid.setSerializableColumns(serializeStr);
        HomeGrid.toExcel("https://dhtmlxgrid.appspot.com/export/excel");
        HomeGrid.collapseAll();
        HomeGrid.loadOpenStates("plangridState");
        HomeGrid.setColumnHidden(ActivityIdindex, true);
        HomeGrid.setColumnHidden(TypeIndex, true);
        if (rowIdArray != undefined) {
            $.each(rowIdArray, function (key) {
                HomeGrid.setRowHidden(rowIdArray[key], false);
            });
        }
        if (HoneyCombSelectedArray != undefined) {
            $.each(HoneyCombSelectedArray, function (key) {
                var columnText = HomeGrid.cells(HoneyCombSelectedArray[key], iconColumnIndex).getValue();
                if (columnText.indexOf('honeycombbox-icon-gantt-Active') <= -1) {
                    HomeGrid.cells(HoneyCombSelectedArray[key], iconColumnIndex).setValue(columnText.replace("honeycombbox-icon-gantt", "honeycombbox-icon-gantt honeycombbox-icon-gantt-Active"));
                }
            });
        }
    }
}
//function to get dependent custom field options for tactic
function GetCustomfieldOptionlist(customFieldId, entityid, cellInd, type) {
   
    var customoption = customfieldOptionList;
    var optionlist;
    var clistitem = [];
    function filterbyparent(obj) {
        if (obj.customFieldId == customFieldId && obj.ParentOptionId != null && obj.ParentOptionId.length > 0)
            return true;
        else
            return false;
    }
    d = customoption.filter(filterbyparent);
    IsDependentTextBox = false;
    if (d != null && d.length > 0) {
        var parentoptid = [];
        $.each(d, function (i, item) {
            if (parentoptid.indexOf(item.ParentOptionId[0]) < 0)
                parentoptid.push(item.ParentOptionId[0]);
        });
        //ajax call
        $.ajax({
            url: urlContent + 'Plan/GetdependantOptionlist/',
            traditional: true,
            async: false,
            data: {
                customfieldId: customFieldId,
                entityid: entityid,
                parentoptionId: (parentoptid),
                Customfieldtype: type
            },
            success: function (data) {
                if (type == "ed" && data.IstextBoxDependent.toString().toLowerCase() == "true") {
                    IsDependentTextBox = true;
                    return false;
                }
                if (data != null && data.optionlist != null && data.optionlist.length > 0)
                    optionlist = data.optionlist;
                if (optionlist != null && optionlist.length > 0 && optionlist != undefined) {
                    $.each(optionlist, function (i, item) {
                        if (clistitem.indexOf(item.value) == -1)
                            clistitem.push(item.value);
                    });
                    HomeGrid.registerCList(cellInd, clistitem);
                }
                else {
                    HomeGrid.registerCList(cellInd, clistitem);
                }
            }
        });
    }
    else {
        function filterbyId(obj) {
            if (obj.customFieldId == customFieldId)
                return true;
            else
                return false;
        }
        optionlist = customoption.filter(filterbyId);
        $.each(optionlist, function (i, item) {
            clistitem.push(item.value);
        });
        HomeGrid.registerCList(cellInd, clistitem);
    }

}

function opencombobox() {
    var topOffset = $(".rowselected td").offset().top;
    var windowHeight = $(".objbox").height();
    var relativeOffset = topOffset - windowHeight;
    if (relativeOffset > 200) {
        $("body").addClass("reverse");
    }
    else {
        $("body").removeClass("reverse");
    }
}
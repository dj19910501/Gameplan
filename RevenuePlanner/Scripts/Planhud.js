
function GetHeadsUpData(HeaderUrl, ChartUrl, activemenu, timeframe) {
    var _filters = {
        selectedPlanIds: [],
        OwnerIds: [],
        TacticTypeids: [],
        StatusIds: [],
        customFieldIds: []

    };

    $('#ulSelectedPlans').find("input[type=checkbox]:checked").each(function () {
        var chkid = $(this).attr("id");
        if (chkid != undefined && chkid != 'undefined') {
            _filters.selectedPlanIds.push(chkid);
        }
    });

    $("#ulSelectedOwner li input:checkbox:checked").map(function () {
        _filters.OwnerIds.push($(this).attr("id"));
    });

    $("#ulTacticType li input:checkbox:checked").map(function () {
        var Value = $(this).attr("id").replace("CbTT", "");
        _filters.TacticTypeids.push(Value);
    });

    $("#ulStatus li input:checkbox:checked").map(function () {
        var Value = $(this).attr("id");
        _filters.StatusIds.push(Value);
    });

    $('#divCustomFieldsFilter').find("input[type=checkbox]").each(function () {
        if ($(this).attr('checked') == 'checked') {
            var chkid = $(this).attr("id");
            if (chkid != undefined && chkid != 'undefined') {
                _filters.customFieldIds.push(chkid);
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
                    _filters.customFieldIds.push(CustomId);

                }
            }
            else if (AllCounter == CheckedCounter) {
                id = this.id;
                if (id != null && id != "" && id.indexOf("-") > -1) {
                    id = this.id.split("-")[1];
                }
                var i = 0, customfieldid;
                for (i = 0; i < _filters.customFieldIds.length; i++) {
                    if (_filters.customFieldIds[i].indexOf("_") > -1) {
                        customfieldid = _filters.customFieldIds[i].split("_")[0];
                        if (id == customfieldid) {
                            _filters.customFieldIds.splice(i, 1);
                            i--;
                        }
                    }
                }
            }
        }
    });

   GetHeaderData(HeaderUrl, activemenu, timeframe, _filters.selectedPlanIds, _filters.customFieldIds, _filters.OwnerIds, _filters.TacticTypeids, _filters.StatusIds);
   GetNumberOfActivityPerMonByPlanId(ChartUrl, activemenu, timeframe, _filters.selectedPlanIds, _filters.customFieldIds, _filters.OwnerIds, _filters.TacticTypeids, _filters.StatusIds);
}


function GetHeaderData(url, activemenu, timeframe, selectedPlanIds, Customid, OwnerId, Tacticids, StatusId) {
    $.ajax(
    {
        type: "POST",
        url: url,
        data: {
            planid: selectedPlanIds.toString(),
            activeMenu: activemenu,
            year: timeframe,
            CustomFieldId: Customid.toString(),
            OwnerIds: OwnerId.toString(),
            TacticTypeids: Tacticids.toString(),
            StatusIds: StatusId.toString(),
            IsGridView:$('#IsGridView').val().toString(),
        },
        dataType: "json",
        success: function (data) {
            if (data != null) {
                $.each(data.lstHomePlanModelHeader, function (index, obj) {
                    if (index == "MQLs") {
                        $("#pMQLs").html(obj);
                        SetPriceValue("#pMQLs");
                    }
                    else if (index == "Budget") {
                        $("#pbudget").html(obj);
                        SetBudget("#pbudget");
                    }
                    else if (index == "TacticCount") {
                        $("#ptacticcount").html(obj);
                    }
                    else if (index == "mqlLabel") {
                        $("#pmqlLabel").html(obj);
                    }
                    else if (index == "costLabel") {
                        $("#pcostLabel").html(obj);
                    }
                    else if (index == "PercentageMQLImproved") {
                        var pMQLImproved = $("#pMQLImproved");
                        pMQLImproved.removeClass("greenfont");
                        pMQLImproved.removeClass("redfont");

                        if (obj != null) {
                            if (obj < 0) {
                                pMQLImproved.html(FormatNumber(obj, true))
                                pMQLImproved.addClass("redfont");
                            }
                            else {
                                pMQLImproved.html("+" + FormatNumber(obj, true))
                                pMQLImproved.addClass("greenfont");
                            }
                        }
                        else {
                            pMQLImproved.html('---');
                        }
                    }

                });

            }
        }
    });
}


function GetNumberOfActivityPerMonByPlanId(url, activemenu, timeframe, selectedPlanIds, Customid, OwnerId, Tacticids, StatusId) {
    $.ajax(
    {
        type: "POST",
        url: url,
        data: {
            planid: selectedPlanIds.toString(),
            strtimeframe: timeframe,
            CustomFieldId: Customid.toString(),
            OwnerIds: OwnerId.toString(),
            TacticTypeids: Tacticids.toString(),
            StatusIds: StatusId.toString(),
            IsGridView: $('#IsGridView').val().toString(),

        },
        dataType: "json",
        success: function (data) {
            if (data != null) {
                $(".dhx_canvas_text").remove();
                $("canvas").remove();
                setgraphdata(data);

            }
        }
    });
}

function setgraphdata(data) {
    $(".dhx_chart_legend").html('');
    var legendvalue = "";
    var activityyear = data.strtimeframe;
    if (activityyear == "" || activityyear == undefined || activityyear == null) {
        activityyear = $('select#ddlUpComingActivites option:selected').val();
    }
    if (activityyear != undefined) {
        if (activityyear.toString().indexOf('-') != -1) {

            if (activityyear.toString().split('-').length > 1) {
                legendvalue = [{ text: activityyear.split('-')[0], color: "#c633c9" }, { text: activityyear.split('-')[1], color: "#407B22" }];
            }
        }
        else {
            legendvalue = [];
        }
    }
    else {
        legendvalue = [];
    }
    var barChart2 = new dhtmlXChart({
        view: "bar",
        container: "chart2",
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
    barChart2.parse(data.lstchart, "json");

    $('.dhx_chart_legend').css({ 'left': '100px', 'top': '21px' });

    var dhtml_length = $('.dhx_chart_legend').length;
    if (activityyear != undefined) {
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
}


function GetMultiplePlanNames() {
    var PlanNames = "";
    var PlanCount = 0;
    $('#ulSelectedPlans').find("input[type=checkbox]:checked").each(function () {
        var Planname = $(this).parent().attr('title');
        PlanNames += Planname + ",";
        PlanCount++;
    });
    PlanNames = PlanNames.slice(0, -1);
    $("#PlanTitle").html(PlanNames);
    $("#PlanTitle").attr('title', PlanNames);
    $("#PlanCount").html(PlanCount + " Plan(s) Selected")

}

function GetGoalValues(url,selectedPlanIds) {
    $.ajax(
   {
       type: "GET",
       url: url,
       data: {
           planids: selectedPlanIds.toString(),
       },
       dataType: "json",
       success: function (data) {
           $("#Revlbl").html(data.RevenueLabel + " :");
           $("#spnrevenue").html(data.RevenueValue);
           SetBudget("#spnrevenue");
           $("#mqllbl").html(data.MQLLabel + " :");
           $("#spnmql").html(data.MQLValue);
           SetPriceValue("#spnmql");
           $("#inqlbl").html(data.INQLabel + " :");
           $("#spninq").html(data.INQValue);
           SetPriceValue("#spninq");
           $("#cwlbl").html(data.CWLabel + " :");
           $("#spncw").html(data.CWValue);
           SetPriceValue("#spncw");
       }
   });

}

function BindUpcomingActivites(SelectedPlanIds) {
    var listCheckbox = $("#ulSelectedYear").find("input[type=checkbox]");
    var years = "";
    $.each(listCheckbox, function () {
        if ($(this).attr("checked")) {
            years += $(this).attr('yearValue') + ",";
        }
    });
    years = years.slice(0, -1);
  
    var currentval = $("#ddlUpComingActivites").val();
    $.ajax({
        type: 'GET',
        url: urlContent + 'Home/BindUpcomingActivitesValues/',
        async: false,
        data: {
            planids: SelectedPlanIds.toString(),
            fltrYears: years,
            IsBudgetGrid: IsBudgetGrid
        },
        success: function (data) {
            var upcomingvalues = [];
            $.each($("#ddlUpComingActivites option"), function () {
                upcomingvalues.push(this.value.toString());
            });

            if (upcomingvalues.indexOf(currentval) > 0) {
                $.each($("#ddlUpComingActivites option"), function () {
                    $(this).removeAttr('selected');
                });
                $("#ddlUpComingActivites option[value='" + currentval + "']").attr('selected', 'selected');
            }
            BindUpcomingActivies(data);
        }
    });
}

function BindUpcomingActivies(items) {
    var $dropdown = $("#ddlUpComingActivites");
    $dropdown.html('');
    var $html = '';
    if (items.length > 0) {
        $.each(items, function (index, pvalues) {
            if (pvalues.Selected) {
                $html += '<option value="' + pvalues.Value + '" selected="selected">' + pvalues.Text + '</option>';
            } else {
                $html += '<option value="' + pvalues.Value + '">' + pvalues.Text + '</option>';
            }
        });
    }
    $dropdown.append($html);
    $("#ddlUpComingActivites").selectbox('detach');
    $("#ddlUpComingActivites").selectbox("attach");
}


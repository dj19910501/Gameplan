var ImprovementTactic = null;
var RecommendedImprovementTacticType = null;
var SuggestionIMPTacticIdList = [];
var sortText = null;
var isasc = true;
var isADS = false;
var isConversion = true;
var isLoadSuggestion = false;
var isImprovementTacticExits = false;
var costTotal = 0;
var ImpTacticId;
var hdnisError = false;

function ImprovementClick() {
    $("a[id^='divimpTactic']").find("span[id^='divPanelImp']").click(function (e) {
        if (isTacticExist == "False") {
            ismsg = AddMarketingActivitiesBeforeAddImprovementActivities;
            hdnisError = 'True';
            Showmsg();
            return false;
        }
        e.stopPropagation();
        $.ajaxSetup({ cache: false });
        var idTactic = $(this).parent().attr("index");
        if (idTactic == 0) {
            $("a#divimpTactic").removeClass('1');
            $(this).parent().addClass('1');
            var improvementTacticId = $("a#divimpTactic.1").attr('data-id-program');
        }
        displayConfirmCommonImprovement('impTacticCommonClick(' + idTactic + ',' + improvementTacticId + ');');
    });
}

function AddIconClick() {
    $("div[class^='add-icon-old impTactic']").click(function (e) {
        $("#slidepanel").css("display", "none");
        if (isTacticExist == "False") {
            ismsg = AddMarketingActivitiesBeforeAddImprovementActivities;
            hdnisError = 'True';
            Showmsg();
            return false;
        }
        var idProgram = $(this).parent().parent().attr('data-id-program');
        ShowModels(InspectPopupModeAdd, ImprovementTactic, 0, idProgram, InspectPopupRequestedModulesIndex);
    });
}

function Showmsg() {
    if (ismsg != '') {
        if (hdnisError == 'True') {
            ShowMessage(hdnisError, ismsg, 3000);
        }
        else {
            ShowMessage(hdnisError, ismsg, 3000);
        }
    }
}


function LoadImprovementTactic(CurrentPlanId) {
    $.ajax({
        beforeSend: function (x) {
            myApp.hidePleaseWait();
        },
        type: 'POST',
        url: GetImprovementTacticURL,
        data: {
            PlanId: CurrentPlanId
        },
        success: function (r) {
            ImprovementTactic = r;
            fillImprovementTacticTable();
            var isImp = false;
            if (typeof ImprovementTactic != 'undefined') {
                if (ImprovementTactic.length) {
                    isImp = true;
                    FillGrayImprovementContainer(ImprovementTactic.length);
                    LoadClickEvents();
                }
            }
            if (!isImp) {
                SetLabelFormaterWithTitle($("#DivImprovementTotal"));
                FillGrayImprovementContainerEmpty();
                LoadClickEvents();
            }

            loadscript(ImprovementTactic.length);
            ImprovementClick();
            AddIconClick();
        }
    });
}

function FillGrayImprovementContainerEmpty() {
    $("#grayFullContainer").css("display", "block");
    $('.grey-container-full').css("display", "block");
    var $html = '<div class=" imp-tatic-content" style="display:block;">' +
                                      '<div id="imp-tactics" class="gray-add-container border-cf"  style="display:block;">' +
     '<p class="text1">Adding improvement tactics can boost your planned efforts.</p>' +
                                    '<p class="text2">Add your marketing activities first, then add</br> improvement tactics.' +
                                    'The impact of improvement tactics across Average Deal Size,' +
                                    'Stage Velocity and Funnel Conversion can be seen here.</p></div></div>';

    var $ContainerImprovement = $('#grayFullContainer');
    $ContainerImprovement.empty();
    $ContainerImprovement.append($html);
}

//fill campaign table with data from json
function fillImprovementTacticTable() {
    var isImpCount = false;
    if (typeof ImprovementTactic != 'undefined') {
        var $html = '';
        if (ImprovementTactic.length) {
            isImprovementTacticExits = true;
            isImpCount = true;
            $(".markTotal").each(function () {
                $(this).css("color", "#D4D4D4");
            });
            for (i in ImprovementTactic) {
                costTotal = costTotal + parseFloat(ImprovementTactic[i].cost);
                addRowImprovementTactic(i, ImprovementTactic[i].id, ImprovementTactic[i].title, ImprovementTactic[i].cost, ImprovementTactic[i].isOwner, ImprovementTactic[i].ImprovementProgramId);
            }
        }
    }

    $html += '<div class="accordion-header campaign-default">' +
                   '<a class="accordion-toggle noborderbottom" data-toggle="collapse" data-id-program="' + ImprovementPlanProgramId + '" id="divimpTactic" index="0">' +
                       '<span class="pull-left width55" id="divPanelImp"><span class="pull-left font-black"></span><span class="font-black underline">Add Improvement Tactic</span></span>' +
                       '<span class="pull-right width50px"><div class="add-icon-old impTactic" data-slidepanel="panel" title="Add" data-toggle="tooltip" ></div></span>' +
                       '<span class="pull-right width100px text-right font-black">&nbsp;</span>' +
                       '<span class="pull-right width150px text-right font-black tipsyclass cost-data">' + CurrencySybmol + '' + 0 + '</span>' +
                       '<span class="pull-right width100px text-right font-black">&nbsp;</span>' +
                   '</a>' +
               '</div>';



    $html += '<div id="impdiv" class="cf border-cf improvement-activities improve-act">' +
                            '<div class="pull-left first-columns font_size16 border-cf-right suggested-tactics" data-id="1"><span class="cf-arrow">SUGGESTED</span></div>' +
                            '<div class="pull-left first-columns font_size16 border-cf-right" data-id="2"><span class="cf-arrow"><span class="cf-icon ads-icon"></span><span id="spanSuggestedADS">+' + CurrencySybmol + '0</span></span></div>' +
                            '<div class="pull-left first-columns font_size16 border-cf-right" data-id="3"><span class="cf-arrow"><span class="cf-icon velocity-icon"></span><span id="spanSuggestedCW">+0</span> CWs</span></div>' +
                            '<div class="pull-left first-columns font_size16" data-id="4"><span class="cf-arrow"><span class="cf-icon conversion-icon"></span><span id="spanSuggestedVelocity">-0</span> DAYS</span></div>' +
                            '<div class="pull-right-assortment width50px total"><span class="cf-arrow cf-arrow-blue pull-right"></span></div>' +

                        '<div class="total improvementTotal pull-right width100px text-right tipsyclass">' + FormatCommas(programTotal.toString(), false) + '</div>' +
                        '<div class="total improvementTotal pull-right width150px text-right tipsyclass Cost_Improvement_Activities">' + CurrencySybmol + '' + FormatCommas(costTotal.toString(), false) + '</div>' +
                        '<div class="total improvementTotal pull-right width100px text-right border-cf-left last-col " id="DivImprovementTotal">' + FormatCommas(mqlTotal.toString(), false) + '</div>' +
                        '<div class="grey-container-full" id="grayFullContainer">' +
                                                    '</div>' +
                    '</div>';
    var $tableImprovementCampaigns = $('.accordion-campaign > #improvementAccordionGroup');
    $tableImprovementCampaigns.append($html);

    if (!isImpCount) {
        $(".improvementTotal").each(function () {
            $(this).css("color", "#D4D4D4");
        });
    }
    addEventTriggerSlidepanel();
    $('.Cost_Improvement_Activities').each(function () {
        SetLabelFormaterWithTitle($(this));
    });
}

function addRowImprovementTactic(_index, _id, _title, _cost, _isOwner, _programId) {
    var $tableImprovementCampaigns = $('.accordion-campaign > #improvementAccordionGroup');
    var $html = '';
    $html += '<div class="accordion-header campaign-default">' +
                    '<a class="accordion-toggle" id="divimpTactic" index="' + _id + '" data-id-program="' + _programId + '">';
    if (_isOwner == 0) {
        $html += '<span class="delete-Improvement-old width5" id="delimpTactic" title="Delete" data-toggle="tooltip"></span>';
    }
    $html += '<span class="pull-left width55" id="divPanelImp"><span class="pull-left font-black"></span><span class="font-black underline" title="' + _title + '">' + GrapSubstring(_title, 40) + '</span></span>' +
                        '<span class="pull-right width50px"><div class="add-icon-old impTactic" data-slidepanel="panel" title="Add" data-toggle="tooltip"></div></span>' +
                        '<span class="pull-right width100px text-right font-black tipsyclass">&nbsp;</span>' +
                        '<span id="spncostAssotment" class="pull-right width150px text-right font-black tipsyclass Cost_Improvement_Activities add-tatic-td">' + FormatNumber(_cost.toString(), false) + '</span>' +
                        '<span class="pull-right width100px text-right font-black">&nbsp;</span>' +
                    '</a>' +
                '</div>';
    $tableImprovementCampaigns.append($html);
}

function FillGrayImprovementContainer(impcount) {
    $.ajax({
        beforeSend: function (x) {
            myApp.hidePleaseWait();
        },
        type: 'POST',
        url: GetImprovementContainerValueURL,
        data: {
        },
        success: function (data) {
            if (typeof data != 'undefined') {
                $("#grayFullContainer").css("display", "block");
                $('.grey-container-full').css("display", "block");
                var $html = '<div class="imp-tatic-content" style="display:block;">' +
                                                  '<div id="imp-tactics" class="gray-add-container border-cf" style="display:block;">';
                $html += '<p class="text1">Executing ';
                if (impcount > 1) {
                    $html += 'these improvement tactics';
                }
                else {
                    $html += 'this improvement tactic';
                }
                $html += ' could deliver...</p>' +
                    '<p class="text2"><strong>';
                var mqlValue = parseFloat(data.MQL);
                if (mqlValue < 0) {
                    $html += FormatCommas(Math.abs(mqlValue).toString(), false) + '</strong> less';
                }
                else {
                    $html += FormatCommas(mqlValue.toString(), false) + '</strong> more';
                }

                var withImprovedMQL = parseFloat(mqlTotal) + parseFloat(mqlValue);
                $('#DivImprovementTotal').html(FormatCommas(withImprovedMQL.toString(), false))

                SetLabelFormaterWithTitle($("#DivImprovementTotal"));

                var mqltext = MQLLable;
                $html += ' ' + mqltext + ' yielding <strong>';
                var cwValue = parseFloat(data.CW);
                if (cwValue < 0) {
                    $html += FormatCommas(Math.abs(cwValue).toString(), true) + '</strong> less';
                    $("#spanSuggestedCW").text("-" + Math.abs(cwValue).toString());
                }
                else {
                    $html += FormatCommas(cwValue.toString(), true) + '</strong> more';
                    $("#spanSuggestedCW").text("+" + Math.abs(cwValue).toString());
                }
                if (cwValue <= 1) {
                    $html += ' CW,<br><strong>';
                }
                else {
                    $html += ' CWs,<br><strong>';
                }
                var adsValue = parseFloat(data.ADS);
                if (adsValue < 0) {
                    $html += FormatCurrency(Math.abs(adsValue).toString(), false) + '</strong> less';
                    $("#spanSuggestedADS").text("-" + CurrencySybmol + "" + Math.abs(adsValue).toString());
                }
                else {
                    $html += FormatCurrency(adsValue.toString(), false) + '</strong> larger';
                    $("#spanSuggestedADS").text("+" + CurrencySybmol + "" + Math.abs(adsValue).toString());
                }

                $html += ' Average Deal Size and<br><strong>';
                var velocityValue = parseFloat(data.Velocity);
                if (velocityValue <= 0) {
                    $html += FormatCommas(Math.abs(velocityValue).toString(), false) + '</strong> less';
                    $("#spanSuggestedVelocity").text("-" + Math.abs(velocityValue).toString());
                }
                else {
                    $html += FormatCommas(velocityValue.toString(), false) + '</strong> more';
                    $("#spanSuggestedVelocity").text("+" + Math.abs(velocityValue).toString());
                }

                $html += ' days in the funnel with</p>' +
                    '<p class="text3">Net Revenue Impact of ';

                var revenueValue = parseFloat(data.Revenue);
                if (revenueValue < 0) {
                    $html += '<span id="spanRevenueImpact" >' + '-' + FormatCurrency(Math.abs(revenueValue).toString(), true) + '</span>';
                }
                else {
                    $html += '<span id="spanRevenueImpact" >' + '' + FormatCurrency(revenueValue.toString(), true) + '</span>';
                }

                $html += '<br> on ';

                var costValue = parseFloat(data.Cost);
                if (costValue < 0) {
                    $html += '<span id="spanRevenuecostValue" >' + '-' + FormatCurrency(Math.abs(costValue).toString(), false) + '</span>' + ' decreased';
                }
                else {
                    $html += '<span id="spanRevenuecostValue" >' + FormatCurrency(costValue.toString(), false) + '</span>' + ' increased';
                }

                $html += ' spend.</p></div></div>';

                var $ContainerImprovement = $('#grayFullContainer');
                $ContainerImprovement.empty();
                $ContainerImprovement.append($html);

                SetLabelFormaterWithTitle($("#spanRevenuecostValue"));
                SetLabelFormaterWithTitle($("#spanRevenueImpact"));
            }
        }
    });
}


function LoadClickEvents() {
    $("[id=divimpTactic][index=" + TacticId + "]").find("#divPanelImp").click();
    return false;
}

function loadscript(ImprovementTacticlength) {

    $('.total .cf-arrow').click(function () {
        $("#grayFullContainer").css("display", "block");
        if (ImprovementTacticlength) {
            FillGrayImprovementContainer(ImprovementTactic.length);
        }
        FillGrayImprovementContainerEmpty();
        $('.first-columns').removeClass('active-column');
        $('.gray-add-container').slideDown(200);
        $("#btnSaveAndContinue").css("display", "block");
        $("#divSuggestionBox").css("padding-bottom", "150px");
    });

    $('.first-columns').click(function () {
        $('.total .cf-arrow').removeClass('cf-arrow-blue');
        $("#btnSaveAndContinue").css("display", "none");
        var dataId = $(this).attr('data-id');
        FillFullContainer(dataId);
        $('.gray-add-container').slideUp();
        $('.improvement-activities').find('.active-column').each(function () {
            $(this).removeClass('active-column');
        });
        $(this).addClass('active-column');

        $('.grey-container-full').slideDown(200);
        $(this).find('.border-bottom').css('display', 'block');

        var dataId = $(this).attr('data-id');
        $('.grey-container-content > div').fadeOut(50);
        $('.grey-container-content > div').fadeIn(200);
    });
}
function FillFullContainer(dataid) {
    var $html = '';
    if (dataid == 1) {
        $html += '<div class="grey-container-content">' +
                                       '<div id="suggested-tactics">' +
                                           '<h2 class="source-sans-probold">Suggested Improvement Activities</h2>' +
                                           '<table class="source-sans-proregular grayContent-tacticTable text-right" id="tableImprovementTypeSuggested">' +
                                           '<thead>' +
                                               '<tr>' +
                                                   '<th data-column="1" class="left-text">Improvement Tactic Type</th>' +
                                                   '<th data-column="2" class="noRightBorder tacticTable-arrow-icon arrow-bottom cost-arrow" width="9%" sortText="Cost">Cost</th>' +
                                                   '<th data-column="3" class="noRightBorder" width="15%">Projected Revenue Without Tactic</th>' +
                                                   '<th data-column="4" class="tacticTable-arrow-icon arrow-bottom" width="17%" sortText="ProjectedRevenueWithTactic">Projected Revenue With Tactic<!--<span class="tacticTable-arrow-icon arrow-bottom"></span> --></th>' +
                                                   '<th id="defaultSortingId" data-column="5" class="tacticTable-arrow-icon arrow-bottom" width="13%" sortText="ProjectedRevenueLift">Projected Revenue Lift</th>' +
                                                   '<th data-column="6" class="tacticTable-arrow-icon arrow-bottom" width="13%" sortText="RevenueToCostRatio">Revenue to Cost Ratio</th>' +
                                                   '<th data-column="7" class="text-center noRightBorder" width="7%">Add to Plan</th>' +
                                               '</tr>' +
                                               '</thead>' +
                                               '<tbody>' +
                                               '</tbody>' +
                                           '</table>' +
                                       '</div></div>';
    }
    else if (dataid == 2) {
        $html += '<div class="grey-container-content">' +
                                     '<div id="ads">' +
                                         '<h2 class="source-sans-probold">Average Dollor Sale Improvement</h2>' +
                                         '<table class="source-sans-proregular grayContent-tacticTable text-right" id="tableImprovementTypeSuggested">' +
                                         '<thead>' +
                                             '<tr>' +
                                                 '<th data-column="1" class="left-text">Improvement Tactic Type</th>' +
                                                 '<th data-column="2" class=" noRightBorder tacticTable-arrow-icon arrow-bottom cost-arrow" width="9%" sortText="Cost">Cost</th>' +
                                                 '<th data-column="3" class="noRightBorder" width="15%">Projected ADS Without Tactic</th>' +
                                                 '<th data-column="4" class="tacticTable-arrow-icon arrow-bottom" width="17%" sortText="ProjectedRevenueWithTactic">Projected ADS With Tactic</th>' +
                                                 '<th id="defaultSortingId" data-column="5" class="tacticTable-arrow-icon arrow-bottom" width="13%" sortText="ProjectedRevenueLift">Projected Revenue Lift</th>' +
                                                 '<th data-column="6" class="tacticTable-arrow-icon arrow-bottom" width="13%" sortText="RevenueToCostRatio">Revenue to Cost Ratio</th>' +
                                                 '<th data-column="7" class="text-center noRightBorder" width="7%">Add to Plan</th>' +
                                             '</tr>' +
                                             '</thead>' +
                                             '<tbody>' +
                                             '</tbody>' +
                                         '</table>' +
                                     '</div></div>';
    }
    else if (dataid == 3) {
        $html += '<div class="grey-container-content">' +
                                    '<div id="conversion">' +
                                    '</div></div>';
    }
    else if (dataid == 4) {
        $html += '<div class="grey-container-content">' +
                                    '<div id="conversion">' +
                                    '</div></div>';
    }

    var $ContainerGreyFullImprovement = $('#grayFullContainer');
    $ContainerGreyFullImprovement.empty();
    $ContainerGreyFullImprovement.append($html);
    isADS = false;
    isLoadSuggestion = false;
    globledataid = dataid;
    if (dataid == 1) {
        if (isTacticExist == "False") {
            $("#suggested-tactics").empty();
            $("#suggested-tactics").html('<h2 class="source-sans-probold">No marketing activities exist.</h2>');
        }
        else {
            LoadRecommendedGrid();
        }
    }
    else if (dataid == 2) {
        if (!isImprovementTacticExits) {
            $("#ads").empty();
            $("#ads").html('<h2 class="source-sans-probold">No improvement tactic(s) exist.</h2>');
        }
        else {
            isADS = true;
            LoadADSGrid();
        }
    }
    else if (dataid == 3) {
        if (!isImprovementTacticExits) {
            $("#conversion").empty();
            $("#conversion").html('<h2 class="source-sans-probold">No improvement tactic(s) exist.</h2>');
        }
        else {
            isConversion = true;
            LoadConversionGrid();
        }
    }
    else if (dataid == 4) {
        if (!isImprovementTacticExits) {
            $("#conversion").empty();
            $("#conversion").html('<h2 class="source-sans-probold">No improvement tactic(s) exist.</h2>');
        }
        else {
            isConversion = false;
            LoadConversionGrid();
        }
    }
}

function LoadRecommendedGrid() {
    $.ajax({
        type: 'POST',
        url: GetRecommendedImprovementTacticTypeURL,
        data: {
        },
        success: function (data) {
            RecommendedImprovementTacticType = data;
            fillRecommendedImprovementTacticType(false);
            if (!isLoadSuggestion) {
                LoadScriptForSuggestion();
            }
            var divsuggstedheight = $(".grey-container-content").outerHeight();
            var finalheight = 50 + parseFloat(divsuggstedheight);
            $("#divSuggestionBox").css("padding-bottom", finalheight);

        }
    });
}

function fillRecommendedImprovementTacticType() {
    $('#tableImprovementTypeSuggested > tbody').empty();
    if (typeof RecommendedImprovementTacticType != 'undefined') {
        var $html = '';
        if (RecommendedImprovementTacticType.data.length) {
            for (i in RecommendedImprovementTacticType.data) {
                addRowRecommendedImprovementTacticType(RecommendedImprovementTacticType.data[i].ImprovementPlanTacticId, RecommendedImprovementTacticType.data[i].ImprovementTacticTypeTitle, RecommendedImprovementTacticType.data[i].ProjectedRevenueWithoutTactic, RecommendedImprovementTacticType.data[i].Cost, RecommendedImprovementTacticType.data[i].ProjectedRevenueWithTactic, RecommendedImprovementTacticType.data[i].ProjectedRevenueLift, RecommendedImprovementTacticType.data[i].RevenueToCostRatio, RecommendedImprovementTacticType.data[i].ImprovementTacticTypeId, RecommendedImprovementTacticType.data[i].IsExits, RecommendedImprovementTacticType.data[i].IsOwner);
            }
        }
    }
    if (!isADS) {
        LoadScriptForTableRow();
    }
    if (isADS) {
        LoadScriptForADS(1);
    }
    $('.SuggestedSection').each(function () {
        SetLabelFormaterWithTitle($(this));
    });
}

function addRowRecommendedImprovementTacticType(ImprovementPlanTacticId, ImprovementTacticTypeTitle, ProjectedRevenueWithoutTactic, Cost, ProjectedRevenueWithTactic, ProjectedRevenueLift, RevenueToCostRatio, ImprovementTacticTypeId, IsExits, IsOwner) {
    var $tableImprovementTactic = $('#tableImprovementTypeSuggested > tbody');
    var $html = '<tr';
    if (IsExits) {
        $html += ' class="added"';
    }
    $html += '><td data-column="1" class="left-text">' + ImprovementTacticTypeTitle + '</td>'
                        + '	<td class="SuggestedSection" data-column="2"';

    if (!isADS) {
        $html += ' class="noRightBorder SuggestedSection"';
    }
    $html += ' >' + FormatNumber(Cost.toString(), false) + '</td>';
    var isDashedDisplay = false;
    if (isADS) {
        if (ProjectedRevenueWithoutTactic == ProjectedRevenueWithTactic) {
            isDashedDisplay = true;
        }
    }
    if (isDashedDisplay) {
        $html += '<td data-column="3" class="noRightBorder">---  </td>'
                        + '	<td data-column="4" >---  </td>';
    }
    else {
        $html += '<td data-column="3" class="noRightBorder SuggestedSection">' + CurrencySybmol + '' + FormatCommas(ProjectedRevenueWithoutTactic.toString(), true) + '</td>'
                     + '	<td class="SuggestedSection" data-column="4" >' + CurrencySybmol + '' + FormatCommas(ProjectedRevenueWithTactic.toString(), true) + '</td>';
    }

    $html += '	<td data-column="5">' + ProjectedRevenueLift + '%</td>'
                        + '	<td data-column="6">' + RevenueToCostRatio + '</td>'
                        + '	<td data-column="7" class="text-center noRightBorder addToPlan"><span';
    if (IsExits) {
        $html += ' IMPTacticTypeId="' + ImprovementPlanTacticId + '"';
        if (IsOwner || isADS) {
            $html += ' class="circle-check-icon-blue"';
        }
    }
    else {
        if (isADS) {
            $html += ' IMPTacticTypeId="' + ImprovementPlanTacticId + '" class="circle-check-icon-gray"';
        }
        else {
            $html += ' IMPTacticTypeId="' + ImprovementTacticTypeId + '" class="circle-check-icon-gray"';
        }

    }
    $html += '></span></td>'
                            + '</tr>';
    $tableImprovementTactic.append($html);
}

function LoadADSGrid() {
    $.ajax({
        type: 'POST',
        url: GetADSImprovementTacticTypeURL,
        data: {
            SuggestionIMPTacticIdList: SuggestionIMPTacticIdList.toString()
        },
        success: function (data) {
            RecommendedImprovementTacticType = data;
            fillRecommendedImprovementTacticType();
            if (!isLoadSuggestion) {
                LoadScriptForSuggestion();
            }
            $.ajax({
                type: 'POST',
                url: GetHeaderValueForSuggestedImprovementURL,
                data: {
                    SuggestionIMPTacticIdList: SuggestionIMPTacticIdList.toString()
                },
                success: function (data) {
                    if (typeof data != 'undefined') {
                        var adsValue = parseFloat(data.ADS);
                        if (adsValue < 0) {
                            $("#spanSuggestedADS").text("-" + CurrencySybmol + "" + Math.abs(adsValue).toString());
                        }
                        else {
                            $("#spanSuggestedADS").text("+" + CurrencySybmol + "" + Math.abs(adsValue).toString());
                        }

                        var cwValue = parseFloat(data.CW);
                        if (cwValue < 0) {
                            $("#spanSuggestedCW").text("-" + Math.abs(cwValue).toString());
                        }
                        else {
                            $("#spanSuggestedCW").text("+" + Math.abs(cwValue).toString());
                        }


                        var velocityValue = parseFloat(data.Velocity);
                        if (velocityValue <= 0) {
                            $("#spanSuggestedVelocity").text("-" + Math.abs(velocityValue).toString());
                        }
                        else {
                            $("#spanSuggestedVelocity").text("+" + Math.abs(velocityValue).toString());
                        }

                    }

                }
            });
            var divsuggstedheight = $(".grey-container-content").outerHeight();
            var finalheight = 50 + parseFloat(divsuggstedheight);
            $("#divSuggestionBox").css("padding-bottom", finalheight);
        }
    });
}

function LoadConversionGrid() {
    $.ajax({
        type: 'POST',
        url: GetConversionImprovementTacticTypeURL,
        data: {
            SuggestionIMPTacticIdList: SuggestionIMPTacticIdList.toString(),
            isConversion: isConversion
        },
        success: function (data) {
            RecommendedImprovementTacticType = data;
            fillRecommendedConversionImprovement();
            $.ajax({
                type: 'POST',
                url: GetHeaderValueForSuggestedImprovementURL,
                data: {
                    SuggestionIMPTacticIdList: SuggestionIMPTacticIdList.toString()
                },
                success: function (data) {
                    if (typeof data != 'undefined') {
                        var adsValue = parseFloat(data.ADS);
                        if (adsValue < 0) {
                            $("#spanSuggestedADS").text("-" + CurrencySybmol + "" + Math.abs(adsValue).toString());
                        }
                        else {
                            $("#spanSuggestedADS").text("+" + CurrencySybmol + "" + Math.abs(adsValue).toString());
                        }

                        var cwValue = parseFloat(data.CW);
                        if (cwValue < 0) {
                            $("#spanSuggestedCW").text("-" + Math.abs(cwValue).toString());
                        }
                        else {
                            $("#spanSuggestedCW").text("+" + Math.abs(cwValue).toString());
                        }


                        var velocityValue = parseFloat(data.Velocity);
                        if (velocityValue <= 0) {
                            $("#spanSuggestedVelocity").text("-" + Math.abs(velocityValue).toString());
                        }
                        else {
                            $("#spanSuggestedVelocity").text("+" + Math.abs(velocityValue).toString());
                        }
                    }
                }
            });
            var divsuggstedheight = $(".grey-container-content").outerHeight();
            var finalheight = 50 + parseFloat(divsuggstedheight);
            $("#divSuggestionBox").css("padding-bottom", finalheight);
        }
    });
}

function fillRecommendedConversionImprovement() {
    $('#conversion').empty();
    if (typeof RecommendedImprovementTacticType != 'undefined') {
        var html = '';
        html += '<h2 class="source-sans-probold">';
        if (isConversion) {
            html += 'Conversion Improvement</h2>';
        }
        else {
            html += 'Velocity Improvement</h2>';
        }
        html += '<table class="source-sans-proregular grayContent-tacticTable text-right" id="tableImprovementTypeSuggested">';

        html += '<thead><tr>' +
                        '<th class="left-text">Improvement Tactic Type</th>' +
                        '<th class=" noRightBorder tacticTable-arrow-icon arrow-bottom cost-arrow" width="9%" data-column="2" sortText="Cost">Cost</th>';

        if (RecommendedImprovementTacticType.datametriclist.length) {
            var maxLength = RecommendedImprovementTacticType.datametriclist.length;
            for (i in RecommendedImprovementTacticType.datametriclist) {
                var title = RecommendedImprovementTacticType.datametriclist[i].Title.toString();
                if (isConversion) {
                    var arrTitle = title.split("->");
                    if (i == 0) {
                        html += '<th class="noRightBorder" width="10%">Weight<br>' + arrTitle[0] + '<span class="arrow-right-icon"></span>' + arrTitle[1] + '</th>';
                    }
                    else if (i == maxLength - 1) {
                        html += '<th width="9%">' + arrTitle[0] + '<span class="arrow-right-icon"></span>' + arrTitle[1] + '</th>';
                    }
                    else {
                        html += '<th class="noRightBorder" width="9%">' + arrTitle[0] + '<span class="arrow-right-icon"></span>' + arrTitle[1] + '</th>';
                    }
                }
                else {
                    if (i == 0) {
                        html += '<th class="noRightBorder" width="8%">Weight<br>' + title + '</th>';
                    }
                    else if (i == maxLength - 1) {
                        html += '<th width="8%">' + title + '</th>';
                    }
                    else {
                        html += '<th class="noRightBorder" width="8%">' + title + '</th>';
                    }
                }
            }
        }

        html += '<th class="text-center noRightBorder" width="7%">Add to Plan</th>' +
                        '						</tr></thead><tbody>';

        if (RecommendedImprovementTacticType.data.length) {
            for (i in RecommendedImprovementTacticType.data) {
                var htmlOut = addRowRecommendedConversion(RecommendedImprovementTacticType.data[i].ImprovementPlanTacticId, RecommendedImprovementTacticType.data[i].ImprovementTacticTypeTitle, RecommendedImprovementTacticType.data[i].Cost, RecommendedImprovementTacticType.data[i].MetricList, RecommendedImprovementTacticType.data[i].IsExits);
                html += htmlOut;
            }
        }

        html += '<tr>' +
            '<th class="noRightBorder left-text"></th>' +
            '<th></th>';
        if (RecommendedImprovementTacticType.datafinalmetriclist.length) {
            var maxLength = RecommendedImprovementTacticType.datafinalmetriclist.length;
            for (i in RecommendedImprovementTacticType.datafinalmetriclist) {
                var mvalue;
                if (isConversion) {
                    mvalue = FormatNumber((Math.round(parseFloat(RecommendedImprovementTacticType.datafinalmetriclist[i].Value) * 100) / 100), true);
                }
                else {
                    mvalue = (Math.round(parseFloat(RecommendedImprovementTacticType.datafinalmetriclist[i].Value) * 100) / 100) + " Days";
                }

                if (i == maxLength - 1) {
                    html += '<th>' + mvalue + '</th>';
                }
                else {
                    html += '<th class="noRightBorder">' + mvalue + '</th>';
                }
            }
        }
        html += '<th class="text-center noRightBorder"></th>' +
            '</tr>';
        html += '</tbody></table>';
        var $htmlFinal = html;
        $('#conversion').append($htmlFinal);
    }
    LoadScriptForConversion();
    LoadScriptForADS();


    $('.SuggestedSection').each(function () {
        SetLabelFormaterWithTitle($(this));
    });
}

function addRowRecommendedConversion(ImprovementPlanTacticId, ImprovementTacticTypeTitle, Cost, MetricList, IsExits) {
    var htmlInner = '<tr';
    if (IsExits) {
        htmlInner += ' class="added"';
    }
    htmlInner += '><td data-column="1" class="left-text">' + ImprovementTacticTypeTitle + '</td>'
                         + '	<td class="SuggestedSection" data-column="2">' + CurrencySybmol + '' + FormatCommas(Cost.toString(), false) + '</td>';
    if (typeof MetricList != 'undefined') {

        if (MetricList.length > 0) {
            var metricMax = MetricList.length - 1;
            for (i in MetricList) {
                var mvalue = MetricList[i].Value;
                var displayMvalue;
                if (mvalue == 0) {
                    displayMvalue = "---";
                }
                else {
                    displayMvalue = mvalue;
                }
                if (i == metricMax) {
                    htmlInner += '<td>' + displayMvalue + '</td>';
                }
                else {
                    htmlInner += '<td class="noRightBorder">' + displayMvalue + '</td>';
                }
            }
        }
    }
    htmlInner += '<td data-column="7" class="text-center noRightBorder addToPlan"><span';
    if (IsExits) {
        htmlInner += ' IMPTacticTypeId="' + ImprovementPlanTacticId + '" class="circle-check-icon-blue"';
    }
    else {
        htmlInner += ' IMPTacticTypeId="' + ImprovementPlanTacticId + '" class="circle-check-icon-gray"';
    }
    htmlInner += '></span></td>'
                            + '</tr>';
    return htmlInner;
}

function LoadScriptForADS(tabvalue) {
    $("#tableImprovementTypeSuggested").find(".addToPlan span").click(function () {

        if ($(this).hasClass('circle-check-icon-gray')) {
            $(this).removeClass('circle-check-icon-gray');
            $(this).addClass('circle-check-icon-blue');
            var impTacticId = $(this).attr('IMPTacticTypeId');
            SuggestionIMPTacticIdList = $.grep(SuggestionIMPTacticIdList, function (value) {
                return value != parseFloat(impTacticId);
            });
        }
        else {
            $(this).removeClass('circle-check-icon-blue');
            $(this).addClass('circle-check-icon-gray');
            var impTacticId = $(this).attr('IMPTacticTypeId');
            SuggestionIMPTacticIdList.push(parseFloat(impTacticId));
        }

        if (tabvalue == 1) {
            LoadADSGrid();
        }
        else {
            LoadConversionGrid();
        }
        LoadTableImprovementSort();

    });
}

function LoadScriptForTableRow() {
    $("#tableImprovementTypeSuggested").find(".addToPlan span").click(function () {
        if ($(this).hasClass('circle-check-icon-gray')) {
            $(this).removeClass('circle-check-icon-gray');
            $(this).addClass('circle-check-icon-blue');
            var impTacticId = $(this).attr('IMPTacticTypeId');
            AddImprovementTactic(impTacticId);
        }
        else {

            var impTacticId = $(this).attr('IMPTacticTypeId');
            var idTactic = impTacticId;
            $("#DivPartialViewForDeleteImpTactic").empty();
            var AssortmentType = true;
            $("#divBackground").css("display", "block");
            var url = ShowDeleteImprovementTacticURL;
            $("#DivPartialViewForDeleteImpTactic").load(url + '?id=' + idTactic + '&AssortmentType=' + AssortmentType + '&RedirectType=' + false);
        }
    });
}

function AddImprovementTactic(impTacticId) {
    $.ajax({
        type: 'POST',
        url: AddSuggestedImprovementTacticURL,
        data:
            {
                improvementPlanProgramId: ImprovementPlanProgramId,
                improvementTacticTypeId: impTacticId
            },
        success: function (data) {
            if (data.redirect) {
                window.location.href = data.redirect;
                return;
            }
            if (data.errormsg) {
                ismsg = data.errormsg;
                hdnisError = true;
                Showmsg();
            }
        }
    });
}

function LoadTableImprovementSort() {
    $("#tableImprovementTypeSuggested").find(".arrow-blue-top").each(function () {
        $(this).removeClass('arrow-blue-top');
    });
    $("#tableImprovementTypeSuggested").find(".arrow-blue-bottom").each(function () {
        $(this).removeClass('arrow-blue-bottom');
    });
    $("#tableImprovementTypeSuggested").find(".tacticTable-arrow-icon").each(function () {
        if ($(this).hasClass('blueHighlight')) {
            $(this).removeClass('blueHighlight');
        }
        if (!$(this).hasClass('arrow-bottom')) {
            $(this).addClass('arrow-bottom');
        }
    });
}

function LoadScriptForConversion() {

    $("#tableImprovementTypeSuggested").find(".tacticTable-arrow-icon").click(function () {
        var column = $(this).attr('data-column');
        var isTop = false;
        if ($(this).hasClass('arrow-blue-top')) {
            isTop = false;
        }
        if ($(this).hasClass('arrow-blue-bottom')) {
            isTop = true;
        }
        $("#tableImprovementTypeSuggested").find(".arrow-top").each(function () {
            $(this).removeClass('arrow-top');
        });
        $("#tableImprovementTypeSuggested").find(".arrow-bottom").each(function () {
            $(this).removeClass('arrow-bottom');
        });
        $("#tableImprovementTypeSuggested").find(".arrow-blue-top").each(function () {
            $(this).removeClass('arrow-blue-top');
        });
        $("#tableImprovementTypeSuggested").find(".arrow-blue-bottom").each(function () {
            $(this).removeClass('arrow-blue-bottom');
        });
        $("#tableImprovementTypeSuggested").find(".tacticTable-arrow-icon").each(function () {
            if ($(this).hasClass('blueHighlight')) {
                $(this).removeClass('blueHighlight');
            }
            if (!$(this).hasClass('arrow-bottom')) {
                $(this).addClass('arrow-bottom');
            }
        });
        if ($(this).hasClass('arrow-bottom')) {
            $(this).removeClass('arrow-bottom');
        }
        sortText = $(this).attr('sortText');
        isasc = true;

        if (isTop) {
            isasc = false;
        }
        else {
            isasc = true;
        }
        sortResults(sortText, isasc);
        $('.tacticTable-arrow-icon').removeClass('arrow-bottom');
        $('.tacticTable-arrow-icon').addClass('blueHighlight');
        if (isTop) {
            $('.tacticTable-arrow-icon').addClass('arrow-blue-top');
        }
        else {
            $('.tacticTable-arrow-icon').addClass('arrow-blue-bottom');
        }
        $('#tableImprovementTypeSuggested td[data-column=' + column + ']').addClass('blueHighlight');
    });
}

function LoadScriptForSuggestion() {
    isLoadSuggestion = true;

    $("#tableImprovementTypeSuggested").find(".tacticTable-arrow-icon").each(function () {
        $(this).click(function () {
            var column = $(this).attr('data-column');
            var isTop = false;
            if ($(this).hasClass('arrow-blue-top')) {
                isTop = false;
            }
            if ($(this).hasClass('arrow-blue-bottom')) {
                isTop = true;
            }
            $("#tableImprovementTypeSuggested").find(".arrow-top").each(function () {
                $(this).removeClass('arrow-top');
            });
            $("#tableImprovementTypeSuggested").find(".arrow-bottom").each(function () {
                $(this).removeClass('arrow-bottom');
            });
            $("#tableImprovementTypeSuggested").find(".arrow-blue-top").each(function () {
                $(this).removeClass('arrow-blue-top');
            });
            $("#tableImprovementTypeSuggested").find(".arrow-blue-bottom").each(function () {
                $(this).removeClass('arrow-blue-bottom');
            });
            $("#tableImprovementTypeSuggested").find(".tacticTable-arrow-icon").each(function () {
                if ($(this).hasClass('blueHighlight')) {
                    $(this).removeClass('blueHighlight');
                }
                if (!$(this).hasClass('arrow-bottom')) {
                    $(this).addClass('arrow-bottom');
                }
            });
            if ($(this).hasClass('arrow-bottom')) {
                $(this).removeClass('arrow-bottom');
            }

            $(this).addClass('blueHighlight');
            sortText = $(this).attr('sortText');
            isasc = true;
            if (isTop) {
                $(this).addClass('arrow-blue-top');
                isasc = false;
            }
            else {
                $(this).addClass('arrow-blue-bottom');
                isasc = true;
            }
            sortResults(sortText, isasc);
            $('#tableImprovementTypeSuggested td[data-column=' + column + ']').addClass('blueHighlight');
        });
    });

}

function sortResults(prop, asc, dataid) {
    RecommendedImprovementTacticType.data = RecommendedImprovementTacticType.data.sort(function (a, b) {
        if (asc) return (a[prop] > b[prop]) ? 1 : ((a[prop] < b[prop]) ? -1 : 0);
        else return (b[prop] > a[prop]) ? 1 : ((b[prop] < a[prop]) ? -1 : 0);
    });
    if (globledataid == 1) {
        fillRecommendedImprovementTacticType();
    }
    else if (globledataid == 2) {
        fillRecommendedImprovementTacticType();
    }
    else if (globledataid == 3) {
        fillRecommendedConversionImprovement();
    }
    else if (globledataid == 4) {
        fillRecommendedConversionImprovement();
    }
}

function addEventTriggerSlidepanel() {
    $('[data-slidepanel]').unbind('slidepanel');
    $('[data-slidepanel]').slidepanel({
        orientation: 'right',
        mode: 'overlay'
    });
}


function displayConfirmCommonImprovement(functionName) {
    if (isImpDataChanged()) {
        $('#spanMessageError').html("You have unsaved changes. Do you wish to leave this page and lose your work?&nbsp;&nbsp;&nbsp;<a id='btnConfirmOK' class='btn-gray CursorHand' style='color:gray;'>Continue</a>&nbsp;&nbsp;<a style='color:gray;' id='confirmClose' href='#' class='underline'>Cancel</a>");
        $("#errorMessage").slideDown(400);
        $("#btnConfirmOK").click(function () {
            $('#spanMessageError').html("");
            $("#errorMessage").hide();
            eval(functionName);
        });
    }
    else {
        eval(functionName);
    }
}

function displayconfirmForDeleteEvent(id, type) {
    if (isImpDataChanged()) {
        $('#spanMessageError').html("You have unsaved changes. Do you wish to leave this page and lose your work?&nbsp;&nbsp;&nbsp;<a id='btnConfirmOK' class='btn-gray CursorHand' style='color:gray;'>Continue</a>&nbsp;&nbsp;<a style='color:gray;' id='confirmClose' href='#' class='underline'>Cancel</a>");
        $("#errorMessage").slideDown(400);
        $("#btnConfirmOK").click(function () {
            $('#spanMessageError').html("");
            $("#errorMessage").hide();
            $("#slidepanel").css("display", "none");
            $("#slidepanel-container").empty();
            deleteCommonClick(id, type);
        });
    }
    else {
        deleteCommonClick(id, type);
    }
}

function isImpDataChanged() {
    var changed = false;
    $('#slidepanel').find("input[type=text],textarea,select, .priceValue").each(function () {
        var iv = $(this).attr("myValue");
        if ($(this).val() != iv) {
            changed = true;
            return false;
        }
    });
    return changed;
}

function deleteCommonClick(id, type) {
    var idTactic = id;
    var AssortmentType = true;
    $("#DivPartialViewForDeleteImpTactic").empty();
    $("#divBackground").css("display", "block");
    var url = ShowDeleteImprovementTacticURL;
    $("#DivPartialViewForDeleteImpTactic").load(url + '?id=' + idTactic + '&AssortmentType=' + AssortmentType + '&RedirectType=' + false);
}
function SetTooltip(){$(".grid_Search").tooltip({container:"body",placement:"bottom"}),$(".grid_add").tooltip({container:"body",placement:"bottom"}),$(".honeycombbox-icon-gantt").tooltip({container:"body",placement:"bottom"})}function GridHideColumn(){StartDateColIndex=HomeGrid.getColIndexById(StartDateId),EndDateColIndex=HomeGrid.getColIndexById(EndDateId),TaskNameColIndex=HomeGrid.getColIndexById(TaskNameId),PlannedCostColIndex=HomeGrid.getColIndexById(PlannedCostId),AssetTypeColIndex=HomeGrid.getColIndexById(AssetTypeId),TypeColIndex=HomeGrid.getColIndexById(TacticTypeId),OwnerColIndex=HomeGrid.getColIndexById(OwnerId),TargetStageGoalColIndex=HomeGrid.getColIndexById(TargetStageGoalId),MQLColIndex=HomeGrid.getColIndexById(MQLId),RevenueColIndex=HomeGrid.getColIndexById(RevenueId),GridHiddenId=HomeGrid.getColIndexById("id"),ActivitypeHidden=HomeGrid.getColIndexById(ActivityTypeId),MachineNameHidden=HomeGrid.getColIndexById(MachineNameId)}function MoveColumn(){HomeGrid.attachEvent("onAfterCMove",function(a,b){for(var c=HomeGrid.getColumnCount(),d=[],e="Common",f=0;f<c;f++){e="Common";var g=HomeGrid.getColWidth(f),h=HomeGrid.getColumnId(f).toString();0!=g&&d.push({AttributeId:h,AttributeType:e,ColumnOrder:parseInt(f)})}null!=d&&d.length>0&&void 0!=d&&(d=JSON.stringify(d),$.ajax({url:urlContent+"ColumnView/SaveColumnView",type:"post",dataType:"json",contentType:"application/json",data:"{'AttributeDetail':"+d+"}",success:function(a){}}))})}function LoadAfterParsing(){0!=eventidonedit&&HomeGrid.detachEvent(eventidonedit),eventidonedit=HomeGrid.attachEvent("onEditCell",doOnEditCell),0!=eventiddrag&&HomeGrid.detachEvent(eventiddrag),eventiddrag=HomeGrid.attachEvent("onDrag",doOnDrag),0!=eventidonscroll&&HomeGrid.detachEvent(eventidonscroll),eventidonscroll=HomeGrid.attachEvent("onScroll",function(a,b){$(".dhx_combo_select").css("display","none"),$(".dhtmlxcalendar_dhx_skyblue").css("display","none"),$("#popupType").css("display","none"),$(".dhx_clist").css("display","none")}),0!=eventidonbeforedrag&&HomeGrid.detachEvent(eventidonbeforedrag),eventidonbeforedrag=HomeGrid.attachEvent("onBeforeDrag",function(a){if(""!=a&&void 0!=a){var b=HomeGrid.cells(a,GridHiddenId).getValue(),c=HomeGrid.cells(a,ActivitypeHidden).getValue();if(b.length>0){if(c.toLowerCase()!=secTactic)return!1;var d=HomeGrid.cells(a,TaskNameColIndex).getAttribute("lo");return null==d||""==d||"1"!=d}}}),SetselectedRow(),0!=editidonOpenEnd&&HomeGrid.detachEvent(editidonOpenEnd),editidonOpenEnd=HomeGrid.attachEvent("onOpenEnd",function(a){SetTooltip()})}function sort_Owner(a,b,c,d,e){return a=HomeGrid.cells(d,OwnerColIndex).getText(),b=HomeGrid.cells(e,OwnerColIndex).getText(),"asc"==c?a>b?1:-1:a>b?-1:1}function sort_TacticType(a,b,c,d,e){var f=HomeGrid.cells(d,ActivitypeHidden).getValue(),g=HomeGrid.cells(e,ActivitypeHidden).getValue();return f.toLowerCase()==secTactic.toLowerCase()&&g.toLowerCase()==secTactic.toLowerCase()?(a=HomeGrid.cells(d,TypeColIndex).getText(),b=HomeGrid.cells(e,TypeColIndex).getText(),"asc"==c?a>b?1:-1:a>b?-1:1):0}function convertNumber(a){var b=0,c=parseFloat(a.replace(CurrencySybmol,""));return b=a.toLowerCase().match(/k/)?Math.round(1e3*c):a.toLowerCase().match(/m/)?Math.round(1e6*c):a.toLowerCase().match(/b/)?Math.round(1e9*c):numb.replace(CurrencySybmol,"")}function ResizeGrid(a){$("#gridbox").attr("width",a),HomeGrid.setSizes(),LoadAfterParsing()}function doOnDrag(b,c){var d="",e="",f="";if(""!=b&&""!=c&&void 0!=b&&void 0!=c){var g=HomeGrid.cells(b,GridHiddenId).getValue(),h=HomeGrid.cells(c,GridHiddenId).getValue(),i=HomeGrid.getParentId(b),j=HomeGrid.cells(i,GridHiddenId).getValue();if(j.length>0&&g.length>0&&h.length>0){if(d=HomeGrid.cells(i,ActivitypeHidden).getValue(),e=HomeGrid.cells(b,ActivitypeHidden).getValue(),f=HomeGrid.cells(c,ActivitypeHidden).getValue(),e.toLowerCase()==secTactic){if(d==f){var k=HomeGrid.getParentId(HomeGrid.getParentId(HomeGrid.getParentId(b))),l=HomeGrid.getParentId(HomeGrid.getParentId(c)),m=HomeGrid.getParentId(b);if(l==k)if(m!=c){var n=new Array;n=HomeGrid.getAllSubItems(c).split(",");var o=HomeGrid.cells(b,HomeGrid.getColIndexById("id")).getValue(),p=HomeGrid.cells(c,HomeGrid.getColIndexById("id")).getValue(),q=HomeGrid.cells(b,TaskNameColIndex).getValue(),r="";for(a in n)if(""!=n[a].toString()&&null!=n[a].toString()&&(r=HomeGrid.cells(n[a].toString(),TaskNameColIndex).getValue(),r==q))return alert("Tactic with same title already exist in Targeted Program."),!1;ProgarmName=HomeGrid.cells(c,TaskNameColIndex).getValue(),$("#lipname").html(ProgarmName),$("#hdnsourceid").val(o),$("#hdndestid").val(p),$("#divMovetacticPopup").modal("show"),RemoveAllHoneyCombData()}else ProgarmName=HomeGrid.cells(c,TaskNameColIndex).getValue(),alert("Tactic is already in "+ProgarmName+".");else alert("Tactic can move only to same plan program.");return!1}return alert(e+" can not move to "+f),!1}return alert("Only tactic can Move."),!1}}}function SaveMoveTactic(){var a=$("#hdnsourceid").val(),b=$("#hdndestid").val();$.ajax({type:"POST",url:urlContent+"Plan/SaveGridDetail",data:{UpdateType:"tactic",UpdateColumn:"ParentID",UpdateVal:b,Id:parseInt(a)},dataType:"json",success:function(a){HomeGrid.saveOpenStates("plangridState"),LoadPlanGrid()}})}function formatDate(a){function b(a){return a<10?"0"+a:""+a}return a=new Date(a),b(a.getMonth()+1)+"/"+b(a.getDate())+"/"+a.getFullYear()}function SetColumUpdatedValue(a,b){progActVal=HomeGrid.cells(progid,a).getValue(),CampActVal=HomeGrid.cells(campid,a).getValue(),PlanActVal=HomeGrid.cells(planid,a).getValue(),newProgVal=parseInt(ReplaceCC(progActVal.toString()))+parseInt(b),newCampVal=parseInt(ReplaceCC(CampActVal.toString()))+parseInt(b),newPlanVal=parseInt(ReplaceCC(PlanActVal.toString()))+parseInt(b)}function doOnEditCell(a,b,c,d,e){updatetype=HomeGrid.cells(b,ActivitypeHidden).getValue();var f,g,h,i=this.cell.cellIndex,k=HomeGrid.getColType(c);if($(".popover").removeClass("in").addClass("out"),0==a){var l=HomeGrid.cells(b,c).getValue();l.indexOf("</div>")>-1&&(l.split("</div>").length>2?(value=l.split("</div>")[0]+"</div>"+l.split("</div>")[1],TacticName=l.split("</div>")[2]):(value=l.split("</div>")[0],TacticName=l.split("</div>")[1]))}else void 0!=d&&(TacticName=d);AssignParentIds(b),g=HomeGrid.getColumnId(i,0);var k=HomeGrid.getColType(c),m=HomeGrid.cells(planid,GridHiddenId).getValue();if(0==a){var n=HomeGrid.cells(b,c).getAttribute("lo");if(null!=n&&""!=n&&"1"==n)return!1;if("newRow_0"==b)return!1;if(i==TypeColIndex)if(updatetype.toLowerCase()==secLineItem){var o=HomeGrid.getCombo(c);o.clear(),lineItemTypefieldOptionList.length>0&&$.each(lineItemTypefieldOptionList,function(a,b){b.PlanId==m&&o.put(b.id,b.value)})}else{var o=HomeGrid.getCombo(c);o.clear(),tacticTypefieldOptionList.length>0&&$.each(tacticTypefieldOptionList,function(a,b){b.PlanId==m&&o.put(b.id,b.value)})}var p=HomeGrid.getColumnId(c);if(p.indexOf("custom_")>=0){var q=p.replace("custom_",""),r=q.split(":")[0],t=HomeGrid.cells(b,GridHiddenId).getValue();if(GetCustomfieldOptionlist(r,t,c,k),IsDependentTextBox)return!1}opencombobox()}if(1==a){if("clist"==k&&1==$(".dhx_clist input").length)return $(".dhx_clist").css("display","none"),!1;if(IsDependentTextBox)return!1;if(updatetype.toLowerCase()==secLineItem.toLowerCase()||updatetype.toLowerCase()==secTactic.toLowerCase()){var u=HomeGrid.cells(b,c).getValue(),v=HomeGrid.cells(b,c).getAttribute("actval");if(isNaN(parseInt(u))){var w;w=updatetype.toLowerCase()==secLineItem?lineItemTypefieldOptionList:tacticTypefieldOptionList;var x=w.filter(function(a){if(a.PlanId==m&&a.value.trim().toLowerCase().toString()==u.trim().toLowerCase().toString())return a.id});x.length>0&&(v=x[0].id)}else v=u;if(1!=c)if(""==u)$('.dhx_combo_select option[value="'+u+'"]').remove();else{var y=parseInt(u);isNaN(y)?($('.dhx_combo_select option[value="'+u+'"]').remove(),$(".dhx_combo_select").val(v)):$(".dhx_combo_select").val(v)}}if($(".dhx_combo_edit").off("keydown"),g!=PlannedCostId&&g!=TargetStageGoalId||($(".dhx_combo_edit").on("keydown",function(a){GridPriceFormatKeydown(a)}),HomeGrid.editor.obj.onkeypress=function(a){if(a=a||window.event,a.keyCode>=47||0==a.keyCode){var b=this.value;if(b.length>10)return!1}}),g==TargetStageGoalId){var z=HomeGrid.cells(b,TargetStageGoalColIndex).getValue().split(" ");this.editor.obj.value=ReplaceCC(z[0])}if(g==PlannedCostId){var A=HomeGrid.cells(b,PlannedCostColIndex).getValue().replace(CurrencySybmol,"");this.editor.obj.value=ReplaceCC(A.toString())}}if(2==a&&(null!=d.trim()&&""!=d.trim()||g.toString().trim().indexOf("custom_")>=0)){var B="",C=htmlDecode(d),D=HomeGrid.cells(b,GridHiddenId).getValue();if(void 0!=AssetTypeColIndex&&(B=HomeGrid.cells(b,AssetTypeColIndex).getValue()),""!=g&&null!=g||(g=HomeGrid.getColumnId(i,0)),g==TaskNameId&&0==CheckHtmlTag(d))return alert(TitleContainHTMLString),!1;if(1==c&&$("div[taskId='"+D+"']").attr("taskname",C),ExportSelectedIds.TaskID.length>0){var E=ExportSelectedIds.Title.indexOf(e);E>=0&&(ExportSelectedIds.Title[E]=C)}if(f=HomeGrid.cells(b,GridHiddenId).getValue(),g==StartDateId){var F=new Date(HomeGrid.cells(planid,StartDateColIndex).getValue()).getFullYear(),G=HomeGrid.cells(b,EndDateColIndex).getValue();if(!CheckDateYear(d,F,StartDateCurrentYear))return!1;if(!validateDateCompare(d,G,DateComapreValidation))return!1;if(updatetype.toLowerCase()==secProgram.toLowerCase()){var H=HomeGrid.getUserData(b,"tsdate");if(!validateDateCompare(d,H,TacticStartDateCompareWithParentStartDate))return!1}if(updatetype.toLowerCase()==secCampaign.toLowerCase()){var I=HomeGrid.getUserData(b,"psdate"),H=HomeGrid.getUserData(b,"tsdate");if(!validateDateCompare(d,I,ProgramStartDateCompareWithParentStartDate))return!1;if(!validateDateCompare(d,H,TacticStartDateCompareWithParentStartDate))return!1}d=formatDate(d),e=formatDate(e)}if(g==PlannedCostId&&(d=d.replace(CurrencySybmol,""),e=e.replace(CurrencySybmol,"")),g==EndDateId){var J=new Date(HomeGrid.cells(planid,StartDateColIndex).getValue()).getFullYear(),K=HomeGrid.cells(b,StartDateColIndex).getValue();if(!CheckDateYear(d,J,EndDateCurrentYear))return!1;if(!validateDateCompare(K,d,DateComapreValidation))return!1;if(updatetype.toLowerCase()==secProgram.toLowerCase()){var L=HomeGrid.getUserData(b,"tedate");if(!validateDateCompare(L,d,TacticEndDateCompareWithParentEndDate))return!1}if(updatetype.toLowerCase()==secCampaign.toLowerCase()){var M=HomeGrid.getUserData(b,"pedate"),L=HomeGrid.getUserData(b,"tedate");if(!validateDateCompare(M,d,ProgramEndDateCompareWithParentEndDate))return!1;if(!validateDateCompare(L,d,TacticEndDateCompareWithParentEndDate))return!1}d=formatDate(d),e=formatDate(e)}if(g.toString().trim()==TargetStageGoalId){var N=e.split(" ");if(d!=ReplaceCC(N[0])){var P=(HomeGrid.getColIndexById("tactictype"),HomeGrid.getUserData(b,"tactictype"));return GetConversionRate(f,P,g,d,b,d,null),!0}return!1}if(g.toString().trim().indexOf("custom_")>=0){var p=g,q=p.replace("custom_",""),r=q.split(":")[0];if(null==d||void 0==d||d==e)return!1;var R=d.split(",");if(_customFieldValues=[],R.length>0){var S=R.length,T=parseInt(100/S),U=parseInt(100%S),V=S-U,W=0;$.each(R,function(a){W+=1,W<=V&&V!=_customFieldValues.length-1?weight=T:weight=T+1,_customFieldValues.push({customFieldId:r,Value:htmlEncode(R[a]),Weight:weight,CostWeight:weight})})}_customFieldValues=JSON.stringify(_customFieldValues)}if(g==TacticTypeId&&updatetype.toLowerCase()==secTactic.toLowerCase()){var x=tacticTypefieldOptionList.filter(function(a){if(a.PlanId==m&&a.value.trim().toLowerCase().toString()==e.trim().toLowerCase().toString())return a.id});x.length>0&&(e=x[0].id);var P=d,X=$(HomeGrid.getRowById(b)).find("div.honeycombbox-icon-gantt"),Y=tacticTypefieldOptionList,Z=Y.filter(function(a){return a.id==P});if(null!=Z&&Z.length>0&&(Z=Z[0].Type),void 0!=X&&null!=X){var _=X.attr("anchortacticid");if(null!=_&&"0"!=_&&null!=B&&""!=B&&B!=Z&&B.toLowerCase()==AssetTypeAsset.toLowerCase()){var aa=confirm("Package associated to this tactic will be deleted. Do you wish to continue?");if(!aa)return!1}}if("true"==IsMediaCodePermission&&null!=Z&&""!=Z&&B!=Z&&Z.toLowerCase()==AssetTypeAsset.toLowerCase()){var aa=confirm("Media code associated to this tactic will be deleted. Do you wish to continue?");if(!aa)return!1}if(d!=e)return $.ajax({type:"POST",url:urlContent+"Plan/LoadTacticTypeValue",data:{tacticTypeId:P},success:function(a){var c=HomeGrid.cells(b,GridHiddenId).getValue();if(ExportSelectedIds.TaskID.length>0){var e=$("div[taskId='"+c+"']").attr("TacticType"),h=ExportSelectedIds.TacticType.indexOf(e);h>=0&&""!=a.TacticTypeName&&(ExportSelectedIds.TacticType[h]=a.TacticTypeName)}""!=a.TacticTypeName&&null!=a.TacticTypeName&&($("div[taskId='"+c+"']").attr("tactictype",a.TacticTypeName),$("div[taskId='"+c+"']").attr("roitactictype",Z)),pcost=a.revenue;var i=a.stageTitle,j=a.projectedStageValue;parseFloat(j)>0?HomeGrid.cells(b,TargetStageGoalColIndex).setValue(FormatCommas(j.toString(),!1)+" "+i):HomeGrid.cells(b,TargetStageGoalColIndex).setValue(j+" "+i),HomeGrid.setUserData(b,"stage",i),HomeGrid.setUserData(b,"tactictype",P),GetConversionRate(f,P,g,j,b,d,a.stageId)}}),!0}if(updatetype.toLowerCase()==secLineItem.toLowerCase()){if(g==TacticTypeId){var ba=lineItemTypefieldOptionList.filter(function(a){if(a.PlanId==m&&a.value.trim().toLowerCase().toString()==e.trim().toLowerCase().toString())return a.id});ba.length>0&&(e=ba[0].id)}var v=HomeGrid.cells(b,c).getAttribute("actval");if(null!=v&&""!=v||(v=e),d!=e&&d!=v){h=d;var ca=HomeGrid.getAllSubItems(tactid);$.ajax({type:"POST",url:urlContent+"Plan/SaveGridDetail",data:{UpdateType:updatetype,UpdateColumn:g.trim(),UpdateVal:h,Id:parseInt(f),CustomFieldInput:_customFieldValues,ColumnType:k.toString(),oValue:e.toString()},dataType:"json",success:function(a){if(HomeGrid.saveOpenStates("plangridState"),null!=a.errormsg&&""!=a.errormsg.trim())return alert(a.errormsg.trim()),HomeGrid.cells(b,c).setValue(e),!1;if(g==PlannedCostId){diff=parseInt(a.lineItemCost)-parseInt(ReplaceCC(e));for(var f=0;f<ca.split(",").length;f++)"False"!=HomeGrid.getUserData(ca.split(",")[f],"IsOther")&&HomeGrid.cells(ca.split(",")[f],PlannedCostColIndex).setValue(CurrencySybmol+(a.tacticCost-a.lineItemCost));RefershPlanHeaderCalc(),ItemIndex=HomeGrid.getRowIndex(tactid),state0=ItemIndex,HomeGrid.cells(b,PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(d))}if(g!=PlannedCostId&&g!=TaskNameId||a.linkTacticId>0&&LoadPlanGrid(),g.toString().trim().indexOf("custom_")>=0){var h=a.DependentCustomfield;if(null!=h&&void 0!=h)for(var f=0;f<h.length;f++){var i=HomeGrid.getColIndexById(h[f].CustomFieldId);void 0!=i&&""!=i&&HomeGrid.cells(b,i).setValue(h[f].OptionValue)}}}})}return!0}if(htmlDecode(d)!=e){if(g!=TacticTypeId&&g.toString().trim()!=TargetStageGoalId){progid=HomeGrid.getParentId(b),campid=HomeGrid.getParentId(progid),planid=HomeGrid.getParentId(campid);var ca=HomeGrid.getAllSubItems(b);h=d,$.ajax({type:"POST",url:urlContent+"Plan/SaveGridDetail",data:{UpdateType:updatetype,UpdateColumn:g.trim(),UpdateVal:h,Id:parseInt(f),CustomFieldInput:_customFieldValues,ColumnType:k.toString(),oValue:e.toString()},dataType:"json",success:function(a){HomeGrid.saveOpenStates("plangridState");var h=HomeGrid.cells(b,GridHiddenId).getValue(),i=$("div[taskId='"+h+"']").attr("OwnerName");if(ExportSelectedIds.TaskID.length>0){var j=ExportSelectedIds.OwnerName.indexOf(i);j>=0&&""!=a.OwnerName&&(ExportSelectedIds.OwnerName[j]=a.OwnerName)}if(""!=a.OwnerName&&null!=a.OwnerName&&$("div[taskId='"+h+"']").attr("ownername",a.OwnerName),null!=a.errormsg&&""!=a.errormsg.trim())return alert(a.errormsg.trim()),HomeGrid.cells(b,c).setValue(e),!1;if(g==StartDateId){if(a.IsExtended)return alert("Since the Tactic is link to another Plan, it cannot be extended"),HomeGrid.cells(b,c).setValue(e),!1;var k=HomeGrid.cells(b,EndDateColIndex).getValue(),l=new Date(k).getFullYear(),m=new Date(d).getFullYear(),n=l-m,o=new Date(e).getFullYear(),p=o-m;if(n>0){var q=HomeGrid.cells(b,TaskNameColIndex).getValue(),r=q.indexOf("unlink-icon");if(r<=-1){var s="<div class='unlink-icon unlink-icon-grid'><i class='fa fa-chain-broken'></i></div>";HomeGrid.cells(b,TaskNameColIndex).setValue(s+q),$("div[tacticaddId='"+h+"']").attr("linktacticper","True")}}else{if(0==p)return!1;var q=HomeGrid.cells(b,TaskNameColIndex).getValue(),r=q.indexOf("</div>");r>-1&&(HomeGrid.cells(b,TaskNameColIndex).setValue(q.split("</div>")[1]),$("div[tacticaddId='"+h+"']").attr("linktacticper","False"))}ComapreDate(updatetype,b,StartDateColIndex,d,g)}if(g==EndDateId){if(a.IsExtended)return alert("Since the Tactic is link to another Plan, it cannot be extended"),HomeGrid.cells(b,c).setValue(e),!1;var t=HomeGrid.cells(b,StartDateColIndex).getValue(),m=new Date(t).getFullYear(),l=new Date(d).getFullYear(),n=l-m,o=new Date(e).getFullYear(),p=o-m;if(n>0){var q=HomeGrid.cells(b,TaskNameColIndex).getValue(),r=q.indexOf("unlink-icon");if(r<=-1){var s="<div class='unlink-icon unlink-icon-grid'><i class='fa fa-chain-broken'></i></div>";HomeGrid.cells(b,TaskNameColIndex).setValue(s+q),$("div[tacticaddId='"+h+"']").attr("linktacticper","True")}}else{if(0==p)return!1;var q=HomeGrid.cells(b,TaskNameColIndex).getValue(),r=q.indexOf("</div>");r>-1&&(HomeGrid.cells(b,TaskNameColIndex).setValue(q.split("</div>")[1]),$("div[tacticaddId='"+h+"']").attr("linktacticper","False"))}ComapreDate(updatetype,b,EndDateColIndex,d,g)}if(g==PlannedCostId){var u=parseInt(ReplaceCC(d).replace(CurrencySybmol,"")),v=parseInt(ReplaceCC(e).replace(CurrencySybmol,""));if(diff=u-v,SetColumUpdatedValue(PlannedCostColIndex,diff),HomeGrid.cells(progid,PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(newProgVal)),HomeGrid.cells(campid,PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(newCampVal)),HomeGrid.cells(planid,PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(newPlanVal)),HomeGrid.cells(b,PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(u)),null!=ca&&""!=ca&&ca.length>0)for(var w=0;w<ca.split(",").length;w++)"False"!=HomeGrid.getUserData(ca.split(",")[w],"IsOther")&&HomeGrid.cells(ca.split(",")[w],PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(u-a.lineItemCost))}if(g!=PlannedCostId&&g!=TaskNameId||(a.linkTacticId>0&&LoadPlanGrid(),RefershPlanHeaderCalc(),ItemIndex=HomeGrid.getRowIndex(b),state0=ItemIndex),g==OwnerId&&(i.toString()!=a.OwnerName.toString()&&""!=a.OwnerName&&null!=a.OwnerName&&0!=planid&&null!=planid&&void 0!=planid&&(GetTacticTypelist(filters.PlanIDs),GetOwnerListForFilter(filters.PlanIDs),SaveLastSetofViews()),CheckPermissionByOwner(b,d,updatetype,parseInt(f))),g==TaskNameId&&($("#txtGlobalSearch").val(""),$("#ExpClose").css("display","none"),$("#ExpSearch").css("display","block"),GlobalSearch()),g.toString().trim().indexOf("custom_")>=0){var x=a.DependentCustomfield;if(null!=x&&void 0!=x)for(var w=0;w<x.length;w++){var y=HomeGrid.getColIndexById(x[w].CustomFieldId);void 0!=y&&""!=y&&HomeGrid.cells(b,y).setValue(x[w].OptionValue)}}}})}return c==TaskNameColIndex&&(void 0!=value&&"undefined"!=value&&null!=value?HomeGrid.cells(b,c).setValue(value+"</div>"+TacticName):HomeGrid.cells(b,c).setValue(TacticName)),value="",$("div[id^='LinkIcon']").each(function(){bootstrapetitle($(this),"This tactic is linked to <U>"+htmlDecode($(this).attr("linkedplanname")+"</U>"),"tipsy-innerWhite")}),!0}return c==TaskNameColIndex&&(void 0!=value&&"undefined"!=value&&null!=value?HomeGrid.cells(b,c).setValue(value+"</div>"+TacticName):HomeGrid.cells(b,c).setValue(TacticName)),value="",$("div[id^='LinkIcon']").each(function(){bootstrapetitle($(this),"This tactic is linked to <U>"+htmlDecode($(this).attr("linkedplanname")+"</U>"),"tipsy-innerWhite")}),!0}}function AssignParentIds(a){updatetype.toLowerCase()==secTactic?(tactid=a,progid=HomeGrid.getParentId(a),campid=HomeGrid.getParentId(progid),planid=HomeGrid.getParentId(campid)):updatetype.toLowerCase()==secLineItem?(tactid=HomeGrid.getParentId(a),progid=HomeGrid.getParentId(tactid),campid=HomeGrid.getParentId(progid),planid=HomeGrid.getParentId(campid)):updatetype.toLowerCase()==secProgram?(campid=HomeGrid.getParentId(a),planid=HomeGrid.getParentId(campid)):updatetype.toLowerCase()==secCampaign?planid=HomeGrid.getParentId(a):updatetype.toLowerCase()==secPlan&&(planid=a)}function CheckPermissionByOwner(a,b,c,d){$.ajax({type:"POST",url:urlContent+"Plan/CheckPermissionByOwner",data:{NewOwnerID:b,UpdateType:c,updatedid:parseInt(d)},dataType:"json",success:function(b){"1"==b.IsLocked&&(HomeGrid.cells(a,TaskNameColIndex).setAttribute("lo",b.IsLocked),HomeGrid.cells(a,StartDateColIndex).setAttribute("lo",b.IsLocked),HomeGrid.cells(a,EndDateColIndex).setAttribute("lo",b.IsLocked),HomeGrid.cells(a,PlannedCostColIndex).setAttribute("lo",b.IsLocked),HomeGrid.cells(a,AssetTypeColIndex).setAttribute("lo",b.IsLocked),HomeGrid.cells(a,TypeColIndex).setAttribute("lo",b.IsLocked),HomeGrid.cells(a,OwnerColIndex).setAttribute("lo",b.IsLocked),HomeGrid.cells(a,TargetStageGoalColIndex).setAttribute("lo",b.IsLocked),HomeGrid.setCellTextStyle(a,TaskNameColIndex,b.cellTextColor),HomeGrid.setCellTextStyle(a,StartDateColIndex,b.cellTextColor),HomeGrid.setCellTextStyle(a,EndDateColIndex,b.cellTextColor),HomeGrid.setCellTextStyle(a,PlannedCostColIndex,b.cellTextColor),HomeGrid.setCellTextStyle(a,AssetTypeColIndex,b.cellTextColor),HomeGrid.setCellTextStyle(a,TypeColIndex,b.cellTextColor),HomeGrid.setCellTextStyle(a,OwnerColIndex,b.cellTextColor),HomeGrid.setCellTextStyle(a,TargetStageGoalColIndex,b.cellTextColor),HomeGrid.setCellTextStyle(a,MQLColIndex,b.cellTextColor),HomeGrid.setCellTextStyle(a,RevenueColIndex,b.cellTextColor))},error:function(a){}})}function GetConversionRate(a,b,c,d,e,f,g){var j=!0,k=0,l=0;progid=HomeGrid.getParentId(e),campid=HomeGrid.getParentId(progid),planid=HomeGrid.getParentId(campid),k=d,$.ajax({type:"POST",url:urlContent+"Plan/CalculateMQL",data:{tactictid:parseInt(a),TacticTypeId:parseInt(b),projectedStageValue:k,RedirectType:j,isTacticTypeChange:!0,StageID:g},success:function(b){if(void 0!=MQLColIndex&&null!=MQLColIndex){var d=HomeGrid.cells(e,MQLColIndex).getValue(),g=0;if(null!=b.revenue&&(l=b.revenue),"N/A"==b.mql)HomeGrid.setCellExcellType(e,MQLColIndex,"ro"),HomeGrid.cells(e,MQLColIndex).setValue(b.mql),diff=parseInt(-d),SetColumUpdatedValue(MQLColIndex,diff);else{null!=b.mql&&(g=b.mql);var h=g.toString();HomeGrid.cells(e,MQLColIndex).setValue(numberWithCommas(h)),diff=parseInt(g)-parseInt(ReplaceCC(d.toString())),SetColumUpdatedValue(MQLColIndex,diff)}HomeGrid.cells(progid,MQLColIndex).setValue(numberWithCommas(newProgVal),!1),HomeGrid.cells(campid,MQLColIndex).setValue(numberWithCommas(newCampVal),!1),HomeGrid.cells(planid,MQLColIndex).setValue(numberWithCommas(newPlanVal),!1);var i=HomeGrid.cells(e,RevenueColIndex).getValue();HomeGrid.cells(e,RevenueColIndex).setValue(CurrencySybmol+numberWithCommas(parseInt(l).toString())),diff=parseInt(l)-parseInt(ReplaceCC(i.toString()).replace(CurrencySybmol,"")),SetColumUpdatedValue(RevenueColIndex,diff),HomeGrid.cells(progid,RevenueColIndex).setValue(CurrencySybmol+numberWithCommas(newProgVal)),HomeGrid.cells(campid,RevenueColIndex).setValue(CurrencySybmol+numberWithCommas(newCampVal)),HomeGrid.cells(planid,RevenueColIndex).setValue(CurrencySybmol+numberWithCommas(newPlanVal))}$.ajax({type:"POST",url:urlContent+"Plan/SaveGridDetail",data:{UpdateType:"Tactic",UpdateColumn:c.trim(),UpdateVal:f,Id:parseInt(a)},dataType:"json",success:function(a){if(HomeGrid.saveOpenStates("plangridState"),c==TargetStageGoalId){var b=HomeGrid.getUserData(e,"stage");HomeGrid.cells(e,TargetStageGoalColIndex).setValue(FormatCommas(f.toString())+" "+b),RefershPlanHeaderCalc()}if(c==TacticTypeId){HomeGrid.cells(planid,GridHiddenId).getValue();$("#ulTacticType li input[type=checkbox]").each(function(){var a=$(this).attr("id");"checked"!=$(this).attr("checked")&&filters.tempTacticTypeIds.push(a.replace("CbTT",""))}),GetTacticTypelist(filters.PlanIDs,!1),SaveLastSetofViews();var g=HomeGrid.getAllSubItems(e),h=0;null!=a.TacticCost&&"undefined"!=a.TacticCost&&(h=a.TacticCost),PlannedCostColIndex=HomeGrid.getColIndexById(PlannedCostId);var i=HomeGrid.cells(e,PlannedCostColIndex).getValue();if(diff=parseInt(h)-parseInt(ReplaceCC(i.toString())),SetColumUpdatedValue(PlannedCostColIndex,diff),HomeGrid.cells(progid,PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(newProgVal)),HomeGrid.cells(campid,PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(newCampVal)),HomeGrid.cells(planid,PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(newPlanVal)),HomeGrid.cells(e,PlannedCostColIndex).setValue(CurrencySybmol+numberWithCommas(h)),null!=g&&""!=g&&g.length>0)for(var j=0;j<g.split(",").length;j++)"False"!=HomeGrid.getUserData(g.split(",")[j],"IsOther")&&HomeGrid.cells(g.split(",")[j],PlannedCostColIndex).setValue(CurrencySybmol+(h-a.lineItemCost));a.linkTacticId>0&&LoadPlanGrid(),RefershPlanHeaderCalc(),ItemIndex=HomeGrid.getRowIndex(e),state0=ItemIndex}}})}})}function ComapreDate(a,b,c,d,e){var f=new Date(formatDate(d));if(a.toLowerCase()==secTactic){progid=HomeGrid.getParentId(b),campid=HomeGrid.getParentId(progid),planid=HomeGrid.getParentId(campid);var g=HomeGrid.cells(progid,GridHiddenId).getValue(),h=HomeGrid.cells(campid,GridHiddenId).getValue(),i=new Date(formatDate(HomeGrid.cells(progid,c).getValue())),j=new Date(formatDate(HomeGrid.cells(campid,c).getValue())),k=new Date(formatDate(HomeGrid.cells(planid,c).getValue()));if(e==StartDateId){i>f&&HomeGrid.cells(progid,c).setValue(formatDate(d)),j>f&&HomeGrid.cells(campid,c).setValue(formatDate(d)),k>f&&HomeGrid.cells(planid,c).setValue(formatDate(d));var l=HomeGrid.getUserData(progid,"tsdate"),m=HomeGrid.getUserData(campid,"psdate");$.ajax({type:"POST",url:urlContent+"GetMinMaxDate",data:{Parentid:parseInt(h),UpdateType:"Tactic",updatedid:parseInt(g)},dataType:"json",success:function(a){formatDate(a.TactMinDate)!=formatDate(l)&&HomeGrid.setUserData(progid,"tsdate",formatDate(a.TactMinDate)),formatDate(a.TactMinDate)!=formatDate(l)&&HomeGrid.setUserData(campid,"tsdate",formatDate(a.TactMinDate)),formatDate(a.ProgMinDate)!=formatDate(m)&&HomeGrid.setUserData(campid,"psdate",formatDate(a.ProgMinDate))},error:function(a){}})}else if(e==EndDateId){i<f&&HomeGrid.cells(progid,c).setValue(formatDate(d)),j<f&&HomeGrid.cells(campid,c).setValue(formatDate(d)),k<f&&HomeGrid.cells(planid,c).setValue(formatDate(d));var n=HomeGrid.getUserData(progid,"tedate"),o=HomeGrid.getUserData(campid,"pedate");$.ajax({type:"POST",url:urlContent+"Plan/GetMinMaxDate",data:{Parentid:parseInt(h),UpdateType:"Tactic",updatedid:parseInt(g)},dataType:"json",success:function(a){formatDate(a.TactMaxDate)!=formatDate(n)&&HomeGrid.setUserData(progid,"tedate",formatDate(a.TactMaxDate)),formatDate(a.TactMaxDate)!=formatDate(n)&&HomeGrid.setUserData(campid,"tedate",formatDate(a.TactMaxDate)),formatDate(a.ProgMaxDate)!=formatDate(o)&&HomeGrid.setUserData(campid,"psdate",formatDate(a.ProgMaxDate))}})}}else if(a.ToLower()==secProgram){campid=HomeGrid.getParentId(b),planid=HomeGrid.getParentId(campid);var g=HomeGrid.cells(b,3).getValue(),h=HomeGrid.cells(campid,3).getValue(),j=new Date(formatDate(HomeGrid.cells(campid,c).getValue())),k=new Date(formatDate(HomeGrid.cells(planid,c).getValue()));if(e==StartDateId){j>f&&HomeGrid.cells(campid,c).setValue(formatDate(d)),k>f&&HomeGrid.cells(planid,c).setValue(formatDate(d));var m=HomeGrid.getUserData(campid,"psdate"),p=HomeGrid.getUserData(campid,"tsdate");$.ajax({type:"POST",url:urlContent+"Plan/GetMinMaxDate",data:{Parentid:parseInt(h),UpdateType:"Program",updatedid:parseInt(g)},dataType:"json",success:function(a){formatDate(a.TactMinDate)!=formatDate(p)&&HomeGrid.setUserData(campid,"tsdate",formatDate(a.TactMinDate)),formatDate(a.ProgMinDate)!=formatDate(m)&&HomeGrid.setUserData(campid,"psdate",formatDate(a.ProgMinDate))},error:function(a){}})}else if(e==EndDateId){j<f&&HomeGrid.cells(campid,c).setValue(formatDate(d)),k<f&&HomeGrid.cells(planid,c).setValue(formatDate(d));var o=HomeGrid.getUserData(campid,"pedate"),q=HomeGrid.getUserData(campid,"tedate");$.ajax({type:"POST",url:urlContent+"Plan/GetMinMaxDate",data:{Parentid:parseInt(h),UpdateType:"Program",updatedid:parseInt(g)},dataType:"json",success:function(a){formatDate(a.TactMaxDate)!=formatDate(q)&&HomeGrid.setUserData(campid,"tedate",formatDate(a.TactMaxDate)),formatDate(a.ProgMaxDate)!=formatDate(o)&&HomeGrid.setUserData(campid,"pedate",formatDate(a.ProgMaxDate))},error:function(a){}})}}else if(a.toLowerCase()==secCampaign){planid=HomeGrid.getParentId(b);var k=new Date(formatDate(HomeGrid.cells(planid,c).getValue()));e==StartDateId?k>f&&HomeGrid.cells(planid,c).setValue(formatDate(d)):e==EndDateId&&k<f&&HomeGrid.cells(planid,c).setValue(formatDate(d))}}function ExportToExcel(a){var b=[],c=[],d=HomeGrid.getColIndexById("Add"),e=HomeGrid.getColIndexById("ColourCode"),g=(HomeGrid.getColIndexById("MachineName"),HomeGrid.getColIndexById("Type")),h=HomeGrid.getColIndexById("ActivityId");"budget"==gridname.toLowerCase()&&(d=HomeGrid.getColIndexById("Buttons"),e=HomeGrid.getColIndexById("colourcode")),a&&HomeGrid.forEachRow(function(a){var e=HomeGrid.cells(a,d).getValue();e.indexOf("honeycombbox-icon-gantt-Active")<=-1?(HomeGrid.setRowHidden(a,!0),b.push(a)):c.push(a)}),HomeGrid.saveOpenStates("plangridState"),HomeGrid.expandAll();HomeGrid.getColIndexById("ActivityId");if("home"==gridname.toLowerCase())HomeGrid.setColumnHidden(d,!0),HomeGrid.setColumnHidden(e,!0);else{HomeGrid.setColumnHidden(h,!1),HomeGrid.setColumnHidden(g,!1);var k=ColumnIds.split(","),l="";for(i=0;i<k.length;++i)l+=0==i?"MachineName"==k[i]||"LineItemTypeId"==k[i]||"Buttons"==k[i]||"colourcode"==k[i]?"false":"true":"MachineName"==k[i]||"LineItemTypeId"==k[i]||"Buttons"==k[i]||"colourcode"==k[i]?",false":",true";HomeGrid.setSerializableColumns(l)}HomeGrid.toExcel("https://dhtmlxgrid.appspot.com/export/excel"),HomeGrid.collapseAll(),HomeGrid.loadOpenStates("plangridState"),"home"==gridname.toLowerCase()?(HomeGrid.setColumnHidden(d,!1),HomeGrid.setColumnHidden(e,!1)):(HomeGrid.setColumnHidden(h,!0),HomeGrid.setColumnHidden(g,!0)),void 0!=b&&$.each(b,function(a){HomeGrid.setRowHidden(b[a],!1)}),void 0!=c&&$.each(c,function(a){var b=HomeGrid.cells(c[a],d).getValue();b.indexOf("honeycombbox-icon-gantt-Active")<=-1&&HomeGrid.cells(c[a],d).setValue(b.replace("honeycombbox-icon-gantt","honeycombbox-icon-gantt honeycombbox-icon-gantt-Active"))})}function GetCustomfieldOptionlist(a,b,c,e){function i(b){return b.customFieldId==a&&null!=b.ParentOptionId&&b.ParentOptionId.length>0}function k(b){return b.customFieldId==a}var g,f=customfieldOptionList,h=[];if(d=f.filter(i),IsDependentTextBox=!1,null!=d&&d.length>0){var j=[];$.each(d,function(a,b){j.indexOf(b.ParentOptionId[0])<0&&j.push(b.ParentOptionId[0])}),$.ajax({url:urlContent+"Plan/GetdependantOptionlist/",traditional:!0,async:!1,data:{customfieldId:a,entityid:b,parentoptionId:j,Customfieldtype:e},success:function(a){return"ed"==e&&"true"==a.IstextBoxDependent.toString().toLowerCase()?(IsDependentTextBox=!0,!1):(null!=a&&null!=a.optionlist&&a.optionlist.length>0&&(g=a.optionlist),void(null!=g&&g.length>0&&void 0!=g?($.each(g,function(a,b){h.indexOf(b.value)==-1&&h.push(b.value)}),HomeGrid.registerCList(c,h)):HomeGrid.registerCList(c,h)))}})}else g=f.filter(k),$.each(g,function(a,b){h.push(b.value)}),HomeGrid.registerCList(c,h)}function opencombobox(){var a=$(".rowselected td").offset().top,b=$(".objbox").height(),c=a-b;c>200?$("body").addClass("reverse"):$("body").removeClass("reverse")}var eventiddrag=0,eventidonedit=0,eventidonbeforedrag=0,eventidonscroll=0,editidonOpenEnd=0,updatetype=0,progid=0,campid=0,planid=0,tactid=0,progActVal=0,CampActVal=0,PlanActVal=0,TactActVal=0,diff=0,newProgVal=0,newCampVal=0,newPlanVal=0,newTactVal=0,value,TacticName,ExportToCsv=!1,_customFieldValues=[],IsDependentTextBox=!1,NodatawithfilterGrid='<div id="NodatawithfilterGrid" style="display:none;"><span class="pull-left margin_t30 bold " style="margin-left: 20px;">No data exists. Please check the filters or grouping applied.</span><br/></div>',$doc=$(document);
$doc.click(function(){$("#popupType").css("display","none"),$("#dhx_combo_select").css("display","none"),$(".dhx_clist").css("display","none")}),$(document).mouseup(function(a){$("#popupType").css("display","none"),$("#dhx_combo_select").css("display","none")}),$(".grid_ver_scroll").scroll(function(){$("#popupType").css("display","none"),$(".dhx_clist").css("display","none")});
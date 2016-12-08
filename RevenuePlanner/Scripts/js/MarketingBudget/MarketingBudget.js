//Added By Jaymin Modi at 01/Dec/2016. For Maintain States of Row.Ticket:-2806
function createCookie(name, value, days) {
    
    if (days) {

        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "; expires=" + date.toGMTString();
    }
    else var expires = "";
    document.cookie = name + "=" + value + expires + ";";
}

/** //Added By Jaymin Modi at 01/Dec/2016. For Maintain States of Row.Ticket:-2806
* Checks if there is saved open state in a cookie.
* If there isn't, then just returns the grid data unmodified (keeping whatever "open" settings it has)
* If there is, then the grid data is modified so that is open settings match what was in the cookie
* Call this when you have a bunch of grid data you are getting ready to load into the grid and want to
* prep the openstates before initial render
*/
function UpdateGridDataFromSavedOpenState(rows, name) {
    var cookie = dhtmlXGridObject.prototype.getCookie(name, "gridOpen");

    if (cookie) {

        // construct a dictionary of the IDS that are open
        var isOpen = Object.create(null);

        var parts = cookie.split("|");

        var numParts = parts.length;
        for (var i = 0; i < numParts; ++i) {
            isOpen[parts[i]] = "1";
        }

        // Now walk the data and set the open property
        function processRows(rows) {
            var n = rows.length;
            for (var i = 0; i < n; ++i) {
                processItem(rows[i]);
            }
        }

        function processItem(item) {
            if (item) {
                item.open = item.id && isOpen[item.id];
                if (Array.isArray(item.rows)) {
                    processRows(item.rows);
                }
            }
        }

        if (Array.isArray(rows)) {

            processRows(rows);
        }
    }

}

// Delete budget items once user has click the proceed button from delete confirmation popup.
$('#proceed-button_DeleteItem').on("click", function () {
    DeleteBudget();
    $('#divDeletePopup').modal("hide");
});

// Bind the Grid data on timeframe value change.
$("#ddlMainGridTimeFrame").change(function () {
    ShowHideSuccessDiv();   // Hide "Success" message if it's already appeared on screen.
    GetGridData();          // Load data to Grid on timeframe value change
});

// function to show hide success message
function ShowHideSuccessDiv() {
    var sucesstextmessagespan = $("#spanMsgSuccess");
    var sucesstextmessagediv = $("#SuccessMsg");
    if (sucesstextmessagespan != undefined && sucesstextmessagespan != null) {
        sucesstextmessagespan.text('')
        if (sucesstextmessagediv != undefined && sucesstextmessagediv != null) {
            sucesstextmessagediv.css('display', 'none');
        }
    }
}

// Created by Komal
// Desc: To bind "filter columns" dropdown list based on user has select "View By" value under View by dropdown list.
// Parameter:
// ColumnSetId:- View By value that user has select from View by dropdown list.
function BindColumnsfilter(ColumnSetId) {
    $.ajax({
        url: urlContent + "MarketingBudget/GetColumns/",
        data: { ColumnSetId: ColumnSetId },   // View By value that user has select from View by dropdown list.
        dataType: "json",
        success: function (result) {

            // Clear existing columns data from "filter columns" dropdown list prior to bind new columns.
            $("#budget-select").html("");

            if (result != null && result != 'undefined') {

                // Get BudgetGrid column list to bind on dropdown list.
                var data = result.Columnlist;

                // Get standard columns(i.e.  Budget, Forecast, Planned, Actual, User, Owner columns) list from result and assigned to global variable "standardcolumns" for further process.
                standardcolumns = result.standardlist;


                // Append items to 'Filter Columns' to dropdown list & Check-Uncheck Filter columns.
                BindFilterColumnsAndCheckUncheck(data);
            }
        }
    });
}

//Created by Komal
//Desc: Append items to 'Filter Columns' to dropdown list.
// Set 'Forecast' item to uncheck and rest of are checked.
function BindFilterColumnsAndCheckUncheck(data) {

    // Add each column value to "filter columns" dropdown list.
    for (var i = 0; i < data.length; i++) {
        $('#budget-select').append("<option value=" + data[i].Value + " id='" + data[i].Text + "'>" + data[i].Text + "</option>");
    }

    // Declare "filter columns" dropdown list as multiselect checkbox list and also define title and noneSelectedText while user has not select any record.
    $('#budget-select').multiselect({ multiple: true, title: 'Filter Columns', noneSelectedText: "Filter Columns", });

    // Get checkbox list of "filter columns"
    var ColumnsCheckBox = $("#multipleselect_budget-select").find('label[class^="ui-corner-all"]');

    // Set 'Forecast' item to uncheck.
    if (ColumnsCheckBox != null && ColumnsCheckBox != 'undefined' && ColumnsCheckBox.length > 0) {
        // select all "filter columns" list checkboxes.
        ColumnsCheckBox.find("input").attr("aria-selected", "true").attr("checked", "checked");

        // Get "Forecast" column checkbox record from "filter columns" list
        var objForecast = ColumnsCheckBox.find('input[id="ui-multiselect-Forecast"]');

        // check whether "Forecast" column exist or not.
        if (objForecast != null && objForecast != 'undefined') {

            // Uncheck "Forecast" column from "filter columns" list.
            objForecast.removeAttr('checked');
            objForecast.attr("aria-selected", "false");
        }
    }

    // Select/UnSelect specific columns while user click on that specific column under "filter columns" dropdown list.
    $("#multipleselect_budget-select").find('label[class^="ui-corner-all"]').click(function () {
        HideShowColumns();
    });

    // Select every columns while user click on "Select All" option under "filter columns" dropdown list.
    $("#selectAll_budget-select").click(function () {
        $("#multipleselect_budget-select").find('label[class^="ui-corner-all"]').find("input").prop('checked', 'checked');
        HideShowColumns();
    });

    // Deselect every columns while user click on "Deselect All" option under "filter columns" dropdown list.
    $("#deselectAll_budget-select").click(function () {
        $("#multipleselect_budget-select").find('label[class^="ui-corner-all"]').find("input").removeAttr('checked');
        HideShowColumns();
    });

}

// Created by Komal
// Desc: Get Budget Grid data from server and bind it based on below parameters.
// BudgetId: Get budget data based on user has selected from Budget dropdown list.
   // TimeFrame: Render Budget Grid columns by timeframe value(i.e. Monthly, Quearterly, This Year etc. )
function GetGridData(budgetId) {

        $('#ddlMainTimeFrameSelectBox').css('display', 'block');
        $('#ddlColumnSetFrameBox').css('display', 'block');
        $('#ddlColumnsFrameBox').css('display', 'block');
        $('#dvExportToExcel').css('display', 'block');

    var BudgetId = "";

    // Check that "budgetId" is null or not.
    if (budgetId != null && budgetId != "undefined" && budgetId != "") {
        BudgetId = budgetId;
    }
    else {
        BudgetId = $('#ddlParentFinanceMain').val();    // If "budgetId" is null then get from Budget dropdown list.
    }

    var mainTimeFrame = $('#ddlMainGridTimeFrame').val();   // Get Timeframe value that user has select under Time Frame dropdown list.
    var CheckedColumns = "";

    // Show the "Select Budget" message if user has not selet any budget value under dropdown list
    if (BudgetId == null || BudgetId == 'undefined' || BudgetId <= 0) {
        ShowMessage(true, "Please select Budget.");
    }

    // Show the "Select Time Frame" message if user has not selet any timeframe value under dropdown list
    if (mainTimeFrame == null || mainTimeFrame == 'undefined' || mainTimeFrame <= 0) {
        ShowMessage(true, "Please select Time Frame.");
    }

    $.ajax({
        url: urlContent + "MarketingBudget/GetBudgetData/",
        data: {
            BudgetId: BudgetId,
                TimeFrame: mainTimeFrame,
        },
        success: function (data) {

            budgetgrid = new dhtmlXGridObject('gridbox');   // Create object of DHTMLXTreeGrid.
            budgetgrid.setImagePath(urlContent + "codebase/imgs/");    // Get necessary images to DHTMLXTreeGrid from specific path.
            budgetgrid.setImageSize(1, 1);
            budgetgrid.enableAutoHeight(false);      // To enable the system to set Grid hight automatically by set true properties "enableAutoHeight" of DHTMLXTreeGrid.
            budgetgrid.enableAutoWidth(false);      // To disable the system to set Grid width automatically by set false properties "enableAutoWidth" of DHTMLXTreeGrid.

            // To show the two header rows (ex. 1st. Q1,Q2,Q3,Q4 & 2nd. Budget,Forecast,Planned,Actual) in the case of user has select "Monthly/Quarterly" timeframe values.
            // In case of "Yearly" timeframe, show the default view of Budget Grid with default columns(i.e. Budget,Forecast,Planned,Actual,User,Owner & LineItemCount)
            if (mainTimeFrame != Yearly) {
                budgetgrid.attachHeader(data.AttacheHeader);
            }
            budgetgrid.init();
            BudgetGridData = data.GridData;
            var rows = BudgetGridData.rows;
            UpdateGridDataFromSavedOpenState(rows, "budgetgridState");//Added By Jaymin Modi at 01/Dec/2016. For Maintain States of Row.Ticket:-2806
            budgetgrid.parse(BudgetGridData, "json");       // Load data to DHTMLXTreeGrid.
            colIdIndex = budgetgrid.getColIndexById('Id');  // Get the index of "Id" hidden column to refer it else to access this column.
            ColTaskNameIndex = budgetgrid.getColIndexById('Name');  // Get the index of "Name" column to refer it else to access this column.
            colOwnerNameIndex = budgetgrid.getColIndexById('Owner');  // Get the index of "Owner" column to refer it else to access this column.
            colIconIndex = budgetgrid.getColIndexById('Add Row');   // Get the index of "Add Row" column to refer it else to access this column.
            budgetgrid.setColumnHidden(colIdIndex, true)            // Hide the "Id" column by enable the DHTMLXTreeGrid property "setColumnHidden".
            HideShowColumns();  // Show/Hide the BudgetGrid columns to show default columns while load Grid 1st time.
            
            //Added By Jaymin Modi at 01/Dec/2016 For Saving Open and Close States.Ticket:-2806 
            var cookieBudgetgridState = dhtmlXGridObject.prototype.getCookie("budgetgridState", "gridOpen");
            if (cookieBudgetgridState == null) {
                budgetgrid.expandAll();
            }
            budgetgrid.attachEvent("onOpenEnd", function () {
                budgetgrid.saveOpenStates("budgetgridState");
                // get scroll set of selected row from grid to calendar

                scrollstate = {
                    y: budgetgrid.objBox.scrollTop,
                    x: budgetgrid.objBox.scrollLeft,
                }

                return true;
            });
            
            //Maintain State of Scroll Position
            if (scrollstate != null && scrollstate != undefined) {
                budgetgrid.objBox.scrollTop = scrollstate.y;
            }
            //-----------------End--------------------------
            // Declare the "EditCell" event of DHTMLXTreeGrid.
                budgetgrid.attachEvent("onEditCell", OnEditMainGridCell);
            }
        });

    }

    //Function to manage dit event on main grid
    function OnEditMainGridCell(stage, rId, cInd, nValue, oValue) {
                var ColumnId = budgetgrid.getColumnId(cInd);            // Get column index.
                var locked = budgetgrid.getUserData(rId, "lo");         // Get "lo"(i.e. row locked or not) property to identify that row is locked or not.
                var Permission = budgetgrid.getUserData(rId, "per");    // Get user permission "per" property.
                var Period = budgetgrid.getColLabel(cInd, 0);
                // Doesn't allow the user to edit while cell is locked and doesn't have edit permission
                if (locked == 1 && (Permission == "View" || Permission == "None")) {
                    budgetgrid.cells(rId, cInd).setDisabled(true);
                    return false;
                }
                    // Doesn't allow the user to edit Planned and Actual coulumn value.
                else if (locked == 1 && (ColumnId.indexOf('Budget') >= 0 || ColumnId.indexOf('Forecast') >= 0)) {
                    budgetgrid.cells(rId, cInd).setDisabled(true);
                    return false;
                }

                // Enable or disable to edit "Task Name" column to user by read "isTitleEdit" property.
                if (cInd == ColTaskNameIndex) {
                    var isTitleEdit = budgetgrid.getUserData(rId, "isTitleEdit");
                    if (isTitleEdit == "0") {
                        budgetgrid.cells(rId, ColTaskNameIndex).setDisabled(true);
                        return false;
                    }
                }
    if (stage == 1) {       
        
        if (ColumnId.split('_').length > 1) {
            ColumnId = ColumnId.split('_')[1];
        }
       
        if (ColumnId == BudgetColumn || ColumnId == ForecastColumn) {
            $(".dhx_combo_edit").off("keydown");
            $(".dhx_combo_edit").on('keydown', (function (e) { GridPriceFormatKeydown(e); }));
            budgetgrid.editor.obj.onkeypress = function (e) {
                e = e || window.event;
                if ((e.keyCode >= 47) || (e.keyCode == 0)) {
                    var text = this.value;
                    if (text.length > 10) { //max length of the text
                        return false;
                    }
                }
            }

            var psv = budgetgrid.cell;
            this.editor.obj.value = (psv.title.replace(/,/g, ""));
            var actualcost = budgetgrid.cells(rId, cInd).getValue().replace(CurrencySybmol, '');
            this.editor.obj.value = (ReplaceCC(actualcost.toString()));
        } else if (ColumnId == "Name") {

            $(".dhx_combo_edit").on('keydown', (
            budgetgrid.editor.obj.onkeypress = function (event) {
                var text = this.value;
                if (event.keyCode == 8 || event.keyCode == 46
                 || event.keyCode == 37 || event.keyCode == 39) {

                    return true;
                }
                else if (text.length > 250) { //max length of the text
                    return false;
                }
                else { return true; }

            }));
        }
    }
    
    if (nValue != oValue && stage.toString() == '2') {       
        var budgetDetailId = '', parentId = '';
            var itemIndex = -1;
            if (_row_parentId != null && _row_parentId.length > 0) {
                splitRowParentIds = _row_parentId.split(',');
                $.each(splitRowParentIds, function (key, val) {
                budgetDetailId = val.split('~')[0]; //get row id from the global id "_row_parentid"
                    parentId = val.split('~')[1]; //get parent id from the global id "_row_parentid"

                    itemIndex = key;
                if (budgetDetailId == rId) {
                        return false;
                    }
            });
        }
        else {
            splitRowParentIds = rId.split('_');
            if (splitRowParentIds.length > 1) {
                budgetDetailId = splitRowParentIds[1];
                parentId = splitRowParentIds[2];
            }
            }

            if (nValue == null || nValue == '' || nValue.replace(/&lt;/g, '<').replace(/&gt;/g, '>') == oValue) {
                return false;
            }
            else {
                    var _budgetIdVal = $("#ddlParentFinanceMain").val();
            if (budgetDetailId == rId) {
                
                    SaveNewBudgetDetail(_budgetIdVal, nValue, parentId); // save data when we add new item/child item
                    _isNewRowAdd = false;
                }
            else {
                if (rId != null && rId != 'undefined' && rId != '') {
                    var ownerId = budgetgrid.cells(rId, colOwnerNameIndex).getValue();
                    var childItems = budgetgrid.getAllSubItems(rId);                    
                    UpdateBudgetDetail(_budgetIdVal, budgetDetailId, parentId, nValue, childItems, ColumnId, Period);


                }
            }
        }
    }
}
    //Added by - Komal rawal
    //To save budget detail when we add any new item/child item
    function SaveNewBudgetDetail(budgetId, budgetDetailName, parentId) {
        var mainTimeFrame = $('#ddlMainGridTimeFrame').val();
        $.ajax({
            url: urlContent + "MarketingBudget/SaveNewBudgetDetail/",
            dataType: 'json',
            data: {
                BudgetId: budgetId,
                BudgetDetailName:budgetDetailName,
                ParentId: parentId,
                mainTimeFrame: mainTimeFrame,
            },
            success: function (data) {
                GetGridData(budgetId); //refresh grid once we add any new item
            _row_parentId = "";
        }
    });
}
function UpdateBudgetDetail(budgetId, budgetDetailId, parentId, nValue, childItems, columnId, period) {    
    var mainTimeFrame = $('#ddlMainGridTimeFrame').val();
    $.ajax({
        type: 'POST',
        url: urlContent + "MarketingBudget/UpdateMarketingBudget/",
        dataType: 'json',
        data: {
            BudgetId: budgetId,
            BudgetDetailId: budgetDetailId,
            ParentId: parentId,            
            nValue: nValue,
            ChildItemIds: childItems,
            ColumnName: columnId,
            AllocationType: mainTimeFrame,
            Period: period
        },
        beforeSend: function (x) {
            myApp.hidePleaseWait();
        },
        success: function (data) {
            //TODO : grid is update only user change the owner.
            GetGridData(budgetId); //refresh grid once we update any new item
            UpdateFinanceHeaderValues();
        }
    });

}
function DeleteBudgetIconClick(data) {

    // Added By Jaymin Modi To get scroll set of selected row from grid
    scrollstate = {
        y: budgetgrid.objBox.scrollTop,
        x: budgetgrid.objBox.scrollLeft,
    }

    var LineItemCount = $(data).attr('licount');
    //Check blank row if user clicks on new row delete icon then delete popup not display
    if (AddNewrow == false) {
        return false;
    }
    else {
        if (LineItemCount != 0) {
            $('#LiWarning').css('display', 'block');
        }
        else {
            $('#LiWarning').css('display', 'none');
        }
        var BudgetRowId = $(data).attr('row-id').split('_')[1];
        var Name = $(data).attr('name');
        $("#lipname").html(Name);
        $("#divDeletePopup").modal('show');
        SelectedBudgetId = BudgetRowId;
        $('#cancel-button_DeleteItem').on("click", function () {
            SelectedBudgetId = "";
            $('#divDeletePopup').modal("hide");

        });
    }
}

function DeleteBudget() {
    var currentBudgetId = $("#ddlParentFinanceMain").val();
    $.ajax({
        type: 'GET',
        url: urlContent + "MarketingBudget/DeleteBudgetData/",
        dataType: 'json',
        data: {
            SelectedBudgetId: SelectedBudgetId,
            BudgetId: currentBudgetId,
        },
        success: function (data) {
 if (data.IsSuccess == false) {
                ShowMessage(true, data.ErrorMessage);
            }
            else {
                var BudgetId = data.budgetId;
                UpdateFinanceHeaderValues(); // Update header values
                GetGridData(BudgetId);
                RefreshBudgetDropdown(false, BudgetId);
                //TODO :  here we need to call Finance Header function to refresh the header after deleting budget data
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            // Handle error
            //alert(errorThrown);
        }
    });
}

    function RefreshBudgetDropdown(BudgetId) {
    var _budgetId = "";
    if (BudgetId != undefined && BudgetId != "") {
        _budgetId = BudgetId.toString();
    }
    else {
        _budgetId = $('#ddlParentFinanceMain').val();
    }

    var $Budgetdropdown = $("#ddlParentFinanceMain");
    $Budgetdropdown.empty();
    var $html = '';

    $.ajax({
        type: 'POST',
        url: urlContent + "MarketingBudget/RefreshBudgetList/",
        data: {},
        async: false,
        success: function (result) {
            if (result.length > 0) {
                $.each(result, function (index, item) {

                    $html += '<option value="' + item.Value + '">' + item.Text + '</option>';

                });
            }
        }
    });
    $Budgetdropdown.append($html);
    var BudgetOptions = {
        values: []
    }

    $.each($("#ddlParentFinanceMain").find("option"), function () {
        BudgetOptions.values.push($(this).val())
    });
    var CheckBudgetIdExist = BudgetOptions.values.indexOf(_budgetId);
    if (CheckBudgetIdExist >= 0) {
        $("#ddlParentFinanceMain option[value='" + _budgetId + "']").attr("selected", "selected");
    }
    $Budgetdropdown.val(_budgetId);
    $Budgetdropdown.find("input[type='search']").val(_budgetId);
    nlformParentFinanceMain = new NLForm(document.getElementById('nl-formParentFinanceMain'));

    $('#nl-formParentFinanceMain > div[class="nl-field nl-dd"]').find('li').click(function (e) {
        $('#errorMsg').css('display', 'none');
        $('#SuccessMsg').css('display', 'none');
        $("#divGridView").show();
        $("#dvNoData").hide();
        var budgetId = $(this).attr('value');
        if (budgetId != null && budgetId != 'undefined' && budgetId != '0') {
            UpdateFinanceHeaderValues(); // Update header values
            GetGridData(budgetId);
            //TODO : Call Finance Header Function.
            $("#dvExportToExcel").css("display", "block");
        }
        else {
            $("#divGridView").hide();
            $("#dvNoData").show();
            $("#dvExportToExcel").css("display", "none");
        }
        $('#hdn_BudgetId').val(budgetId);
        $('#nl-formParentFinanceMain .nl-field-toggle').text($(this).text());
    });
    var NlForms = $("#nl-formParentFinanceMain .nl-field.nl-dd");

    for (var i = 0; i < NlForms.length - 1 ; i++) {
        $(NlForms[i]).remove();
    }

    if ($('#nl-formParentFinanceMain .nl-field-toggle').text().length > 30) {
        $('#nl-formParentFinanceMain .nl-field-toggle').text().substring(0, 30) + '...';
    }
}
// Open modal popup for import file
function showModal() {
    ShowHideSuccessDiv();
    LoadInputModelBox();
    $('#ImportModal').modal('show');
}
function ExportToExcel() {
    ShowHideSuccessDiv();
    budgetgrid.setColumnHidden(colIdIndex, false);
    budgetgrid.setColumnHidden(colIconIndex, true);
    budgetgrid.toExcel("https://dhtmlxgrid.appspot.com/export/excel");
    budgetgrid.setColumnHidden(colIdIndex, true);
    budgetgrid.setColumnHidden(colIconIndex, false);
}

// #2804 File input popup function for import finance marketing budget
function LoadInputModelBox() {
    var importurl = urlContent + "MarketingBudget/ExcelFileUpload";
    $('.fileinput-upload-button').hide();
    $('#ImportModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
        $('#input-43').fileinput('clear');
    });
    $('#input-43').off('fileclear').on('fileclear', function (event) {
        $('.fileinput-upload-button').hide();
    });

    $("#input-43").fileinput({
        type: 'POST',
        showPreview: false,
        uploadUrl: importurl,
        allowedFileExtensions: ["xls", "xlsx"],
        msgInvalidFileExtension: 'Incorrect file format. Please export the file and use that file to upload your changes',
        elErrorContainer: "#errorBlock"
    });

    $('#input-43').off('filepreupload').on('filepreupload', function (event, data, previewId, index) {
        myApp.showPleaseWait();
    });

    $('#input-43').off('fileuploaded').on('fileuploaded', function (event, data, previewId, index) {
        if (data.response.conflict == true) {
            ShowMessage(false, data.response.message);
        }

        var sucesstextmessagespan = $("#spanMsgSuccess");
        var sucesstextmessagediv = $("#SuccessMsg");
        if (sucesstextmessagespan != undefined && sucesstextmessagespan != null) {
            sucesstextmessagespan.text('file imported successfully.')
            if (sucesstextmessagediv != undefined && sucesstextmessagediv != null) {
                sucesstextmessagediv.css('display', 'block');
            }
        }
        myApp.hidePleaseWait();
        $('#ImportModal').modal('hide');
        UpdateFinanceHeaderValues(); // Update header values
        GetGridData();
    });

    $('#input-43').off('fileloaded').on('fileloaded', function (event, file, previewId, index, reader) {
        $('.fileinput-upload-button').show();
    });
    $('#input-43').off('fileuploaderror').on('fileuploaderror', function (event, data, msg) {
        $('.fileinput-upload-button').hide();
    });
}


function HideShowColumns() {
    var columnid = 0;
    var Title = "";
    var Hidecol = [];
    var ColumnsCheckBox = $("#multipleselect_budget-select label[class^=ui-corner-all] input:not(:checked)");


    for (var i = 0; i < budgetgrid.getColumnsNum() ; i++) {
        columnid = budgetgrid.getColumnId(i);
        if (columnid != Id && columnid != Name
                     && columnid != "Add Row" && columnid != Lineitems) {
            $.each(ColumnsCheckBox, function () {
                Title = $(this).parent().find('span').attr('title');
                if (standardcolumns.indexOf(Title) >= 0) {
                    if (columnid.indexOf(Title) >= 0) {
                        Hidecol.push(i);
                    }

                }
                else {
                    if (columnid == Title) {
                        Hidecol.push(i);
                    }

                }
            });
            budgetgrid.setColumnHidden(i, false)
        }

    }

    if (Hidecol != null && Hidecol != undefined && Hidecol.length > 0) {
        $.each(Hidecol, function (index, val) {
            budgetgrid.setColumnHidden(val, true)
        });

    }

}

// Function to update header values for finance
function UpdateFinanceHeaderValues() {
    // Get selected budget id
    var BudgetId = $('#ddlParentFinanceMain').val();
    $.ajax({
        type: 'GET',
        url: urlContent + "MarketingBudget/GetFinanceHeaderValues/",
        dataType: 'json',
        data: { BudgetId: BudgetId },
        success: function (data) {

            // update header values with currency symbol(decoded symbol)
            $("#budgetID").text($('</div>').html(CurrencySymbol).text() + data.Budget);
            $("#ForecastID").text($('</div>').html(CurrencySymbol).text() + data.Forecast);
            $("#PlannedID").text($('</div>').html(CurrencySymbol).text() + data.Planned);
            $("#ActualID").text($('</div>').html(CurrencySymbol).text() + data.Actual);

            // format values and apply bootstrap tooltip
            ApplyFormattingAndTooltip("#budgetID");
            ApplyFormattingAndTooltip("#ForecastID");
            ApplyFormattingAndTooltip("#PlannedID");
            ApplyFormattingAndTooltip("#ActualID");
        }
    });
}

// Function to format values and apply bootstrap tooltip
function ApplyFormattingAndTooltip(idName) {
    var originalValue = $(idName).html();
    if (typeof (maxsize) === 'undefined') maxsize = 5;
    $(idName).popover('destroy');
    if (originalValue != undefined && originalValue != null && originalValue != '') {
        $(idName).attr("data-original-title", originalValue);
        setBootstrapTooltip(idName, originalValue, maxsize, true); // Apply tooltip
    }
}



    function AddRow(cntrl) {
        var attrRowId = $(cntrl).attr('row-id');
        var rowIndex = budgetgrid.getRowIndex(attrRowId);
        DisplayPopUpMenu(cntrl, rowIndex);
    }

    function DisplayPopUpMenu(addControl, rowIndex) {
        //Add 2 options in the poup menu i.e new item , new child item
        var ul, newItemList = '';
        if (rowIndex > 0) { // if parent item then there will be just one option i.2 new child item
            newItemList = "<li class='new-finance new-prog' id='newFinanceItem' itemType='parallelitem'>New Item</li>";
        }
        ul = "<ul style='margin: 0;'>  " + newItemList + " <li class='new-finance new-prog' id='newFinacneChildItem' itemType='childitem'>New Child Item</li>  </ul>";


        $('#popupType').css('display', 'block');
        $('#popupType').html(ul);

        //Set position of the popup
        var left = $(addControl).position().left + 45;//e.pageX;
        var targetOffset = $(addControl).offset().top;
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
        //end

        $('.new-finance').click(function () {
            var itemtype = $(this).attr('itemType');
            AddNewRowbyType(itemtype, addControl); //adds new item at required position
            $('#popupType').css('display', 'none');
        });

    }

    $(document).click(function () {
        $('#popupType').css('display', 'none');
    });

    $(document).mouseup(function (e) {
        $('#popupType').css("display", "none");
    });



    function AddNewRowbyType(itemType, cntrl) {

        if (_isNewRowAdd == false) { // checks if an new item is already added then dont add another.
            var row_id = $(cntrl).attr('row-id');
            var childrencount = budgetgrid.hasChildren(row_id); // Get Current Row Children count.
            var insertrowindex = childrencount + 1; // get the index at which the new item is to be placed
            /*Start: Get ParentId and BudgetDetailId from RowId */
            var budgetDetailId = 0;
            var isRootMostParent = IsRootMostParentId(row_id);
            if (row_id !== null && row_id !== 'undefined' && row_id !== '') {

                if (itemType.toLowerCase() == 'parallelitem' && !isRootMostParent) {
                    var curntRowParentId = budgetgrid.getParentId(row_id);
                    var arr = curntRowParentId.split('_'); // split thecurrent row id and get the parent id for the new record to be inserted
                    if (arr !== null && arr !== 'undefined' && arr.length > 0) {
                        if (arr[1] != null && arr[1] != 'undefined') {
                            _newParentId = budgetDetailId = arr[1];
                        }
                    }
                }
                else if (itemType.toLowerCase() == 'childitem') {
                    var arr = row_id.split('_'); // split thecurrent row id and get the parent id for the new record to be inserted
                    if (arr !== null && arr !== 'undefined' && arr.length > 0) {
                        if (arr[1] != null && arr[1] != 'undefined') {
                            _newParentId = budgetDetailId = arr[1];
                        }
                    }
                }
                else {
                    _newParentId = budgetDetailId = 0;
                }
            }
            // Create RowId for newRecord.
            _newrowid = "new" + insertrowindex.toString() + "_0_" + _newParentId;  // RowId format: NameofItem_BudgetDetailId_ParentId.
            _row_parentId = _row_parentId + "," + _newrowid + "~" + _newParentId + "~" + row_id; // Store NewRowId & ParentRowId in global variable This variable use to save data in OnEditCell event.
            _isNewRowAdd = true;

            var TotalColumn = budgetgrid.getColumnCount();
            var AddRowString = [];

            //set properties for the newly created item i.e only task name will be editable rest all will be read only
            var AddRowColTypes = "ro,tree,ro";
            var ColumnsVisibility = "";
            AddRowString.push("");
            AddRowString.push("New item");
            AddRowString.push("");
            for (var k = 0; k < TotalColumn; k++) {
                ColumnsVisibility += budgetgrid.isColumnHidden(k) + ",";
                if (k > 2 && k < (TotalColumn - 3)) {
                    AddRowString.push("0");
                    AddRowColTypes += ",ro";
                }
            }
            AddRowString.push("0 | Edit");
            AddRowString.push("0");
            AddRowString.push("User");
            AddRowColTypes += ",ro,ro,ro";
            ColumnsVisibility = ColumnsVisibility.slice(0, -1);
            budgetgrid.setColTypes(AddRowColTypes);
            budgetgrid.setColumnsVisibility(ColumnsVisibility); // set column visiblity
            //end 

            if (itemType.toLowerCase() == 'childitem') {
                budgetgrid.addRow(_newrowid, AddRowString, -1, row_id);
            }
            else if (isRootMostParent) {
                budgetgrid.addRow(_newrowid, AddRowString);
            }
            else {
                // Insert record at Parallel Level.
                var curntRowParentId = budgetgrid.getParentId(row_id);
                budgetgrid.addRow(_newrowid, AddRowString, -1, curntRowParentId);
            }

            var titleIndex = budgetgrid.getColIndexById('title');
            budgetgrid.setCellTextStyle(_newrowid, titleIndex, "border-right:0px solid #d4d4d4;");
            budgetgrid.openItem(row_id);
            var _newRowIndex = budgetgrid.getRowIndex(_newrowid.toString());
            window.setTimeout(function () {
                budgetgrid.selectCell(_newRowIndex, 0, false, false, true, true);
                budgetgrid.editCell();
            }, 1);
        }
    }

    function IsRootMostParentId(curntRowid) {
        var parentid = budgetgrid.getParentId(curntRowid);
        var result = false;
        if (parentid == null || parentid == 'undefined' || parentid <= 0) {
            result = true;
        }
        return result;
    }


    $("#btnAddNewBudget").click(function () {
        $('#errorMsg').css('display', 'none');
        $('#SuccessMsg').css('display', 'none');
        $("#divGridView").show();
    
       
        //Append please slect to the budget dropdown list
        $('#nl-formParentFinanceMain > div[class="nl-field nl-dd"]').find("li[class='nl-dd-checked']").removeClass('nl-dd-checked');
        $('#nl-formParentFinanceMain > div[class="nl-field nl-dd"]').find("a[class='nl-field-toggle']").html('Please Select');
        $('#ddlParentFinanceMain').prepend('<option value="0" selected="selected">Please Select</option>');
        $('#nl-formParentFinanceMain > div[class="nl-field nl-dd"]').find('ul').prepend("<li class='nl-dd-checked' value='0' originalvalue='Please Select' textvalue='0'>Please Select</li>");
        //end

        UpdateFinanceHeaderValues(); //update header values

        var url = urlContent + "MarketingBudget/LoadnewBudget/"; //Load add new budget screen
        $('#divGridView').load(url);
    });

    function OnEditCell(stage, id, index, newVal, oldVal) {
        ValidParentId = newTreeGrid.getParentId(id);
        var ColumnName = budgetgrid.getColLabel(index, 0);

        if (stage == 1) {

            if (ColumnName == "Task Name") {
                $(".dhx_combo_edit").off("keydown");
                $(".dhx_combo_edit").on('keydown', (
                newTreeGrid.editor.obj.onkeypress = function (event) {

                    var text = this.value;
                    if (event.keyCode == 8 || event.keyCode == 46
                     || event.keyCode == 37 || event.keyCode == 39) {

                        return true;
                    }
                    else if (text.length > 250) { //max length of the text
                        return false;
                    }
                    else { return true; }

                }));
            }
        }

        if (stage.toString() == '0' || stage.toString() == '1') {
            return true;
        }
        if (IsValid) { // checks if the name is valid then saves the data
            if (stage.toString() == '2') {
                if (newVal == null || newVal == '') {
                    return false;
                }
                else {
                    SaveNewBudget(htmlDecode(newVal));//method to save the data
                    return true;
                }
            }
        }
    }


    //Added by : Komal Rawal
    // to save the details of new budget .
    //Budget name : name of the new budget created
    function SaveNewBudget(budgetName) {

        $('#btnAddNewBudget').prop('disabled', false);

        $.ajax({
            url: urlContent + "MarketingBudget/SaveNewBudget/",
            dataType: 'json',
            data: {
                budgetName: budgetName,
            },
            success: function (data) {
                var BudgetId = data.budgetId;
                RefreshBudgetDropdown(BudgetId);
                GetGridData(BudgetId);
            }
        });
    }
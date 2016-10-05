///Calendar Functions
///Start

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

var bodyscrollpos = 0;

var type = 'Tactic';    //TODO: Set Fix value. //Need to talk 
var arrOpenTask = [];

function BindPlanCalendar() {
    var strURL = urlContent + 'Home/LoadPlanCalendar/';
    $.ajax({
        url: strURL,
        type: 'POST',
        success: function (data) {
            if (data != 'undefined' && data != null) {
                var calendarHtml = '<div id="NodatawithfilterCalendar" style="display:none;">' +
  '<span class="pull-left margin_t30 bold " style="margin-left: 20px;">No data exists. Please check the filters or grouping applied.</span>' +
'<br/></div>';
                calendarHtml += data;
                $("#GridGanttContent").html('');
                $("#GridGanttContent").html(calendarHtml);
            GetCalendarDataInJsonFormat();
            $('#ChangeView').show();
				   }

        }
    });
}

function GetCalendarDataInJsonFormat() {
    var strURL = urlContent + 'Home/GetCalendarData/';
    filters = GetFilterIds();
    var timeframe = $("#ddlUpComingActivites").val();
var viewBy = $('#ddlTabViewBy').val();
    $.ajax({
        url: strURL,
        type: 'POST',
        data: {
            planIds: filters.PlanIDs.toString(),
            ownerIds: filters.OwnerIds.toString(),
            tactictypeIds: filters.TacticTypeids.toString(),
            statusIds: filters.StatusIds.toString(),
            customFieldIds: filters.customFieldIds.toString(),
            timeframe: timeframe,
            viewBy: viewBy
        },
        success: function (data) {
            var permission = Boolean($("#IsPlanEditable").val());
            if (data.data.length > 0 || (NoPlanCreated.toString().toLowerCase()=='true' && permission.toString().toLowerCase()=='true' )) {
            ConfigureGanttwithdefaultSettings();    // Configure Calendar with default configuration.
            SetGanttData(data.data);            // Render Calendar.
            $('#exp-serach').css('display', 'block');
                $('#ChangeView').show();
            } else {
                $('#NodatawithfilterCalendar').show();
                $('#ChangeView').hide();
            }

        }
    });
}

function ConfigureGanttwithdefaultSettings() {
    gantt.config.grid_width = 410;
    gantt.config.readonly = true;
    gantt.config.autofit = true;
    gantt.config.drag_links = false;
    gantt.config.drag_resize = true;
    gantt.config.drag_progress = false;
    gantt.config.smart_rendering = true;
}

//// Function to render gantt i.e. for this year or this quarter.
function SetGanttData(resultdata) {
    var isRender;
    var taskData = resultdata;
    //var staticData = JSON.parse('[{"id":"L20220","text":"Plan_Usability_V","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":1,"open":false,"color":"","colorcode":"333333","planid":20220,"type":"Plan","TacticType":"--","Status":"Published","OwnerName":"Admin admin","Permission":true},{"id":"L20220_C27294","text":"C1","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":2,"open":false,"parent":"L20220","color":"","colorcode":"4798ba","plancampaignid":27294,"Status":"Created","type":"Campaign","TacticType":"--","OwnerName":"Admin admin","Permission":true},{"id":"L20220_C27294_P33913_T128321_Y31104","text":"T1","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294_P33913","color":"","colorcode":"317232","isSubmitted":false,"isDeclined":false,"projectedStageValue":"0","mqls":"0","cost":100000,"cws":0,"plantacticid":128321,"Status":"Created","type":"Tactic","TacticType":"Email","OwnerName":"Admin admin","ROITacticType":"Promotion","IsAnchorTacticId":0,"PlanTacticId":128321,"CalendarHoneycombpackageIDs":"","Permission":true,"LinkTacticPermission":false,"LinkedTacticId":null,"LinkedPlanName":null},{"id":"L20220_C27294_P33913_T129873_Y31104","text":"T2","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294_P33913","color":"","colorcode":"317232","isSubmitted":false,"isDeclined":false,"projectedStageValue":"0","mqls":"0","cost":100000,"cws":0,"plantacticid":129873,"Status":"Created","type":"Tactic","TacticType":"Email","OwnerName":"Admin admin","ROITacticType":"Promotion","IsAnchorTacticId":0,"PlanTacticId":129873,"CalendarHoneycombpackageIDs":"","Permission":true,"LinkTacticPermission":false,"LinkedTacticId":null,"LinkedPlanName":null},{"id":"L20220_C27294_P33913","text":"P1","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294","color":"","colorcode":"c6ebf3","planprogramid":33913,"Status":"Created","type":"Program","TacticType":"--","OwnerName":"Admin admin","Permission":true}]');
    var timeframe = $("#ddlUpComingActivites").val();

    var tasks = {
        data: taskData
    };

    GanttTaskData = tasks;
    $.each(taskData, function (index, objData) {
        objData.start_date = new Date(objData.start_date);
    });

    var taskcnt = gantt.getTaskCount();
    if (taskcnt > 0) {
        gantt.clearAll();
    }
    
    gantt.config.select_task = false;
    var currentDate = new Date();
    if (timeframe == null || timeframe == 'undefined' || timeframe == "") {
        timeframe = currentDate.getFullYear().toString();
    }

    
    if (timeframe == "thisquarter") {
        //// Setting scale.
        gantt.config.scale_unit = "month";
        gantt.config.step = 12;
        gantt.config.date_scale = " %Y";
        gantt.config.row_height = 25;
        gantt.config.scale_height = 60;
        gantt.config.columns = [{ name: "colorcode", label: "", tree: false, width: 10, resize: false }, { name: "text", label: "Task name", tree: true, width: 310, min_width: 310, resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 90 }];
        gantt.config.subscales = [{ unit: "month", step: 1, date: "%M" }];

        //// Getting quarter
        var startDate = getQuarterStartDate();
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
        if (json_length == 0) {
            filteredTasks = AddCalenderBlankRow(startDate);
        }
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
    else if (timeframe == "thismonth") {
        gantt.config.scale_unit = "month";
        gantt.config.step = 12;
        gantt.config.date_scale = " %Y";
        gantt.config.row_height = 25;
        gantt.config.scale_height = 60;
        gantt.config.columns = [{ name: "colorcode", label: "", tree: false, width: 10, resize: false }, { name: "text", label: "Task name", tree: true, width: 310, min_width: 310, resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 90 }];
        //gantt.config.columns = [{ name: "text", label: "Task name", tree: true, width: '*', resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 70 }];
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
        if (json_length == 0) {
            filteredTasks = AddCalenderBlankRow(startDate);
        }
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
else if ( $.isNumeric(timeframe)) {

        var PlanYears = timeframe.split("-");
        var yearDiffrence = 1;
        if (PlanYears.length > 1) {
            yearDiffrence = (parseInt(PlanYears[1]) - parseInt(PlanYears[0])) + 1;
        }
        gantt.config.scale_unit = "year";
        gantt.config.step = 1;
        gantt.config.date_scale = "%Y";
        gantt.config.scale_height = 60;
        gantt.config.row_height = 25;
        gantt.config.columns = [{ name: "colorcode", label: "", tree: false, width: 10, resize: false }, { name: "text", label: "Task name", tree: true, width: 310, min_width: 310, resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 90 }];
        //gantt.config.columns = [{ name: "text", label: "Task name", tree: true, width: '*', resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 70 }];
        gantt.config.subscales = [{ unit: "month", step: 1, date: "%M" }];
      
        var date = new Date();
        var startDate = new Date(parseInt(PlanYears[0]), 00, 01);
        gantt.config.start_date = startDate
        gantt.config.end_date = new Date(parseInt(PlanYears[0]) + yearDiffrence, 00, 01);

        gantt.init("gantt_here");

        var json_length = tasks.data.length;
        var gantt_div = document.getElementById("gantt_here");

        // To resolve extra space
        CalGanntHeight();

        //// Function to add class to each task bar.
        gantt.templates.task_class = function (start, end, task) {
            return getCSSForTask(task);
        };
        if (json_length == 0)
        {
            tasks = AddCalenderBlankRow(startDate);
        }
        gantt.parse(tasks);
    }
    else {
        // Desc :: For 24 months
        var    PlanYears = timeframe.split("-");
        
        
        var yearDiffrence = 1;
        if (PlanYears.length > 1) {
            yearDiffrence = (parseInt(PlanYears[1]) - parseInt(PlanYears[0])) + 1;
        }
        gantt.config.scale_unit = "year";
        gantt.config.step = 1;
        gantt.config.date_scale = "%Y";
        gantt.config.row_height = 25;
        gantt.config.scale_height = 60;
        //gantt.config.columns = [{ name: "text", label: "Task name", tree: true, width: '*', resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 70 }];
        gantt.config.columns = [{ name: "colorcode", label: "", tree: false, width: 10, resize: false }, { name: "text", label: "Task name", tree: true, width: 310, min_width: 310, resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 90 }];
        gantt.config.subscales = [{ unit: "month", step: 1, date: "%M" }];

        var date = new Date();
        var startDate = new Date(parseInt(PlanYears[0]), 00, 01);
        gantt.config.start_date = startDate
        gantt.config.end_date = new Date(parseInt(PlanYears[0]) + yearDiffrence, 00, 01);
        gantt.init("gantt_here");

        var json_length = tasks.data.length;
        var gantt_div = document.getElementById("gantt_here");

        // To resolve extra space
        CalGanntHeight();

        //// Function to add class to each task bar.
        gantt.templates.task_class = function (start, end, task) {
            return getCSSForTask(task);
        };
        if (json_length == 0) {
            tasks = AddCalenderBlankRow(startDate);
        }
        gantt.parse(tasks);
    }
    GetOpenCloseState();
    gantt.eachTask(function (task) {

        // Planning Window doesnt remember previous view
       if (arrOpenTask == null || arrOpenTask == 'undefined' || arrOpenTask.length == 0) {
        if (arrClosedTask.indexOf(task.id) > -1) {
            gantt.getTask(task.id).$open = false;
        }
        else {
            gantt.getTask(task.id).$open = true;
        }
        }
        else {
            if (arrOpenTask.indexOf(task.id) > -1) {
                gantt.getTask(task.id).$open = true;
            }
            else {
                gantt.getTask(task.id).$open = false;
            }
        }
        //CountTacticForRenderChart(task.id, '');
    });

    AttachEventToTactic();
    AttachEventToonTaskRowClick();
    //GlobalSearch();

    setTimeout(function () {
        var _scrollY = scrollstate.y
        gantt.scrollTo(0, _scrollY);
    }, 250);

    gantt.refreshData();   // Refresh Gantt to expand all tasks.
    
    SetTooltip();
}
//method to get open close state to load context from grid to calendar #2677
function GetOpenCloseState() {
    var cookie = document.cookie;
    if (cookie.indexOf("gridOpenplangridState") > -1) {
        var cookies = cookie.split(';');
        for (var i = 0; i < cookies.length; i++) {
            if (cookies[i].indexOf("gridOpenplangridState") > -1) {
                var cookievalue = cookies[i].split('=')[1];
                 arrOpenTask = cookievalue.split('|');
            }
        }
    }
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

//// Function to start date of current quarter.
function getQuarterStartDate() {
    var currentDate = new Date();
    var quater = Math.floor((currentDate.getMonth()) / 3) + 1;
    var startDate;
    var fullYear = currentDate.getFullYear();
    switch (quater) {
        case 1:
            startDate = new Date(fullYear, 00, 01);
            break;
        case 2:
            startDate = new Date(fullYear, 03, 01);
            break;
        case 3:
            startDate = new Date(fullYear, 06, 01);
            break;
        case 4:
            startDate = new Date(fullYear, 09, 01);
            break;
        default:
            startDate = currentDate;
    }

    return startDate;
}

gantt.attachEvent("onTaskOpened", function (id) {
    arrClosedTask = jQuery.grep(arrClosedTask, function (value) {
        return value != id;
    });
    manageAddPopup();
});

gantt.attachEvent("onTaskClosed", function (id) {

    if (arrClosedTask.indexOf(id) == -1) {
        arrClosedTask.push(id);
    }
    manageAddPopup();
});

function manageAddPopup()
{
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
    isCopyTacticHomeGrid = 0;
    isEditTacticHomeGrid = 0;
}

function AttachEventToTactic() {

    var eventTaskClick; //// Variable to hold double click event.
    var eventdblTaskClick;
    //// Detaching single click event.
    if (eventTaskClick != undefined) {
        gantt.detachEvent(eventTaskClick);
    }
        //// Detaching double click event.
    if (eventdblTaskClick != undefined) {
        gantt.detachEvent(eventdblTaskClick);
    }
    //// Attaching single click event
    eventTaskClick = gantt.attachEvent("onTaskClick", function (taskId, e) {        
        gantt.selectTask(taskId);
    });

    //// Attaching double click event
    eventdblTaskClick = gantt.attachEvent("onTaskDblClick", function (taskId, e) {
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
        planCampaignId = task.PlanCampaignId;
        planProgramId = task.PlanProgramId;
        planId = task.PlanId;
        planTacticId = task.PlanTacticId;
    } else if (isShowInspect) {
        planTacticId = $("#hdnshowInspectForPlanTacticId").val();
        planCampaignId = $("#hdnshowInspectForPlanCampaignId").val();
        planProgramId = $("#hdnshowInspectForPlanProgramId").val();
        planLineItemId = $("#hdnShowInspectForPlanLineItemId").val();
        planId = $("#CurrentPlanId").val();
    }
    //// Checking whether current task is tactic or not.
    if (typeof planTacticId != 'undefined' && planTacticId != 0 && planTacticId != null && planTacticId != '') {
        modalFullPosition(); // Added by Kapil Antala on 17 Sep 2014 for #732 - new popup design
        $('.modal-backdrop').addClass('modalFull-backdrop');
        $('.modal-backdrop').attr("style", "display:none !important;");
        $("#successMessage").css("display", "none");
        $("#errorMessage").css("display", "none");
        $("#modal-container-186470").modal('show');
        loadInspectPopup(planTacticId, secTactic, "Setup", inspectEdit, 0, inspectRequestIndex);
    }

        //// Checking whether current task is tactic or not.
    else if (typeof planProgramId != 'undefined' && planProgramId != 0 && planProgramId != null && planProgramId != '') {
        modalFullPosition(); // Added by Kapil Antala on 17 Sep 2014 for #732 - new popup design
        $('.modal-backdrop').addClass('modalFull-backdrop');
        $('.modal-backdrop').attr("style", "display:none !important;");
        $("#successMessage").css("display", "none");
        $("#errorMessage").css("display", "none");
        $("#modal-container-186470").modal('show');
        loadInspectPopup(planProgramId, secProgram, "Setup", inspectEdit, 0, inspectRequestIndex);//Modified by Komal rawal For #1325 - To get the default mode as read only
    }
        //// Checking whether current task is tactic or not.
    else if (typeof planCampaignId != 'undefined' && planCampaignId != 0 && planCampaignId != null && planCampaignId != '') {
        modalFullPosition(); // Added by Kapil Antala on 17 Sep 2014 for #732 - new popup design
        $('.modal-backdrop').addClass('modalFull-backdrop');
        $('.modal-backdrop').attr("style", "display:none !important;");
        $("#successMessage").css("display", "none");
        $("#errorMessage").css("display", "none");
        $("#modal-container-186470").modal('show');
        loadInspectPopup(planCampaignId, secCampaign, "Setup", inspectEdit, 0, inspectRequestIndex); //Modified by Komal rawal For #1324 - To get the default mode as read only
    }
        //// Checking whether current task is tactic or not.
    else if (typeof planLineItemId != 'undefined' && planLineItemId != 0 && planLineItemId != null && planLineItemId != '') {
        modalFullPosition(); // Added by Kapil Antala on 17 Sep 2014 for #732 - new popup design
        $('.modal-backdrop').addClass('modalFull-backdrop');
        $('.modal-backdrop').attr("style", "display:none !important;");
        $("#successMessage").css("display", "none");
        $("#errorMessage").css("display", "none");
        $("#modal-container-186470").modal('show');
        loadInspectPopup(planLineItemId, secLineItem, "Setup", inspectEdit, 0, inspectRequestIndex);
    }
    else if (typeof planId != 'undefined' && planId != 0 && planId != null && planId != '') {
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
//function add blank row in calendar when there is no plan for client #2587
function AddCalenderBlankRow(startDate)
{
    startDate = "01-04-"+startDate.getFullYear();
    var tasks = {
        data: [
            {
                id: "000", text: "Your plan goes here", start_date: startDate, duration: 0, color: "ffffff",
                progress: 0.4, parent: null, type: "Plan", Permission: true,
            }
        ]
    };
    return tasks;
}
//end


var PdfFilters = {
    customFieldIds: [],
    PlanIDs: [],
    SelectedPlans: [],
    PlanTitles: [],
    OwnerIds: [],
    TacticTypeids: [],
    StatusIds: []
};

// Export PDF function for calendar.
function CallPDF() {
    var timeframe = $("#ddlUpComingActivites").val();

    var isAvailablerecord = false;

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

    
    // Desc :: To resolve click on clear filters
    if (PdfFilters.SelectedPlans.toString().trim().length > 0) {
        isAvailablerecord = true;
    }
    
    if (isAvailablerecord) {
        // Check Export only filtered data after performing global search.
        if ($('#txtGlobalSearch').val().trim() != undefined && $('#txtGlobalSearch').val().trim() != null && $('#txtGlobalSearch').val().trim() != "") {

            $(GanttTaskData.data).each(function () {
                if ($('#searchCriteria').val().replace(" ", "").toUpperCase() == ActivityName) {        // The 'ActivityName' variable set at Index page through Enum properties.
                    if (this.text.toLowerCase().indexOf($('#txtGlobalSearch').val().trim().toLowerCase()) > -1) {
                        ExportSelectedIdsafterSearch.TaskID.push(this.id);
                    }

                }
                if ($('#searchCriteria').val().replace(" ", "").toUpperCase() == ExternalName) {        // The 'ExternalName' variable set at Index page through Enum properties.
                    if (this.machineName.toLowerCase().indexOf($('#txtGlobalSearch').val().trim().toLowerCase()) > -1) {
                        ExportSelectedIdsafterSearch.TaskID.push(this.id);
                    }

                }
            });

        }
        if (ExportSelectedIdsafterSearch.TaskID.length > 0) {
            CallPdfHoneyComb();
            ExportSelectedIdsafterSearch.TaskID = [];
            RemoveAllHoneyCombData();
        }
        else {
            $.each(GanttTaskData.data, function () {
                this.color = '#' + this.colorcode;
            });

            gantt.init("gantt_here");
            gantt.parse(GanttTaskData);
            var htmltestdiv = '';
            htmltestdiv += '<h2 style="color: #040707; margin: 0 0 10px;">Marketing Calendar</h2>';
            htmltestdiv += '<p style=" line-height: 24px;margin: 0;">';
            htmltestdiv += '<label style="color:#050708;font-weight: 600;">Time Frame: </label>';
            htmltestdiv += '<span style="color: #9C9C9C; font-weight: 600; font-size: 14px;">';
            htmltestdiv += timeframe + '</span>';
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
            var pdfowner = PdfFilters.OwnerIds.toString().trim();
            if (pdfowner.length > 0) {
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
            $("#mqllabeltest").text($("#pmqlLabel").text());
            $("#mqlspanvalue").text($("#pMQLs").text());
            $("#mqlspanpercentage").text($("#pMQLImproved").text());
            $("#pMQLImproved").parent().find('.greenfont').each(function () {
                $("#mqlspanpercentage").css("color", "#559659");
            });
            $("#pMQLImproved").parent().find('.redfont').each(function () {
                $("#mqlspanpercentage").css("color", "#EF2240");
            });

            var originurl = document.location.origin;
            var mypathurl = document.location.pathname.split('/')[1];
            var urlpdf = originurl + "/" + mypathurl + "/"; // This line use for stage servers.
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


            $("#budgetlabeltest").text($("#pcostLabel").text());
            $("#budgetspanvalue").text($("#pbudget").text());
            $("#ptacticcounttest").text($("#ptacticcount").text());


            if (timeframe.split('-').length > 1) {
                $("#AttachChartCanvas").html('');
                $("#legendchart").show();
                var ChartHtml = $("#chartCanvasmultiyear").html();
                var Chartlegend = $("#legendchart").html();
                var Canvas = $("#chart2").find(".dhx_canvas_text");
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
                        AttachChartHtml += "</div>";
                    }

                    i++;
                });

                $("#firstyear").text(timeframe.split('-')[0]);
                $("#secondyear").text(timeframe.split('-')[1]);
                $("#AttachChartCanvasmultiyear").html('');
                $("#AttachChartCanvasmultiyear").html(AttachChartHtml);
            }
            else {
                $("#legendchart").hide();
                $("#AttachChartCanvasmultiyear").html('');
                var ChartHtml = $("#chartCanvas").html();
                var Canvas = $("#chart2").find(".dhx_canvas_text");
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
                $('#AttachChartCanvas').css('margin-left', '55px');
            }

            var headerhtml = $("#testdiv").html();
            var style = "<style> div[task_id='L10874']:last-of-type {background-color:rgb(202, 60, 206)} </style>";
            var datestart;
            var dateend;
            if ($.isNumeric(timeframe)) {
                datestart = "01-01-" + timeframe;
                dateend = "01-01-" + (parseInt(timeframe) + 1).toString();
            }
            else if (timeframe == 'thismonth') {
                var date = new Date(), m = date.getMonth();
                datestart = "01-" + (m + 1).toString() + "-" + date.getFullYear().toString();
                dateend = "01-" + (m + 2).toString() + "-" + date.getFullYear().toString();
                $('#mainpdfdiv').css('width', '1000px');
                var headerhtml = $("#testdiv").html();
            }
            else if (timeframe == 'thisquarter') {
                var startDate = getQuarterStartDate();
                var m = startDate.getMonth();
                datestart = "01-" + (m + 1).toString() + "-" + startDate.getFullYear().toString();
                dateend = "01-" + (m + 4).toString() + "-" + startDate.getFullYear().toString();
                $('#mainpdfdiv').css('width', '1000px');
                var headerhtml = $("#testdiv").html();
            }
            else { 
                var PlanYears = timeframe.split("-");
                var yearDiffrence = (parseInt(PlanYears[1]) - parseInt(PlanYears[0])) + 1;
                datestart = "01-01-" + timeframe.split("-")[0];
                dateend = "01-01-" + (parseInt(timeframe) + yearDiffrence).toString();
                $('#mainpdfdiv').css('width', '');
                var headerhtml = $("#testdiv").html();
            }
            gantt.getGridColumn("colorcode").hide = true;   // Hide 'ColorCode' column on export to PDF file.
            gantt.exportToPDF({
                header: headerhtml,
                start: datestart,
                end: dateend

            });
        }
    }
    else {
        $('#cErrorInspectPopup').html('No plan selected to export the data in .pdf');
        $('#errorMessageInspectPopup').css("display", "block");
        $('#errorMessageInspectPopup').removeClass('message-position');
    }
}

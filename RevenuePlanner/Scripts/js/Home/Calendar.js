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

var scrollstate = 0;
var bodyscrollpos = 0;

var type = 'Tactic';    //TODO: Set Fix value. //Need to talk 

function BindPlanCalendar() {
    var strURL = urlContent + 'Home/LoadPlanCalendar/';
    $.ajax({
        url: strURL,
        type: 'POST',
        success: function (data) {
            if (data != 'undefined' && data != null) {
                $('#GridGanttContent').html(data);
            }
            GetCalendarDataInJsonFormat();
            $('#ChangeView').show();

        }
    });
}

function GetCalendarDataInJsonFormat() {
    var strURL = urlContent + 'Home/GetCalendarData/';
    filters = GetFilterIds();
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
            timeframe: '',
            viewBy: viewBy
        },
        success: function (data) {
            ConfigureGanttwithdefaultSettings();    // Configure Calendar with default configuration.
            SetGanttData(data.data);            // Render Calendar.
            $('#exp-serach').css('display', 'block');
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
    //var taskData = JSON.parse('[{"id":"L20220","text":"Plan_Usability_V","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":1,"open":false,"color":"","colorcode":"333333","planid":20220,"type":"Plan","TacticType":"--","Status":"Published","OwnerName":"Admin admin","Permission":true},{"id":"L20220_C27294","text":"C1","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":2,"open":false,"parent":"L20220","color":"","colorcode":"4798ba","plancampaignid":27294,"Status":"Created","type":"Campaign","TacticType":"--","OwnerName":"Admin admin","Permission":true},{"id":"L20220_C27294_P33913_T128321_Y31104","text":"T1","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294_P33913","color":"","colorcode":"317232","isSubmitted":false,"isDeclined":false,"projectedStageValue":"0","mqls":"0","cost":100000,"cws":0,"plantacticid":128321,"Status":"Created","type":"Tactic","TacticType":"Email","OwnerName":"Admin admin","ROITacticType":"Promotion","IsAnchorTacticId":0,"PlanTacticId":128321,"CalendarHoneycombpackageIDs":"","Permission":true,"LinkTacticPermission":false,"LinkedTacticId":null,"LinkedPlanName":null},{"id":"L20220_C27294_P33913_T129873_Y31104","text":"T2","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294_P33913","color":"","colorcode":"317232","isSubmitted":false,"isDeclined":false,"projectedStageValue":"0","mqls":"0","cost":100000,"cws":0,"plantacticid":129873,"Status":"Created","type":"Tactic","TacticType":"Email","OwnerName":"Admin admin","ROITacticType":"Promotion","IsAnchorTacticId":0,"PlanTacticId":129873,"CalendarHoneycombpackageIDs":"","Permission":true,"LinkTacticPermission":false,"LinkedTacticId":null,"LinkedPlanName":null},{"id":"L20220_C27294_P33913","text":"P1","machineName":"","start_date":"09/08/2016","duration":30.999999999998842,"progress":0,"open":false,"parent":"L20220_C27294","color":"","colorcode":"c6ebf3","planprogramid":33913,"Status":"Created","type":"Program","TacticType":"--","OwnerName":"Admin admin","Permission":true}]');
    var isQuater = '2016';  //TODO: Set Fix value.
    var planYear = 2016;    //TODO: Set Fix value.


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

    gantt.config.select_task = true;
    if (isQuater == "thisquarter") {
        //// Setting scale.
        gantt.config.scale_unit = "month";
        gantt.config.step = 12;
        gantt.config.date_scale = " %Y";
        gantt.config.row_height = 25;
        gantt.config.scale_height = 60;
        gantt.config.columns = [{ name: "colorcode", label: "", tree: false, width: 10, resize: false }, { name: "text", label: "Task name", tree: true, width: 310, min_width: 310, resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 90 }];
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
        gantt.config.row_height = 25;
        gantt.config.columns = [{ name: "colorcode", label: "", tree: false, width: 10, resize: false }, { name: "text", label: "Task name", tree: true, width: 310, min_width: 310, resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 90 }];
        //gantt.config.columns = [{ name: "text", label: "Task name", tree: true, width: '*', resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 70 }];
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
        gantt.config.row_height = 25;
        gantt.config.scale_height = 60;
        //gantt.config.columns = [{ name: "text", label: "Task name", tree: true, width: '*', resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 70 }];
        gantt.config.columns = [{ name: "colorcode", label: "", tree: false, width: 10, resize: false }, { name: "text", label: "Task name", tree: true, width: 310, min_width: 310, resize: true }, { name: "machineName", label: "Machine name", tree: true, resize: true, align: "center", hide: true }, { name: "add", label: "", width: 90 }];
        //gantt.config.subscales = [{ unit: "month", step: 1, date: "%M" }];

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

    AttachEventToTactic();
    AttachEventToonTaskRowClick();
    //GlobalSearch();

    setTimeout(function () {
        var _scrollY = scrollstate.y
        gantt.scrollTo(0, _scrollY);
    }, 250);

    gantt.refreshData();   // Refresh Gantt to expand all tasks.
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

    arrClosedTask = jQuery.grep(arrClosedTask, function (value) {
        return value != id;
    });

    manageAddPopup();
}

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
    GlobalSearch();

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

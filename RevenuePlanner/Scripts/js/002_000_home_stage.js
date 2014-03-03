var tasks = {
    data:[
        {id:1, text:"SUS",start_date:new Date(2013, 00, 05), duration:10,progress: 1, open: true},
        {id:2, text:"INQ",   start_date:new Date(2013, 02, 05), duration:10, progress: 1, open: false},
        {id:3, text:"AQL", start_date:new Date(2013, 03, 05), duration:10,  progress: 1,   open: false},
        {id:4, text:"TAL", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false},
        {id:5, text:"TQL", start_date:new Date(2013, 05, 05), duration:10,     progress: 1, open: false},
        {id:6, text:"SAL",   start_date:new Date(2013, 01, 05), duration:10,  progress: 1,   open: false},
        {id:7, text:"SQL",start_date:new Date(2013, 06, 05), duration:10,    progress: 1, open: false},
        {id:8, text:"CW",start_date:new Date(2013, 06, 05), duration:10,      progress: 1, open: false},
        {id:9, text:"ERP V10 Product Launch",start_date:new Date(2013, 06, 05), duration:10,   progress: 1, open: true, parent:1},
        {id:10, text:"Q2 Lead Gen",   start_date:new Date(2013, 07, 05), duration:10,    progress: 1,   open: false,parent:1},
        {id:11, text:"INQ 2",   start_date:new Date(2013, 07, 05), duration:10,   progress: 1,   open: false,parent:2},
        {id:12, text:"AQL 2",   start_date:new Date(2013, 08, 05), duration:10,      progress: 1, open: false,parent:3},



        {id:13, text:"TAL 2",start_date:new Date(2013, 00, 05), duration:10,progress: 1, open: false,parent:4},
        {id:14, text:"TQL 1",   start_date:new Date(2013, 02, 05), duration:10, progress: 1, open: false,parent:5},
        {id:15, text:"SAL 2", start_date:new Date(2013, 03, 05), duration:10,  progress: 1,   open: false,parent:6},
        {id:17, text:"SQL 2", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:7},
        {id:18, text:"CW 2", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:8},
        {id:19, text:"Program 1 Title", start_date:new Date(2013, 03, 05), duration:10,  progress: 1,   open: false,parent: 9},
        {id:20, text:"Title 8", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:19},
        {id:21, text:"Title 8", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:10},
    
    ]
};
gantt.config.scale_unit = "year";
gantt.config.step = 1;
gantt.config.date_scale = "%Y";
gantt.config.scale_height = 60;
gantt.config.grid_width = 236;
gantt.config.readonly = true;
gantt.config.autofit = true;
gantt.config.show_progress = false;
gantt.config.subscales = [{unit:"month", step:1, date:"%M" }]; 
gantt.config.columns =  [{name:"text",label:"Task name",  tree:true, width:'*'},];
gantt.init("gantt_here", new Date(2013,0,1), new Date(2014,0,1));   
gantt.parse (tasks);


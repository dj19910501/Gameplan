var tasks = {
    data:[
        {id:1, text:"Large Group",start_date:new Date(2013, 00, 05), duration:10,progress: 1, open: true},
        {id:2, text:"Small Group",   start_date:new Date(2013, 02, 05), duration:10, progress: 1, open: true},
        {id:3, text:"Individual", start_date:new Date(2013, 03, 05), duration:10,  progress: 1,   open: true},
        {id:4, text:"Pharmacy", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: true},
        {id:5, text:" ERP V10 Product Launch", start_date:new Date(2013, 05, 05), duration:10,     progress: 1, open: true,parent:1},
        {id:6, text:"Q2 Lead Gen",   start_date:new Date(2013, 01, 05), duration:10,  progress: 1,   open: false, parent:2},
        {id:7, text:"Nurture Campaign",start_date:new Date(2013, 06, 05), duration:10,    progress: 1, open: false, parent:2},
        {id:8, text:"Holiday Campaign",start_date:new Date(2013, 06, 05), duration:10,      progress: 1, open: false, parent:3},
        {id:9, text:"Conference",start_date:new Date(2013, 06, 05), duration:10,   progress: 1, open: false, parent:4},
        {id:10, text:"Program 1 Title",   start_date:new Date(2013, 07, 05), duration:10,    progress: 1,   open: false,parent:5},
        {id:11, text:"Program 2 Title",   start_date:new Date(2013, 07, 05), duration:10,   progress: 1,   open: false,parent:5},
        {id:12, text:"Program 3 Title",   start_date:new Date(2013, 08, 05), duration:10,      progress: 1, open: false,parent:5},



        {id:13, text:"Program 4 Title",start_date:new Date(2013, 00, 05), duration:10,progress: 1, open: false,parent:5},
        {id:14, text:"Title 1",   start_date:new Date(2013, 02, 05), duration:10, progress: 1, open: false,parent:11},
        {id:15, text:"Title 2", start_date:new Date(2013, 03, 05), duration:10,  progress: 1,   open: false,parent:12},
        {id:17, text:"Title 4", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:13},
        {id:18, text:"Title 5", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:10},
        {id:19, text:"Title 7", start_date:new Date(2013, 03, 05), duration:10,  progress: 1,   open: false,parent:7},
        {id:20, text:"Title 8", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:9},
        {id:21, text:"Title 9", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:10},     
        {id:20, text:"Title 8", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:6},
        {id:21, text:"Title 9", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false,parent:8},
    
    
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


var tasks = {
    data:[
        {id:1, text:"ERP V10 Product Launch",start_date:new Date(2013, 00, 05), duration:10,progress: 1, open: false},
        {id:2, text:"Q2 Lead Gen",   start_date:new Date(2013, 02, 05), duration:10, progress: 1, open: false},
        {id:3, text:"Conference", start_date:new Date(2013, 03, 05), duration:10,  progress: 1,   open: false},
        {id:4, text:"Holiday Campaign", start_date:new Date(2013, 04, 05), duration:10,  progress: 1, open: false},
        {id:5, text:"Nature Campaign", start_date:new Date(2013, 05, 05), duration:10,     progress: 1, open: false},
        {id:6, text:"Program 1 Title",   start_date:new Date(2013, 01, 05), duration:10,  progress: 1,   open: false, parent:1},
        {id:7, text:"Program 2 Title",start_date:new Date(2013, 06, 05), duration:10,    progress: 1, open: false, parent:1},
        {id:8, text:"Program 3 Title",start_date:new Date(2013, 06, 05), duration:10,      progress: 1, open: false, parent:1},
        {id:9, text:"Program 4 Title",start_date:new Date(2013, 06, 05), duration:10,   progress: 1, open: false, parent:1},
        {id:10, text:"ERP High Value Laun",   start_date:new Date(2013, 07, 05), duration:10,    progress: 1,   open: true,parent:6},
        {id:11, text:"Editorial Webinar-",   start_date:new Date(2013, 07, 05), duration:10,   progress: 1,   open: false,parent:6},
        {id:12, text:"Online Banner",   start_date:new Date(2013, 08, 05), duration:10,      progress: 1, open: false,parent:6},
        {id:13, text:"Subtitle 2", start_date:new Date(2013, 09, 05), duration:10,     progress: 1,   open: false,parent:7},
        {id:14, text:"Subtitle 3", start_date:new Date(2013, 10, 05), duration:10,     progress: 1, open: false,parent:8},
        {id:15, text:"Subtitle 4", start_date:new Date(2013, 11, 05), duration:10,   progress: 1, open: false,parent:9},
        {id:16, text:"Subtitle 5", start_date:new Date(2013, 11, 05), duration:10,    progress: 1, open: false,parent:2},
        {id:17, text:"Subtitle 6", start_date:new Date(2013, 11, 05), duration:10,       progress: 1, open: false,parent:3},
        {id:18, text:"Subtitle 7", start_date:new Date(2013, 11, 05), duration:10,         progress: 1, open: false,parent:4},
        {id:19, text:"Subtitle 8", start_date:new Date(2013, 11, 05), duration:10,   progress: 1, open: false,parent:5},
        {id:20, text:"Subtitle 9", start_date:new Date(2013, 10, 05), duration:10,     progress: 1, open: false,parent:8},
        {id:21, text:"Subtitle 10", start_date:new Date(2013, 11, 05), duration:10,   progress: 1, open: false,parent:9},
        {id:22, text:"Subtitle 11", start_date:new Date(2013, 11, 05), duration:10,    progress: 1, open: false,parent:2},
        {id:23, text:"Subtitle 12", start_date:new Date(2013, 11, 05), duration:10,       progress: 1, open: false,parent:3},
        {id:24, text:"Subtitle 13", start_date:new Date(2013, 11, 05), duration:10,         progress: 1, open: false,parent:4},
        {id:25, text:"Subtitle 14", start_date:new Date(2013, 11, 05), duration:10,   progress: 1, open: false,parent:5},

    
    
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


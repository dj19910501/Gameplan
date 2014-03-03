

var tasks = {
    data:[
    
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
gantt.config.columns =  [{name:"text",label:"Task name",  tree:true, width:'*'},{name:"add",label:"", width:44 }];
gantt.init("gantt_here_empty", new Date(2013,0,1), new Date(2014,0,1));   
gantt.parse (tasks);


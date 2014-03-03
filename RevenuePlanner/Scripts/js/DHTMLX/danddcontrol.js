var leftLimit;
var rightLimit;

gantt.attachEvent("onTaskDrag", function(id, mode, task, original, e){
    var modes = gantt.config.drag_mode;
    if(mode == modes.move || mode == modes.resize){
        //if  have children, validate children limits
        if(gantt.getChildren(task.id).length > 0)
        {
            if(mode==="resize" || mode==="move")
            {
                var children=gantt.getChildren(id);
                var limit_start_date=this.getTask(children[0]).start_date;
                var limit_end_date=this.getTask(children[0]).end_date;
                for (var i=0; i<children.length-1; i++)//search the limits dates in children dates
                {
                    if(this.getTask(children[i+1]).start_date < this.getTask(children[i]).start_date)
                        limit_start_date=this.getTask(children[i+1]).start_date
                    if(this.getTask(children[i+1]).end_date > this.getTask(children[i]).end_date)
                        limit_end_date=this.getTask(children[i+1]).end_date                        
                 }
                    if (limit_start_date < task.start_date)
                    {
                        task.start_date = limit_start_date;
                        if(mode === "move")
                            task.start_date = original.start_date;
                    }
                        
                    if (limit_end_date > task.end_date)
                    {
                        task.end_date = limit_end_date ;
                        if(mode === "move")
                            task.end_date = original.end_date;
                    }
                    if(task.parent != 0) //if is a program
                    {

                        if (this.getTask(task.parent).start_date > task.start_date)
                        {
                            task.start_date = this.getTask(task.parent).start_date;
                            if(mode === "move")
                                task.start_date = original.start_date;
                        }
                            
                        if (this.getTask(task.parent).end_date < task.end_date)
                        {
                            task.end_date = this.getTask(task.parent).end_date;
                            if(mode === "move")
                                task.end_date = original.end_date;
                        }   
                    }
                }
        }
        else
        {        
            var parentInfo = this.getTask(task.parent);
            leftLimit=parentInfo.start_date;
            rightLimit=parentInfo.end_date;
            if(+task.end_date > +rightLimit){                    
                task.end_date = new Date(rightLimit);
                if(mode == modes.move)
                    task.start_date = new Date(task.end_date - original.duration*(1000*60*60*24));
            }

            if(+task.start_date < +leftLimit){
                task.start_date = new Date(leftLimit);
                if(mode == modes.move)
                    task.end_date = new Date(+task.start_date + original.duration*(1000*60*60*24));
            }
            /*if children want to pass end day of parent*/
            var realEndDate=gantt.date.add(new Date(rightLimit), -1, 'day');
            if(+task.start_date >= +realEndDate){
                task.start_date = realEndDate;
                if(mode == modes.move)
                    task.start_date = realEndDate;
            }                    
        }
    }
    return true;
});





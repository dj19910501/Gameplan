taskRender = function (item, col, columns) {
    var _item = item.text.replace(/'/g, "&#39;");
    var value;
    if ("add" == col.name && i == columns.length - 1) {
        if (item.type == "Plan" && item.Permission == true) {
            // #1780		        
            //value = "<div id='" + item.type + "' class='gantt_add' Name='" + item.id + "' aria-label='" + _item + "' Permission='" + item.Permission + "'></div> <div id='" + item.type + "'  class='add_Remove_Entity' name1='" + item.id + "' aria-label='" + _item + "' Permission='" + item.Permission + "'></div>  ";
            value = "<div id='" + item.type + "' class='gantt_add' Name='" + item.id + "' aria-label='" + _item + "' Permission='" + item.Permission + "' onclick='DisplayPopUpMenu(this,event)'></div> <div id='" + item.type + "'  class='honeycombbox-icon-gantt calender-view-honeycomb' name1='" + item.id + "' ColorCode= '" + item.colorcode + "' TacticType= '" + item.TacticType + "' OwnerName= '" + item.OwnerName + "' aria-label='" + _item + "' Permission='" + item.Permission + "'>" + "</div>  ";
            // #1780
        }

        else if (item.type == "Campaign" && item.Permission == true) {
            // #1780
            //value = "<div id='" + item.type + "' class='gantt_add' Name='" + item.id + "' aria-label='" + item.text + "' Permission='" + item.Permission + "'></div> <div id='" + item.type + "'  class='add_Remove_Entity' name1='" + item.id + "' aria-label='" + item.text + "' Permission='" + item.Permission + "'></div>  ";
            value = "<div id='" + item.type + "' class='gantt_add' Name='" + item.id + "' aria-label='" + _item + "' Permission='" + item.Permission + "' onclick='DisplayPopUpMenu(this,event)'></div> <div id='" + item.type + "'  class='honeycombbox-icon-gantt calender-view-honeycomb' name1='" + item.id + "' ColorCode= '" + item.colorcode + "' TacticType= '" + item.TacticType + "' OwnerName= '" + item.OwnerName + "'   aria-label='" + _item + "' Permission='" + item.Permission + "'>" + "</div>  ";
            // #1780

        }

        else if (item.type == "Program" && item.Permission == true) {
            // #1780
            //value = "<div id='" + item.type + "' class='gantt_add' Name='" + item.id + "' aria-label='" + item.text + "' Permission='" + item.Permission + "'></div> <div id='" + item.type + "'  class='add_Remove_Entity' name1='" + item.id + "' aria-label='" + item.text + "' Permission='" + item.Permission + "'></div>  ";
            value = "<div id='" + item.type + "' class='gantt_add' Name='" + item.id + "' aria-label='" + _item + "' Permission='" + item.Permission + "' onclick='DisplayPopUpMenu(this,event)'></div> <div id='" + item.type + "'  class='honeycombbox-icon-gantt calender-view-honeycomb' name1='" + item.id + "' ColorCode = '" + item.colorcode + "' TacticType= '" + item.TacticType + "' OwnerName= '" + item.OwnerName + "'  aria-label='" + _item + "' Permission='" + item.Permission + "'>" + "</div>  ";
            // #1780
        }

        else if (item.type == "Tactic" && item.Permission == true) {
            // #1780
            //value = "<div id='" + item.type + "' class='gantt_add' Name='" + item.id + "' aria-label='" + item.text + "' Permission='" + item.Permission + "'></div> <div id='" + item.type + "'  class='add_Remove_Entity' name1='" + item.id + "' aria-label='" + item.text + "' Permission='" + item.Permission + "'></div>  ";
            value = "<div id='" + item.type + "' class='gantt_add' Name='" + item.id + "' aria-label='" + _item + "' Permission='" + item.Permission + "' LinkTacticPermission='" + item.LinkTacticPermission + "' LinkedTacticId = '" + item.LinkedTacticId + "' onclick='DisplayPopUpMenu(this,event)'></div> <div id='" + item.type + "'  class='honeycombbox-icon-gantt calender-view-honeycomb' name1='" + item.id + "' ColorCode= '" + item.colorcode + "'  TacticType='" + item.TacticType + "' OwnerName= '" + item.OwnerName + "'  aria-label='" + _item + "' Permission='" + item.Permission + "'>" + "</div>  ";
            // #1780
        }

        else if (item.type == "Imp Tactic" && item.Permission == true) {
            value = " <div id='" + item.type + "' class='gantt_add' Name='" + item.id + "' aria-label='" + item.text + "' Permission='" + item.Permission + "' onclick='DisplayPopUpMenu(this,event)'></div>  ";
        }
        else {
            value = "";
        }
    }
    else {
        //(value = col.template ? col.template(columns) : columns[col.name],
        //value instanceof Date && (value = this.templates.date_grid(value)), value = "<div class='gantt_tree_content'>" + value + "</div>");
        if (col.name == "add") {
            value = "<div class='gantt_add'></div>";
        } else {
            if (col.template)
                value = col.template(item);
            else
                value = item[col.name];

            if (value instanceof Date)
                value = this.templates.date_grid(value, item);
            value = "<div class='gantt_tree_content'>" + value + "</div>";
        }
    }
    return value;
}
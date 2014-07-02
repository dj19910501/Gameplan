var elmHeight = "17";	// should be specified based on image size

// Extend JQuery Functionality For Custom Radio Button Functionality
jQuery.fn.extend({
    dgStyle: function () {
	// Initialize with initial load time control state
	$.each($(this), function(){
		var elm	=	$(this).children().get(0);
		elmType = $(elm).attr("type");
		$(this).data('type',elmType);
		$(this).data('checked',$(elm).attr("checked"));
		$(this).dgClear();
		
            if ($(elm).attr('checked') == 'checked') {
		    $(elm).dgCheck(this);
		}
		
		if($(elm).attr("disabled") == "disabled") {
			//$(elm).parent('span').css("backgroundPosition","left -"+(elmHeight*4)+"px");
			$(elm).parent('span').addClass('disabled');

			$(this).mouseup(function() {
				$(this).dgHandle();
			});
		}
		else {
		}
		
	});
	$(this).mousedown(function() { $(this).dgEffect(); });
	$(this).mouseup(function() { $(this).dgHandle(); });	
},
    dgClear: function () {
	if($(this).data("checked") == true)
		$(this).css("backgroundPosition","left -"+(elmHeight*2)+"px");
	else
		$(this).css("backgroundPosition","left 0");
},
    dgEffect: function () {
    if($(this).data('type') == 'radio' && $(this).data("checked") == true)
	    return;
	    
	if($(this).data("checked") == true)
		$(this).css({backgroundPosition:"left -"+(elmHeight*3)+"px"});
	else
		$(this).css({backgroundPosition:"left -"+(elmHeight)+"px"});
},
    dgHandle: function () {
    if($(this).data('type') == 'radio' && $(this).data("checked") == true)
	    return;

	var elm	=	$(this).children().get(0);
        /*Modified by Mitesh Vaishnav on 02/07/2014 for prevent to check/uncheck disabled radio buttons */
        if (typeof ($(elm).attr("disabled")) == 'undefined') {
	if($(this).data("checked") == true)
		$(elm).dgUncheck(this);
	else
		$(elm).dgCheck(this);
	
            if ($(this).data('type') == 'radio') {
                $.each($("input[name='" + $(elm).attr("name") + "']"), function () {
			if(elm!=this)
				$(this).dgUncheck(-1);
		});
	}
        }


},
    dgCheck: function (div) {
	$(this).attr("checked",true);
	$(div).data('checked',true).css({backgroundPosition:"left -"+(elmHeight*2)+"px"});

},
    dgUncheck: function (div) {
	$(this).attr("checked",false);
	if(div != -1)
		$(div).data('checked',false).css({backgroundPosition:"left 0"});
	else
		$(this).parent().data("checked",false).css({backgroundPosition:"left 0"});

}
});

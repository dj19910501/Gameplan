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
        //Added by Rahul Shah on 06/10/2015 for PL#1638
	if ($('.Yes_BudgetCreateEdit').attr('checked') == 'checked') {

	    $('.Yes_BudgetView').attr("checked", true);
	    $('.Yes_BudgetView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_BudgetView').attr("checked", false);
	    $('.No_BudgetView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.Yes_ForecastView').attr("checked", true);
	    $('.Yes_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_ForecastView').attr("checked", false);
	    $('.No_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	}
	else {

	    $('.Yes_BudgetView').parent().css('opacity', '');
	    $('.No_BudgetView').parent().css('opacity', '');
	    //$('.Yes_ForecastView').parent().css('opacity', '');
	    //$('.No_ForecastView').parent().css('opacity', '');
	}

	if ($('.Yes_BudgetView').attr('checked') == 'checked') {

	    //$('.Yes_BudgetView').attr("checked", true);
	    //$('.Yes_BudgetView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
        	  
	    $('.Yes_ForecastView').attr("checked", true);
	    $('.Yes_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_ForecastView').attr("checked", false);
	    $('.No_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	}
	else {

	    //$('.Yes_BudgetView').parent().css('opacity', '');
	    
	    $('.Yes_ForecastView').parent().css('opacity', '');
	    $('.No_ForecastView').parent().css('opacity', '');
	}

	if ($('.Yes_ForecastCreateEdit').attr('checked') == 'checked') {

	    $('.Yes_ForecastView').attr("checked", true);
	    $('.Yes_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_ForecastView').attr("checked", false);
	    $('.No_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	}
	else {
	    if ($('.Yes_BudgetCreateEdit').attr('checked') == 'checked') {
	        $('.Yes_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	        $('.No_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	    }
	    else {
	        $('.Yes_ForecastView').parent().css('opacity', '');
	        $('.No_ForecastView').parent().css('opacity', '');
	    }
	}
	if ($('.No_BudgetCreateEdit').attr('checked') == 'checked' && $('.No_BudgetView').attr('checked') == 'checked') {

	    $('.No_ForecastCreateEdit').attr("checked", true);
	    $('.No_ForecastCreateEdit').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.Yes_ForecastCreateEdit').attr("checked", false);
	    $('.Yes_ForecastCreateEdit').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.No_ForecastView').attr("checked", true);
	    $('.No_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.Yes_ForecastView').attr("checked", false);
	    $('.Yes_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	}
	else {
	    $('.No_ForecastCreateEdit').parent().css('opacity', '');
	    $('.Yes_ForecastCreateEdit').parent().css('opacity', '');
	}
	if ($('.Yes_MultiCurrencyNone').attr('checked') == 'checked') {

	    $('.Yes_MultiCurrencyViewOnly').attr("checked", false);
	    $('.Yes_MultiCurrencyViewOnly').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	    
	    $('.No_MultiCurrencyViewOnly').attr("checked", true);
	    $('.No_MultiCurrencyViewOnly').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.Yes_MultiCurrencyEdit').attr("checked", false);
	    $('.Yes_MultiCurrencyEdit').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.No_MultiCurrencyEdit').attr("checked", true);
	    $('.No_MultiCurrencyEdit').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	}
	else {
	    $('.Yes_MultiCurrencyViewOnly').parent().css('opacity', '');
	    $('.No_MultiCurrencyViewOnly').parent().css('opacity', '');
	    $('.Yes_MultiCurrencyEdit').parent().css('opacity', '');
	    $('.No_MultiCurrencyEdit').parent().css('opacity', '');
	}

	if ($('.Yes_MultiCurrencyViewOnly').attr('checked') == 'checked') {

	    $('.Yes_MultiCurrencyNone').attr("checked", false);
	    $('.Yes_MultiCurrencyNone').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.No_MultiCurrencyNone').attr("checked", true);
	    $('.No_MultiCurrencyNone').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	}
	else {
	    if ($('.No_MultiCurrencyEdit').attr('checked') == 'checked') {
	        $('.Yes_MultiCurrencyNone').parent().css('opacity', '');
	        $('.No_MultiCurrencyNone').parent().css('opacity', '');
	    }
	    else {
	        $('.Yes_MultiCurrencyNone').attr("checked", false);
	        $('.Yes_MultiCurrencyNone').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	        $('.No_MultiCurrencyNone').attr("checked", true);
	        $('.No_MultiCurrencyNone').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	    }
	}

	if ($('.Yes_MultiCurrencyEdit').attr('checked') == 'checked') {

	    $('.Yes_MultiCurrencyViewOnly').attr("checked", true);
	    $('.Yes_MultiCurrencyViewOnly').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_MultiCurrencyViewOnly').attr("checked", false);
	    $('.No_MultiCurrencyViewOnly').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.No_MultiCurrencyNone').attr("checked", true);
	    $('.No_MultiCurrencyNone').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.Yes_MultiCurrencyNone').attr("checked", false);
	    $('.Yes_MultiCurrencyNone').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	}
	else {
	    if ($('.No_MultiCurrencyViewOnly').attr('checked') == 'checked') {
	        $('.Yes_MultiCurrencyNone').parent().css('opacity', '');
	        $('.No_MultiCurrencyNone').parent().css('opacity', '');
	    }
	    else {
	        $('.Yes_MultiCurrencyNone').attr("checked", false);
	        $('.Yes_MultiCurrencyNone').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	        $('.No_MultiCurrencyNone').attr("checked", true);
	        $('.No_MultiCurrencyNone').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	    }
	}
},
    dgUncheck: function (div) {
	$(this).attr("checked",false);
	if(div != -1)
		$(div).data('checked',false).css({backgroundPosition:"left 0"});
	else
		$(this).parent().data("checked",false).css({backgroundPosition:"left 0"});
        //Added by Rahul Shah on 06/10/2015 for PL#1638
	if ($('.Yes_BudgetCreateEdit').attr('checked') == 'checked') {

	    $('.Yes_BudgetView').attr("checked", true);
	    $('.Yes_BudgetView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_BudgetView').attr("checked", false);
	    $('.No_BudgetView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.Yes_ForecastView').attr("checked", true);
	    $('.Yes_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_ForecastView').attr("checked", false);
	    $('.No_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	}
	else {

	    $('.Yes_BudgetView').parent().css('opacity', '');
	    $('.No_BudgetView').parent().css('opacity', '');
	  
	}

	if ($('.Yes_BudgetView').attr('checked') == 'checked') {
        	   
	    $('.Yes_ForecastView').attr("checked", true);
	    $('.Yes_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_ForecastView').attr("checked", false);
	    $('.No_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	}
	else {
           
	    $('.Yes_ForecastView').parent().css('opacity', '');
	    $('.No_ForecastView').parent().css('opacity', '');
	}
	if ($('.Yes_ForecastCreateEdit').attr('checked') == 'checked') {

	    $('.Yes_ForecastView').attr("checked", true);
	    $('.Yes_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_ForecastView').attr("checked", false);
	    $('.No_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	}
	else {
	    if ($('.Yes_BudgetCreateEdit').attr('checked') == 'checked' || $('.Yes_BudgetView').attr('checked') == 'checked') {
	        $('.Yes_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	        $('.No_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	    }
	    else {
	        $('.Yes_ForecastView').parent().css('opacity', '');
	        $('.No_ForecastView').parent().css('opacity', '');
	    }

	}

	if ($('.No_BudgetCreateEdit').attr('checked') == 'checked' && $('.No_BudgetView').attr('checked') == 'checked') {

	    $('.No_ForecastCreateEdit').attr("checked", true);
	    $('.No_ForecastCreateEdit').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.Yes_ForecastCreateEdit').attr("checked", false);
	    $('.Yes_ForecastCreateEdit').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.No_ForecastView').attr("checked", true);
	    $('.No_ForecastView').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.Yes_ForecastView').attr("checked", false);
	    $('.Yes_ForecastView').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	}
	else {
	    $('.No_ForecastCreateEdit').parent().css('opacity', '');
	    $('.Yes_ForecastCreateEdit').parent().css('opacity', '');
	}
	if ($('.Yes_MultiCurrencyNone').attr('checked') == 'checked') {

	    $('.Yes_MultiCurrencyViewOnly').attr("checked", false);
	    $('.Yes_MultiCurrencyViewOnly').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.No_MultiCurrencyViewOnly').attr("checked", true);
	    $('.No_MultiCurrencyViewOnly').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.Yes_MultiCurrencyEdit').attr("checked", false);
	    $('.Yes_MultiCurrencyEdit').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.No_MultiCurrencyEdit').attr("checked", true);
	    $('.No_MultiCurrencyEdit').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	}
	else {
	    $('.Yes_MultiCurrencyViewOnly').parent().css('opacity', '');
	    $('.No_MultiCurrencyViewOnly').parent().css('opacity', '');
	    $('.Yes_MultiCurrencyEdit').parent().css('opacity', '');
	    $('.No_MultiCurrencyEdit').parent().css('opacity', '');
	}

	if ($('.Yes_MultiCurrencyViewOnly').attr('checked') == 'checked') {

	    $('.Yes_MultiCurrencyNone').attr("checked", false);
	    $('.Yes_MultiCurrencyNone').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.No_MultiCurrencyNone').attr("checked", true);
	    $('.No_MultiCurrencyNone').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	}
	else {
	    if ($('.No_MultiCurrencyEdit').attr('checked') == 'checked') {
	        $('.Yes_MultiCurrencyNone').parent().css('opacity', '');
	        $('.No_MultiCurrencyNone').parent().css('opacity', '');
	    }
	    else {
	        $('.Yes_MultiCurrencyNone').attr("checked", false);
	        $('.Yes_MultiCurrencyNone').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	        $('.No_MultiCurrencyNone').attr("checked", true);
	        $('.No_MultiCurrencyNone').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	    }
	}

	if ($('.Yes_MultiCurrencyEdit').attr('checked') == 'checked') {

	    $('.Yes_MultiCurrencyViewOnly').attr("checked", true);
	    $('.Yes_MultiCurrencyViewOnly').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.No_MultiCurrencyViewOnly').attr("checked", false);
	    $('.No_MultiCurrencyViewOnly').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	    $('.No_MultiCurrencyNone').attr("checked", true);
	    $('.No_MultiCurrencyNone').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });

	    $('.Yes_MultiCurrencyNone').attr("checked", false);
	    $('.Yes_MultiCurrencyNone').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });
	}
	else {
	    if ($('.No_MultiCurrencyViewOnly').attr('checked') == 'checked') {
	        $('.Yes_MultiCurrencyNone').parent().css('opacity', '');
	        $('.No_MultiCurrencyNone').parent().css('opacity', '');
	    }
	    else {
	        $('.Yes_MultiCurrencyNone').attr("checked", false);
	        $('.Yes_MultiCurrencyNone').parent().data('checked', false).css({ backgroundPosition: "left 0", opacity: "0.4" });

	        $('.No_MultiCurrencyNone').attr("checked", true);
	        $('.No_MultiCurrencyNone').parent().data('checked', true).css({ backgroundPosition: "left -" + (elmHeight * 2) + "px", opacity: "0.4" });
	    }
	}
}
});

$(document).ready(function () {

    /*check by the first time if options are default and change backgorund color*/
        $('.nl-field').each(function(){
          if($(this).next().attr('data-default')==$(this).children('a').text()){
            $(this).children('a').addClass("defaul-value-form");
          } 
          else{
            $(this).children('a').removeClass("defaul-value-form");
                $(this).children('a').removeClass("error-form");
          }     
    });

    /*to add background color to <a> when is  a default value as a warning*/
    $('.nl-field').click(function(){
          if($(this).next().attr('data-default')==$(this).children('a').text()){
            $(this).children('a').addClass("defaul-value-form");
          } 
          else{
            $(this).children('a').removeClass("defaul-value-form");
                $(this).children('a').removeClass("error-form");
          }     
    });

     /*to change color on select option*/
    $('#dual-select ul li').click(function(){
        $("#status-option .status-option-title").show();
          if( $('#dual-select .nl-dd-checked').text() == 'Not yet Approved'){
            $("#dual-select .nl-field-toggle").removeClass("approved-form");
            $("#dual-select .nl-field-toggle").addClass("not-approved-form");
            $("#status-option span").text(" Not yet Approved");
            $("#status-option span").removeClass("approved-form");
            $("#status-option span").addClass("not-approved-form");
          } 
          else{
            $("#dual-select .nl-field-toggle").removeClass("not-approved-form");
            $("#dual-select .nl-field-toggle").addClass("approved-form");
            $("#status-option span").text(" Completed");
            $("#status-option span").removeClass("not-approved-form");
            $("#status-option span").addClass("approved-form");
          }            
    });   

    // Added By Bhavesh : if When popup open than display status & assign color.
    //$("#status-option .status-option-title").show();
    //if ($('#dual-select .nl-dd-checked').text() == 'Not yet Approved') {
    //    $("#dual-select .nl-field-toggle").removeClass("approved-form");
    //    $("#dual-select .nl-field-toggle").addClass("not-approved-form");
    //    $("#status-option span").text(" Not yet Approved");
    //    $("#status-option span").removeClass("approved-form");
    //    $("#status-option span").addClass("not-approved-form");
    //}
    //else {
    //    $("#dual-select .nl-field-toggle").removeClass("not-approved-form");
    //    $("#dual-select .nl-field-toggle").addClass("approved-form");
    //    $("#status-option span").text(" Completed");
    //    $("#status-option span").removeClass("not-approved-form");
    //    $("#status-option span").addClass("approved-form");
    //}

  /*save button*/
    $("#button-review-tactic").click(function(){
    $('.nl-field').each(function(){
          if($(this).next().attr('data-default')==$(this).children('a').text()){
            $(this).children('a').addClass("error-form");
          } 
          else{
            $(this).children('a').removeClass("error-form");
          }     
    });

  });
});



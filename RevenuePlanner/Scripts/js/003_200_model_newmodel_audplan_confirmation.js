$(document).ready(function () {
    /*show success message*/
      $("#successMessage").slideDown(1200);    // Show the Alert
      $("#successMessage").slideUp(3000);

    /*close x event on message*/
    $(".alert").find(".close").on("click", function (e) {
        e.stopPropagation();
        e.preventDefault(); 
        $(this).closest(".alert").slideUp(400);
    });    

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

    /*change color on inputs when input has a value*/
    $('#inputs-group input').change(function(){
          if($(this).val()==''){
            $(this).addClass("error");
          } 
          else{
            $(this).removeClass("error");
          }     
    }); 
  /*change color on inputs with prepend when input has a value*/
   $('#inputs-group .input-prepend input').change(function(){
          if($(this).val()==''){
            $(this).addClass("error");
            $(this).prev().addClass("error");
          } 
          else{
            $(this).removeClass("error");
            $(this).prev().removeClass("error");
          }     
    }); 

 /*save button*/
    $("#button-save").click(function(){
      var needAlert=false;
    $('.nl-field').each(function(){
          if($(this).next().attr('data-default')==$(this).children('a').text()){
            $(this).children('a').addClass("error-form");
            needAlert=true;
          } 
          else{
            $(this).children('a').removeClass("error-form");
          }     
    });

    $('#inputs-group input').each(function(){
          if($(this).val()==''){
            $(this).addClass("error");
            needAlert=true;
          } 
          else{
            $(this).removeClass("error");
          }     
    }); 

    $('#inputs-group .input-prepend input').each(function(){
          if($(this).val()==''){
            $(this).addClass("error");
            $(this).prev().addClass("error");
            needAlert=true;
          } 
          else{
            $(this).removeClass("error");
            $(this).prev().removeClass("error");
          }     
    }); 

    //show alert or disappear alert
    if(needAlert==true){
      $("#errorMessage").slideDown(400);    // Show the Alert
    }
    else{
      $("#errorMessage").slideUp(400);
    }

  });
});



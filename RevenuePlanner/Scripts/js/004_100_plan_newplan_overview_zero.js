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
          

        if ($('.nl-field').children('a').eq(0).text()=== $('.nl-field').next().eq(0).attr('data-default') ||
            $('.nl-field').children('a').eq(1).text()=== $('.nl-field').next().eq(1).attr('data-default') ||
            $('.nl-field').children('a').eq(2).text()=== $('.nl-field').next().eq(2).attr('data-default') ||
            $('.nl-field').children('a').eq(3).text()=== $('.nl-field').next().eq(3).attr('data-default') ){
            $('#btn_next_plan').removeClass("btn-blue");
            $('#btn_next_plan').addClass("btn-blue-disable");
        }
        else
        {
          $('#btn_next_plan').removeClass("btn-blue-disable");
          $('#btn_next_plan').addClass("btn-blue");
        }

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
  });
});



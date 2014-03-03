$(document).ready(function () {
  
    $('form[name="login-register"]').on("submit", function (e) {
        var email = $(this).find('input[name="email"]');
        var password = $(this).find('input[name="password"]');

        if ($.trim(email.val())=== "admin@admin.com" && $.trim(password.val())=== "admin" ) {
            $("#errorMessage").slideUp(400);
        } else {
          e.preventDefault();
          $("#errorMessage").slideDown(400);    // Show the Alert
        }
    });
    
    $(".alert").find(".close").on("click", function (e) {
        e.stopPropagation();
        e.preventDefault(); 
        $(this).closest(".alert").slideUp(400);
    });
});
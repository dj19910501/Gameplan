$(document).ready(function () {
    //slidepanel animation
    $('[data-slidepanel]').slidepanel({
        orientation: 'right',
        mode: 'overlay'
    });

    //function to change arrows on accordions 
    $('.accordion').collapse({
        toggle: false
    }).on('show', function (e) {
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        $(e.target).prev().find(".chevron-right").removeClass("chevron-right").addClass("chevron-down");
    }).on('hide', function (e) {
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        $(e.target).prev().find(".chevron-down").removeClass("chevron-down").addClass("chevron-right");
    });

    //function to change arrows on accordions 
    $('.accordion-campaign').collapse({
        toggle: false
    }).on('show', function (e) {
        $(e.target).prev().find(".chevron-right").removeClass("chevron-right").addClass("chevron-down");
    }).on('hide', function (e) {

        $(e.target).prev().find(".chevron-down").removeClass("chevron-down").addClass("chevron-right");
    });

    //function to change text of "select" component when user select an option.(bootstrap component: btn-group ul.dropdown-menu)
    $(document).on("click", ".btn-group ul.dropdown-menu li a", function () {
        var text = $(this).html();
        $(this).parent().parent().parent().find("button:first-child").html(text);
    })

    //scroll home left nav
    $(function () {
        $('.scrolled_div').slimScroll({
            height: '230px',
            alwaysVisible: false,
            distance: '6px',
            wheelStep: 3,
            allowPageScroll: true,
            disableFadeOut: false
        });
        //sideBar bottomshadow
        var actualWidth = $('.scrolled_div').parent().width();
        $('.scrolled_div').parent().css("padding-right", "20px");
        var h = $(".slimScrollDiv").height();
        if (h >= 230) {
            $(".shadow-scroll").css('visibility', 'visible');
        } else {
            $(".shadow-scroll").css('visibility', 'hidden');
        }
    });


    //click event on accordion-toggle elements to verify height of slimScrollDiv container and show or hidden shadow in the bottom of nav.
    $('.colors .accordion-toggle').click(function () {
        var h = $(".slimScrollDiv").height();
        if (h >= 225 && h < 230) {
            $(".shadow-scroll").css('visibility', 'visible');
        } else {
            $(".shadow-scroll").css('visibility', 'hidden');
        }
    });

    /*add mouse wheel event to scroll home left nav*/
    $('.scrolled_div').slimScroll().bind('slimscroll', function (e, pos) {
        if (pos == 'top') {
            var h = $(".slimScrollDiv").height();
            if (h >= 230) {
                $(".shadow-scroll").css('visibility', 'visible');
            }
            else {
                $(".shadow-scroll").css('visibility', 'hidden');
            }
        }
        else { $(".shadow-scroll").css('visibility', 'hidden'); }
    });
    /*Changed : add Scroll bar by Nirav Shah on 4 Feb 2014  */
    /*slidepannel scroll*/
    $('#slidepanel-container').slimScroll({
        height: 'auto',
        alwaysVisible: true,
        distance: '7px',
        wheelStep: 3,
        allowPageScroll: true,
        disableFadeOut: false,
        color: '#fff',
    });
});

/*Added by Mitesh Vaishnav on 30 May 2014 for #492 Difficulty entering weight values when creating an improvement tactic */
/*function used at textbox which contains only numeric value*/
function priceFormatKeydown(e) {

    // Allow: backspace, delete, tab, escape, enter and .
    if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
        // Allow: Ctrl+A
        (e.keyCode == 65 && e.ctrlKey === true) ||
        // Allow: home, end, left, right
        (e.keyCode >= 35 && e.keyCode <= 39)) {
        // let it happen, don't do anything
        return;
    }
    // Ensure that it is a number and stop the keypress
    if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
        e.preventDefault();
    }

};

/*Added by Mitesh Vaishnav on 30 May 2014 for #492 Difficulty entering weight values when creating an improvement tactic */
/*function used at textbox when replace blank value with '0'*/
function priceFormatChange(e) {
    if (e.target.value == '') {
        e.target.value = 0;
    }
};






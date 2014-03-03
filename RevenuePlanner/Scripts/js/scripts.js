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

        $(e.target).parent().find(".chevron-right").removeClass("chevron-right").addClass("chevron-down");
    }).on('hide', function (e) {

        $(e.target).parent().find(".chevron-down").removeClass("chevron-down").addClass("chevron-right");
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
            allowPageScroll: false,
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
        allowPageScroll: false,
        disableFadeOut: false,
        color: '#fff',
    });
});






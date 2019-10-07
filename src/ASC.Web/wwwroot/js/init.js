//$(document).ready(function () {

//});

(function ($) {

    $(function () {

        $('.button-collapse').sideNav();
        $('.parallax').parallax();

        //Prevent browser back and forward buttons.
        if (window.history && window.history.pushState) {
            window.history.pushState('forward', '', window.location.href);
            $(window).on('popstate', function (e) {

                window.history.pushState('forward', '', window.location.href);
                e.preventDefault();
            });
        }

        //Prevent right-click on entire window
        $(document).ready(function () {
            $(window).on("contextmenu", function () {
                return false;
            });
        });

        // Labels overlapping prefilled content in forms
        $('.input-field label').addClass('active');
        setTimeout(function () { $('.input-field label').addClass('active'); }, 1);



    }); // end of document ready
})(jQuery); // end of jQuery name space
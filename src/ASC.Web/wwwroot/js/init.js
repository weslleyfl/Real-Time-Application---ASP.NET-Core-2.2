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

        
        $(document).ready(function () {

            //Prevent right-click on entire window
            $(window).on("contextmenu", function () {
                return false;
            });

            $('#selectCulture').material_select();

            $("#selectLanguage select").change(function () {
                $(this).parent().submit();
                //console.log('disparou disparou disparou');
            });

        });

        // Labels overlapping prefilled content in forms
        $('.input-field label').addClass('active');
        setTimeout(function () { $('.input-field label').addClass('active'); }, 1);



    }); // end of document ready

    // Future Date Vallidation method.
    $.validator.addMethod('futuredate', function (value, element, params) {

        var selectedDate = new Date(value),
            now = new Date(),
            futureDate = new Date(),
            todaysUtcDate = new Date(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate(), now.getUTCHours(), now.getUTCMinutes(), now.getUTCSeconds());

        futureDate.setDate(todaysUtcDate.getDate() + parseInt(params[0]));

        if (selectedDate >= futureDate) {
            return false;
        }
        return true;
    });
    // Add Future Date adapter.
    $.validator.unobtrusive.adapters.add('futuredate',
        ['days'],
        function (options) {
            options.rules['futuredate'] = [parseInt(options.params['days'])];
            options.messages['futuredate'] = options.message;
        });



})(jQuery); // end of jQuery name space
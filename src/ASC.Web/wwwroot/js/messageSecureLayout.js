$(document).ready(function () {

    // Listing 11-36. JQuery code to show tap target notification - SIGNALR

    //<!--Connect to server hub: eventHub, transport type default would be: websocket-->
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/serviceMessagesHub")
        .build();


    // $.connection.serviceMessagesHub.client.publishNotification = showNotification;
    connection.on('publishNotification', function (data) {
        showNotification(data);
    });


    // $.connection.serviceMessagesHub.client.publishPromotion = showPromotion;
    connection.on('publishPromotion', function (data) {
        showPromotion(data);
    });


    //<!--error logging-->
    connection.start().catch(function (err) {
        return console.error(err.toString());
    });


    function showNotification(data) {
        var notificationText = $('.divNotification').html();
        $('.divNotification').html(notificationText.replace('{$}', data.status));
        $('.tap-target').removeClass('hide');
        $('.tap-target').tapTarget('open');
        setTimeout(function () {
            $('.tap-target').tapTarget('close');
            $('.tap-target').addClass('hide');
        }, 5000);
    }

    var counter = 0;
    function showPromotion(data) {
        counter++;
        var promotionTemplate = $('.promotionTemplate').clone().html();
        promotionTemplate = promotionTemplate.replace('{Header}', data.header);
        promotionTemplate = promotionTemplate.replace('{Content}', data.content);
        promotionTemplate = promotionTemplate.replace('{Style}', data.partitionKey === 'Discount' ? 'light-green darken-2' : 'light-blue darken-2');

        // Prepend newly added promotion to to divPromotions on Promotions view.
        $('.divPromotions').prepend(promotionTemplate);

        // show notification counter on the left navigation menu item.
        $('#ancrPromotions .badge').remove();
        $('#ancrPromotions').prepend('<span class="new badge">' + counter + '</span>');
    }

    //// Start the client side HUB
    //$.connection.hub.start();

});
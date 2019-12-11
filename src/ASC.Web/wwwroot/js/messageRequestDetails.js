$(document).ready(function () {

    //<!--Connect to server hub: eventHub, transport type default would be: websocket-->
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/serviceMessagesHub")
        .build();

    ////<!--error logging-->
    //connection.start().catch(function (err) {
    //    return console.error(err.toString());
    //});

    //@* Need to pass Verification Token to get Request Validated for Forgery *@
    var token = $('input[type=hidden][name=__RequestVerificationToken]', document).val();
    $.ajaxSetup({
        // Disable caching of AJAX responses
        cache: false,
        headers: { 'RequestVerificationToken': token, 'ServiceRequestId': $('#ServiceRequest_RowKey').val() }
    });

    //@* Get all previous messages for the service request *@
    $.get('/ServiceRequests/ServiceRequest/ServiceRequestMessages?serviceRequestId=' + $('#ServiceRequest_RowKey').val(),
        function (data, status) {
            addMessagesToList(data);
        });

    //@* Function to scroll the messages panel to latest message *@
    function scrollToLatestMessages() {
        $('.messages').animate({ scrollTop: 10000 }, 'normal');
    }

    //@* Function which is used to list of messages to UI *@
    function addMessagesToList(messages) {
        if (messages.length === 0) {
            $('.noMessages').removeClass('hide');
        }

        $.each(messages, function (index) {
            var message = messages[index];
            displayMessage(message);
        });

        scrollToLatestMessages();
    }

    //@* Function which is invoked by SignalR Hub when a new message is broadcasted *@
    function addMessage(message) {

        if (message.PartitionKey !== $('#ServiceRequest_RowKey').val()) {
            return;
        }

        if (message !== null) {
            $('.noMessages').addClass('hide');
        }

        displayMessage(message);
        scrollToLatestMessages();
    }

    //@* Function used to display message on UI *@
    function displayMessage(message) {
        var isCustomer = $("#hdnCustomerEmail").val() === message.FromEmail ? 'blue lighten-1' : 'teal lighten- 2';

        $('#messagesList').append(
            '<li class="card-panel ' + isCustomer + ' white-text padding-10px">' +
            '<div class="col s12 padding-0px">' +
            '<div class="col s8 padding-0px"><b>' + message.FromDisplayName + '</b></div>' +
            '<div class="col s4 padding-0px font-size-12px right-align">' + (new Date(message.MessageDate)).toLocaleString() + '</div>' +
            '</div><br>' + message.Message + '</li>'
        );
    }

    //@* Get the proxy of SignalR Hub and associate client side function. *@
    // $.connection.serviceMessagesHub.client.publishMessage = addMessage;            
    // $.connection.serviceMessagesHub.client.online = updateUserStatus;

    connection.on("publishMessage", function (message) {

        var menssagem = new Object();

        menssagem.FromEmail = message.fromEmail;
        menssagem.PartitionKey = message.partitionKey;
        menssagem.MessageDate = message.messageDate;
        menssagem.Message = message.message;
        menssagem.FromDisplayName = message.fromDisplayName;

        menssagem.CreatedDate = message.createdDate;
        menssagem.IsDeleted = message.isDeleted;
        menssagem.RowKey = message.rowKey;
        menssagem.Timestamp = message.timestamp;
        menssagem.UpdatedDate = message.updatedDate;

        addMessage(menssagem);
    });

    connection.on("online", function (data) {
        updateUserStatus(data);
    });


    //<!--error logging-->
    connection.start().catch(function (err) {
        return console.error(err.tostring());
    });


    //@* Function which will toggle the status of online / offline of users. *@
    function updateUserStatus(data) {
        $('div.chip img[data-id="isAd"]').attr('src', data.isAd ?
            '/images/green_dot.png' : '/images/red_dot.png');

        $('div.chip img[data-id="isCu"]').attr('src', data.isCu ?
            '/images/green_dot.png' : '/images/red_dot.png');

        $('div.chip img[data-id="isSe"]').attr('src', data.isSe ?
            '/images/green_dot.png' : '/images/red_dot.png');
    }

    //@* Unload function to make sure the user is marked as offline. *@    
    $(window).unload(function () {
        $.get('/ServiceRequests/ServiceRequest/MarkOfflineUser',
            function (data, status) {
            });
    });


    //@* Function used to post message to server on keypress *@
    $('#txtMessage').keypress(function (e) {
        var key = e.which;

        if (key === 13) {
            var message = new Object();
            message.Message = $('#txtMessage').val();
            message.PartitionKey = $('#ServiceRequest_RowKey').val();

            $.post('/ServiceRequests/ServiceRequest/CreateServiceRequestMessage',
                { menssagem: message },
                function (data, status, xhr) {
                    if (data) {
                        $('.noMessages').addClass('hide');
                        $('#txtMessage').val('');
                    }
                });
            scrollToLatestMessages();
        }
    });

});
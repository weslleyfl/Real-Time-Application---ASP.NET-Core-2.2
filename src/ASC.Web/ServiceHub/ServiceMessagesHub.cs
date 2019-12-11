//using Microsoft.AspNetCore.SignalR;
//using Microsoft.AspNet.SignalR;
using ASC.Business.Interfaces;
using ASC.Web.Configuration;
using ASC.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Utilities;

namespace ASC.Web.ServiceHub
{
    public class ServiceMessagesHub : Hub
    {
        //private readonly IConnectionManager _signalRConnectionManager;
        private readonly IHubContext<ServiceMessagesHub> _signalRConnectionManager;
        private readonly IHttpContextAccessor _userHttpContext;
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOnlineUsersOperations _onlineUsersOperations;
        private readonly IOptions<ApplicationSettings> _options;
        private static string _serviceRequestId;


        public ServiceMessagesHub(IHubContext<ServiceMessagesHub> signalRConnectionManager,
            IHttpContextAccessor userHttpContext,
            IServiceRequestOperations serviceRequestOperations,
            UserManager<ApplicationUser> userManager,
            IOnlineUsersOperations onlineUsersOperations,
            IOptions<ApplicationSettings> options)
        {
            _signalRConnectionManager = signalRConnectionManager;
            _userHttpContext = userHttpContext;
            _serviceRequestOperations = serviceRequestOperations;
            _userManager = userManager;
            _onlineUsersOperations = onlineUsersOperations;
            _options = options;

            //_serviceRequestId = _userHttpContext.HttpContext.Request.Headers["ServiceRequestId"];            

        }

        public static void GetWebRequest(string serviceRequestId) => _serviceRequestId = serviceRequestId;

        public async override Task OnConnectedAsync()
        {

            if (!string.IsNullOrWhiteSpace(_serviceRequestId))
            {
                await _onlineUsersOperations.CreateOnlineUserAsync(_userHttpContext.HttpContext.User.GetCurrentUserDetails().Email);
                await UpdateServiceRequestClients();
            }

            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            if (!string.IsNullOrWhiteSpace(_serviceRequestId))
            {
                await _onlineUsersOperations.DeleteOnlineUserAsync(_userHttpContext.HttpContext.User.GetCurrentUserDetails().Email);
                await UpdateServiceRequestClients();
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateServiceRequestClients()
        {
            // Get Service Request Details
            var serviceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(_serviceRequestId);

            // Get Customer and Service Engineer names
            var customerName = (await _userManager.FindByEmailAsync(serviceRequest.PartitionKey)).UserName;
            var serviceEngineerName = string.Empty;

            if (!string.IsNullOrWhiteSpace(serviceRequest.ServiceEngineer))
            {
                serviceEngineerName = (await _userManager.FindByEmailAsync(serviceRequest.ServiceEngineer)).UserName;
            }

            var adminName = (await _userManager.FindByEmailAsync(_options.Value.AdminEmail)).UserName;

            // check Admin, Service Engineer and customer are connected.
            var isAdminOnline = await _onlineUsersOperations.GetOnlineUserAsync(_options.Value.AdminEmail);
            var isServiceEngineerOnline = false;

            if (!string.IsNullOrWhiteSpace(serviceRequest.ServiceEngineer))
            {
                isServiceEngineerOnline = await _onlineUsersOperations.GetOnlineUserAsync(serviceRequest.ServiceEngineer);
            }

            var isCustomerOnline = await _onlineUsersOperations.GetOnlineUserAsync(serviceRequest.PartitionKey);

            List<string> users = new List<string>();

            if (isAdminOnline) users.Add(adminName);

            if (!string.IsNullOrWhiteSpace(serviceEngineerName))
            {
                if (isServiceEngineerOnline) users.Add(serviceEngineerName);
            }

            if (isCustomerOnline) users.Add(customerName);

            await _signalRConnectionManager.Clients.All.SendAsync("online", new
            {
                isAd = isAdminOnline,
                isSe = isServiceEngineerOnline,
                isCu = isCustomerOnline
            });

        }


    }
}

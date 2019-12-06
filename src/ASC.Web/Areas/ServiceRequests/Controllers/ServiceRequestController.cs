using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Business.Interfaces;
using ASC.Models.BaseTypes;
using ASC.Models.Models;
using ASC.Web.Areas.ServiceRequests.Models;
using ASC.Web.Data.Cache;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ASC.Utilities;
using ASC.Web.Controllers;
using ASC.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using ASC.Web.Services;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;

        public ServiceRequestController(IServiceRequestOperations operations,
              IMapper mapper,
              IMasterDataCacheOperations masterData,
              UserManager<ApplicationUser> userManager,
              IEmailSender emailSender, ISmsSender smsSender)
        {
            _serviceRequestOperations = operations;
            _mapper = mapper;
            _masterData = masterData;
            _userManager = userManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            MasterDataCache masterData = await _masterData.GetMasterDataCacheAsync();

            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == nameof(MasterKeys.VehicleType)).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == nameof(MasterKeys.VehicleName)).ToList();

            return View(new NewServiceRequestViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var masterData = await _masterData.GetMasterDataCacheAsync();
                ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == nameof(MasterKeys.VehicleType)).ToList();
                ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == nameof(MasterKeys.VehicleName)).ToList();

                return View(request);
            }

            // Map the view model to Azure model
            var serviceRequest = _mapper.Map<NewServiceRequestViewModel, ServiceRequest>(request);

            // Set RowKey, PartitionKey, RequestedDate, Status properties
            serviceRequest.PartitionKey = HttpContext.User.GetCurrentUserDetails().Email;
            serviceRequest.RowKey = Guid.NewGuid().ToString();
            serviceRequest.RequestedDate = request.RequestedDate;
            serviceRequest.Status = nameof(Status.New);

            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);

            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });

        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestDetails(string id)
        {

            var serviceRequestDetails = await _serviceRequestOperations.GetServiceRequestByRowKey(id);

            // Access Check
            if (HttpContext.User.IsInRole(nameof(Roles.Engineer)) &&
                serviceRequestDetails.ServiceEngineer != HttpContext.User.GetCurrentUserDetails().Email)
            {
                throw new UnauthorizedAccessException();
            }

            if (HttpContext.User.IsInRole(nameof(Roles.User)) &&
                serviceRequestDetails.PartitionKey != HttpContext.User.GetCurrentUserDetails().Email)
            {
                throw new UnauthorizedAccessException();
            }

            var serviceRequestAuditDetails = await _serviceRequestOperations.GetServiceRequestAuditByPartitionKey(serviceRequestDetails.PartitionKey + "-" + id);
            // Select List Data
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            ViewBag.Status = Enum.GetValues(typeof(Status)).Cast<Status>().Select(v => v.ToString()).ToList();
            ViewBag.ServiceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());


            return View(new ServiceRequestDetailViewModel
            {
                ServiceRequest = _mapper.Map<ServiceRequest, UpdateServiceRequestViewModel>(serviceRequestDetails),
                ServiceRequestAudit = serviceRequestAuditDetails.OrderByDescending(p => p.Timestamp).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServiceRequestDetails(UpdateServiceRequestViewModel serviceRequest)
        {
            var originalServiceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(serviceRequest.RowKey);
            originalServiceRequest.RequestedServices = serviceRequest.RequestedServices;

            // Exercise 1
            // Send an Email alert to customer when his service request is marked as PendingCustomerApproval either by
            // Admin and Service Engineer.
            var isServiceRequestStatusUpdated = false;

            // Update Status only if user role is either Admin or Engineer
            // Or Customer can update the status if it is only in Pending Customer Approval.
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()) ||
                HttpContext.User.IsInRole(Roles.Engineer.ToString()) ||
                (HttpContext.User.IsInRole(Roles.User.ToString()) && originalServiceRequest.Status == Status.PendingCustomerApproval.ToString()))
            {

                // SMS
                if (originalServiceRequest.Status != serviceRequest.Status)
                {
                    isServiceRequestStatusUpdated = true;
                }

                originalServiceRequest.Status = serviceRequest.Status;
            }

            // Update Service Engineer field only if user role is Admin
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
            {
                originalServiceRequest.ServiceEngineer = serviceRequest.ServiceEngineer;
            }

            await _serviceRequestOperations.UpdateServiceRequestAsync(originalServiceRequest);

            if (HttpContext.User.IsInRole(Roles.Admin.ToString()) ||
                HttpContext.User.IsInRole(Roles.Engineer.ToString()) || originalServiceRequest.Status == Status.PendingCustomerApproval.ToString())
            {
                await _emailSender.SendEmailAsync(originalServiceRequest.PartitionKey,
                        "Your Service Request is almost completed!!!",
                        "Please visit the ASC application and review your Service request.");
            }

            // Send SMS
            if (isServiceRequestStatusUpdated)
            {
                await SendSmsAndWebNotifications(originalServiceRequest);
            }

            return RedirectToAction("ServiceRequestDetails", "ServiceRequest", new { Area = "ServiceRequests", Id = serviceRequest.RowKey });
        }

        private async Task SendSmsAndWebNotifications(ServiceRequest serviceRequest)
        {
            // Send SMS Notification
            var phoneNumber = (await _userManager.FindByEmailAsync(serviceRequest.PartitionKey)).PhoneNumber;

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                await _smsSender.SendSmsAsync(string.Format("+91{0}", phoneNumber),
                            string.Format("Service Request Status updated to {0}", serviceRequest.Status));
            }

            // Get Customer name
            var customerName = (await _userManager.FindByEmailAsync(serviceRequest.PartitionKey)).UserName;


            // Send web notifications
            //_signalRConnectionManager.GetHubContext<ServiceMessagesHub>()
            //   .Clients
            //   .User(customerName)
            //   .publishNotification(new
            //   {
            //       status = serviceRequest.Status
            //   });

        }

        [AcceptVerbs("GET", "POST")]
        //public async Task<IActionResult> CheckDenialService(DateTime requestedDate)
        public async Task<IActionResult> CheckDenialService()
        {
            var serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                                                                    DateTime.UtcNow.AddDays(-90),
                                                                    new List<string>() { nameof(Status.Denied) },
                                                                    HttpContext.User.GetCurrentUserDetails().Email);

            if (serviceRequests.Any())
                return Json(data: $"Há uma solicitação de serviço negada para você nos últimos 90 dias. Entre em contato com o administrador da ASC.");

            return Json(data: true);
        }

        [HttpGet]
        public IActionResult SearchServiceRequests()
        {
            return View(new SearchServiceRequestsViewModel());
        }


        [HttpGet]
        public async Task<IActionResult> SearchServiceRequestResults(string email, DateTime? requestedDate)
        {
            List<ServiceRequest> results = new List<ServiceRequest>();

            if (string.IsNullOrEmpty(email) && (!requestedDate.HasValue))
                return Json(new { data = results });

            if (HttpContext.User.IsInRole(nameof(Roles.Admin)))
                results = await _serviceRequestOperations
                                    .GetServiceRequestsByRequestedDateAndStatus(requestedDate, null, email);
            else
                results = await _serviceRequestOperations
                                    .GetServiceRequestsByRequestedDateAndStatus(requestedDate, null, HttpContext.User.GetCurrentUserDetails().Email);

            return Json(new { data = results.OrderByDescending(p => p.RequestedDate).ToList() });

        }


    }
}
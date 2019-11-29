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

        public ServiceRequestController(IServiceRequestOperations operations,
              IMapper mapper,
              IMasterDataCacheOperations masterData,
              UserManager<ApplicationUser> userManager,
              IEmailSender emailSender)
        {
            _serviceRequestOperations = operations;
            _mapper = mapper;
            _masterData = masterData;
            _userManager = userManager;
            _emailSender = emailSender;
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
    }
}
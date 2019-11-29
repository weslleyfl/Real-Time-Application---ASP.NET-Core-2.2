using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Models.Models;
using ASC.Models.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Business
{
    public class ServiceRequestOperations : IServiceRequestOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateServiceRequestAsync(ServiceRequest request)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ServiceRequest>().AddAsync(request);
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task<ServiceRequest> UpdateServiceRequestAsync(ServiceRequest request)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ServiceRequest>().UpdateAsync(request);
                _unitOfWork.CommitTransaction();

                return request;
            }
        }

        public async Task<ServiceRequest> UpdateServiceRequestStatusAsync(string rowKey, string partitionKey, string status)
        {
            using (_unitOfWork)
            {
                var serviceRequest = await _unitOfWork.Repository<ServiceRequest>().FindAsync(partitionKey, rowKey);

                if (serviceRequest == null)
                    throw new NullReferenceException();

                serviceRequest.Status = status;

                await _unitOfWork.Repository<ServiceRequest>().UpdateAsync(serviceRequest);
                _unitOfWork.CommitTransaction();

                return serviceRequest;

            }
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByRequestedDateAndStatus(DateTime? requestedDate, List<string> status = null, string email = "", string serviceEngineerEmail = "")
        {
            var query = Queries.GetDashboardQuery(requestedDate, status, email, serviceEngineerEmail);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByQuery(query);
            return serviceRequests.ToList();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsFormAudit(string serviceEngineerEmail = "")
        {
            var query = Queries.GetDashboardAuditQuery(serviceEngineerEmail);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllInAuditByQuery(query);
            return serviceRequests.ToList();
        }

        public async Task<List<ServiceRequest>> GetActiveServiceRequests(List<string> status)
        {
            var query = Queries.GetDashboardServiceEngineersQuery(status);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByQuery(query);
            return serviceRequests.ToList();

        }
    }
}

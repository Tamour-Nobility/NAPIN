using System;
using System.Collections.Generic;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly IDashboardRepository _dashboardService;
        public DashboardController(IDashboardRepository dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public ResponseModel GetDashboardData(long practiceCode,string fromDate,string toDate)
        {
            return _dashboardService.GetDashboardData(practiceCode,fromDate,toDate, GetUserId());
        }

        [HttpGet]
        public ResponseModel GetExternalPractices()
        {
            return _dashboardService.GetExternalPractices();
        }
       

    }
}
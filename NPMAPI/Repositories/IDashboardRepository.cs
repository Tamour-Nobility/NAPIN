using NPMAPI.Models;
using System;
using System.Collections.Generic;

namespace NPMAPI.Repositories
{
    public interface IDashboardRepository
    {
        ResponseModel GetDashboardData(long practiceCode, string fromDate, string toDate, long userId);

        ResponseModel GetExternalPractices();
    }

    
}
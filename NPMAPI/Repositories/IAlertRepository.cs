using NPMAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMAPI.Repositories
{
    public interface IAlertRepository
    {

        ResponseModel SaveAlert(NpmAlert model, long userId,string userName);
        ResponseModel GetAlertForPatient(long patientaccount);
        ResponseModel GetLoginAlert();
        ResponseModel SaveLoginAlert(NpmAlert model, long userId, string userName);

    }
}

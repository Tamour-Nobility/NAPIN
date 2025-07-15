using NPMAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMAPI.Repositories
{
    internal interface IClaimAssignmentRepository
    {

        ResponseModel GetSelectedUserList(long practicecode);
    }
}

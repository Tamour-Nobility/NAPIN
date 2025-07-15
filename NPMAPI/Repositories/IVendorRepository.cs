using System.Collections.Generic;
using NPMAPI.Models;

namespace NPMAPI.Repositories
{
    public interface IVendorRepository
    {
        List<SelectListViewModel> GetVendor();

    }
}
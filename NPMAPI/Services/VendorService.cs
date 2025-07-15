using System.Collections.Generic;
using System.Linq;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class VendorService : IVendorRepository
    {
        public List<SelectListViewModel> GetVendor()
        {

            using (var ctx = new NPMDBEntities())
            {
                return ctx.Vendors
                    .Where(v => v.IsActive == true && v.IsDeleted == false)
                    .Select(vendor =>
                     new SelectListViewModel()
                     {
                         Id = vendor.VendorId,
                         Name = vendor.VendorName
                     }
                 ).ToList();
            }




        }


    }
}
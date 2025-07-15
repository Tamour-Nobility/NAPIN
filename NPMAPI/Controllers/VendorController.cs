using System.Collections.Generic;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class VendorController : BaseController
    {
        private readonly IVendorRepository _vendorService;
        public VendorController(IVendorRepository vendorService)
        {
            _vendorService = vendorService;

        }

        [HttpGet]
        public List<SelectListViewModel> GetVendorList()
        {
            return _vendorService.GetVendor();
        }

    }
}

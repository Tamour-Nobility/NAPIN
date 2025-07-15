using NPMAPI;
using NPMAPI.Models;
using NPMAPI.Repositories;
using System;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

public class ReferralPhysiciansService : IReferralPhysicianRepository
{
    public ReferralPhysiciansService()
    {
    }

    public async Task<ResponseModel> CreateReferralPhysician(ReferralPhysicianViewModel model, long userId)
    {
        var responseModel = new ResponseModel();
        try
        {
            using (var ctx = new NPMDBEntities())
            {
                var refPhy = new Referral_Physicians();
                refPhy.Created_Date = DateTimeOffset.Now;
                refPhy.Created_By = userId;
                refPhy.Deleted = false;
                refPhy.Pin = model.Pin;
                refPhy.Modified_By = userId;
                refPhy.Referral_License = model.Referral_License;
                refPhy.Exported = model.Exported;
                refPhy.Export_Date = null;
                refPhy.In_Active = false;
                refPhy.Modified_Date = DateTimeOffset.Now;
                refPhy.NPI = model.NPI;
                refPhy.Referral_Mi = model.Referral_Mi;
                refPhy.Referral_Phone = model.Referral_Phone;
                refPhy.Referral_Lname = model.Referral_Lname;
                refPhy.Referral_Fname = model.Referral_Fname;
                refPhy.Referral_Fax = model.Referral_Fax;
                refPhy.Referral_Email = model.Referral_Email;
                refPhy.Recent_Use = model.Recent_Use;
                refPhy.Referral_Address = model.Referral_Address;
                refPhy.Referral_City = model.Referral_City;
                refPhy.Referral_Code = model.Referral_Code;
                refPhy.Referral_Contact_Person = model.Referral_Contact_Person;
                refPhy.Referral_Ssn = model.Referral_Ssn;
                refPhy.Referral_State = model.Referral_State;
                refPhy.Referral_Taxonomy_Code = model.Referral_Taxonomy_Code;
                refPhy.Referral_Tax_Id = model.Referral_Tax_Id;
                refPhy.Referral_Upin = model.Referral_Upin;
                refPhy.Referral_Zip = model.Referral_Zip;
                refPhy.Title = model.Title;


                long refCode = Convert.ToInt64(ctx.SP_TableIdGenerator("Referral_Code").FirstOrDefault().ToString());

                refPhy.Referral_Code = refCode;

                ctx.Referral_Physicians.Add(refPhy);
                await ctx.SaveChangesAsync();

                responseModel.Status = "success";
                responseModel.Response = "Referral physician added";
            }
        }
        catch (DbEntityValidationException ex)
        {
            var validationErrors = ex.EntityValidationErrors.SelectMany(eve => eve.ValidationErrors);
            var errorMessages = validationErrors.Select(ve => ve.ErrorMessage);
            var detailedMessage = string.Join(Environment.NewLine, errorMessages);

            responseModel.Status = "error";
            responseModel.Response = "Validation failed: " + detailedMessage;
        }
        return responseModel;
    }

    public async Task<ResponseModel> ChangeDeleteStatus(long refCode)
    {
        using (var ctx = new NPMDBEntities())
        {
            var refPhy = ctx.Referral_Physicians.FirstOrDefault(x => x.Referral_Code == refCode);
            if (refPhy != null)
            {
                if (refPhy.Deleted.HasValue)
                {
                    refPhy.Deleted = !refPhy.Deleted;
                }
                else
                {
                    refPhy.Deleted = true;
                }
                refPhy.Modified_Date = DateTimeOffset.Now;

                await ctx.SaveChangesAsync();

                var responseModel = new ResponseModel
                {
                    Response = "delete status changed",
                    Status = "success"
                };
                return responseModel;
            }
            else
            {
                var responseModel = new ResponseModel
                {
                    Response = "Referral physician not found",
                    Status = "failure"
                };
                return responseModel;
            }
        }
    }

    public async Task<ResponseModel> ChangeActiveStatus(long refCode)
    {
        using (var ctx = new NPMDBEntities())
        {
            var refPhy = ctx.Referral_Physicians.FirstOrDefault(x => x.Referral_Code == refCode);
            if (refPhy != null)
            {
                if (refPhy.In_Active.HasValue)
                {
                    refPhy.In_Active = !refPhy.In_Active;
                }
                else
                {
                    refPhy.In_Active = true;
                }
                refPhy.Modified_Date = DateTimeOffset.Now;

                await ctx.SaveChangesAsync();

                var responseModel = new ResponseModel
                {
                    Response = "Activity status changed",
                    Status = "success"
                };
                return responseModel;
            }
            else
            {
                var responseModel = new ResponseModel
                {
                    Response = "Referral physician not found",
                    Status = "failure"
                };
                return responseModel;
            }
        }
    }

    public async Task<ResponseModel> GetReferralPhysician(long refCode)
    {
        var responseModel = new ResponseModel();
        try
        {
            using (var ctx = new NPMDBEntities())
            {
                var referralPhysician = await ctx.Referral_Physicians.FirstOrDefaultAsync(x => x.Referral_Code == refCode);

                var specialityGroupNo = (long)0;
                if (!string.IsNullOrEmpty(referralPhysician.Referral_Taxonomy_Code))
                {
                    specialityGroupNo = await ctx.Specialty_Category
                   .Where(x => x.TAXONOMY_CODE == referralPhysician.Referral_Taxonomy_Code)
                   .Select(s => s.GROUP_NO)
                   .FirstOrDefaultAsync();
                }
                if (referralPhysician != null)
                {
                    ReferralPhysicianViewModel viewModel = new ReferralPhysicianViewModel();

                    viewModel.Referral_Code = referralPhysician.Referral_Code;
                    viewModel.Referral_Lname = referralPhysician.Referral_Lname;
                    viewModel.Referral_Fname = referralPhysician.Referral_Fname;
                    viewModel.Referral_Mi = referralPhysician.Referral_Mi;
                    viewModel.Referral_Address = referralPhysician.Referral_Address;
                    viewModel.Referral_City = referralPhysician.Referral_City;
                    viewModel.Referral_State = referralPhysician.Referral_State;
                    viewModel.Referral_Zip = referralPhysician.Referral_Zip;
                    viewModel.Referral_Phone = referralPhysician.Referral_Phone;
                    viewModel.Referral_Contact_Person = referralPhysician.Referral_Contact_Person;
                    viewModel.Referral_Tax_Id = referralPhysician.Referral_Tax_Id;
                    viewModel.Referral_License = referralPhysician.Referral_License;
                    viewModel.Referral_Upin = referralPhysician.Referral_Upin;
                    viewModel.Referral_Ssn = referralPhysician.Referral_Ssn;
                    viewModel.Exported = referralPhysician.Exported.HasValue ? referralPhysician.Exported.Value : false;
                    viewModel.Recent_Use = referralPhysician.Recent_Use.HasValue ? referralPhysician.Recent_Use.Value : false;
                    viewModel.Export_Date = referralPhysician.Export_Date.HasValue ? referralPhysician.Export_Date.Value : default(DateTime);
                    viewModel.Deleted = referralPhysician.Deleted.HasValue ? referralPhysician.Deleted.Value : false;
                    viewModel.Created_Date = referralPhysician.Created_Date.HasValue ? referralPhysician.Created_Date.Value : default(DateTimeOffset);
                    viewModel.Modified_By = referralPhysician.Modified_By.HasValue ? referralPhysician.Modified_By.Value : 0;
                    viewModel.Modified_Date = referralPhysician.Modified_Date.HasValue ? referralPhysician.Modified_Date.Value : default(DateTimeOffset);
                    viewModel.Pin = referralPhysician.Pin;
                    viewModel.NPI = referralPhysician.NPI;
                    viewModel.Referral_Fax = referralPhysician.Referral_Fax;
                    viewModel.Title = referralPhysician.Title;
                    viewModel.Referral_Email = referralPhysician.Referral_Email;
                    viewModel.In_Active = referralPhysician.In_Active.HasValue ? referralPhysician.In_Active.Value : false;
                    viewModel.Referral_Taxonomy_Code = referralPhysician.Referral_Taxonomy_Code;

                    viewModel.SpecialityGroupNo = specialityGroupNo;


                    responseModel.Status = "success";
                    responseModel.Response = viewModel;
                }
                else
                {
                    responseModel.Status = "failure";
                    responseModel.Response = "Referral physician not found";
                }
            }
        }
        catch (Exception ex)
        {
            responseModel.Status = "failure";
            responseModel.Response = "An error occurred while retrieving the referral physician.";
        }

        return responseModel;
    }
    public async Task<ResponseModel> GetActiveReferralPhysicians()
    {
        var resposneModel = new ResponseModel();
        using (var db = new NPMDBEntities())
        {
            var refPhy = await db.Referral_Physicians.Where(x => x.In_Active == null || x.In_Active == false && (x.Deleted.HasValue && x.Deleted == false))
                .Select(s => new
                {
                    s.Referral_Code,
                    s.Referral_Fname,
                    s.Referral_Lname
                })
                .ToListAsync();

            resposneModel.Status = "success";
            resposneModel.Response = refPhy;

            return resposneModel;
        }
    }

    public async Task<ResponseModel> GetActiveInactiveReferralPhysicians()
    {
        var resposneModel = new ResponseModel();
        using (var db = new NPMDBEntities())
        {
            var refPhy = await db.Referral_Physicians
                .Where(x => x.Deleted.HasValue && x.Deleted == false)
                .Select(s => new
                {
                    s.Referral_Code,
                    s.Referral_Fname,
                    s.Referral_Lname
                })
                .ToListAsync();

            resposneModel.Status = "success";
            resposneModel.Response = refPhy;

            return resposneModel;
        }
    }

    public async Task<ResponseModel> GetReferralPhysicians(int page, int count, string pattern)
    {
        var responseModel = new ResponseModel();
        var pagingResponse = new PagingResponse();
        try
        {
            using (var ctx = new NPMDBEntities())
            {
                var query = ctx.Referral_Physicians.AsQueryable();
                if (!string.IsNullOrEmpty(pattern))
                {
                
                    //query = query.Where(x => x.Referral_Fname.Contains(pattern) ||
                    //                     x.Referral_Lname.Contains(pattern) ||
                    //                     x.Referral_Taxonomy_Code.Contains(pattern) ||
                    //                     x.NPI.Contains(pattern));

                    //above code replaced with below to handel referal code search
                    query = query.Where(x => x.Referral_Fname.Contains(pattern) ||
                                         x.Referral_Lname.Contains(pattern) ||
                                         x.Referral_Taxonomy_Code.Contains(pattern) ||
                                         x.NPI.Contains(pattern) ||
                                         x.Referral_Code.ToString().Contains(pattern));
                }
                query = query.Where(x => x.Deleted.HasValue && x.Deleted == false);
                query = query.OrderByDescending(s => s.Created_Date);

                // Calculate the total count before applying pagination
                pagingResponse.TotalRecords = query.Count();

                // Calculate the number of records to skip based on the page number and count.
                int skipCount = (page - 1) * count;
                query = query.Skip(skipCount).Take(count);

                pagingResponse.FilteredRecords = await query.CountAsync(); // Count after pagination
                pagingResponse.CurrentPage = page;
                pagingResponse.data = await query.Select(s => new
                {
                    s.Referral_Code,
                    s.Referral_Fname,
                    s.Referral_Lname,
                    s.Referral_Address,
                    s.Referral_State,
                    s.In_Active,
                    s.Deleted,
                    s.Created_Date,
                    s.Modified_Date,
                    s.Referral_City,
                    s.NPI,
                    s.Referral_Taxonomy_Code
                }).ToListAsync();

                responseModel.Status = "success";
                responseModel.Response = pagingResponse;
            }
        }
        catch (Exception ex)
        {
            responseModel.Status = "failure";
            responseModel.Response = "An error occurred while retrieving the referral physician(s)";
        }
        return responseModel;
    }

    public async Task<ResponseModel> UpdateReferralPhysician(ReferralPhysicianViewModel model, long userId)
    {
        var responseModel = new ResponseModel();
        try
        {
            using (var ctx = new NPMDBEntities())
            {
                var refPhy = ctx.Referral_Physicians.Where(x => x.Referral_Code == model.Referral_Code).FirstOrDefault();
                if (refPhy != null)
                {
                    refPhy.Pin = model.Pin;
                    refPhy.Modified_By = userId;
                    refPhy.Referral_License = model.Referral_License;
                    refPhy.Exported = model.Exported;
                    refPhy.Export_Date = null;
                    refPhy.Modified_Date = DateTimeOffset.Now;
                    refPhy.NPI = model.NPI;
                    refPhy.Referral_Mi = model.Referral_Mi;
                    refPhy.Referral_Phone = model.Referral_Phone;
                    refPhy.Referral_Lname = model.Referral_Lname;
                    refPhy.Referral_Fname = model.Referral_Fname;
                    refPhy.Referral_Fax = model.Referral_Fax;
                    refPhy.Referral_Email = model.Referral_Email;
                    refPhy.Recent_Use = model.Recent_Use;
                    refPhy.Referral_Address = model.Referral_Address;
                    refPhy.Referral_City = model.Referral_City;
                    refPhy.Referral_Code = model.Referral_Code;
                    refPhy.Referral_Contact_Person = model.Referral_Contact_Person;
                    refPhy.Referral_Ssn = model.Referral_Ssn;
                    refPhy.Referral_State = model.Referral_State;
                    refPhy.Referral_Taxonomy_Code = model.Referral_Taxonomy_Code;
                    refPhy.Referral_Tax_Id = model.Referral_Tax_Id;
                    refPhy.Referral_Upin = model.Referral_Upin;
                    refPhy.Referral_Zip = model.Referral_Zip;
                    refPhy.Title = model.Title;

                    ctx.Entry(refPhy).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();

                    responseModel.Status = "success";
                    responseModel.Response = "Referral physician updated";
                }
                else
                {
                    responseModel.Status = "error";
                    responseModel.Response = "referral physician does not exist";
                }
            }
        }
        catch (Exception ex)
        {
            responseModel.Status = "failure";
            responseModel.Response = "An error occurred while updating the referral physician.";
        }
        return responseModel;
    }
}

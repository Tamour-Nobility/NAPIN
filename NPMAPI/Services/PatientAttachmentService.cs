using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class PatientAttachmentService : IPatientAttachment
    {
        private readonly NPMDBEntities _db;

        public PatientAttachmentService(NPMDBEntities db)
        {
            _db = db;
        }

        public ResponseModel Delete(long Id, long userId)
        {
            try
            {
                var attachment = _db.Patient_Attachments.Where(a => a.Patient_Attachment_Id == Id && !(a.Deleted ?? false)).FirstOrDefault();
                if (attachment != null)
                {
                    attachment.Deleted = true;
                    attachment.ModifiedBy = userId;
                    attachment.ModifiedDate = DateTime.Now;
                    _db.SaveChanges();
                    return new ResponseModel()
                    {
                        Status = "success",
                    };
                }
                else
                    return new ResponseModel()
                    {
                        Status = "Failure",
                        Response = "Attachment not found"
                    };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel GetAll(long patientAccount)
        {
            try
            {
                var results = _db.uspGetPatientAttachments(patientAccount).ToList();
                return new ResponseModel()
                {
                    Status = "success",
                    Response = results
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel Save(CreateAttachmentRequest request, long userId)
        {
            try
            {
                    var attachment = new Patient_Attachments()
                    {
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        ModifiedBy = null,
                        ModifiedDate = null,
                        Patient_Attachment_Id = Convert.ToInt64(_db.SP_TableIdGenerator("Patient_Attachment_Id").FirstOrDefault().ToString()),
                        FilePath = request.FilePath,
                        FileName = request.FileName,
                        Patient_Account = request.Patient_Account,
                        Attachment_TypeCode_Id = request.Attachment_TypeCode_Id.ToString()
                    };
                    _db.Patient_Attachments.Add(attachment);
                    if (_db.SaveChanges() > 0)
                        return new ResponseModel() { Status = "success", Response = attachment };
                    else
                        return new ResponseModel() { Status = "Failure", Response = "Unable to save file" };          
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ResponseModel GetAttachmentTypeCodesList()
        {
            try
            {
                var codes = _db.Attachment_TypeCode.Where(a => !(a.Deleted ?? false)).Select(a => new SelectListViewModel()
                {
                    IdStr = a.TypeCode_Id,
                    Name = a.Description
                }).OrderBy(a => a.IdStr).ToList();
                return new ResponseModel()
                {
                    Status = "success",
                    Response = codes
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel Get(long attachmentId)
        {
            try
            {
                return new ResponseModel()
                {
                    Status = "success",
                    Response = _db.Patient_Attachments.Where(pt => pt.Patient_Attachment_Id == attachmentId && !(pt.Deleted ?? false)).FirstOrDefault()
                };
            }
            catch (Exception)
            {
                throw;
            }
        }


        public ResponseModel SaveTicketAttachment(CreateAttachmentRequest request, long userId)
        {
            try
            {
                    var attachment = new Ticket_attachments()
                    {
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        ModifiedBy = null,
                        ModifiedDate = null,
                        FilePath = request.FilePath,
                        FileName = request.FileName,
                        Ticket_Id = request.Ticket_id
                    };
                    _db.Ticket_attachments.Add(attachment);
                    if (_db.SaveChanges() > 0)
                        return new ResponseModel() { Status = "success", Response = attachment };
                    else
                        return new ResponseModel() { Status = "Failure", Response = "Unable to save file" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
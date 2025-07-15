using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Services
{
    public class TicketAttachmentsService : ITicketAttachment
    {
        private readonly NPMDBEntities _db;
        public TicketAttachmentsService(NPMDBEntities db)
        {
            _db = db;
        }

        //public ResponseModel Save(CreateAttachmentRequest request, long userId)
        //{
        //    try
        //    {
        //        var attachment = new Ticket_attachments
        //        {
        //            Ticket_Attachment_Id = Convert.ToInt64(_db.SP_TableIdGenerator("Ticket_Attachment_Id").FirstOrDefault()),
        //            FileName = request.FileName,
        //            FilePath = request.FilePath,
        //            Deleted = false,
        //            CreatedBy = userId,
        //            CreatedDate = DateTimeOffset.Now,
        //            ModifiedBy = null,
        //            ModifiedDate = null
        //        };
        //        _db.Ticket_attachments.Add(attachment);
        //        if (_db.SaveChanges() > 0)
        //        {
        //            return new ResponseModel
        //            {
        //                Status = "success",
        //                Response = attachment
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseModel
        //            {
        //                Status = "Failure",
        //                Response = "Unable to save file"
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel
        //        {
        //            Status = "Error",
        //            Response = ex.Message
        //        };
        //    }

        //}
    }
}
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using System.Threading.Tasks;

namespace NPMAPI.Repositories
{
    public interface IPatientAttachment
    {
        ResponseModel Save(CreateAttachmentRequest request, long userId);
        ResponseModel Delete(long Id, long userId);
        ResponseModel GetAll(long patientAccount);
        ResponseModel Get(long attachmentId);
        ResponseModel GetAttachmentTypeCodesList();
        ResponseModel SaveTicketAttachment(CreateAttachmentRequest request, long userId);

    }
}

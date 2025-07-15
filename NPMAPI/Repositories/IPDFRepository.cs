using NPMAPI.Models;

namespace NPMAPI.Repositories
{
    public interface IPDFRepository
    {
        byte[] GenerateItemizedPatientStatment(long patientAccount, long practiceCode, long userId, string messageToPrint);
        byte[] GenerateItemizedPsForDownload(StatmentDownloadRequestModel model, long userId );
        byte[] GenerateRollingFordowloadForDownload(string prac_code, string duration );


        ResponseModel AddMessage(PatientStatementMessages Model, long userid);
        //ResponseModel DeleteMessage(int Id );

        ResponseModel EditMessage(PatientStatementMessages Model, long userid);

        ResponseModel GetAllMessages();

        ResponseModel DeleteMessage(long MessageID);
    }
}

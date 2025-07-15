namespace NPMAPI.Repositories
{
    public interface IFTP
    {
        void upload(string host, int port, string username, string password, string source, string destination);
    }
}

using System;
using System.IO;
using NPMAPI.Repositories;
using Renci.SshNet;

namespace NPMAPI.Services
{
    public class FTPService : IFTP
    {
        public void upload(string host, int port, string username, string password, string source, string destination)
        {
            try
            {
                using (SftpClient client = new SftpClient(host, port, username, password))
                {
                    client.Connect();
                    if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
                        client.ChangeDirectory(destination);
                    using (FileStream fs = new FileStream(source, FileMode.Open))
                    {
                        client.BufferSize = 4 * 1024;
                        client.UploadFile(fs, Path.GetFileName(source));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Threading;
using NPMAPI.Models;
using iTextSharp.text.pdf.qrcode;
using System.Web.Services.Description;

namespace NPMAPI
{
    internal class EncryptionHandler : DelegatingHandler
    {

        protected async override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            List<string> excludeURL = GetExcludeURLList();
            if (!excludeURL.Contains(request.RequestUri.LocalPath)){

                if (request.Method == HttpMethod.Post)
                {
                    if (await request.Content.ReadAsStringAsync() != null)
                    {
                        var c = DecryptString(await request.Content.ReadAsStringAsync());
                        var newContent = new StringContent(c, Encoding.UTF8, "text/json");
                        request.Content = newContent;
                    }

                }
                else if (request.Method == HttpMethod.Get)
                {
                    var d = request.RequestUri.Query;

                    if (d.Length > 0)
                    {
                        var c = DecryptString(d.Remove(0, 1));
                        var newC = c.Remove(0, 1);
                        var newCC = newC.Remove(newC.Length - 1, 1);
                        var newString = request.RequestUri.OriginalString.Split('/');
                        var newURL = newString[0] + "//" + request.RequestUri.Authority + request.RequestUri.AbsolutePath + "?" + newCC;
                        request.RequestUri = new Uri(newURL);

                    }

                }
            }
          






            // Call the inner handler./

          var response = await base.SendAsync(request, cancellationToken);
      // if(request.Method== HttpMethod.Post || request.Method == HttpMethod.Get)
       //   {
         //     if(response.IsSuccessStatusCode == true)
         // {//
            // var c = Encrypt(await response.Content.ReadAsStringAsync());
              //var newContent = new StringContent(c, Encoding.UTF8 , "text/plain");
           //  response.Content = newContent;
         //  }

        //  }
           
            return response;
        }

        // This function is not needed but if we want anything to encrypt then we can use
        private CryptoStream EncryptStream(Stream responseStream)
        {
            Aes aes = GetEncryptionAlgorithm();
            ToBase64Transform base64Transform = new ToBase64Transform();
            CryptoStream base64EncodedStream = new CryptoStream(responseStream, base64Transform, CryptoStreamMode.Write);
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            CryptoStream cryptoStream = new CryptoStream(base64EncodedStream, encryptor, CryptoStreamMode.Write);
            return cryptoStream;
        }
        static string Encrypt(string plainText)
        {
            byte[] encrypted;
            using (AesManaged aes = new AesManaged())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
           var encrypteds= String.Join(" ", encrypted);
           string str = Encoding.UTF8.GetString(encrypted);


            return Convert.ToString(encrypted) ;
        }
        // This are main functions that we decrypt the payload and  parameter which we pass from the angular service.
        private Stream DecryptStream(Stream cipherStream)
        {
            Aes aes = GetEncryptionAlgorithm();
            FromBase64Transform base64Transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
            CryptoStream base64DecodedStream = new CryptoStream(cipherStream, base64Transform, CryptoStreamMode.Read);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            CryptoStream decryptedStream = new CryptoStream(base64DecodedStream, decryptor, CryptoStreamMode.Read);
            return decryptedStream;
        }
        private string DecryptString(string cipherText)
        {
            Aes aes = GetEncryptionAlgorithm();
            byte[] buffer = Convert.FromBase64String(cipherText);
            MemoryStream memoryStream = new MemoryStream(buffer);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }
        // We have to use same KEY and IV as we use for encryption in angular side.
        // _appSettings.EncryptKey= 1203199320052021
        // _appSettings.EncryptIV = 1203199320052021
        private Aes GetEncryptionAlgorithm()
        {
            Aes aes = Aes.Create();
            var secret_key = Encoding.UTF8.GetBytes("1203199320052021");
            var initialization_vector = Encoding.UTF8.GetBytes("1203199320052021");
            aes.Key = secret_key;
            aes.Key = secret_key;
            aes.IV = initialization_vector;
            return aes;
        }
        // This are excluded URL from encrypt- decrypt that already we added in angular side and as well as in ASP.NET CORE side.
        private List<string> GetExcludeURLList()
        {
            List<string> excludeURL = new List<string>();
            excludeURL.Add("/api/InboxHealth/Webhook/");
            excludeURL.Add("/api/patientattachments/Attach");
            excludeURL.Add("/api/patientattachments/UploadFiles");
            excludeURL.Add("/api/patientattachments/GetAll");
            excludeURL.Add("/api/patientattachments/GetAttachmentCodeList");
            excludeURL.Add("/api/scrubber/getAllViolations");
            excludeURL.Add("/api/scrubber/getAllCleanClaims");
            excludeURL.Add("/api/scrubber/PutClaim");
            excludeURL.Add("/api/ERA/Import");
            excludeURL.Add("/api/ERA/WeekHistoryOfERA");
            excludeURL.Add("/api/Dashboard/GetExternalPractices");
            return excludeURL;
        }
    }
}
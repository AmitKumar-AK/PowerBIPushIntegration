using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;

namespace Sitecore.XConnect.ServicePlugins.InteractionsTracker
{
    public class PowerBIAdapter : IDisposable
    {
        private readonly string clientID;
        private readonly string clientSecret;
        private readonly string groupID;
        private readonly string datasetID;
        private readonly string token;
        private readonly string userEmail;
        private readonly string userPassword;
        private readonly string powerBIResourceUri;
        private readonly string powerBIAuthorityUri;
        private readonly string powerBIApiPostUrl;

        public PowerBIAdapter()
        {
            clientID = ConfigurationManager.AppSettings["PowerBIClientID"];
            clientSecret = ConfigurationManager.AppSettings["PowerBIClientSecret"];
            groupID = ConfigurationManager.AppSettings["PowerBIGroupID"];
            datasetID = ConfigurationManager.AppSettings["PowerBIDatasetID"];
            userEmail = ConfigurationManager.AppSettings["PowerBIUserEmail"];
            userPassword = ConfigurationManager.AppSettings["PowerBIUserPassword"];
            powerBIResourceUri = ConfigurationManager.AppSettings["PowerBIResourceUri"];
            powerBIAuthorityUri = ConfigurationManager.AppSettings["PowerBIAuthorityUri"];
            powerBIApiPostUrl = ConfigurationManager.AppSettings["PowerBIApiAddRowsUrl"];

            token = GetToken();
        }

        private string GetToken()
        {
            //Get access token:
            // To call a Power BI REST operation, create an instance of AuthenticationContext and call AcquireToken
            // AuthenticationContext is part of the Active Directory Authentication Library NuGet package
            // To install the Active Directory Authentication Library NuGet package in Visual Studio,
            //  run "Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory" from the nuget Package Manager Console.

            // AcquireToken will acquire an Azure access token
            // Call AcquireToken to get an Azure token from Azure Active Directory token issuance endpoint
            AuthenticationContext authContext = new AuthenticationContext(powerBIAuthorityUri);

            var userCredential = new UserPasswordCredential(userEmail, userPassword);

            Task<AuthenticationResult> task = authContext.AcquireTokenAsync(powerBIResourceUri, clientID, userCredential);

            task.Wait();

            string token = task.Result.AccessToken;

            return token;
        }

        public void PushRow<T>(T data, string tableName)
        {
            var json = (JObject)JToken.FromObject(data);

            string rowsJson = "{\"rows\":[" + json + "]}";

            SendData(rowsJson, tableName);
        }

        public void PushRows<T>(IEnumerable<T> data, string tableName)
        {
            var json = (JArray)JToken.FromObject(data);

            string rowsJson = "{\"rows\":" + json + "}";

            SendData(rowsJson, tableName);
        }

        private void SendData(string json, string tableName)
        {

            string powerBIApiAddRowsUrl = String.Format(powerBIApiPostUrl, groupID, datasetID, tableName);

            //POST web request to add rows.
            //Change request method to "POST"
            HttpWebRequest request = System.Net.WebRequest.Create(powerBIApiAddRowsUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentLength = 0;
            request.ContentType = "application/json";

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            //POST web request
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json.ToString());
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);

                var response = (HttpWebResponse)request.GetResponse();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;


namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        private static CloudQueue queue;

        [WebMethod]
        public string CreateAccountInWorkerRole(string username, string phrase)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference("password");
            queue.CreateIfNotExists();

            //add message
            CloudQueueMessage message = new CloudQueueMessage(username + " " + phrase);
            queue.AddMessage(message);
            return "done";
        }


        [WebMethod]
        public string GetPasswordFromTableStorage(string username, string reversePhrase)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
               CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("reverse");
            TableOperation retrieveOperation = TableOperation.Retrieve<Password>(username, reversePhrase);
            TableResult retrievedResult = table.Execute(retrieveOperation);

            return ((Password)retrievedResult.Result).password;
        }

        [WebMethod]
        public int CountPasswords()
        {
            int count = 0;
            return count;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Table;
using System.Security.Cryptography;
using System.Text;
using WebRole1;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private static CloudQueue queue;
        private static CloudTable table;

        public override void Run()
        {
            while (true)
            {
                Thread.Sleep(100);
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference("password");


                //read and remove msg
                CloudQueueMessage message = queue.GetMessage(TimeSpan.FromMinutes(5));
                if (message != null)
                {
                    string msg = "" + message.AsString;
                    queue.DeleteMessage(message);
                    string[] words = msg.Split(' ');
                    string username = words[0];
                    string phrase = words[1];
                    string reversePhrase = Reverse(phrase);

                    string password = (username + "info344");
                    //MD5 md5 = System.Security.Cryptography.MD5.Create();

                    // byte array representation of that string
                    byte[] encodedPassword = new UTF8Encoding().GetBytes(password);

                    // need MD5 to calculate the hash
                    byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

                    // string representation (similar to UNIX format)
                    string encoded = BitConverter.ToString(hash)
                       // without dashes
                       .Replace("-", string.Empty)
                       // make lowercase
                       .ToLower();


                    Password p = new Password(username, encoded, reversePhrase);

                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    table = tableClient.GetTableReference("reverse");
                    //table.DeleteIfExists();  
                    table.CreateIfNotExists();

                    TableOperation insertOperation = TableOperation.Insert(p);
                    table.Execute(insertOperation);
                }
            }
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}

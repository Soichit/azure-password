using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebRole1
{
    public class Password : TableEntity
    {

        public string username { get; set; }
        public string password { get; set; }
        public string reversePhrase { get; set; }

        public Password(string username, string password, string reversePhrase)
        {
            this.PartitionKey = password;
            this.RowKey = reversePhrase;
            this.username = username;
            this.password = password;
            this.reversePhrase = reversePhrase;
        }

        public Password() { }

    }
}
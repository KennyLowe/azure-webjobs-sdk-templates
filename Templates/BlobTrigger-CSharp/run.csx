public static void Run(Stream myBlob, string name, TraceWriter log)

{
    
	log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

}

        public static void QueueBackup([Queue("backupqueue")] ICollector<string> message, TextWriter log)
        {
            //extract the storage account names and keys
            string sourceUri, destUri, sourceKey, destKey;
            GetAcctInfo("Source-Account", "Backup-Account", out sourceUri, out destUri, out sourceKey, out destKey);
 
            //create a job name to make it easier to trace this through the WebJob logs.
            string jobName = "Full Backup";
 
            //Use time stamps to create unique container names for weekly backup
            string datestamp = DateTime.Today.ToString("yyyyMMdd");
 
            //set backup type either "full" or "incremental"
            string backup = "full";
 
            //Add the json from CreateJob() to the WebJobs queue, pass in the Container name for each call
            message.Add(CreateJob(jobName, "images", datestamp, sourceUri, destUri, sourceKey, destKey, backup, log));
            message.Add(CreateJob(jobName, "stuff", datestamp, sourceUri, destUri, sourceKey, destKey, backup, log));
        }
        public static void GetAcctInfo(string from, string to, out string sourceUri, out string destUri, out string sourceKey, out string destKey)
        {
            //Get the Connection Strings for the Storage Accounts to copy from and to
            string source = ConfigurationManager.ConnectionStrings[from].ToString();
            string dest = ConfigurationManager.ConnectionStrings[to].ToString();
 
            //Split the connection string along the semi-colon
            string sourceaccount = source.Split(';')[0].ToString();
            //write out the URI to the container 
            sourceUri = @"https://" + sourceaccount + @".blob.external.azurestack.local/";
            //and save the account key
            sourceKey = source.Split(';')[1].ToString();
 
            string destaccount = dest.Split(';')[0].ToString();
            destUri = @"https://" + destaccount + @".blob.core.windows.net/";
            destKey = dest.Split(';')[1].ToString();
        }
        public static string CreateJob(string job, string container, string datestamp, string sourceUri, string destUri, string sourceKey, string destKey, string backup, TextWriter log)
        {
            //Create a Dictionary object, then serialize it to pass to the WebJobs queue
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("job", job + " " + container);
            d.Add("source", sourceUri + container + @"/");
            d.Add("destination", destUri + container + datestamp);
            d.Add("sourcekey", sourceKey);
            d.Add("destkey", destKey);
            d.Add("backup", backup);
            log.WriteLine("Queued: " + job);
 
            return JsonConvert.SerializeObject(d);
        }
    }
}
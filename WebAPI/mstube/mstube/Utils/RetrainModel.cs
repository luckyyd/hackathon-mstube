using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Timers;

namespace mstube.Utils
{
    public class AzureBlobDataReference
    {
        // Storage connection string used for regular blobs. It has the following format:
        // DefaultEndpointsProtocol=https;AccountName=ACCOUNT_NAME;AccountKey=ACCOUNT_KEY
        // It's not used for shared access signature blobs.
        public string ConnectionString { get; set; }

        // Relative uri for the blob, used for regular blobs as well as shared access 
        // signature blobs.
        public string RelativeLocation { get; set; }

        // Base url, only used for shared access signature blobs.
        public string BaseLocation { get; set; }

        // Shared access signature, only used for shared access signature blobs.
        public string SasBlobToken { get; set; }
    }

    public enum BatchScoreStatusCode
    {
        NotStarted,
        Running,
        Failed,
        Cancelled,
        Finished
    }

    public class BatchScoreStatus
    {
        // Status code for the batch scoring job
        public BatchScoreStatusCode StatusCode { get; set; }


        // Locations for the potential multiple batch scoring outputs
        public IDictionary<string, AzureBlobDataReference> Results { get; set; }

        // Error details, if any
        public string Details { get; set; }
    }

    public class BatchExecutionRequest
    {
        public IDictionary<string, AzureBlobDataReference> Inputs { get; set; }
        public IDictionary<string, string> GlobalParameters { get; set; }

        // Locations for the potential multiple batch scoring outputs
        public IDictionary<string, AzureBlobDataReference> Outputs { get; set; }
    }

    public class RetrainModel
    {
        public static void InvokeBatchExecutionService1(object source, ElapsedEventArgs e)
        {
            InvokeBatchExecutionService().Wait();
        }

        static async Task WriteFailedResponse(HttpResponseMessage response)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

            // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
            System.Diagnostics.Debug.WriteLine(response.Headers.ToString());

            string responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine(responseContent);
        }

        static void SaveBlobToFile(AzureBlobDataReference blobLocation, string resultsLabel)
        {
            const string OutputFileLocation = "myresults.csv"; // Replace this with the location you would like to use for your output file

            var credentials = new StorageCredentials(blobLocation.SasBlobToken);
            var blobUrl = new Uri(new Uri(blobLocation.BaseLocation), blobLocation.RelativeLocation);
            var cloudBlob = new CloudBlockBlob(blobUrl, credentials);

            System.Diagnostics.Debug.WriteLine(string.Format("Reading the result from {0}", blobUrl.ToString()));
            cloudBlob.DownloadToFile(OutputFileLocation, FileMode.Create);

            System.Diagnostics.Debug.WriteLine(string.Format("{0} have been written to the file {1}", resultsLabel, OutputFileLocation));
        }

        static void UploadFileToBlob(string inputFileLocation, string inputBlobName, string storageContainerName, string storageConnectionString)
        {
            // Make sure the file exists
            if (!File.Exists(inputFileLocation))
            {
                throw new FileNotFoundException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "File {0} doesn't exist on local computer.",
                        inputFileLocation));
            }

            System.Diagnostics.Debug.WriteLine("Uploading the input to blob storage...");

            var blobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(storageContainerName);
            container.CreateIfNotExists();
            var blob = container.GetBlockBlobReference(inputBlobName);
            blob.UploadFromFile(inputFileLocation);
        }

        static async void ProcessResults(BatchScoreStatus status)
        {


            bool first = true;
            foreach (var output in status.Results)
            {
                var blobLocation = output.Value;
                System.Diagnostics.Debug.WriteLine(string.Format("The result '{0}' is available at the following Azure Storage location:", output.Key));
                System.Diagnostics.Debug.WriteLine(string.Format("BaseLocation: {0}", blobLocation.BaseLocation));
                System.Diagnostics.Debug.WriteLine(string.Format("RelativeLocation: {0}", blobLocation.RelativeLocation));
                System.Diagnostics.Debug.WriteLine(string.Format("SasBlobToken: {0}", blobLocation.SasBlobToken));

                // Save the first output to disk
                if (first)
                {
                    first = false;

                    // Update model
                    await OverwriteModel(blobLocation.BaseLocation, blobLocation.RelativeLocation, blobLocation.SasBlobToken);

                    //SaveBlobToFile(blobLocation, string.Format("The results for {0}", output.Key));
                }
            }
        }

        public static async Task InvokeBatchExecutionService()
        {
            // How this works:
            //
            // 1. Assume the input is present in a local file (if the web service accepts input)
            // 2. Upload the file to an Azure blob - you'd need an Azure storage account
            // 3. Call the Batch Execution Service to process the data in the blob. Any output is written to Azure blobs.
            // 4. Download the output blob, if any, to local file
            string BaseUrl = Properties.Settings.Default.AzureMLRetrainBaseUrl;

            const string StorageAccountName = "mstubeblob"; // Replace this with your Azure Storage Account name
            const string StorageContainerName = "mstube-container"; // Replace this with your Azure Storage Container name
            string StorageAccountKey = Properties.Settings.Default.StorageAccountKey;
            string apiKey = Properties.Settings.Default.AzureMLRetrainApiKey; // Replace this with the API key for the web service

            // set a time out for polling status
            const int TimeOutInMilliseconds = 120 * 1000; // Set a timeout of 2 minutes

            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

            //Get Preference data from SQL Server
            //This procedure is done by Azure Data Factory

           //Below is for DEBUG ONLY
           // UploadFileToBlob(AppDomain.CurrentDomain.BaseDirectory + "App_Data/PreferenceSample.csv"  /*Replace this with the location of your input file*/,
           //    "TrainingInputdatablob.csv" /*Replace this with the name you would like to use for your Azure blob; this needs to have the same extension as the input file */,
           //    StorageContainerName, storageConnectionString);

            using (HttpClient client = new HttpClient())
            {
                var request = new BatchExecutionRequest()
                {

                    Inputs = new Dictionary<string, AzureBlobDataReference>()
                    {

                        {
                            "TrainingInput",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = storageConnectionString,
                                RelativeLocation = string.Format("{0}/TrainingInputdatablob.csv", StorageContainerName)
                            }
                        },
                    },

                    Outputs = new Dictionary<string, AzureBlobDataReference>()
                    {
                        {
                            "TrainingOutput",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = storageConnectionString,
                                RelativeLocation = string.Format("/{0}/TrainingOutputresults.ilearner", StorageContainerName)
                            }
                        },

                        {
                            "TrainingOutput1",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = storageConnectionString,
                                RelativeLocation = string.Format("/{0}/TrainingOutput1results.csv", StorageContainerName)
                            }
                        },

                        {
                            "TrainingOutput2",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = storageConnectionString,
                                RelativeLocation = string.Format("/{0}/TrainingOutput2results.csv", StorageContainerName)
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)


                System.Diagnostics.Debug.WriteLine("Submitting the job...");

                // submit the job
                var response = await client.PostAsJsonAsync(BaseUrl + "?api-version=2.0", request).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    await WriteFailedResponse(response);
                    return;
                }

                System.Diagnostics.Debug.WriteLine(response.StatusCode);
                System.Diagnostics.Debug.WriteLine(response.Content);

                string jobId = await response.Content.ReadAsAsync<string>();
                System.Diagnostics.Debug.WriteLine(string.Format("Job ID: {0}", jobId));


                // start the job
                System.Diagnostics.Debug.WriteLine("Starting the job...");
                response = await client.PostAsync(BaseUrl + "/" + jobId + "/start?api-version=2.0", null);
                if (!response.IsSuccessStatusCode)
                {
                    await WriteFailedResponse(response);
                    return;
                }

                string jobLocation = BaseUrl + "/" + jobId + "?api-version=2.0";
                Stopwatch watch = Stopwatch.StartNew();
                bool done = false;
                while (!done)
                {
                    System.Diagnostics.Debug.WriteLine("Checking the job status...");
                    response = await client.GetAsync(jobLocation);
                    if (!response.IsSuccessStatusCode)
                    {
                        await WriteFailedResponse(response);
                        return;
                    }

                    BatchScoreStatus status = await response.Content.ReadAsAsync<BatchScoreStatus>();
                    if (watch.ElapsedMilliseconds > TimeOutInMilliseconds)
                    {
                        done = true;
                        System.Diagnostics.Debug.WriteLine(string.Format("Timed out. Deleting job {0} ...", jobId));
                        await client.DeleteAsync(jobLocation);
                    }
                    switch (status.StatusCode)
                    {
                        case BatchScoreStatusCode.NotStarted:
                            System.Diagnostics.Debug.WriteLine(string.Format("Job {0} not yet started...", jobId));
                            break;
                        case BatchScoreStatusCode.Running:
                            System.Diagnostics.Debug.WriteLine(string.Format("Job {0} running...", jobId));
                            break;
                        case BatchScoreStatusCode.Failed:
                            System.Diagnostics.Debug.WriteLine(string.Format("Job {0} failed!", jobId));
                            System.Diagnostics.Debug.WriteLine(string.Format("Error details: {0}", status.Details));
                            done = true;
                            break;
                        case BatchScoreStatusCode.Cancelled:
                            System.Diagnostics.Debug.WriteLine(string.Format("Job {0} cancelled!", jobId));
                            done = true;
                            break;
                        case BatchScoreStatusCode.Finished:
                            done = true;
                            System.Diagnostics.Debug.WriteLine(string.Format("Job {0} finished!", jobId));

                            ProcessResults(status);
                            break;
                    }

                    if (!done)
                    {
                        Thread.Sleep(1000); // Wait one second
                    }
                }
            }
        }

        private static async Task OverwriteModel(string BaseLocation, string RelativeLocation, string SasBlobToken)
        {
            string apiKey = Properties.Settings.Default.AzureMLOverwriteApiKey;
            string endpointUrl = Properties.Settings.Default.AzureMLOverwriteUrl;
            var resourceLocations = new
            {
                Resources = new[]
                {
            new
            {
                Name = "Trained recommender",
                Location = new AzureBlobDataReference()
                {
                    BaseLocation = BaseLocation,
                    RelativeLocation = RelativeLocation,
                    SasBlobToken = SasBlobToken
                }
            }
        }
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpointUrl))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(resourceLocations), System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        await WriteFailedResponse(response);
                    }

                    // Do what you want with a successful response here.
                    System.Diagnostics.Debug.WriteLine("Update Model Successful!");
                }
            }
        }
    }
}
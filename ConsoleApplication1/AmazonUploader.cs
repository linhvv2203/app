using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class AmazonUploader
    {
        protected string AWSAccessKey = ConfigurationSettings.AppSettings["AWSAccessKey"].ToString();
        protected string AWSSecretKey = ConfigurationSettings.AppSettings["AWSSecretKey"].ToString();
        protected string endPoint = ConfigurationSettings.AppSettings["endPoint"].ToString();

        //static IAmazonS3 client;
        public bool sendMyFileToS3(string localFilePath, string bucketName, string subDirectoryInBucket, string fileNameInS3)
        {
            try
            {

                AmazonS3Config config = new AmazonS3Config
                {
                    ServiceURL = endPoint,
                    ForcePathStyle = true
                };

                IAmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey, config);
                //IAmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.EUWest1);

                // create a TransferUtility instance passing it the IAmazonS3 created in the first step
                TransferUtility utility = new TransferUtility(client);
                // making a TransferUtilityUploadRequest instance
                TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();

                if (subDirectoryInBucket == "" || subDirectoryInBucket == null)
                {
                    request.BucketName = bucketName; //no subdirectory just bucket name
                }
                else
                {   // subdirectory and bucket name
                    request.BucketName = bucketName + @"/" + subDirectoryInBucket;
                }
                request.Key = fileNameInS3; //file name up in S3
                request.FilePath = localFilePath; //local file name
                request.CannedACL = S3CannedACL.PublicRead;
                request.StorageClass = S3StorageClass.ReducedRedundancy;
                utility.Upload(request); //commensing the transfer
            }
            catch (AmazonS3Exception ex)
            {
                Console.Write(ex.ToString());
                Ultils.AddToLogFile("ex " + ex.ToString());
                //Console.WriteLine(s3Exception.Message,
                //                  s3Exception.InnerException);
            }
            return true; //indicate that the file was sent
        }
    }
}
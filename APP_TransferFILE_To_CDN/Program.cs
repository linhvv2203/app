using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APP_TransferFILE_To_CDN
{
    class Program
    {
        static void Main(string[] args)
        {

            while (1 == 1)
            {
                uploadFile_User();

            }
        }

        /// <summary>
        /// upload file create from user
        /// </summary>
        public static void uploadFile_User()
        {
            #region DECLARE Variable
            long ID = 0;
            string ItemFile = "";
            string type = "";
            string file_path = "";
            string FolderName = "";
            long idVideo = 0;
            bool status = false;
            string folder = "";
            DataSet ds = null;

            string FolderMP4 = ConfigurationSettings.AppSettings["FolderMP4"];

            //store upload file single
            string storegetsys = ConfigurationSettings.AppSettings["storegetsys"];
            string storeupdatesys_success = ConfigurationSettings.AppSettings["storeupdatesys_success"];
            string storeupdatesys_fail = ConfigurationSettings.AppSettings["storeupdatesys_fail"];

            string ConnStr = ConfigurationSettings.AppSettings["ConnStr"];
            string ConnStr1 = ConfigurationSettings.AppSettings["ConnStr1"];


            string myBucketName = ConfigurationSettings.AppSettings["bucket"];
            string s3DirectoryName = ConfigurationSettings.AppSettings["folder"];
            string s3FileName = ConfigurationSettings.AppSettings["s3FileName"];

            string foderUpload = ConfigurationSettings.AppSettings["foderUpload"];
            #endregion

            try
            {
                ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storegetsys);

                int n = ds.Tables[0].Rows.Count;

                if (n < 1)
                {
                    ds.Clear();
                    Console.WriteLine("! chua co file Video dong bo from Itemmovie to Storage -" + DateTime.Now + " !");
                    //Ultils.AddToLogFile("! chua co file Video dong bo from Itemmovie to Storage -" + DateTime.Now + " !");
                    Thread.Sleep(2000);
                    return;
                }

                for (int i = 0; i < n; i++)
                {

                    ID = Convert.ToInt64(ds.Tables[0].Rows[i]["ID"]);
                    idVideo = Convert.ToInt64(ds.Tables[0].Rows[i]["Itemcode"]);
                    type = ds.Tables[0].Rows[i]["type"].ToString();
                    ItemFile = ds.Tables[0].Rows[i]["ItemFile"].ToString();
                    FolderName = ds.Tables[0].Rows[i]["FolderName"].ToString();

                    Console.WriteLine("! Dang dong bo file Video " + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + DateTime.Now + " !");
                    //Ultils.AddToLogFile("! Dang dong bo file Video " + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + DateTime.Now + " !");
                    try
                    {
                        if (type == "1")
                        {
                            file_path = FolderMP4 + FolderName + "/" + ItemFile;
                            s3DirectoryName = s3DirectoryName + "/" + FolderName;
                            // upload file storage
                            if (sendMyFileToS3(file_path, myBucketName, s3DirectoryName, s3FileName)) status = true;
                        }

                        if (status == true)
                        {

                            SqlParameter[] param = {
                                                           new SqlParameter("@id",ID),
                                                           new SqlParameter("@idvideo",idVideo),
                                                           new SqlParameter("@status",1)
                                                       };
                            ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storeupdatesys_success, param);

                            Console.WriteLine("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage success- " + DateTime.Now + " !");
                            //Ultils.AddToLogFile("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage success- " + DateTime.Now + " !");

                        }
                        else
                        {

                            SqlParameter[] param = {
                                                           new SqlParameter("@id",ID),
                                                           new SqlParameter("@idvideo",idVideo),
                                                           new SqlParameter("@status",2)
                                                       };

                            ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storeupdatesys_fail, param);

                            Console.WriteLine("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail -" + DateTime.Now + " !");
                            //Ultils.AddToLogFile("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail -" + DateTime.Now + " !");
                        }
                    }
                    catch (Exception ex)
                    {
                        SqlParameter[] param = {
                                                           new SqlParameter("@id",ID),
                                                           new SqlParameter("@idvideo",idVideo),
                                                           new SqlParameter("@status",2)
                                                       };

                        ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storeupdatesys_fail, param);

                        Console.WriteLine("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail " + DateTime.Now + " !");
                        //Ultils.AddToLogFile("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail " + DateTime.Now + " !");
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// send file to s3
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <param name="bucketName"></param>
        /// <param name="subDirectoryInBucket"></param>
        /// <param name="fileNameInS3"></param>
        /// <returns></returns>
        public static bool sendMyFileToS3(string localFilePath, string bucketName, string subDirectoryInBucket, string fileNameInS3)
        {
            string AWSAccessKey = ConfigurationSettings.AppSettings["AWSAccessKey"].ToString();
            string AWSSecretKey = ConfigurationSettings.AppSettings["AWSSecretKey"].ToString();
            string endPoint = ConfigurationSettings.AppSettings["endPoint"].ToString();

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
                //Ultils.AddToLogFile("ex" + ex.ToString());
                ////Console.WriteLine(s3Exception.Message,
                //                  s3Exception.InnerException);
                return false;
            }
            return true; //indicate that the file was sent
        }
    }
}

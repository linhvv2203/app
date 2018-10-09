using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class SynFile
    {
        protected long ID = 0;
        protected string ItemFile = "";
        protected string type = "";
        protected string file_path = "";
        protected string FolderName = "";
        protected long idVideo = 0;
        protected bool status;
        protected string folder = "";
        protected DataSet ds = null;

        public string FolderMP4 = ConfigurationSettings.AppSettings["FolderMP4"];

        //store upload file single
        public string storegetsys = ConfigurationSettings.AppSettings["storegetsys"];
        public string storeupdatesys_success = ConfigurationSettings.AppSettings["storeupdatesys_success"];
        public string storeupdatesys_fail = ConfigurationSettings.AppSettings["storeupdatesys_fail"];

        //store upload file batch excel
        public string storegetsys_excel = ConfigurationSettings.AppSettings["storegetsys_excel"];
        public string storeupdatesys_success_excel = ConfigurationSettings.AppSettings["storeupdatesys_success_excel"];
        public string storeupdatesys_fail_excel = ConfigurationSettings.AppSettings["storeupdatesys_fail_excel"];


        public string ConnStr = ConfigurationSettings.AppSettings["ConnStr"];
        public string ConnStr1 = ConfigurationSettings.AppSettings["ConnStr1"];

        protected string AWSAccessKey = ConfigurationSettings.AppSettings["AWSAccessKey"].ToString();
        protected string AWSSecretKey = ConfigurationSettings.AppSettings["AWSSecretKey"].ToString();
        protected string endPoint = ConfigurationSettings.AppSettings["endPoint"].ToString();
        protected string myBucketName = ConfigurationSettings.AppSettings["bucket"];
        protected string s3DirectoryName = ConfigurationSettings.AppSettings["folder"];
        protected string s3FileName = ConfigurationSettings.AppSettings["s3FileName"];

        protected string foderUpload = ConfigurationSettings.AppSettings["foderUpload"];

        /// <summary>
        /// upload file create from user
        /// </summary>
        public void uploadFile_User()
        {
            try
            {
                ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storegetsys);

                int n = ds.Tables[0].Rows.Count;

                if (n < 1)
                {
                    ds.Clear();
                    //Console.WriteLine("! chua co file Video dong bo from Itemmovie to Storage -" + DateTime.Now + " !");
                    Ultils.AddToLogFile("! chua co file Video dong bo from Itemmovie to Storage -" + DateTime.Now + " !");
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

                    //Console.WriteLine("! Dang dong bo file Video " + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + DateTime.Now + " !");
                    Ultils.AddToLogFile("! Dang dong bo file Video " + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + DateTime.Now + " !");
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

                            //Console.WriteLine("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage success- " + DateTime.Now + " !");
                            Ultils.AddToLogFile("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage success- " + DateTime.Now + " !");

                        }
                        else
                        {

                            SqlParameter[] param = {
                                                           new SqlParameter("@id",ID),
                                                           new SqlParameter("@idvideo",idVideo),
                                                           new SqlParameter("@status",2)
                                                       };

                            ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storeupdatesys_fail, param);

                            //Console.WriteLine("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail -" + DateTime.Now + " !");
                            Ultils.AddToLogFile("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail -" + DateTime.Now + " !");
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

                        //Console.WriteLine("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail " + DateTime.Now + " !");
                        Ultils.AddToLogFile("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail " + DateTime.Now + " !");
                    }

                }

            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// upload file create from user with excel
        /// </summary>
        public void uploadFile_User_WithExcel()
        {
            try
            {
                ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storegetsys);

                int n = ds.Tables[0].Rows.Count;

                if (n < 1)
                {
                    ds.Clear();
                    //Console.WriteLine("! chua co file Video dong bo from Itemmovie to Storage -" + DateTime.Now + " !");
                    Ultils.AddToLogFile("! chua co file Video dong bo from Itemmovie to Storage -" + DateTime.Now + " !");
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

                    //Console.WriteLine("! Dang dong bo file Video " + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + DateTime.Now + " !");
                    Ultils.AddToLogFile("! Dang dong bo file Video " + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + DateTime.Now + " !");

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
                            ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storeupdatesys_success_excel, param);

                            //Console.WriteLine("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage success- " + DateTime.Now + " !");
                            Ultils.AddToLogFile("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage success- " + DateTime.Now + " !");

                        }
                        else
                        {

                            SqlParameter[] param = {
                                                           new SqlParameter("@id",ID),
                                                           new SqlParameter("@idvideo",idVideo),
                                                           new SqlParameter("@status",2)
                                                       };

                            ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storeupdatesys_fail_excel, param);

                            //Console.WriteLine("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail -" + DateTime.Now + " !");
                            Ultils.AddToLogFile("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail -" + DateTime.Now + " !");

                        }
                    }
                    catch (Exception ex)
                    {
                        SqlParameter[] param = {
                                                           new SqlParameter("@id",ID),
                                                           new SqlParameter("@idvideo",idVideo),
                                                           new SqlParameter("@status",2)
                                                       };

                        ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, storeupdatesys_fail_excel, param);

                        //Console.WriteLine("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail " + DateTime.Now + " !");
                        Ultils.AddToLogFile("! Upload phim:" + FolderName + "/" + ItemFile + "--- idmoviegroup" + idVideo + "--" + " to storage fail " + DateTime.Now + " !");
                    }

                }

            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// upload file get all file from DB
        /// </summary>
        public void uploadFile_DB()
        {
            //string[] directories = Directory.GetDirectories(foderUpload);
            Console.WriteLine("start upload file to s3");
            Ultils.AddToLogFile("start upload file to s3");
            try
            {
                string query = "select * from itemmovie_uploadS3 where status = 1";
                DataSet ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.Text, query);

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string folder_path = "";
                    folder_path = foderUpload + dr["folder"].ToString() + "\\" + dr["ItemFile"].ToString();
                    string id = dr["id"].ToString();

                    Console.WriteLine("-- check file upload--" + folder_path + "--------------------");
                    Ultils.AddToLogFile("-- check file upload--" + folder_path + "--------------------");

                    if (File.Exists(folder_path))
                    {
                        Console.WriteLine("--upload--" + folder_path);
                        Ultils.AddToLogFile("--upload--" + folder_path);

                        if (sendMyFileToS3(folder_path, myBucketName, s3DirectoryName + "/" + dr["folder"].ToString(), s3FileName))
                        {
                            Console.WriteLine("--upload success--" + foderUpload);
                            Ultils.AddToLogFile("--upload success--" + foderUpload);

                            query = "update itemmovie_uploadS3 set status = 2 where id= " + id;
                            ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.Text, query);

                            query = "update itemmoviegroup set media_path_cdn ='" + s3DirectoryName + "/" + dr["folder"].ToString() + "/" + dr["ItemFile"].ToString() + "' where id= " + dr["id_part"].ToString();
                            ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.Text, query);

                        }
                        else
                        {
                            Console.WriteLine("--upload fail--" + foderUpload);
                            Ultils.AddToLogFile("--upload fail--" + foderUpload);

                            query = "update itemmovie_uploadS3 set status = 3 where id= " + id;
                            ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.Text, query);

                        }
                    }
                }
                Console.WriteLine("--------------done-----------");
                Ultils.AddToLogFile("--------------done-----------");

            }
            catch (Exception ex)
            {
                //Console.WriteLine("ex " + ex);
                Ultils.AddToLogFile("ex " + ex);
            }

        }
        /// <summary>
        /// upload file from folder local to storage
        /// </summary>
        public void uploadFile_to_foderLocal()
        {
            string[] directories = Directory.GetDirectories(foderUpload);
            //Console.WriteLine("start upload file to s3");
            Ultils.AddToLogFile("start upload file to s3");

            foreach (string path in directories)
            {
                //Console.WriteLine("start upload file to s3" + path);
                Ultils.AddToLogFile("start upload file to s3" + path);

                FileInfo fileInfo = new FileInfo(path);

                string foderName = fileInfo.Name;
                string[] fileName = Directory.GetFiles(path);

                int count = fileName.Count();

                foreach (string filePath in fileName)
                {
                    try
                    {
                        if (sendMyFileToS3(filePath, myBucketName, s3DirectoryName + "/" + foderName, s3FileName))
                        {
                            //Console.WriteLine("upload file" + filePath + " success");
                            Ultils.AddToLogFile("upload file" + filePath + " success");
                        }
                        else
                        {
                            //Console.WriteLine("upload file" + filePath + " false");
                            Ultils.AddToLogFile("upload file" + filePath + " false");

                            SqlParameter[] param = { new SqlParameter("@filepath", filePath), };
                            ds = SqlHelper.ExecuteDataset(ConnStr1, CommandType.StoredProcedure, "sp_logUploadS3", param);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                //sendMyFileToS3(path+)

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
                //Console.Write(ex.ToString());
                Ultils.AddToLogFile("ex" + ex.ToString());
                ////Console.WriteLine(s3Exception.Message,
                //                  s3Exception.InnerException);
                return false;
            }
            return true; //indicate that the file was sent
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="fileKey"></param>
        /// <returns></returns>
        public string DeleteFileS3(string bucketName, string fileKey)
        {
            string result = "";

            try
            {
                AmazonS3Config config = new AmazonS3Config
                {
                    ServiceURL = endPoint,
                    ForcePathStyle = true
                };
                IAmazonS3 client = null;
                client = Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey, config);

                if (Exists(client, fileKey, bucketName))
                {
                    client.DeleteObject(new Amazon.S3.Model.DeleteObjectRequest() { BucketName = bucketName, Key = fileKey });
                    result = "delete success " + bucketName + "/" + fileKey;
                }
                else
                {
                    result = "file not exists " + bucketName + "/" + fileKey;
                }
            }
            catch (Exception ex)
            {
                result = "error!";
            }


            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        public void DownloadFile1()
        {
            //Your code goes here
            var client = new WebDownload(120000);
            Uri address = new Uri("http://cdn-vttvas.public.storebox.vn/video1/2017/10/20/21/283a3a73/283a3a73-a503-4249-8afd-da507f31286e_3.mp4");
            //client.DownloadFileAsync(address, @"E:\907648_flash_wap_684.jpg");
            var dateStart = Convert.ToInt32(DateTime.Now.ToString("HH")) * 3600 + Convert.ToInt32(DateTime.Now.ToString("mm")) * 60 + Convert.ToInt32(DateTime.Now.ToString("ss"));

            client.DownloadFile(address, @"E:\dl_test.mp4");
            var dateEnd = Convert.ToInt32(DateTime.Now.ToString("HH")) * 3600 + Convert.ToInt32(DateTime.Now.ToString("mm")) * 60 + Convert.ToInt32(DateTime.Now.ToString("ss"));
            Console.WriteLine("success : " + (dateEnd - dateStart));
        }

        public bool Exists(IAmazonS3 client, string fileKey, string bucketName)
        {
            try
            {
                S3FileInfo file = new S3FileInfo(client, bucketName, fileKey);
                return file.Exists;//if the file exists return true, in other case false

            }

            catch (Amazon.S3.AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                //status wasn't not found, so throw the exception
                throw;
            }
        }

        public void DowloadFile()
        {
            try
            {
                string host = "10.60.135.32";
                string user = "ftp_user";
                string pass = "Kh#Tuy564gggFDfjk";
                string path_host = "/media2/phimonbox/mp4/";//20171020/a6110324-17f6-460d-9927-8b72e769b06a_3.mp4
                string path_local = @"E:\fileMP4.mp4";
                DataSet ds = null;
                string store_sync_to_cdn = "PHIM_sync_to_cdn";

                ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, store_sync_to_cdn);
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

                    string ID = ds.Tables[0].Rows[i]["ID"].ToString();
                    string FileName = ds.Tables[0].Rows[i]["FileName"].ToString();
                    string Folder = ds.Tables[0].Rows[i]["Folder"].ToString();
                    string id_group = ds.Tables[0].Rows[i]["id_group"].ToString();
                    FileName = FileName + ".mp4";

                    string path_host1 = path_host + Folder + "/" + FileName;
                    path_local = @"E:\onkeeng\mp4\" + Folder + "/" + FileName;

                    if (!Directory.Exists(@"E:\onkeeng\mp4\" + Folder))
                    {
                        Directory.CreateDirectory(@"E:\onkeeng\mp4\" + Folder);
                    }

                    if (FTPDownload(host, user, pass, path_host1, path_local))
                    {

                        SqlParameter[] param = {
                                                           new SqlParameter("@itemfile",FileName),
                                                           new SqlParameter("@idvideo",id_group),
                                                       };

                        ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, "INSERT_SYNC_TO_CDN", param);

                        Console.WriteLine("upload success " + ds.Tables[0].Rows[0][0].ToString());
                    }else
                    {
                        SqlParameter[] param = {
                                                           new SqlParameter("@idvideo",id_group),
                                                           new SqlParameter("@status",3),
                                                       };

                        ds = SqlHelper.ExecuteDataset(ConnStr, CommandType.StoredProcedure, "UPDATE_SYNC_TO_CDN", param);
                    }


                }
            }
            catch (Exception ex)
            {
            }

        }

        public bool FTPDownload(string host, string user, string pass, string path_host, string path_file_local)
        {
            try
            {
                //WebClient ftpClient = new WebClient();
                EDSWebClient ftpClient = new EDSWebClient();

                // The user name and password are stored here
                NetworkCredential nc = new NetworkCredential(user, pass);
                ftpClient.Credentials = nc;
                ftpClient.Proxy = null;

                string path_file_server = String.Format("ftp://{0}/{1}", host, path_host);

                ftpClient.DownloadFile(path_file_server, path_file_local);

                return true;
            }
            catch (Exception ex)
            {
                Ultils.AddToLogFile("ex " + ex.ToString());
                //Console.WriteLine("Local " + path_file_local);
                //Console.WriteLine("server " + String.Format("ftp://{0}/{1}", host, path_host));
                //Console.WriteLine(ex.ToString());
                //txtLog.Text += ex.Message + "\n";
                //MessageBox.Show("Loi " + ex.ToString());
                //MessageBox.Show("Local " + path_file_local);
                //MessageBox.Show("server " + String.Format("ftp://{0}/{1}", host, path_host));

                //AddToLogFile("Loi " + path_file_local);


                return false;
            }
        }



    }
    public class EDSWebClient : WebClient
    {
        //time in milliseconds
        private int timeout;

        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }


        public EDSWebClient()
        {
            this.timeout = 60000;
        }

        public EDSWebClient(int timeout)
        {
            this.timeout = timeout;
        }
    }
}

using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APP_AutoCDR_FOX
{
    class Program
    {
        public static string folder_temp = ConfigurationSettings.AppSettings["Temp"];
        public static string folder_backup = ConfigurationSettings.AppSettings["Backup"];

        public static string server = ConfigurationSettings.AppSettings["host"];
        public static string user = ConfigurationSettings.AppSettings["user"];
        public static string pass = ConfigurationSettings.AppSettings["pass"];
        public static string ftpdir = ConfigurationSettings.AppSettings["ftpdir"];
        public static string name_system = ConfigurationSettings.AppSettings["System"];
        public static string name_file = ConfigurationSettings.AppSettings["FileName"];

        static void Main(string[] args)
        {
            string time_delay = ConfigurationSettings.AppSettings["TimeDelay"];
            int i_time_delay = int.Parse(time_delay);
            i_time_delay = i_time_delay * 60000;

            //Pass the Directory for displaying the contents
            while (0 == 0)
            {
                //Gen CDR
                AutoGenCDR();

                //Auto FTP
                FTPAuto();

                //To Screen
                Console.WriteLine("Da day xong!! Cho " + time_delay + " phut de day tiep!!!");
                System.Threading.Thread.Sleep(i_time_delay);
            }

            Console.WriteLine("Hoan thanh");
            Console.ReadLine();
        }

        #region "Bussiness"


        public static void AutoGenCDR()
        {
            Console.WriteLine("GEN CDR !!! " + DateTime.Now.ToString());

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ConfigurationSettings.AppSettings["ConnectionStringOn"], CommandType.StoredProcedure, ConfigurationSettings.AppSettings["Store"]);
                string cdrs_Fox = "";

                if (ds != null)
                {
                    if (ds.Tables.Count >= 1)
                    {
                        //CDR FOX
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                cdrs_Fox += dr[0].ToString() + "\r\n";
                            }
                        }
                    }
                }

                //CDR FOX
                if (cdrs_Fox.Length > 2)
                {
                    cdrs_Fox = cdrs_Fox.Substring(0, cdrs_Fox.Length - 2);

                    string name_file_full = String.Format("{0}_{1}.txt", name_file, DateTime.Now.ToString("yyyyMMddhhmmff"));

                    System.IO.File.WriteAllText(ConfigurationSettings.AppSettings["Temp"] + @"/" + name_file_full, cdrs_Fox);

                    Console.WriteLine("--> GEN FILE CDR Fox : " + name_file_full);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message + " " + DateTime.Now.ToString());
            };
        }


        public static void FTPAuto()
        {
            Console.WriteLine("Bat dau thuc hien!!!");

            DirectoryInfo dir_source = new DirectoryInfo(folder_temp);

            try
            {
                foreach (FileInfo info in dir_source.GetFiles())
                {
                    string file_path_source = info.FullName;
                    string name_file = info.Name;

                    //Step 1: transfer
                    if (FTPUpload(server, user, pass, ftpdir + "/" + name_file, file_path_source))
                    {
                        string path_folder_backup = folder_backup + @"/" + DateTime.Now.ToString("yyyyMMdd") + @"/";

                        //Step 2: copy file to folder backup
                        if (!System.IO.Directory.Exists(path_folder_backup))
                        {
                            System.IO.Directory.CreateDirectory(path_folder_backup);
                        }

                        if (!System.IO.File.Exists(path_folder_backup + name_file))
                        {
                            File.Copy(file_path_source, path_folder_backup + name_file);
                        }

                        //Step 3: delete file
                        File.Delete(file_path_source);

                        Console.WriteLine("Transer file: " + name_file + " thanh cong!");
                    }
                    else
                    {
                        Console.WriteLine("!!No Transed file: " + name_file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi thu muc: " + folder_temp + " -> " + ex.Message);
            }
        }

        #endregion

        #region "Common"

        /// <summary>
        /// FTP upload file to server
        /// </summary>
        /// <param name="host">host: ip</param>
        /// <param name="user">user</param>
        /// <param name="pass">pass</param>
        /// <param name="path_host">path file in host</param>
        /// <param name="path_file_local">path file in local</param>
        /// <param name="folder_path">path folder in server</param>
        /// <returns></returns>
        public static bool FTPUpload(string host, string user, string pass, string path_host, string path_file_local)
        {
            try
            {
                EDSWebClient ftpClient = new EDSWebClient();

                // The user name and password are stored here
                NetworkCredential nc = new NetworkCredential(user, pass);
                ftpClient.Credentials = nc;
                ftpClient.Proxy = null;

                string path_file_server = String.Format("ftp://{0}{1}", host, path_host);

                ftpClient.UploadFile(path_file_server, path_file_local);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Make folder by FTP
        /// </summary>
        /// <param name="host">host: ip</param>
        /// <param name="user">user</param>
        /// <param name="pass">pass</param>
        /// <param name="folder_path">path of folder in server</param>
        /// <returns></returns>
        public static bool FTPMakeFolder(string host, string user, string pass, string folder_path)
        {
            string path_folder_server = String.Format("ftp://{0}{1}", host, folder_path);

            try
            {
                WebRequest request = WebRequest.Create(path_folder_server);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(user, pass);
                FtpWebResponse resp = (FtpWebResponse)request.GetResponse();

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}


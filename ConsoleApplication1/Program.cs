using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //string host = "10.60.135.32";
            //string user = "ftp_user";
            //string pass = "Kh#Tuy564gggFDfjk";
            //string path_host = "/media2/phimonbox/mp4/20171020/a6110324-17f6-460d-9927-8b72e769b06a_3.mp4";
            //string path_local = "";

            //for (; true; )
            //{
            //    SynFile o = new SynFile();
            //    o.DowloadFile();
            //}

            for (; true; )
            {
                SynFile obj = new SynFile();
                obj.uploadFile_User();


            }

            //for (; true; )
            //{
            //    SynFile obj = new SynFile();
            //    //obj.uploadFile_User_WithExcel();

            //    string bucket = "kfilm";
            //    string fileKey = "mp4/20170926/test.jpg";
            //    string resp ="";

            //    resp = obj.DeleteFileS3(bucket,fileKey);

            //    Console.WriteLine(resp);
            //}

            //SynFile obj = new SynFile();
            //obj.uploadFile_User_WithExcel();

            Console.ReadKey();
        }
    }

    public class WebDownload : WebClient
    {
        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int Timeout { get; set; }

        public WebDownload() : this(60000) { }

        public WebDownload(int timeout)
        {
            this.Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = this.Timeout;
            }
            return request;
        }
    }

}

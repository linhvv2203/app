using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class Ultils
    {
        public static void AddToLogFile(string content)
        {
            string fn = DateTime.Now.ToString("yyyy") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") + DateTime.Now.ToString("HH") + ".txt";
            System.IO.StreamWriter writer = null;
            try
            {

                String path = "";

                path = ConfigurationSettings.AppSettings["Log_Folder"];

                path = path + "/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd");
                if (!System.IO.File.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                path += "/" + fn;
                if (path != "")
                {
                    writer = new StreamWriter(path, true, System.Text.Encoding.UTF8);
                    writer.WriteLine(content);
                    writer.Flush();
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch
            {

            }
        }
    }
}

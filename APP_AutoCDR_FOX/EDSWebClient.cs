using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APP_AutoCDR_FOX
{
    public class EDSWebClient : WebClient
    {
        //time in milliseconds
        private int timeout;
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        public EDSWebClient()
        {
            this.timeout = 60000;
        }

        public EDSWebClient(int timeout)
        {
            this.timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            FtpWebRequest result = (FtpWebRequest)base.GetWebRequest(address);
            //result.UsePassive = true;

            result.Timeout = this.timeout;
            return result;
        }
    }
}

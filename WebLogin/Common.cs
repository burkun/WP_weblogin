using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;

namespace WebLogin
{
   

    class Srun
    {

        private String _uName;
        private String _uPwd;
        private const String PostUrl = "http://219.223.254.66/cgi-bin/srun_portal";
        private const String InfoUrl = "http://219.223.254.66/cgi-bin/rad_user_info";
        private const String DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        private const String Login_post_template = "action=login&username={0}&password={1}&ac_id=6&type=1&wbaredirect=&mac=&user_ip=&nas_ip=&pop=1&is_ldap=1&nas_init_port=1";
        private const String Logout_post_str = "action=logout";

        private HttpWebRequest _initRequest(String url)
        {

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);

            request.Method = "post";
            request.UserAgent = DefaultUserAgent;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            return request;
        }

        private async Task<string> _doPost(string url, string postdata)
        {
            var request = _initRequest(url);
            byte[] data = Encoding.UTF8.GetBytes(postdata);
            request.ContentLength = data.Length;
            using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, request))
            {
                await requestStream.WriteAsync(data, 0, data.Length);
            }
            
            WebResponse responseObject = await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
            var responseStream = responseObject.GetResponseStream();
            var sr = new StreamReader(responseStream);
            string received = await sr.ReadToEndAsync();

            return received;
        }
       
        /// <summary>
        /// constructor function 
        /// </summary>
        /// <param name="uName">user account</param>
        /// <param name="uPwd">user password</param>
        public Srun(String uName, String uPwd)
        {
            _uName = uName;
            _uPwd = uPwd;
        }


        public async Task<string> Login()
        {
           return await _doPost(PostUrl, String.Format(Login_post_template, _uName, _uPwd));
        }
        public async Task<string> Logout()
        {
            return await _doPost(PostUrl, Logout_post_str);
        }

        public async Task<string> GetInfo()
        {
            return await _doPost(InfoUrl, "");
        }
    }
}

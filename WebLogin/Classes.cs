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

namespace WebLogin1
{
    public delegate void UpdataUIHandler(String msg); 
    class MyPackage
    {
        public HttpWebRequest Reqest { get; set; }
        public String PostData { get; set; }
        public UpdataUIHandler ShowMsgs { get; set; }
    }

    class Srun 
    {
        
        private String _uName;
        private String _uPwd;
        private const String PostUrl = "http://219.223.254.66/cgi-bin/srun_portal";
        private const String InfoUrl = "http://219.223.254.66/cgi-bin/rad_user_info";
        private const String DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        private const String Login_post_template = "action=login&username={0}&password={1}&ac_id=6&type=1&wbaredirect=&mac=&user_ip=&nas_ip=&pop=1&is_ldap=1&nas_init_port=1";
        private const String Logout_post_str = "action=longout";

        ManualResetEvent alldone = new ManualResetEvent(false);


        private HttpWebRequest _initRequest(String url)
        {

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            
            request.Method = "post";
            request.UserAgent = DefaultUserAgent;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            return request;
        }

        private void _doPost(MyPackage mp){

            //HttpUtility.UrlEncode()
            mp.Reqest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), mp);
            //ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), request, _timeout, true);
            alldone.WaitOne();
            alldone.Reset();
            mp.Reqest.BeginGetResponse(new AsyncCallback(GetResponsetStreamCallback), mp);
            Debug.WriteLine("1212321312312");
            alldone.WaitOne();
            alldone.Reset();
            Debug.WriteLine("done");
        }

        /*
        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                {
                    request.Abort();
                }
            }
        }
        */

        private void GetRequestStreamCallback(IAsyncResult asynchResult)
        {
            
            MyPackage myPackage = (MyPackage)asynchResult.AsyncState;
            HttpWebRequest myRequest = myPackage.Reqest;
          
            String postStr = myPackage.PostData;
            if (!String.IsNullOrEmpty(postStr))
            {
                Stream postStream = myRequest.EndGetRequestStream(asynchResult);
                byte[] bData = Encoding.UTF8.GetBytes(myPackage.PostData);
                postStream.Write(bData, 0, bData.Length);
                postStream.Close();
            }
            Debug.WriteLine("in 1======");
            alldone.Set();


        }

        private void GetResponsetStreamCallback(IAsyncResult callbackResult)
        {
            Debug.WriteLine("in 2");
            MyPackage myPackage = (MyPackage)callbackResult.AsyncState;
            HttpWebRequest request = myPackage.Reqest;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(callbackResult);
                string responseString = "";
                Stream streamResponse = response.GetResponseStream();
                StreamReader reader = new StreamReader(streamResponse);
                responseString = reader.ReadToEnd();
                streamResponse.Close();
                reader.Close();
                response.Close();
                myPackage.ShowMsgs(responseString);
                Debug.WriteLine(responseString);
            }
            catch (Exception e)
            {
                request.Abort();
                myPackage.ShowMsgs("ERROR:Something wrong happend in this Request!\n" + e.Message);
            }
            finally
            {
                alldone.Set();
            }
        }



        public bool IsLogin() 
        { 
            bool islogin = false;
            UpdataUIHandler TestLoginHandler = (String msg)=>
            {
                if(!String.IsNullOrEmpty(msg) || msg != "not_online")
                {
                   islogin = true;
                    
                }
            };
            GetInfo(TestLoginHandler);
            return islogin;
        }

        /// <summary>
        /// constructor function 
        /// </summary>
        /// <param name="uName">user account</param>
        /// <param name="uPwd">user password</param>
        public Srun(String uName, String uPwd) {
            _uName = uName;
            _uPwd = uPwd;
        }


        public void Login(UpdataUIHandler handler) 
        {
            HttpWebRequest req = _initRequest(PostUrl);
            MyPackage pack = new MyPackage()
            {
                PostData = String.Format(Login_post_template, _uName, _uPwd),
                Reqest = req,
                ShowMsgs = handler
            };
            _doPost(pack);

        }
        public void Logout(UpdataUIHandler handler) 
        {
            HttpWebRequest req = _initRequest(PostUrl);
            MyPackage pack = new MyPackage()
            {
                PostData =Logout_post_str,
                Reqest = req,
                ShowMsgs = handler
            };
            _doPost(pack);
        }

        public void GetInfo(UpdataUIHandler handler)
        {
            HttpWebRequest req = _initRequest(InfoUrl);
            MyPackage pack = new MyPackage()
            {
                PostData = "",
                Reqest = req,
                ShowMsgs = handler
            };
            _doPost(pack);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;

namespace WebLogin
{
    public partial class Info : PhoneApplicationPage
    {
        private String uname;
        private String upwd;
        private Srun runner;
        public Info()
        {
            InitializeComponent();
            _hiddenInfoCtrl();
        }

        private void _hiddenInfoCtrl()
        {
            InfoGrid.Visibility = System.Windows.Visibility.Collapsed;
            LogoutBtn.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void _showInfoCtrl()
        {
            InfoGrid.Visibility = System.Windows.Visibility.Visible;
            LogoutBtn.Visibility = System.Windows.Visibility.Visible;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            GetParams();
            runner = new Srun(uname, upwd);
            login();
            LogoutBtn.Click += LogoutBtn_Click;
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            Action asyncClick = async () =>
            {
                string res = await runner.Logout();
                Debug.WriteLine(res);
                NavigationService.GoBack();
            };
            asyncClick();
        }

        private void _showInfo(string res)
        {

            string[] resNew = res.Split(",".ToCharArray());
            AccountInfoLabel.Text = resNew[0];
            long liuliang = Convert.ToInt64(resNew[6]) / 1000;
            KBInfoLabel.Text = liuliang.ToString() + "KB";
            MoneyInfoLabel.Text = resNew[11];

        }

        private async void login() {
            ServerInfoLabel.Visibility = Visibility.Collapsed;
            //string res = await runner.Login();
            string res = await runner.GetInfo();
            Debug.WriteLine(res);
            if (_checkLogin(res)) 
            {
                _showInfoCtrl();
                _showInfo(res);
                ServerInfoLabel.Visibility = Visibility.Visible;
                ServerInfoLabel.Text = "登陆成功！";
            }
            else
            {
                string body_str  = await runner.Login();
                if (body_str.LastIndexOf("help.html") !=-1 || body_str.LastIndexOf("login_ok") !=-1)
                {
                    res = await runner.GetInfo();
                    _showInfoCtrl();
                    _showInfo(res);
                    ServerInfoLabel.Visibility = Visibility.Visible;
                    ServerInfoLabel.Text = "登陆成功！";
                }
                else
                {
                    Debug.WriteLine(res);

                    if (!_checkLogin(res))
                    {
                        login();
                    }
                    else
                    {
                        _showInfoCtrl();
                        _showInfo(res);
                        ServerInfoLabel.Visibility = Visibility.Visible;
                        ServerInfoLabel.Text = "登陆成功！";
                    }
                }
            }
            
        }

        private bool _checkLogin(string res)
        {
            if (res != "" && res != "not_online")
            {
                return true;
            }
            return false;
        }


        private void GetParams() 
        {
            NavigationContext.QueryString.TryGetValue("uname", out uname);
            NavigationContext.QueryString.TryGetValue("upwd", out upwd);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GoogleDrive.REST.Model;

namespace GoogleDrive
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            OAuth2Authenticator authenticator = BuildGoogleAuthenticator();
            PresentUILoginScreen(authenticator);

            return;
        }

        private void Button_Clicked_5(object sender, EventArgs e)
        {
            OAuth2Authenticator authenticator = BuildBoxAuthenticator();
            PresentUILoginScreen(authenticator);

            return;
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if (AuthenticationState.Token != null)
            {
                REST.RestService.GetGooGle().Token = AuthenticationState.Token;
                var files = await REST.RestService.GetGooGle().ListGoogleFiles("root");
                foreach (GoogleFile file in files.Files)
                {
                    System.Diagnostics.Debug.WriteLine(file.Name);
                }
            }

            return;
        }

        private async void Button_Clicked_6(object sender, EventArgs e)
        {
            if (AuthenticationState.Token != null)
            {
                REST.RestService.GetBox().Token = AuthenticationState.Token;
                var files = await REST.RestService.GetBox().ListBoxFiles("0");
                foreach(var file in files.Entries)
                {
                    System.Diagnostics.Debug.WriteLine(file.Name);
                }
            }

            return;
        }

        private OAuth2Authenticator BuildBoxAuthenticator()
        {
            var authenticator = new Xamarin.Auth.OAuth2Authenticator(
                clientId: "oc7mh1pncn2v3uyk99gyg2lzqcbszswo",
                scope: null,
                clientSecret: "b4C5lPkEfyUkD3SrvM7nIGnUizq4gS7E",                
                authorizeUrl: new Uri("https://account.box.com/api/oauth2/authorize"),
                accessTokenUrl: new Uri("https://api.box.com/oauth2/token"),
                redirectUrl: new Uri("http://localhost"))
            {
                AllowCancel = true,
            };

            authenticator.Completed +=
                (s, ea) =>
                {
                    StringBuilder sb = new StringBuilder();

                    if (ea.Account != null && ea.Account.Properties != null)
                    {
                        string token = ea.Account.Properties["access_token"];
                        AuthenticationState.Token = token;

                        sb.Append("Token = ").AppendLine($"{token}");
                    }
                    else
                    {
                        sb.Append("Not authenticated ").AppendLine($"Account.Properties does not exist");
                    }

                    DisplayAlert
                            (
                                "Authentication Results",
                                sb.ToString(),
                                "OK"
                            );

                    Pop();

                    return;
                };

            authenticator.Error +=
                (s, ea) =>
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Error = ").AppendLine($"{ea.Message}");

                    DisplayAlert
                            (
                                "Authentication Error",
                                sb.ToString(),
                                "OK"
                            );

                    Pop();

                    return;
                };

            AuthenticationState.Authenticator = authenticator;
            return authenticator;
        }

        private OAuth2Authenticator BuildGoogleAuthenticator()
        {
            Xamarin.Auth.OAuth2Authenticator authenticator = new Xamarin.Auth.OAuth2Authenticator
                        (
                            clientId: new Func<string>(() =>
                            {
                                string retval_client_id = "oops something is wrong!";

                                switch (Xamarin.Forms.Device.RuntimePlatform)
                                {
                                    case "Android":
                                        retval_client_id = "___.apps.googleusercontent.com";
                                        break;
                                    case "iOS":
                                        retval_client_id = "1088006062727-iapgdobasdgml7j3eratjls7h27ecr1c.apps.googleusercontent.com";
                                        break;
                                }
                                return retval_client_id;
                            }).Invoke(),
                            clientSecret: null,
                            authorizeUrl: new Uri("https://accounts.google.com/o/oauth2/auth"),
                            accessTokenUrl: new Uri("https://www.googleapis.com/oauth2/v4/token"),
                            redirectUrl: new Func<Uri>(() =>
                            {

                                string uri = null;

                                switch (Xamarin.Forms.Device.RuntimePlatform)
                                {
                                    case "Android":
                                        uri = "com.googleusercontent.apps.___:/oauth2redirect";
                                        break;
                                    case "iOS":
                                        uri = "com.googleusercontent.apps.1088006062727-iapgdobasdgml7j3eratjls7h27ecr1c:/oauth2redirect";
                                        break;
                                }

                                return new Uri(uri);
                            }).Invoke(),
                            scope: "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/drive.readonly",
                            getUsernameAsync: null,
                            isUsingNativeUI: true
                        )
            {
                AllowCancel = true,
            };

            authenticator.Completed +=
                (s, ea) =>
                {
                    StringBuilder sb = new StringBuilder();

                    if (ea.Account != null && ea.Account.Properties != null)
                    {
                        string token = ea.Account.Properties["access_token"];
                        AuthenticationState.Token = token;

                        sb.Append("Token = ").AppendLine($"{token}");                        
                    }
                    else
                    {
                        sb.Append("Not authenticated ").AppendLine($"Account.Properties does not exist");
                    }

                    DisplayAlert
                            (
                                "Authentication Results",
                                sb.ToString(),
                                "OK"
                            );

                    Pop();

                    return;
                };

            authenticator.Error +=
                (s, ea) =>
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Error = ").AppendLine($"{ea.Message}");

                    DisplayAlert
                            (
                                "Authentication Error",
                                sb.ToString(),
                                "OK"
                            );

                    Pop();

                    return;
                };

            AuthenticationState.Authenticator = authenticator;
            return authenticator;
        }

        private void Pop()
        {
            if (navigation_push_modal == true)
            {
                Application.Current.MainPage.Navigation.PopModalAsync();
            }
            else
            {

            }
        }

        bool navigation_push_modal = false;
        private void PresentUILoginScreen(OAuth2Authenticator authenticator)
        {
            Xamarin.Auth.XamarinForms.AuthenticatorPage ap;
            ap = new Xamarin.Auth.XamarinForms.AuthenticatorPage()
            {
                Authenticator = authenticator,
            };

            if (navigation_push_modal == true)
            {
                Application.Current.MainPage.Navigation.PushModalAsync(ap);
            }
            else
            {
                Application.Current.MainPage.Navigation.PushAsync(ap);
            }

            return;
        }

        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            if (AuthenticationState.Token != null)
            {
                REST.RestService.GetGooGle().Token = AuthenticationState.Token;
                var file = await REST.RestService.GetGooGle().GetGoogleFile("1fYK1pCXVhpdSomRt9ivnFn1KgL9SeLBC");

                await DisplayAlert
                (
                    "PNG File Size",
                    file.Length.ToString(),
                    "OK"
                );
            }

            return;
        }

        private async void Button_Clicked_3(object sender, EventArgs e)
        {
            if (AuthenticationState.Token != null)
            {
                REST.RestService.GetGooGle().Token = AuthenticationState.Token;
                var file = await REST.RestService.GetGooGle().GetGoogleFile("13z8Vsq0rhhB1Qw2v1mVRKnGR7F3Q7uH5");

                await DisplayAlert
                (
                    "JPEG File Size",
                    file.Length.ToString(),
                    "OK"
                );
            }

            return;
        }

        private async void Button_Clicked_4(object sender, EventArgs e)
        {
            if (AuthenticationState.Token != null)
            {
                REST.RestService.GetGooGle().Token = AuthenticationState.Token;
                var file = await REST.RestService.GetGooGle().GetGoogleFile("1MBL2TfCXQj-zZrQp5h0vNCW-zSxyeAnL");

                await DisplayAlert
                (
                    "PDF File Size",
                    file.Length.ToString(),
                    "OK"
                );
            }

            return;
        }

        private async void Button_Clicked_7(object sender, EventArgs e)
        {
            if (AuthenticationState.Token != null)
            {
                REST.RestService.GetBox().Token = AuthenticationState.Token;
                var file = await REST.RestService.GetBox().GetBoxFile("252250532950");

                await DisplayAlert
                (
                    "PNG File Size",
                    file.Length.ToString(),
                    "OK"
                );
            }

            return;
        }

        private async void Button_Clicked_8(object sender, EventArgs e)
        {
            if (AuthenticationState.Token != null)
            {
                REST.RestService.GetBox().Token = AuthenticationState.Token;
                var file = await REST.RestService.GetBox().GetBoxFile("252250549596");

                await DisplayAlert
                (
                    "JPEG File Size",
                    file.Length.ToString(),
                    "OK"
                );
            }

            return;
        }

        private async void Button_Clicked_9(object sender, EventArgs e)
        {
            if (AuthenticationState.Token != null)
            {
                REST.RestService.GetBox().Token = AuthenticationState.Token;
                var file = await REST.RestService.GetBox().GetBoxFile("3881621740");

                await DisplayAlert
                (
                    "PDF File Size",
                    file.Length.ToString(),
                    "OK"
                );
            }

            return;
        }

    }
}

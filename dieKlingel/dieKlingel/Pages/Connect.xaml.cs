using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.NetworkInformation;
using System.IO;
using System.Net;
using Xamarin.Essentials;
using Acr.UserDialogs;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using Renci.SshNet;

namespace dieKlingel.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Connect : ContentPage
    {
        private const string HOSTNAME = "dieklingel";
        private bool isSearching = false;
        private List<IPAddress> devices = new List<IPAddress>();
        public Connect()
        {
            InitializeComponent();

            Finder.SecondIcon.IsVisible = false;

            Finder.FirstIcon.Clicked += Finder_FirstIconClicked;
            Finder.SecondIcon.Clicked += Finder_SecondIconClicked;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            isSearching = false;
        }

        private void SearchForDevices(string hostname)
        {
            devices.Clear();
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostname);
                foreach (IPAddress device in hostEntry.AddressList)
                {
                    if (device.AddressFamily == AddressFamily.InterNetwork)
                    {
                        try
                        {
                            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            IPEndPoint endPoint = new IPEndPoint(device, 22);
                            IAsyncResult ar = socket.BeginConnect(endPoint, null, null);
                            ar.AsyncWaitHandle.WaitOne(10000);
                            if (ar.IsCompleted)
                            {
                                socket.EndConnect(ar);
                                devices.Add(device);
                            }
                            socket.Close();
                        }
                        catch (SocketException e)
                        {
                            Console.WriteLine(e.Message);
                        }  
                        
                    }
                    if (!isSearching)
                    {
                        break;
                    }

                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                NavigationPage.SetHasBackButton(this, true);
                Finder.StopSearching();
                BtnSearch.IsVisible = true;
                if (devices.Count > 0)
                {
                    Finder.SecondIcon.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Offline", "Es wurde keine Klingel in deiner Nähe gefunden", "Ok");
                }
            });
            isSearching = false;
        }

        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            BtnSearch.IsVisible = false;
            Finder.SecondIcon.IsVisible = false;
            Finder.StartSearching();
            NavigationPage.SetHasBackButton(this, false);
            isSearching = true;

            Task.Run(() => SearchForDevices(HOSTNAME));
        }

        private void Finder_FirstIconClicked(object sender, EventArgs e)
        {
            //BtnSearch.IsVisible = true;
            //Finder.StopSearching();
        }

        private async void Finder_SecondIconClicked(object sender, EventArgs e)
        {

            IPAddress ipaddress = null;
            if (devices.Count > 1)
            {
                List<string> devs = new List<string>();
                foreach (IPAddress device in devices)
                {
                    devs.Add(device.ToString());
                }
                //Finder.StopSearching();
                string result = await DisplayActionSheet("Klingel", "Abbrechen", null, devs.ToArray());
                if(result != "Abbrechen")
                {
                    ipaddress = IPAddress.Parse(result);
                }
            }
            else
            {
                ipaddress = IPAddress.Parse(devices[0].ToString());
                //await DisplayAlert("", devices[0].ToString(), "Ok");
            }
            if(ipaddress != null)
            {
                CheckLogin(ipaddress, "Gib deine Lokalen Login Informationen ein");
            }
        }

        private async void CheckLogin(IPAddress ipaddress, string message)
        {
            LoginResult loginResult = await UserDialogs.Instance.LoginAsync("Verbinden", message, null);
            if (loginResult.Ok && !string.IsNullOrEmpty(loginResult.LoginText) && !string.IsNullOrEmpty(loginResult.Password))
            {
                using (SshClient sshClient = new SshClient(ipaddress.ToString(), 22, loginResult.LoginText, loginResult.Password))
                {
                    try
                    {
                        sshClient.Connect();
                        sshClient.Disconnect();
                        await Navigation.PushAsync(new Pages.RemoteEditor(loginResult.LoginText, loginResult.Password, ipaddress, 22));
                    }
                    catch (Exception e)
                    {
                        //await DisplayAlert("Error", "Der Benutzername oder das Password ist nicht korrekt", "Erneut versuchen", "Abbrechen");
                        CheckLogin(ipaddress, "Der Benutzername oder das Password ist nicht korrekt");
                    }
                }
            }
        }
    }
}
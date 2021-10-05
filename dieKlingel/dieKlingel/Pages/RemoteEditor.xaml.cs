using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Acr.UserDialogs;
using System.Threading;

namespace dieKlingel.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RemoteEditor : ContentPage
    {
        const string BASEPATH = "/etc/dieklingel";
       
        private string username = "";
        private string password = "";
        private IPAddress ipaddress = IPAddress.None;
        private int port = 22;
        private ContentView editor = new Controls.Editor.Plain();

        private Dictionary<string, string> files = new Dictionary<string, string>();
        private int lastSelectedFile = -1;

        public RemoteEditor(string username, string password, IPAddress ipaddress, int port = 22)
        {
            InitializeComponent();
            this.username = username;
            this.password = password;
            this.ipaddress = ipaddress;
            this.port = port;
        }

        private void RemoteEditor_CustomCommand(object sender, Controls.Editor.CustomCommandEventArgs e)
        {
            using (SshClient sshClient = new SshClient(ipaddress.ToString(), port, username, password))
            {
                sshClient.Connect();
                SshCommand cmd = sshClient.CreateCommand(e.Command);
                cmd.Execute();
                sshClient.Disconnect();
            } 
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            files.Add("Konfiguration", "config.ini");
            files.Add("Benutzer", "users.json");
            files.Add("Befehle", "commands.cmd");
            files.Add("on-boot Event", "scripts/on-boot.sh");
            files.Add("on-ring Event", "scripts/on-ring.sh");
            files.Add("on-unlock Event", "scripts/on-unlock.sh");
            files.Add("on-call-start Event", "scripts/on-call-start.sh");
            files.Add("on-call-end Event", "scripts/on-call-end.sh");

            PckrFile.ItemsSource = new List<string>(files.Keys);
        }

        private async void BtnSave_Clicked(object sender, EventArgs e)
        {
            var choices = new[] { "Nur Speichern", "Nachladen" };

            var choice = await UserDialogs.Instance.ActionSheetAsync("Speichern ?", "Abbrechen", null, CancellationToken.None, choices);

            if (!string.IsNullOrEmpty(choice) && choice != "Abbrechen")
            {
                using (SftpClient sftpClient = new SftpClient(ipaddress.ToString(), port, username, password))
                {
                    sftpClient.Connect();
                    // Uploading File to server
                    string file = ((Controls.Editor.IBase)editor).Text;
                    using (MemoryStream fileStream = GenerateStreamFromString(file))
                    {
                        sftpClient.UploadFile(fileStream, Path.Combine(BASEPATH, files[PckrFile.SelectedItem.ToString()] ));
                    } 
                    sftpClient.Disconnect();
                }
                if(choice == "Nachladen")
                {
                    using (SshClient sshClient = new SshClient(ipaddress.ToString(), port, username, password))
                    {
                        sshClient.Connect();
                        SshCommand cmd = sshClient.CreateCommand("sudo systemctl restart dieklingel && sudo systemctl restart dieklingel-gui");
                        cmd.Execute();
                        sshClient.Disconnect();
                    }
                }
            }
        }

        private async void BtnAdd_Clicked(object sender, EventArgs e)
        {
            string message = "Mit Benutzerdefinierten Dateien kann die Strukture der Basis zerstört werden, fahren Sie nur fort, wenn Sie sich Sicher sind was Sie tun.";
            bool res = await DisplayAlert("Warning", message, "Weiter", "Abbrechen");
            if(res)
            {
                PromptResult result = await UserDialogs.Instance.PromptAsync("Dateiname eingeben", "Benutzerdefinierte Datei", "Speichern", "Abbrechen");

                if (result.Ok && !string.IsNullOrWhiteSpace(result.Text))
                {
                    files.Add("*" + result.Text, result.Text);
                }
                PckrFile.ItemsSource = new List<string>(files.Keys);
                PckrFile.SelectedIndex = lastSelectedFile;
            }
        }

        private void BtnReload_Clicked(object sender, EventArgs e)
        {
            if (PckrFile.SelectedIndex >= 0)
            {
                LoadFile(files[PckrFile.SelectedItem.ToString()]);
            }
        }

        private void PckrFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PckrFile.SelectedIndex >= 0)
            {
                LoadFile(files[PckrFile.SelectedItem.ToString()]);
            }
        }

        private void EdtrFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Text Changed");
        }

        private async void LoadFile(string filename)
        {
            using (SftpClient sftpClient = new SftpClient(ipaddress.ToString(), port, username, password))
            {
                sftpClient.Connect();
                using (MemoryStream fileStream = new MemoryStream())
                {
                    try
                    {
                        sftpClient.DownloadFile(Path.Combine(BASEPATH, filename), fileStream);
                        string extension = files[PckrFile.SelectedItem.ToString()].Split(".").Last().ToLower();
                        StackLayout stackLayout = this.FindByName<StackLayout>("stack_layout");
                        stackLayout.Children.Remove(editor);
                        switch (extension)
                        {
                            case "ini":
                                editor = new Controls.Editor.Ini();
                                break;
                            case "json":
                                editor = new Controls.Editor.Json();
                                //editor = new Controls.Editor.Plain();
                                break;
                            case "cmd":
                                editor = new Controls.Editor.Cmd();
                                break;
                            default:
                                editor = new Controls.Editor.Plain();
                                break;
                        }
                        //editor.Margin = 10;
                        ((Controls.Editor.IBase)editor).Text = Encoding.ASCII.GetString(fileStream.ToArray());
                        editor.VerticalOptions = LayoutOptions.FillAndExpand;
                        ((Controls.Editor.IBase)editor).CustomCommand += RemoteEditor_CustomCommand;
                        //EdtrFile.Text = Encoding.ASCII.GetString(fileStream.ToArray());
                        stackLayout.Children.Add(editor);
                        lastSelectedFile = PckrFile.SelectedIndex;
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error on sftp:" + ex.Message);
                        await DisplayAlert("Error", "Die gewählte Datei wird nicht Unterstützt!", "Ok");
                        PckrFile.SelectedIndex = lastSelectedFile;

                    }
                }
                sftpClient.Disconnect();
            }
        }

        private static MemoryStream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseManage.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace FirebaseManage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Authentication : Window
    {
        private readonly BackgroundWorker loadDataBackground;
        private IEnumerable<dynamic> userInListView = new List<dynamic>();
        public Authentication()
        {
            InitializeComponent();

            loadDataBackground = new BackgroundWorker();
            loadDataBackground.DoWork += LoadDataBackground_DoWork;
            loadDataBackground.RunWorkerCompleted += LoadDataBackground_RunWorkerCompleted;
        }

        private void LoadDataBackground_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            pgbLoading.Visibility = Visibility.Hidden;

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                return;
            }
            
            new Main(userInListView).Show();
            this.Close();
        }

        private void LoadDataBackground_DoWork(object? sender, DoWorkEventArgs e)
        {
            var page = Server.FirebaseAuth
                .ListUsersAsync(new ListUsersOptions { PageSize = 20 })
                .ReadPageAsync(20)
                .Result;
            Server.NextPageToken.Add("user", page.NextPageToken);
            Server.CurrentPageToken.Add("user", string.Empty);

            userInListView = page.GetEnumerator().ToUsers();



        }

        private void ImportFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "JSON|*json";
            dialog.Title = "Tải tệp xác thực Admin Firebase";

            if (dialog.ShowDialog() == true)
            {
                pgbLoading.Visibility = Visibility.Visible;
                try
                {
                    Server.FirebaseApp = FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(dialog.FileName)
                    });

                    loadDataBackground.RunWorkerAsync();
                }
                catch (Exception error)
                {
                    pgbLoading.Visibility = Visibility.Hidden;
                    MessageBox.Show(error.Message);
                }
            }
        }

        private void OpenTutoral(object sender, RoutedEventArgs e)
        {
            modalTutoral.IsOpen = true;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = (sender as Hyperlink).NavigateUri.AbsoluteUri,
                UseShellExecute = true
            });
        }
    }
}

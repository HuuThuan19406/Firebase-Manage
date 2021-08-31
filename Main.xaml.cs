using FirebaseAdmin.Auth;
using FirebaseManage.Models;
using Seleni.VietnamFormat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FirebaseManage
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        private readonly BackgroundWorker refreshDataBackground;
        private readonly BackgroundWorker createUserBackground;
        private IEnumerable<dynamic> userInListView = new List<dynamic>();
        private UserRecordArgs newUser;
        private string roleOfNewUser;

        public Main(IEnumerable<dynamic> userInListView)
        {
            InitializeComponent();
            lvUsers.ItemsSource = userInListView;

            refreshDataBackground = new BackgroundWorker();
            refreshDataBackground.DoWork += (s, e) => RefreshData();
            refreshDataBackground.RunWorkerCompleted += RefreshDataBackground_RunWorkerCompleted;

            createUserBackground = new BackgroundWorker();
            createUserBackground.DoWork += CreateUserBackground_DoWork;
            createUserBackground.RunWorkerCompleted += CreateUserBackground_RunWorkerCompleted;
        }

        private void CreateUserBackground_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            pgbModalLoaing.Visibility = Visibility.Hidden;

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                modalCreateUser.IsOpen = false;
                refreshDataBackground.RunWorkerAsync();
                MessageBox.Show("Tạo tài khoản thành công");

                txtDialogDisplayName.Text = txtDialogEmail.Text = txtDialogPassword.Text = txtDialogPhoneNumber.Text = txtDialogRole.Text = txtDialogUrlAvatar.Text = string.Empty;
            }
        }

        private void CreateUserBackground_DoWork(object? sender, DoWorkEventArgs e)
        {
            var userCreated = Server.FirebaseAuth
                .CreateUserAsync(newUser)
                .Result;

            if ((userCreated != null) && !string.IsNullOrWhiteSpace(roleOfNewUser))
            {
                Server.FirebaseAuth
                    .SetCustomUserClaimsAsync(userCreated.Uid, new Dictionary<string, object>
                    {
                        {roleOfNewUser, true }
                    })
                    .Wait();
            }
        }

        private void RefreshDataBackground_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            pgbLoading.Visibility = Visibility.Hidden;
            lvUsers.ItemsSource = userInListView;

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
        }

        private void RefreshData()
        {
            var page = Server.FirebaseAuth
                .ListUsersAsync(new ListUsersOptions { PageSize = 20 })
                .ReadPageAsync(20)
                .Result;

            userInListView = page.GetEnumerator().ToUsers();
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    user = Server.FirebaseAuth
            //    .GetUserByEmailAsync(txtEmail.Text)
            //    .Result;

            //    txtDisplayName.Text = user.DisplayName;
            //    //txtStatusEmail.Text = user.EmailVerified ? "Đã xác thực" : "Chưa xác thực";
            //    //txtLastLogin.Text = user.UserMetaData.LastSignInTimestamp.HasValue ? user.UserMetaData.LastSignInTimestamp.Value.ToString("HH:mm:ss ddd, dd MMM yyyy") : "Chưa đăng nhập";
            //}
            //catch(Exception error)
            //{
            //    MessageBox.Show(error.Message);
            //}
        }

        private void Decentralization(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, object> claim = new Dictionary<string, object>
                {
                     {txtRole.Text, true }
                };

                Server.FirebaseAuth
                    .SetCustomUserClaimsAsync(txbUid.Text, claim)
                    .Wait();

                pgbLoading.Visibility = Visibility.Visible;
                refreshDataBackground.RunWorkerAsync();
                MessageBox.Show("Phân quyền hoàn tất");
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void lvUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvUsers.SelectedIndex == -1)
            {
                txtDisplayName.Text = txbUid.Text = txtRole.Text = txbLastSignIn.Text = string.Empty;
                return;
            }

            var user = (lvUsers.SelectedItem as dynamic).User as ExportedUserRecord;
            if (user != null)
            {
                txtDisplayName.Text = user.DisplayName;
                txbUid.Text = user.Uid;
                txtRole.Text = user.CustomClaims.Keys.FirstOrDefault();

                btnToogleVerifyEmail.Content = user.EmailVerified ? "Đánh dấu email chưa được xác thực" : "Đánh dấu đã xác thực email";
                btnToogleVerifyEmail.Tag = user.EmailVerified;

                btnToogleDisable.Content = user.Disabled ? "Cho phép tài khoản hoạt động" : "Vô hiệu hóa tài khoản";
                btnToogleDisable.Tag = user.Disabled;
                btnToogleDisable.Background = user.Disabled ? Brushes.Green : (FindResource("MaterialDesignValidationErrorBrush") as SolidColorBrush);

                txbLastSignIn.Text = user.UserMetaData.LastSignInTimestamp.HasValue ? user.UserMetaData.LastSignInTimestamp.Value.ToLocalTime().VietnamFormatFullDateTime() : "_";
                txbCreatedAt.Text = user.UserMetaData.CreationTimestamp.HasValue ? user.UserMetaData.CreationTimestamp.Value.ToLocalTime().VietnamFormatShortDate() : "_";
            }
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbUid.Text);
        }

        private void RefreshData(object sender, RoutedEventArgs e)
        {
            try
            {
                pgbLoading.Visibility = Visibility.Visible;
                refreshDataBackground.RunWorkerAsync();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void UpdateUser(UserRecordArgs user)
        {
            try
            {
                Server.FirebaseAuth
                    .UpdateUserAsync(user)
                    .Wait();

                pgbLoading.Visibility = Visibility.Visible;
                refreshDataBackground.RunWorkerAsync();
                MessageBox.Show("Cập nhật hoàn tất");
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void UpdateInfo(object sender, RoutedEventArgs e)
        {
            UpdateUser(new UserRecordArgs
            {
                Uid = txbUid.Text,
                DisplayName = txtDisplayName.Text
            });
        }

        private void ToogleVerifyEmail(object sender, RoutedEventArgs e)
        {
            UpdateUser(new UserRecordArgs
            {
                Uid = txbUid.Text,
                EmailVerified = !Convert.ToBoolean(btnToogleVerifyEmail.Tag)
            });
        }

        private void ToogleDisable(object sender, RoutedEventArgs e)
        {
            UpdateUser(new UserRecordArgs
            {
                Uid = txbUid.Text,
                Disabled = !Convert.ToBoolean(btnToogleDisable.Tag)
            });
        }

        private void lvUsers_LostFocus(object sender, RoutedEventArgs e)
        {
            lvUsers.SelectedIndex = -1;
        }

        private void CreateUser(object sender, RoutedEventArgs e)
        {
            newUser = new UserRecordArgs
            {
                Email = txtDialogEmail.Text,
                Password = txtDialogPassword.Text,
                DisplayName = string.IsNullOrWhiteSpace(txtDialogDisplayName.Text) ? null : txtDialogDisplayName.Text,
                PhoneNumber = string.IsNullOrWhiteSpace(txtDialogPhoneNumber.Text) ? null : txtDialogPhoneNumber.Text,
                PhotoUrl = string.IsNullOrWhiteSpace(txtDialogUrlAvatar.Text) ? null : txtDialogUrlAvatar.Text,
                EmailVerified = true
            };

            roleOfNewUser = txtDialogRole.Text;

            pgbModalLoaing.Visibility = Visibility.Visible;
            createUserBackground.RunWorkerAsync();
        }

        private void OpenCreateUser(object sender, RoutedEventArgs e)
        {
            modalCreateUser.IsOpen = true;
        }

        private void DeleteUser(object sender, RoutedEventArgs e)
        {
            if (lvUsers.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                var user = (lvUsers.SelectedItem as dynamic).User as ExportedUserRecord;                

                if (MessageBox.Show($"Xác nhận xóa tài khoản {user.Email} ?", string.Empty, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    Server.FirebaseAuth
                        .DeleteUserAsync(user.Uid)
                        .Wait();

                    MessageBox.Show("Đã xóa tài khoản");

                    pgbLoading.Visibility = Visibility.Visible;
                    refreshDataBackground.RunWorkerAsync();
                }
            }
            catch(Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
    }
}

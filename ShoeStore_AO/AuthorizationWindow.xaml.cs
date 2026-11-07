using System.Linq;
using System.Windows;

namespace ShoeStore_AO
{
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow() { InitializeComponent(); }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            { MessageBox.Show("Введите логин и пароль!"); return; }

            using (var db = new ShoeStore_AOEntities1())
            {
                var user = db.Users.Include("Roles")
                    .FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user == null) { MessageBox.Show("Неверный логин или пароль!"); return; }

                CurrentUser.UserID = user.UserID;
                CurrentUser.FullName = user.FullName;
                CurrentUser.RoleName = user.Roles != null ? user.Roles.RoleName : "Клиент";

                new MainMenuWindow().Show();
                this.Close();
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.UserID = 0;
            CurrentUser.FullName = "Гость";
            CurrentUser.RoleName = "Гость";
            new MainMenuWindow().Show();
            this.Close();
        }
    }
}

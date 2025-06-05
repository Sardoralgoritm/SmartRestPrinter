using SmartRestaurant.BusinessLogic.Services.Users.DTOs;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Components.User
{
    /// <summary>
    /// Interaction logic for UserComponent.xaml
    /// </summary>
    public partial class UserComponent : UserControl, IDisposable
    {

        public event EventHandler<Guid>? UserDeleted;
        public event EventHandler<Guid>? UserUpdated;
        public event EventHandler<Guid>? UserViewed;
        private Guid _userId;
        public UserComponent()
        {
            InitializeComponent();
        }

        public void SetUserData(UserDto user, int number)
        {
            _userId = user.Id;
            txtName.Text = user.FirstName;
            txtSurname.Text = user.LastName;
            txtPhoneNumber.Text = user.PhoneNumber;
            txtRole.Text = user.Role;
            no.Text = number.ToString();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxManager.ShowConfirmation("Siz haqiqatan ham bu hodimni o'chirishni xohlaysizmi?"))
            {
                UserDeleted?.Invoke(this, _userId);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            UserUpdated?.Invoke(this, _userId);
        }

        private void View_Button_Click(object sender, RoutedEventArgs e)
        {
            UserViewed?.Invoke(this, _userId);
        }

        public void Dispose()
        {
            UserDeleted = null;
            UserUpdated = null;
            UserViewed = null;
        }
    }
}

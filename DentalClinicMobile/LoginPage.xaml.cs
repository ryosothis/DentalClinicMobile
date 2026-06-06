using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DentalClinicMobile
{
    public partial class LoginPage : ContentPage
    {
        private DatabaseService _databaseService;

        public LoginPage()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                string email = EmailEntry.Text?.Trim();
                string password = PasswordEntry.Text;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    await DisplayAlert("Ошибка", "Введите email и пароль", "OK");
                    return;
                }

                var user = await _databaseService.AuthorizeUserAsync(email, password);

                if (user != null)
                {
                    AuthManager.CurrentUser = user;
                    AuthManager.CurrentUserEmail = email;

                    await DisplayAlert("Успех", "Вход выполнен успешно!", "OK");

                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    await DisplayAlert("Ошибка", "Неверный email или пароль", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка входа: {ex.Message}", "OK");
            }
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }

        private async void OnHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }

        private async void OnAboutTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AboutPage());
        }

        private async void OnServicesTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ServicesPage());
        }

        private async void OnPriceTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PricePage());
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
                await Navigation.PushAsync(new ProfilePage());
            else
                await Navigation.PushAsync(new LoginPage());
        }
    }
}
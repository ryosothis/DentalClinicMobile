using System;
using System.Text.RegularExpressions;

namespace DentalClinicMobile
{
    public partial class RegisterPage : ContentPage
    {
        private DatabaseService _databaseService;

        public RegisterPage()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
            BirthDatePicker.MaximumDate = DateTime.Today;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            try
            {
                string firstName = FirstNameEntry.Text?.Trim();
                string lastName = LastNameEntry.Text?.Trim();
                string middleName = MiddleNameEntry.Text?.Trim();
                string phone = PhoneEntry.Text?.Trim();
                string email = EmailEntry.Text?.Trim();
                string password = PasswordEntry.Text;
                string confirmPassword = ConfirmPasswordEntry.Text;
                DateTime? birthDate = BirthDatePicker.Date;
                bool agreement = AgreementCheckBox.IsChecked;

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                    string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    await DisplayAlert("Ошибка", "Заполните все обязательные поля (Имя, Фамилия, Email, Пароль)", "OK");
                    return;
                }

                if (!agreement)
                {
                    await DisplayAlert("Ошибка", "Необходимо согласие с условиями использования", "OK");
                    return;
                }

                if (password != confirmPassword)
                {
                    await DisplayAlert("Ошибка", "Пароли не совпадают", "OK");
                    return;
                }

                if (password.Length < 6)
                {
                    await DisplayAlert("Ошибка", "Пароль должен содержать минимум 6 символов", "OK");
                    return;
                }

                if (!IsValidEmail(email))
                {
                    await DisplayAlert("Ошибка", "Введите корректный email", "OK");
                    return;
                }

                if (birthDate == null || birthDate.Value == DateTime.MinValue)
                {
                    await DisplayAlert("Ошибка", "Укажите дату рождения", "OK");
                    return;
                }

                if (birthDate.Value > DateTime.Today)
                {
                    await DisplayAlert("Ошибка", "Дата рождения не может быть в будущем", "OK");
                    return;
                }

                if (await _databaseService.CheckEmailExistsAsync(email))
                {
                    await DisplayAlert("Ошибка", "Пользователь с таким email уже существует", "OK");
                    return;
                }

                int? userId = await _databaseService.RegisterUserAsync(
                    email, password, firstName, middleName, lastName, phone, birthDate.Value);

                if (userId.HasValue && userId.Value > 0)
                {
                    await DisplayAlert("Успех", "Регистрация прошла успешно!", "OK");
                    await Navigation.PushAsync(new LoginPage());
                }
                else
                {
                    await DisplayAlert("Ошибка", "Ошибка при регистрации пользователя", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
        }

        private async void OnLoginTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
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
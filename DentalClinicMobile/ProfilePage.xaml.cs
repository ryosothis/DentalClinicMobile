using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace DentalClinicMobile
{
    public partial class ProfilePage : ContentPage
    {
        private DatabaseService _databaseService;
        private int _currentUserId;

        public ProfilePage(int userId)
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
            _currentUserId = userId;
            Loaded += async (s, e) => await LoadDataAsync();
        }

        public ProfilePage() : this(GetCurrentUserIdFromAuth()) { }

        private static int GetCurrentUserIdFromAuth()
        {
            if (AuthManager.CurrentUser != null) return AuthManager.CurrentUser.Id;
            var userIdFromApp = AuthManager.GetCurrentUserId();
            return userIdFromApp.HasValue ? userIdFromApp.Value : -1;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_currentUserId > 0)
            {
                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (_currentUserId <= 0)
                {
                    await Navigation.PushAsync(new MainPage());
                    return;
                }
                ShowLoadingIndicator(true);
                bool userExists = await _databaseService.UserExistsAsync(_currentUserId);
                if (!userExists)
                {
                    await DisplayAlert("Ошибка", "Пользователь не найден. Пожалуйста, войдите снова.", "OK");
                    AuthManager.Logout();
                    await Navigation.PushAsync(new LoginPage());
                    return;
                }
                await Task.WhenAll(LoadUserDataAsync(), LoadMedicalHistoryAsync());
                ShowRoleSpecificButtons();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки данных: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private void ShowRoleSpecificButtons()
        {
            RoleButtonsPanel.IsVisible = true;
            if (AuthManager.IsDoctor())
                DoctorAppointmentsButton.IsVisible = true;
        }

        private async Task LoadUserDataAsync()
        {
            try
            {
                DataTable userData = await _databaseService.GetUserProfileAsync(_currentUserId);
                if (userData.Rows.Count > 0)
                {
                    DataRow user = userData.Rows[0];
                    string firstName = user["first_name"].ToString();
                    string lastName = user["last_name"].ToString();
                    string middleName = user["middle_name"]?.ToString() ?? "";
                    string fullName = $"{lastName} {firstName} {middleName}".Trim();
                    FullNameTextBox.Text = fullName;

                    if (user["birth_date"] != DBNull.Value)
                    {
                        DateTime birthDate = (DateTime)user["birth_date"];
                        BirthDateTextBox.Text = birthDate.ToString("dd.MM.yyyy");
                    }
                    else BirthDateTextBox.Text = "Не указана";

                    EmailTextBox.Text = user["email"].ToString();
                    PhoneTextBox.Text = user["phone_number"]?.ToString() ?? "Не указан";
                    RoleTextBlock.Text = $"Роль: {AuthManager.GetRoleName()}";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки профиля: {ex.Message}", "OK");
            }
        }

        private async Task LoadMedicalHistoryAsync()
        {
            try
            {
                DataTable medicalHistory = await _databaseService.GetUserMedicalHistoryAsync(_currentUserId);
                MedicalHistoryStackLayout.Children.Clear();
                if (medicalHistory.Rows.Count > 0)
                {
                    foreach (DataRow record in medicalHistory.Rows)
                    {
                        var border = new Border
                        {
                            BackgroundColor = Colors.White,
                            StrokeShape = new RoundRectangle { CornerRadius = 12 },
                            Stroke = Color.FromArgb("#E1E8FF"),
                            StrokeThickness = 1,
                            Padding = new Thickness(15)
                        };

                        var grid = new Grid();
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                        string diagnosis = record["diagnosis"]?.ToString() ?? "Консультация";
                        string doctorName = record["doctor_name"]?.ToString() ?? "Врач не указан";
                        DateTime visitDate = (DateTime)record["visit_date"];

                        var leftStack = new VerticalStackLayout();
                        leftStack.Add(new Label { Text = "Медицинская запись", TextColor = Color.FromArgb("#828CA0"), FontSize = 10, FontAttributes = FontAttributes.Bold });
                        leftStack.Add(new Label { Text = diagnosis, FontAttributes = FontAttributes.Bold, FontSize = 14, TextColor = Color.FromArgb("#244484") });
                        leftStack.Add(new Label { Text = $"Врач: {doctorName}", TextColor = Color.FromArgb("#244484"), FontSize = 12 });

                        string treatment = record["treatment"]?.ToString();
                        if (!string.IsNullOrEmpty(treatment))
                        {
                            leftStack.Add(new Label { Text = $"Лечение: {treatment}", TextColor = Color.FromArgb("#244484"), FontSize = 11 });
                        }

                        var rightStack = new VerticalStackLayout
                        {
                            HorizontalOptions = LayoutOptions.End,
                            Margin = new Thickness(10, 0, 0, 0)
                        };
                        rightStack.Add(new Label { Text = "Дата приёма", TextColor = Color.FromArgb("#828CA0"), FontSize = 11, FontAttributes = FontAttributes.Bold });
                        rightStack.Add(new Label { Text = visitDate.ToString("dd.MM.yyyy HH:mm"), FontAttributes = FontAttributes.Bold, FontSize = 12, TextColor = Color.FromArgb("#244484") });

                        grid.Add(leftStack, 0, 0);
                        grid.Add(rightStack, 1, 0);
                        border.Content = grid;
                        MedicalHistoryStackLayout.Children.Add(border);
                    }
                }
                else
                {
                    MedicalHistoryStackLayout.Children.Add(new Label
                    {
                        Text = "Медицинских записей не найдено",
                        TextColor = Color.FromArgb("#828CA0"),
                        FontSize = 14,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки истории: {ex.Message}", "OK");
            }
        }

        private async void OnEditNameClicked(object sender, EventArgs e)
        {
            try
            {
                string currentFullName = FullNameTextBox.Text;

                var dialog = new EditNameDialog(currentFullName);
                await Navigation.PushModalAsync(dialog);

                var (lastName, firstName, middleName) = await dialog.ShowDialogAsync();

                if (!string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(firstName))
                {
                    bool success = await _databaseService.UpdateUserNameAsync(_currentUserId, firstName, lastName, middleName);

                    if (success)
                    {
                        await DisplayAlert("Успех", "ФИО успешно обновлено", "OK");
                        string fullName = $"{lastName} {firstName} {middleName}".Trim();
                        FullNameTextBox.Text = fullName;

                        if (AuthManager.CurrentUser != null)
                        {
                            AuthManager.CurrentUser.FirstName = firstName;
                            AuthManager.CurrentUser.LastName = lastName;
                            AuthManager.CurrentUser.MiddleName = middleName;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Ошибка при обновлении ФИО", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
        }

        private async void OnEditBirthDateClicked(object sender, EventArgs e)
        {
            try
            {
                DateTime? currentDate = null;
                if (DateTime.TryParseExact(BirthDateTextBox.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    currentDate = parsedDate;
                }

                var dialog = new EditDateDialog("Дата рождения", currentDate);
                await Navigation.PushModalAsync(dialog);

                if (dialog.IsConfirmed)
                {
                    DateTime newDate = dialog.SelectedDate;

                    bool success = await _databaseService.UpdateUserBirthDateAsync(_currentUserId, newDate);

                    if (success)
                    {
                        await DisplayAlert("Успех", "Дата рождения успешно обновлена", "OK");
                        BirthDateTextBox.Text = newDate.ToString("dd.MM.yyyy");

                        if (AuthManager.CurrentUser != null)
                        {
                            AuthManager.CurrentUser.BirthDate = newDate;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Ошибка при обновлении даты рождения", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
        }
        private async void OnEditEmailClicked(object sender, EventArgs e)
        {
            try
            {
                string currentEmail = EmailTextBox.Text;

                var dialog = new EditTextDialog("Email", currentEmail, "Введите новый email");
                await Navigation.PushModalAsync(dialog);

                string newEmail = await dialog.ShowDialogAsync();

                if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != currentEmail)
                {
                    if (!IsValidEmail(newEmail))
                    {
                        await DisplayAlert("Ошибка", "Введите корректный email", "OK");
                        return;
                    }

                    if (await _databaseService.CheckEmailExistsAsync(newEmail))
                    {
                        await DisplayAlert("Ошибка", "Этот email уже используется", "OK");
                        return;
                    }

                    int result = await _databaseService.UpdateUserEmailAsync(_currentUserId, newEmail);

                    if (result == 1)
                    {
                        await DisplayAlert("Успех", "Email успешно обновлен", "OK");
                        EmailTextBox.Text = newEmail;
                        if (AuthManager.CurrentUser != null) AuthManager.CurrentUser.Email = newEmail;
                        AuthManager.CurrentUserEmail = newEmail;
                    }
                    else if (result == -1)
                    {
                        await DisplayAlert("Ошибка", "Этот email уже используется", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Ошибка при обновлении email", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
        }

        private async void OnEditPhoneClicked(object sender, EventArgs e)
        {
            try
            {
                string currentPhone = PhoneTextBox.Text == "Не указан" ? "" : PhoneTextBox.Text;

                var dialog = new EditTextDialog("Телефон", currentPhone, "Введите новый номер");
                await Navigation.PushModalAsync(dialog);

                string newPhone = await dialog.ShowDialogAsync();

                if (newPhone != null && newPhone != currentPhone)
                {
                    bool success = await _databaseService.UpdateUserPhoneAsync(_currentUserId, newPhone);

                    if (success)
                    {
                        await DisplayAlert("Успех", "Номер телефона обновлен", "OK");
                        PhoneTextBox.Text = string.IsNullOrEmpty(newPhone) ? "Не указан" : newPhone;
                        if (AuthManager.CurrentUser != null) AuthManager.CurrentUser.PhoneNumber = newPhone;
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Ошибка при обновлении телефона", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Выход", "Вы уверены, что хотите выйти?", "Да", "Нет");
            if (result)
            {
                AuthManager.Logout();
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try { return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"); }
            catch { return false; }
        }

        private void ShowLoadingIndicator(bool show)
        {
            ProgressBar.IsVisible = show;
            ProgressBar.IsRunning = show;
            MainContent.IsVisible = !show;
            RoleButtonsPanel.IsVisible = !show;
        }

        private async void OnAdminPanelClicked(object sender, EventArgs e) { }

        private async void OnDoctorAppointmentsClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new DoctorAppointmentsPage());

        private async void OnHomeTapped(object sender, EventArgs e)
            => await Navigation.PushAsync(new MainPage());

        private async void OnAboutTapped(object sender, EventArgs e)
            => await Navigation.PushAsync(new AboutPage());

        private async void OnServicesTapped(object sender, EventArgs e)
            => await Navigation.PushAsync(new ServicesPage());

        private async void OnPriceTapped(object sender, EventArgs e)
            => await Navigation.PushAsync(new PricePage());

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
                await Navigation.PushAsync(new ProfilePage());
            else
                await Navigation.PushAsync(new LoginPage());
        }
    }
}
using System;
using System.Data;
using Microsoft.Maui.Controls.Shapes;

namespace DentalClinicMobile
{
    public partial class AboutPage : ContentPage
    {
        private DatabaseService _databaseService;

        public AboutPage()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            Loaded += async (s, e) => await LoadDoctorsAsync();
        }

        private async Task LoadDoctorsAsync()
        {
            try
            {
                DataTable doctorsData = await _databaseService.GetDoctorsAsync();
                DoctorsFlexLayout.Children.Clear();

                if (doctorsData.Rows.Count > 0)
                {
                    foreach (DataRow doctor in doctorsData.Rows)
                    {
                        var doctorCard = CreateDoctorCard(doctor);
                        DoctorsFlexLayout.Children.Add(doctorCard);
                    }
                }
                else
                {
                    ShowNoDoctorsMessage();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки врачей: {ex.Message}", "OK");
            }
        }

        private Border CreateDoctorCard(DataRow doctor)
        {
            var border = new Border
            {
                Style = (Style)Resources["DoctorCardStyle"]
            };

            var stack = new VerticalStackLayout();

            string firstName = doctor["first_name"].ToString();
            string lastName = doctor["last_name"].ToString();
            string initials = $"{firstName[0]}{lastName[0]}".ToUpper();

            var avatarBorder = new Border
            {
                WidthRequest = 50,
                HeightRequest = 50,
                BackgroundColor = Color.FromArgb("#E1E8FF"),
                StrokeShape = new RoundRectangle { CornerRadius = 25 },
                Stroke = Color.FromArgb("#C9DEFF"),
                StrokeThickness = 2,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var initialsLabel = new Label
            {
                Text = initials,
                TextColor = Color.FromArgb("#244484"),
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            avatarBorder.Content = initialsLabel;

            string middleName = doctor["middle_name"]?.ToString() ?? "";
            string fullName = $"{lastName} {firstName} {middleName}".Trim();

            var nameLabel = new Label
            {
                Text = fullName,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                TextColor = Color.FromArgb("#244484"),
                HorizontalOptions = LayoutOptions.Center
            };

            var specializationLabel = new Label
            {
                Text = doctor["specialization"]?.ToString() ?? "Стоматолог",
                TextColor = Color.FromArgb("#828CA0"),
                FontSize = 10,
                HorizontalOptions = LayoutOptions.Center
            };

            int experienceYears = Convert.ToInt32(doctor["experience_years"]);
            var experienceLabel = new Label
            {
                Text = $"{experienceYears} лет",
                TextColor = Color.FromArgb("#467EEA"),
                FontSize = 9,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 2, 0, 0)
            };

            stack.Children.Add(avatarBorder);
            stack.Children.Add(nameLabel);
            stack.Children.Add(specializationLabel);
            stack.Children.Add(experienceLabel);

            border.Content = stack;
            return border;
        }

        private void ShowNoDoctorsMessage()
        {
            var noDoctorsLabel = new Label
            {
                Text = "Информация о врачах временно недоступна",
                TextColor = Color.FromArgb("#828CA0"),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            DoctorsFlexLayout.Children.Add(noDoctorsLabel);
        }

        private async void OnHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }

        private void OnAboutTapped(object sender, EventArgs e)
        {
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

        private async void OnBookAppointmentClicked(object sender, EventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
                await Navigation.PushAsync(new ServicesPage());
            else
                await Navigation.PushAsync(new LoginPage());
        }
    }
}
using System;
using System.Data;
using Microsoft.Maui.Controls.Shapes;

namespace DentalClinicMobile
{
    public partial class DoctorAppointmentsPage : ContentPage
    {
        private DatabaseService _databaseService;
        private int _doctorUserId;
        private int _currentDoctorId;

        public DoctorAppointmentsPage()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
            _doctorUserId = AuthManager.GetCurrentUserId() ?? -1;

            Loaded += async (s, e) => await LoadAppointmentsAsync();
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                if (!AuthManager.IsDoctor())
                {
                    await DisplayAlert("Ошибка", "Доступ только для врачей", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                ShowLoadingIndicator(true);

                _currentDoctorId = await _databaseService.GetDoctorIdByUserIdAsync(_doctorUserId);

                if (_currentDoctorId <= 0)
                {
                    await DisplayAlert("Ошибка", "Профиль врача не найден", "OK");
                    return;
                }

                DataTable appointments = await _databaseService.GetDoctorAppointmentsForTodayAsync(_doctorUserId);

                DateLabel.Text = $"На {DateTime.Today:dd.MM.yyyy}";
                AppointmentsStackLayout.Children.Clear();

                if (appointments.Rows.Count > 0)
                {
                    NoAppointmentsLabel.IsVisible = false;

                    foreach (DataRow appointment in appointments.Rows)
                    {
                        var appointmentCard = CreateAppointmentCard(appointment);
                        AppointmentsStackLayout.Children.Add(appointmentCard);
                    }
                }
                else
                {
                    NoAppointmentsLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки записей: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private Border CreateAppointmentCard(DataRow appointment)
        {
            var border = new Border
            {
                BackgroundColor = Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Stroke = Color.FromArgb("#E1E8FF"),
                StrokeThickness = 1,
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 15)
            };

            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new VerticalStackLayout();

            var timeLabel = new Label
            {
                Text = $"⏰ {((DateTime)appointment["appointment_date"]):HH:mm}",
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                TextColor = Color.FromArgb("#244484"),
                Margin = new Thickness(0, 0, 0, 10)
            };

            string patientName = $"{appointment["last_name"]} {appointment["first_name"]} {appointment["middle_name"]}".Trim();
            var patientLabel = new Label
            {
                Text = $"👤 {patientName}",
                FontSize = 14,
                TextColor = Color.FromArgb("#244484"),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var serviceLabel = new Label
            {
                Text = $"🦷 {appointment["service_name"]}",
                FontSize = 14,
                TextColor = Color.FromArgb("#244484"),
                Margin = new Thickness(0, 0, 0, 5)
            };

            string phone = appointment["phone_number"]?.ToString() ?? "Не указан";
            var contactLabel = new Label
            {
                Text = $"📞 {phone}",
                FontSize = 12,
                TextColor = Color.FromArgb("#828CA0"),
                Margin = new Thickness(0, 0, 0, 10)
            };

            leftPanel.Children.Add(timeLabel);
            leftPanel.Children.Add(patientLabel);
            leftPanel.Children.Add(serviceLabel);
            leftPanel.Children.Add(contactLabel);

            Grid.SetColumn(leftPanel, 0);

            var rightPanel = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };

            var diagnosisButton = new Button
            {
                Text = "Поставить диагноз",
                BackgroundColor = Color.FromArgb("#4CAF50"),
                TextColor = Colors.White,
                CornerRadius = 6,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                Padding = new Thickness(12, 6)
            };

            if (appointment["user_id"] != null && appointment["user_id"] != DBNull.Value)
            {
                int patientId = Convert.ToInt32(appointment["user_id"]);
                diagnosisButton.Clicked += async (s, e) => await DiagnosisButton_Click(patientId);
            }
            else
            {
                diagnosisButton.Text = "Ошибка данных";
                diagnosisButton.IsEnabled = false;
            }

            rightPanel.Children.Add(diagnosisButton);
            Grid.SetColumn(rightPanel, 1);

            mainGrid.Children.Add(leftPanel);
            mainGrid.Children.Add(rightPanel);
            border.Content = mainGrid;

            return border;
        }

        private async Task DiagnosisButton_Click(int patientId)
        {
            try
            {
                DateTime visitDate = DateTime.Now;

                string diagnosis = await DisplayPromptAsync("Диагноз", "Введите диагноз:", maxLength: 500);
                if (string.IsNullOrWhiteSpace(diagnosis))
                {
                    await DisplayAlert("Ошибка", "Диагноз обязателен для заполнения", "OK");
                    return;
                }

                string treatment = await DisplayPromptAsync("Лечение", "Введите назначенное лечение:", maxLength: 500);

                int recordId = await _databaseService.CreateMedicalRecordAsync(
                    patientId, _currentDoctorId, visitDate, diagnosis, treatment ?? "");

                if (recordId > 0)
                {
                    await DisplayAlert("Успех", "Диагноз успешно сохранен в медицинскую карту", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Ошибка при сохранении диагноза", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
        }

        private void ShowLoadingIndicator(bool show)
        {
            LoadingIndicator.IsVisible = show;
            LoadingIndicator.IsRunning = show;
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
            await Navigation.PushAsync(new ProfilePage());
        }
    }
}
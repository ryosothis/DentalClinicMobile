using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace DentalClinicMobile
{
    public partial class AppointmentPage : ContentPage
    {
        private DatabaseService _databaseService;
        private int _selectedServiceId = -1;
        private int _selectedDoctorId = -1;
        private decimal _selectedServicePrice = 0;
        private string _selectedServiceName = "";
        private string _selectedDoctorName = "";

        public AppointmentPage()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            AppointmentDatePicker.MinimumDate = DateTime.Today;

            Loaded += async (s, e) => await LoadDataAsync();
        }

        public AppointmentPage(int serviceId) : this()
        {
            _selectedServiceId = serviceId;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                ShowLoadingIndicator(true);

                var servicesTask = _databaseService.GetServicesAsync();
                var doctorsTask = _databaseService.GetDoctorsAsync();

                await Task.WhenAll(servicesTask, doctorsTask);

                LoadServices(servicesTask.Result);
                LoadDoctors(doctorsTask.Result);

                if (_selectedServiceId > 0)
                {
                    SelectServiceById(_selectedServiceId);
                }
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

        private void LoadServices(DataTable servicesData)
        {
            ServicesStackLayout.Children.Clear();

            if (servicesData.Rows.Count > 0)
            {
                foreach (DataRow service in servicesData.Rows)
                {
                    var serviceCard = CreateServiceCard(service);
                    ServicesStackLayout.Children.Add(serviceCard);
                }
            }
            else
            {
                ServicesStackLayout.Children.Add(new Label
                {
                    Text = "Услуги временно недоступны",
                    TextColor = Color.FromArgb("#828CA0"),
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center
                });
            }
        }

        private void LoadDoctors(DataTable doctorsData)
        {
            DoctorsStackLayout.Children.Clear();

            if (doctorsData.Rows.Count > 0)
            {
                foreach (DataRow doctor in doctorsData.Rows)
                {
                    var doctorCard = CreateDoctorCard(doctor);
                    DoctorsStackLayout.Children.Add(doctorCard);
                }
            }
            else
            {
                DoctorsStackLayout.Children.Add(new Label
                {
                    Text = "Врачи временно недоступны",
                    TextColor = Color.FromArgb("#828CA0"),
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center
                });
            }
        }

        private Border CreateServiceCard(DataRow service)
        {
            int serviceId = Convert.ToInt32(service["id"]);
            string serviceName = service["name"].ToString();
            string description = service["description"]?.ToString() ?? "Подробное описание услуги";
            decimal price = Convert.ToDecimal(service["price"]);

            var border = new Border
            {
                Style = (Style)Resources["SelectionCardStyle"]
            };

            var stack = new VerticalStackLayout();

            var nameLabel = new Label
            {
                Text = serviceName,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Color.FromArgb("#244484")
            };

            var priceLabel = new Label
            {
                Text = $"{price:N0} ₽",
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                TextColor = Color.FromArgb("#467EEA"),
                HorizontalOptions = LayoutOptions.End
            };

            var namePriceGrid = new Grid();
            namePriceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            namePriceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            namePriceGrid.Add(nameLabel, 0, 0);
            namePriceGrid.Add(priceLabel, 1, 0);

            var descriptionLabel = new Label
            {
                Text = GetShortDescription(description),
                TextColor = Color.FromArgb("#828CA0"),
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0)
            };

            stack.Children.Add(namePriceGrid);
            stack.Children.Add(descriptionLabel);
            border.Content = stack;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => SelectService(serviceId, serviceName, price, border);
            border.GestureRecognizers.Add(tapGesture);

            return border;
        }

        private Border CreateDoctorCard(DataRow doctor)
        {
            int doctorId = Convert.ToInt32(doctor["id"]);
            string firstName = doctor["first_name"].ToString();
            string lastName = doctor["last_name"].ToString();
            string middleName = doctor["middle_name"]?.ToString() ?? "";
            string specialization = doctor["specialization"]?.ToString() ?? "Стоматолог";
            int experienceYears = Convert.ToInt32(doctor["experience_years"]);

            string fullName = $"{lastName} {firstName} {middleName}".Trim();
            string initials = $"{firstName[0]}{lastName[0]}".ToUpper();

            var border = new Border
            {
                Style = (Style)Resources["SelectionCardStyle"]
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var avatarBorder = new Border
            {
                WidthRequest = 50,
                HeightRequest = 50,
                BackgroundColor = Color.FromArgb("#E1E8FF"),
                StrokeShape = new RoundRectangle { CornerRadius = 25 },
                Stroke = Color.FromArgb("#C9DEFF"),
                StrokeThickness = 2,
                Margin = new Thickness(0, 0, 12, 0)
            };

            var initialsLabel = new Label
            {
                Text = initials,
                TextColor = Color.FromArgb("#244484"),
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            avatarBorder.Content = initialsLabel;

            var infoStack = new VerticalStackLayout();

            var nameLabel = new Label
            {
                Text = fullName,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Color.FromArgb("#244484"),
                Margin = new Thickness(0, 0, 0, 2)
            };

            var specializationLabel = new Label
            {
                Text = specialization,
                TextColor = Color.FromArgb("#828CA0"),
                FontSize = 12
            };

            var experienceLabel = new Label
            {
                Text = $"Опыт работы: {experienceYears} лет",
                TextColor = Color.FromArgb("#467EEA"),
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 4, 0, 0)
            };

            infoStack.Children.Add(nameLabel);
            infoStack.Children.Add(specializationLabel);
            infoStack.Children.Add(experienceLabel);

            grid.Add(avatarBorder, 0, 0);
            grid.Add(infoStack, 1, 0);
            border.Content = grid;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => SelectDoctor(doctorId, fullName, border);
            border.GestureRecognizers.Add(tapGesture);

            return border;
        }

        private void SelectService(int serviceId, string serviceName, decimal price, Border selectedBorder)
        {
            foreach (var child in ServicesStackLayout.Children)
            {
                if (child is Border border)
                {
                    border.Style = (Style)Resources["SelectionCardStyle"];
                    border.BackgroundColor = Colors.White;
                    border.Stroke = Color.FromArgb("#E1E8FF");
                    border.StrokeThickness = 1;
                }
            }

            selectedBorder.BackgroundColor = Color.FromArgb("#F0F5FF");
            selectedBorder.Stroke = Color.FromArgb("#467EEA");
            selectedBorder.StrokeThickness = 2;

            _selectedServiceId = serviceId;
            _selectedServiceName = serviceName;
            _selectedServicePrice = price;

            UpdateAppointmentInfo();
        }

        private void SelectServiceById(int serviceId)
        {
            foreach (var child in ServicesStackLayout.Children)
            {
                if (child is Border border)
                {
                    var content = border.Content as VerticalStackLayout;
                    if (content != null && content.Children.Count > 0)
                    {
                        var namePriceGrid = content.Children[0] as Grid;
                        if (namePriceGrid != null && namePriceGrid.Children.Count > 0)
                        {
                            var nameLabel = namePriceGrid.Children[0] as Label;
                            var priceLabel = namePriceGrid.Children[1] as Label;

                            if (nameLabel != null && priceLabel != null)
                            {
                                string serviceName = nameLabel.Text;
                                string priceStr = priceLabel.Text.Replace(" ₽", "").Replace(" ", "");
                                if (decimal.TryParse(priceStr, out decimal price))
                                {
                                    SelectService(serviceId, serviceName, price, border);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SelectDoctor(int doctorId, string doctorName, Border selectedBorder)
        {
            foreach (var child in DoctorsStackLayout.Children)
            {
                if (child is Border border)
                {
                    border.BackgroundColor = Colors.White;
                    border.Stroke = Color.FromArgb("#E1E8FF");
                    border.StrokeThickness = 1;
                }
            }

            selectedBorder.BackgroundColor = Color.FromArgb("#F0F5FF");
            selectedBorder.Stroke = Color.FromArgb("#467EEA");
            selectedBorder.StrokeThickness = 2;

            _selectedDoctorId = doctorId;
            _selectedDoctorName = doctorName;

            UpdateAppointmentInfo();
        }

        private void UpdateAppointmentInfo()
        {
            SelectedServiceText.Text = _selectedServiceId > 0 ? _selectedServiceName : "Не выбрана";
            SelectedDoctorText.Text = _selectedDoctorId > 0 ? _selectedDoctorName : "Не выбран";
            SelectedPriceText.Text = _selectedServiceId > 0 ? $"{_selectedServicePrice:N0} ₽" : "0 ₽";
        }

        private string GetShortDescription(string fullDescription)
        {
            if (string.IsNullOrEmpty(fullDescription) || fullDescription == "Подробное описание услуги")
                return "Профессиональная стоматологическая услуга";

            if (fullDescription.Length > 60)
                return fullDescription.Substring(0, 60) + "...";

            return fullDescription;
        }

        private void ShowLoadingIndicator(bool show)
        {
            LoadingIndicator.IsVisible = show;
            LoadingIndicator.IsRunning = show;
            MainContent.IsVisible = !show;
        }

        private async void OnConfirmAppointmentClicked(object sender, EventArgs e)
        {
            try
            {
                if (_selectedServiceId <= 0)
                {
                    await DisplayAlert("Внимание", "Пожалуйста, выберите услугу", "OK");
                    return;
                }

                if (_selectedDoctorId <= 0)
                {
                    await DisplayAlert("Внимание", "Пожалуйста, выберите врача", "OK");
                    return;
                }

                if (TimePicker.SelectedItem == null)
                {
                    await DisplayAlert("Внимание", "Пожалуйста, выберите время приема", "OK");
                    return;
                }

                DateTime selectedDate = AppointmentDatePicker.Date ?? DateTime.Today;
                string selectedTime = TimePicker.SelectedItem.ToString();

                DateTime appointmentDateTime = selectedDate.Add(TimeSpan.Parse(selectedTime));

                if (appointmentDateTime < DateTime.Now)
                {
                    await DisplayAlert("Ошибка", "Нельзя записаться на прошедшую дату", "OK");
                    return;
                }

                bool isTimeAvailable = await _databaseService.IsAppointmentTimeAvailableAsync(
                    _selectedDoctorId, appointmentDateTime);

                if (!isTimeAvailable)
                {
                    await DisplayAlert("Время занято",
                        $"Врач {_selectedDoctorName} уже занят на выбранное время ({appointmentDateTime:HH:mm}).\nПожалуйста, выберите другое время.",
                        "OK");
                    return;
                }

                int? userId = AuthManager.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    await DisplayAlert("Ошибка", "Ошибка авторизации. Пожалуйста, войдите снова.", "OK");
                    await Shell.Current.GoToAsync("LoginPage");
                    return;
                }

                bool success = await _databaseService.CreateAppointmentAsync(
                    userId.Value, _selectedDoctorId, _selectedServiceId, appointmentDateTime);

                if (success)
                {
                    await DisplayAlert("Успех",
                        $"Запись на прием успешно создана!\n\n" +
                        $"Услуга: {_selectedServiceName}\n" +
                        $"Врач: {_selectedDoctorName}\n" +
                        $"Дата: {appointmentDateTime:dd.MM.yyyy}\n" +
                        $"Время: {appointmentDateTime:HH:mm}",
                        "OK");

                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Ошибка при создании записи. Пожалуйста, попробуйте позже.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при создании записи: {ex.Message}", "OK");
            }
        }
    }
}

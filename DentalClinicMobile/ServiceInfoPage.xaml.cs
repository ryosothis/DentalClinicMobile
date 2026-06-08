using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace DentalClinicMobile
{
    public partial class ServiceInfoPage : ContentPage
    {
        private DatabaseService _databaseService;
        private int _serviceId;

        public ServiceInfoPage(int serviceId)
        {
            InitializeComponent();

            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
            _serviceId = serviceId;

            Loaded += async (s, e) => await LoadServiceInfoAsync();
        }

        private async Task LoadServiceInfoAsync()
        {
            try
            {
                ShowLoadingIndicator(true);

                DataTable serviceData = await _databaseService.GetServiceByIdAsync(_serviceId);

                if (serviceData.Rows.Count > 0)
                {
                    DataRow service = serviceData.Rows[0];

                    string serviceName = service["name"].ToString();
                    string description = service["description"]?.ToString() ?? "Описание временно недоступно";
                    decimal price = Convert.ToDecimal(service["price"]);

                    ServiceNameText.Text = serviceName;
                    ServiceDescriptionText.Text = description;
                    ServicePriceText.Text = $"{price:N0} руб.";

                    AddAdvantages(serviceName);
                }
                else
                {
                    await DisplayAlert("Ошибка", "Услуга не найдена", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки информации об услуге: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private void AddAdvantages(string serviceName)
        {
            AdvantagesStackLayout.Children.Clear();

            var advantages = GetAdvantagesForService(serviceName);

            foreach (string advantage in advantages)
            {
                var advantagePanel = new HorizontalStackLayout
                {
                    Spacing = 12,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var iconBorder = new Border
                {
                    WidthRequest = 24,
                    HeightRequest = 24,
                    BackgroundColor = Color.FromArgb("#E1E8FF"),
                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center
                };

                var iconLabel = new Label
                {
                    Text = "✓",
                    TextColor = Color.FromArgb("#467EEA"),
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                iconBorder.Content = iconLabel;

                var advantageLabel = new Label
                {
                    Text = advantage,
                    TextColor = Color.FromArgb("#244484"),
                    FontSize = 14,
                    VerticalOptions = LayoutOptions.Center
                };

                advantagePanel.Children.Add(iconBorder);
                advantagePanel.Children.Add(advantageLabel);

                AdvantagesStackLayout.Children.Add(advantagePanel);
            }
        }

        private string[] GetAdvantagesForService(string serviceName)
        {
            string lowerName = serviceName.ToLower();

            if (lowerName.Contains("консультация"))
            {
                return new string[]
                {
                    "Профессиональная диагностика",
                    "Индивидуальный план лечения",
                    "Ответы на все вопросы",
                    "Рекомендации по уходу"
                };
            }
            else if (lowerName.Contains("лечение") || lowerName.Contains("кариес") || lowerName.Contains("пульпит"))
            {
                return new string[]
                {
                    "Безболезненное лечение",
                    "Современные материалы",
                    "Пожизненная гарантия на работу",
                    "Сохраняем здоровые ткани"
                };
            }
            else if (lowerName.Contains("гигиена") || lowerName.Contains("чистка"))
            {
                return new string[]
                {
                    "Удаление зубного камня",
                    "Отбеливание на 1-2 тона",
                    "Фторирование эмали",
                    "Профилактика кариеса"
                };
            }
            else if (lowerName.Contains("удаление"))
            {
                return new string[]
                {
                    "Безболезненная процедура",
                    "Быстрое восстановление",
                    "Минимальная травматичность",
                    "Профессиональный уход после операции"
                };
            }
            else
            {
                return new string[]
                {
                    "Высокое качество материалов",
                    "Опытные специалисты",
                    "Современное оборудование",
                    "Индивидуальный подход"
                };
            }
        }

        private void ShowLoadingIndicator(bool show)
        {
            LoadingIndicator.IsVisible = show;
            LoadingIndicator.IsRunning = show;
            MainContent.IsVisible = !show;
        }

        private async void OnBookAppointmentClicked(object sender, EventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
            {
                await Shell.Current.GoToAsync($"AppointmentPage?serviceId={_serviceId}");
            }
            else
            {
                await DisplayAlert("Внимание", "Пожалуйста, войдите в систему для записи на прием", "OK");
                await Shell.Current.GoToAsync("LoginPage");
            }
        }
    }
}

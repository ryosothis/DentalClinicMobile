using System;
using System.Data;
using Microsoft.Maui.Controls.Shapes;

namespace DentalClinicMobile
{
    public partial class ServicesPage : ContentPage
    {
        private DatabaseService _databaseService;

        public ServicesPage()
        {
            InitializeComponent();

            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            Loaded += async (s, e) => await LoadServicesAsync();
        }

        private async Task LoadServicesAsync()
        {
            try
            {
                ShowLoadingIndicator(true);

                DataTable servicesData = await _databaseService.GetServicesAsync();

                ServicesFlexLayout.Children.Clear();

                if (servicesData.Rows.Count > 0)
                {
                    foreach (DataRow service in servicesData.Rows)
                    {
                        var serviceCard = CreateServiceCard(service);
                        ServicesFlexLayout.Children.Add(serviceCard);
                    }
                }
                else
                {
                    ShowNoServicesMessage();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки услуг: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private Border CreateServiceCard(DataRow service)
        {
            var border = new Border
            {
                Style = (Style)Resources["ServiceCardStyle"]
            };

            int serviceId = Convert.ToInt32(service["id"]);
            string serviceName = service["name"].ToString();
            string description = service["description"]?.ToString() ?? "Подробное описание услуги";
            decimal price = Convert.ToDecimal(service["price"]);
            string icon = GetServiceIcon(serviceName);

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var topPanel = new HorizontalStackLayout
            {
                Spacing = 10,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var iconLabel = new Label
            {
                Text = icon,
                FontSize = 20,
                VerticalOptions = LayoutOptions.Center
            };

            var nameLabel = new Label
            {
                Text = serviceName,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                TextColor = Color.FromArgb("#244484"),
                VerticalOptions = LayoutOptions.Center
            };

            topPanel.Children.Add(iconLabel);
            topPanel.Children.Add(nameLabel);
            Grid.SetRow(topPanel, 0);

            var descriptionLabel = new Label
            {
                Text = GetShortDescription(description, serviceName),
                FontSize = 13,
                TextColor = Color.FromArgb("#828CA0")
            };
            Grid.SetRow(descriptionLabel, 1);

            var bottomBorder = new Border
            {
                BackgroundColor = Color.FromArgb("#E1E8FF"),
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(12, 6, 12, 6),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.End
            };

            var priceLabel = new Label
            {
                Text = $"{price:N0} руб.",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#244484")
            };

            bottomBorder.Content = priceLabel;
            Grid.SetRow(bottomBorder, 2);

            mainGrid.Children.Add(topPanel);
            mainGrid.Children.Add(descriptionLabel);
            mainGrid.Children.Add(bottomBorder);

            border.Content = mainGrid;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => ServiceCard_Click(serviceId, serviceName);
            border.GestureRecognizers.Add(tapGesture);

            return border;
        }

        private async void ServiceCard_Click(int serviceId, string serviceName)
        {
            await Navigation.PushAsync(new AppointmentPage(serviceId));
        }

        private string GetServiceIcon(string serviceName)
        {
            string lowerName = serviceName.ToLower();

            if (lowerName.Contains("консультация") || lowerName.Contains("диагностика") || lowerName.Contains("осмотр"))
                return "🔍";
            else if (lowerName.Contains("лечение") || lowerName.Contains("кариес") || lowerName.Contains("пульпит") || lowerName.Contains("терапия"))
                return "💊";
            else if (lowerName.Contains("гигиена") || lowerName.Contains("чистка") || lowerName.Contains("профилактика"))
                return "✨";
            else if (lowerName.Contains("отбеливание") || lowerName.Contains("эстетик"))
                return "⭐";
            else if (lowerName.Contains("протезирование") || lowerName.Contains("коронк") || lowerName.Contains("мост"))
                return "🦷";
            else if (lowerName.Contains("имплантация") || lowerName.Contains("имплант"))
                return "⚡";
            else if (lowerName.Contains("прикус") || lowerName.Contains("брекет") || lowerName.Contains("ортодонт") || lowerName.Contains("элайнер"))
                return "🦴";
            else if (lowerName.Contains("удаление") || lowerName.Contains("хирург"))
                return "❌";
            else if (lowerName.Contains("детск") || lowerName.Contains("ребенок"))
                return "👶";
            else
                return "🎯";
        }

        private string GetShortDescription(string fullDescription, string serviceName)
        {
            if (!string.IsNullOrEmpty(fullDescription) && fullDescription != "Подробное описание услуги")
            {
                if (fullDescription.Length > 60)
                    return fullDescription.Substring(0, 60) + "...";
                else
                    return fullDescription;
            }

            string lowerName = serviceName.ToLower();

            if (lowerName.Contains("консультация"))
                return "Профессиональный осмотр и консультация стоматолога с составлением плана лечения";
            else if (lowerName.Contains("диагностика"))
                return "Точная диагностика с использованием современного оборудования и составление плана лечения";
            else if (lowerName.Contains("лечение кариеса"))
                return "Безболезненное лечение кариеса с использованием современных пломбировочных материалов";
            else if (lowerName.Contains("пульпит"))
                return "Лечение корневых каналов с использованием микроскопа и современных методик";
            else if (lowerName.Contains("гигиена"))
                return "Профессиональная чистка зубов с удалением налета и зубного камня";
            else if (lowerName.Contains("отбеливание"))
                return "Безопасное отбеливание эмали с гарантированным результатом и минимальной чувствительностью";
            else if (lowerName.Contains("протезирование"))
                return "Восстановление утраченных зубов с использованием современных материалов и технологий";
            else if (lowerName.Contains("имплантация"))
                return "Современные методы имплантации с пожизненной гарантией и быстрым восстановлением";
            else if (lowerName.Contains("прикус") || lowerName.Contains("ортодонт"))
                return "Исправление положения зубов и прикуса с использованием брекет-систем и элайнеров";
            else if (lowerName.Contains("удаление"))
                return "Безболезненное удаление зубов любой сложности с современной анестезией";
            else
                return "Профессиональная стоматологическая услуга с использованием современных технологий и материалов";
        }

        private void ShowNoServicesMessage()
        {
            var noServicesLabel = new Label
            {
                Text = "Услуги временно недоступны",
                TextColor = Color.FromArgb("#828CA0"),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            ServicesFlexLayout.Children.Add(noServicesLabel);
        }

        private void ShowLoadingIndicator(bool show)
        {
            LoadingIndicator.IsVisible = show;
            LoadingIndicator.IsRunning = show;
            MainContent.IsVisible = !show;
        }

        private async void OnHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }

        private async void OnAboutTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AboutPage());
        }

        private void OnServicesTapped(object sender, EventArgs e)
        {
        }

        private async void OnPriceTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PricePage());
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
            {
                await Navigation.PushAsync(new ProfilePage());
            }
            else
            {
                await DisplayAlert("Внимание", "Пожалуйста, войдите в систему для просмотра профиля", "OK");
            }
        }

        private async void OnAppointmentButtonClicked(object sender, EventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
            {
                await Navigation.PushAsync(new AppointmentPage());
            }
            else
            {
                await DisplayAlert("Внимание", "Пожалуйста, войдите в систему для записи на прием", "OK");
                await Navigation.PushAsync(new LoginPage());
            }
        }

        private async void OnVkTapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://vk.com/fearless");
        }

        private async void OnWhatsAppTapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://wa.me/89539096254");
        }

        private async void OnTelegramTapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://t.me/flexstylist");
        }
    }
}
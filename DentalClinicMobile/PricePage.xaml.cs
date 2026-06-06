using System;
using System.Data;

namespace DentalClinicMobile
{
    public partial class PricePage : ContentPage
    {
        private DatabaseService _databaseService;

        public PricePage()
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

                ServicesGrid.Children.Clear();
                ServicesGrid.RowDefinitions.Clear();

                ServicesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                int rowIndex = 1;

                if (servicesData.Rows.Count > 0)
                {
                    for (int i = 0; i < servicesData.Rows.Count; i++)
                    {
                        ServicesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        DataRow service = servicesData.Rows[i];
                        string serviceName = service["name"].ToString();
                        decimal price = Convert.ToDecimal(service["price"]);

                        var nameLabel = new Label
                        {
                            Text = serviceName,
                            FontSize = 15,
                            TextColor = Color.FromArgb("#244484"),
                            VerticalOptions = LayoutOptions.Center
                        };
                        Grid.SetRow(nameLabel, rowIndex);
                        Grid.SetColumn(nameLabel, 0);
                        ServicesGrid.Children.Add(nameLabel);

                        var priceLabel = new Label
                        {
                            Text = $"{price:N0} руб.",
                            FontSize = 15,
                            TextColor = Color.FromArgb("#467EEA"),
                            FontAttributes = FontAttributes.Bold,
                            VerticalOptions = LayoutOptions.Center,
                            Margin = new Thickness(20, 0, 0, 0)
                        };
                        Grid.SetRow(priceLabel, rowIndex);
                        Grid.SetColumn(priceLabel, 1);
                        ServicesGrid.Children.Add(priceLabel);

                        rowIndex++;
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

        private void ShowNoServicesMessage()
        {
            ServicesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var noServicesLabel = new Label
            {
                Text = "Информация об услугах временно недоступна",
                TextColor = Color.FromArgb("#828CA0"),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(noServicesLabel, 1);
            Grid.SetColumn(noServicesLabel, 0);
            Grid.SetColumnSpan(noServicesLabel, 2);
            ServicesGrid.Children.Add(noServicesLabel);
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

        private async void OnServicesTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ServicesPage());
        }

        private void OnPriceTapped(object sender, EventArgs e)
        {
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

        private async void OnBookAppointmentClicked(object sender, EventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
            {
                await Navigation.PushAsync(new ServicesPage());
            }
            else
            {
                await DisplayAlert("Внимание", "Пожалуйста, войдите в систему для записи на прием", "OK");
                await Navigation.PushAsync(new LoginPage());
            }
        }
    }
}
using System;
using System.Data;
using System.Diagnostics;

namespace DentalClinicMobile
{
    public partial class MainPage : ContentPage
    {
        private DatabaseService _databaseService;
        private CancellationTokenSource _searchCancellationTokenSource;

        public MainPage()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.Trim() ?? "";

            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(500);

                if (_searchCancellationTokenSource.Token.IsCancellationRequested)
                    return;

                if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
                {
                    HideSearchResults();
                    return;
                }

                DataTable servicesData = await _databaseService.GetServicesAsync();

                if (_searchCancellationTokenSource.Token.IsCancellationRequested)
                    return;

                var filteredServices = servicesData.AsEnumerable()
                    .Where(row => row.Field<string>("name").ToLower().Contains(searchText.ToLower()) ||
                                 (row.Field<string>("description")?.ToLower().Contains(searchText.ToLower()) ?? false))
                    .Take(10)
                    .Select(row => new SearchResultItem
                    {
                        Id = Convert.ToInt32(row["id"]),
                        Name = row["name"].ToString(),
                        Description = row.Field<string>("description") ?? "Описание отсутствует",
                        Price = Convert.ToDecimal(row["price"])
                    })
                    .ToList();

                if (_searchCancellationTokenSource.Token.IsCancellationRequested)
                    return;

                if (filteredServices.Any())
                {
                    ShowSearchResults(filteredServices);
                }
                else
                {
                    HideSearchResults();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                HideSearchResults();
                Debug.WriteLine($"Ошибка поиска: {ex.Message}");
            }
        }

        private void ShowSearchResults(System.Collections.Generic.List<SearchResultItem> services)
        {
            SearchResultsStack.Children.Clear();

            foreach (var service in services)
            {
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnSearchResultSelected(service);

                var frame = new Frame
                {
                    BackgroundColor = Colors.White,
                    BorderColor = Color.FromArgb("#E1E8FF"),
                    CornerRadius = 8,
                    Margin = new Thickness(0, 2),
                    Padding = new Thickness(15, 10),
                    GestureRecognizers = { tapGesture }
                };

                var stack = new VerticalStackLayout();
                stack.Add(new Label
                {
                    Text = service.Name,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#244484")
                });
                stack.Add(new Label
                {
                    Text = service.Description,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#828CA0")
                });
                stack.Add(new Label
                {
                    Text = $"Цена: {service.Price} руб.",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#4CAF50"),
                    FontAttributes = FontAttributes.Bold
                });

                frame.Content = stack;
                SearchResultsStack.Children.Add(frame);
            }

            SearchResultsBorder.IsVisible = true;
        }

        private void HideSearchResults()
        {
            SearchResultsBorder.IsVisible = false;
            SearchResultsStack.Children.Clear();
        }

        private async void OnSearchResultSelected(SearchResultItem service)
        {
            HideSearchResults();
            SearchEntry.Text = "";
            await Navigation.PushAsync(new ServiceInfoPage(service.Id));
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

        private async void OnMakeAppointmentClicked(object sender, EventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
                await Navigation.PushAsync(new AppointmentPage());
            else
                await Navigation.PushAsync(new LoginPage());
        }

        private async void OnAddressClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Наш адрес", "г. Москва, ул. Стоматологическая, д. 123\nТелефон: +7 (495) 123-45-67", "OK");
        }

        private async void OnMapsClicked(object sender, EventArgs e)
        {
            try
            {
                string address = "г. Москва, ул. Стоматологическая, д. 123";
                string url = $"https://yandex.ru/maps/?text={Uri.EscapeDataString(address)}";
                await Launcher.OpenAsync(url);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть Яндекс Карты: {ex.Message}", "OK");
            }
        }
    }
}
using Microsoft.Maui.Controls;

namespace DentalClinicMobile
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }

        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoginPage());
        }
    }
}
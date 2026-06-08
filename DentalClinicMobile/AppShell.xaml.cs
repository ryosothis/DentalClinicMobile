namespace DentalClinicMobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("AppointmentPage", typeof(AppointmentPage));
        Routing.RegisterRoute("ServiceInfoPage", typeof(ServiceInfoPage));
        Routing.RegisterRoute("DoctorAppointmentsPage", typeof(DoctorAppointmentsPage));
    }
}

namespace DentalClinicMobile;

public class AppointmentViewModel
{
    public int Id { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public string AppointmentDate { get; set; } = string.Empty;
}
namespace DentalClinicMobile;

public class DoctorViewModel
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string ExperienceText { get; set; } = string.Empty;

    public string Education { get; set; } = string.Empty;
}
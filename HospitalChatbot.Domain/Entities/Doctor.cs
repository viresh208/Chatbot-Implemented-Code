namespace HospitalChatbot.Domain.Entities;

public class Doctor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public string Qualifications { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public List<string> Symptoms { get; set; } = new();
    public bool IsAvailable { get; set; }
}

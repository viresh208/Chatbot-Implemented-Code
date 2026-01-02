using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Application.Interfaces;
using HospitalChatbot.Domain.Entities;
using System.Text;
using System.Text.Json;
using RestSharp;

namespace HospitalChatbot.Infrastructure.Services;

public class DoctorService : IDoctorService
{
    private readonly HttpClient _httpClient;
    private const string DoctorSearchApiUrl = "https://cb45c777-9cd1-4079-b906-48fd30e9653c.mock.pstmn.io/doctorsearch";
    private const string SymptomsApiUrl = "https://cb45c777-9cd1-4079-b906-48fd30e9653c.mock.pstmn.io/symptoms";
    private const string DepartmentsApiUrl = "https://cb45c777-9cd1-4079-b906-48fd30e9653c.mock.pstmn.io/departments";

    // Temporary cache for current session only
    private static readonly List<Doctor> _doctors = new();

    public DoctorService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<List<Doctor>> GetDoctorsByClinicAsync(Guid clinicId)
    {
        return Task.FromResult(_doctors.Where(d => d.ClinicId == clinicId && d.IsAvailable).ToList());
    }

    public async Task<List<Doctor>> GetDoctorsByClinicNameAsync(string clinicName)
    {
        try
        {
            // Prepare RestSharp client and request with EXACT Postman format
            var client = new RestClient();

            // Create JSON with exact spacing to match Postman examples
            var restBody = $"{{\r\n\"Name\" : \"{clinicName}\"\r\n}}";

            Console.WriteLine($"=== DOCTOR SEARCH REQUEST ===");
            Console.WriteLine($"Request Body: {restBody}");

            var request = new RestRequest(DoctorSearchApiUrl, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody(restBody, DataFormat.Json);

            var response = await client.ExecuteAsync(request);
            Console.WriteLine($"Doctor Search Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                Console.WriteLine($"Doctor Search Response: {responseContent}");

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doctorResponses = JsonSerializer.Deserialize<List<DoctorSearchResponse>>(responseContent, jsonOptions);

                if (doctorResponses != null && doctorResponses.Any())
                {
                    var doctors = new List<Doctor>();
                    foreach (var dr in doctorResponses)
                    {
                        var doctor = _doctors.FirstOrDefault(d => d.Name.Contains(dr.DoctorName) ||
                                                                  dr.DoctorName.Contains(d.Name));

                        if (doctor == null)
                        {
                            doctor = new Doctor
                            {
                                Id = Guid.NewGuid(),
                                Name = dr.DoctorName,
                                Specialization = "General",
                                Qualifications = "MBBS",
                                //YearsOfExperience = 5,
                                Symptoms = new List<string>(),
                                IsAvailable = true
                            };
                            _doctors.Add(doctor);
                        }

                        doctors.Add(doctor);
                    }

                    return doctors;
                }
            }

            return new List<Doctor>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Doctor Search Exception: {ex.Message}");
            return new List<Doctor>();
        }
    }

    public async Task<List<Doctor>> GetDoctorsBySymptomsAsync(string symptoms)
    {
        try
        {
            // Prepare RestSharp client and request with EXACT Postman format
            var client = new RestClient();

            // Create JSON with exact spacing to match Postman examples
            var restBody = $"{{\r\n\"Symptoms\" : \"{symptoms}\"\r\n}}";

            Console.WriteLine($"=== SYMPTOMS REQUEST ===");
            Console.WriteLine($"Request Body: {restBody}");

            var request = new RestRequest(SymptomsApiUrl, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody(restBody, DataFormat.Json);

            var response = await client.ExecuteAsync(request);
            Console.WriteLine($"Symptoms Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                Console.WriteLine($"Symptoms Response: {responseContent}");

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var symptomsResponse = JsonSerializer.Deserialize<SymptomsResponse>(responseContent, jsonOptions);

                if (symptomsResponse != null)
                {
                    // Now call departments API to get doctors for this department
                    return await GetDoctorsByDepartmentAsync(symptomsResponse.DepartmentName);
                }
            }

            return new List<Doctor>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Symptoms Search Exception: {ex.Message}");
            return new List<Doctor>();
        }
    }

    private async Task<List<Doctor>> GetDoctorsByDepartmentAsync(string departmentName)
    {
        try
        {
            // Prepare RestSharp client and request with EXACT Postman format
            var client = new RestClient();

            // Create JSON with exact spacing to match Postman examples
            var restBody = $"{{\r\n\"DepartmentName\" : \"{departmentName}\"\r\n}}";

            Console.WriteLine($"=== DEPARTMENT REQUEST ===");
            Console.WriteLine($"Request Body: {restBody}");

            var request = new RestRequest(DepartmentsApiUrl, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody(restBody, DataFormat.Json);

            var response = await client.ExecuteAsync(request);
            Console.WriteLine($"Department Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                Console.WriteLine($"Department Response: {responseContent}");

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doctorResponses = JsonSerializer.Deserialize<List<DepartmentDoctorResponse>>(responseContent, jsonOptions);

                if (doctorResponses != null && doctorResponses.Any())
                {
                    var doctors = new List<Doctor>();
                    foreach (var dr in doctorResponses)
                    {
                        var doctor = _doctors.FirstOrDefault(d => d.Name.Contains(dr.DoctorName) ||
                                                                  dr.DoctorName.Contains(d.Name));

                        if (doctor == null)
                        {
                            doctor = new Doctor
                            {
                                Id = Guid.NewGuid(),
                                Name = dr.DoctorName,
                                Specialization = departmentName,
                                Qualifications = "MBBS",
                                YearsOfExperience = 5,
                                Symptoms = new List<string>(),
                                IsAvailable = true
                            };
                            _doctors.Add(doctor);
                        }

                        doctors.Add(doctor);
                    }

                    return doctors;
                }
            }

            return new List<Doctor>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Department Search Exception: {ex.Message}");
            return new List<Doctor>();
        }
    }

    public Task<Doctor?> GetDoctorByIdAsync(Guid id)
    {
        var doctor = _doctors.FirstOrDefault(d => d.Id == id);
        return Task.FromResult(doctor);
    }
}

using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Application.Interfaces;
using HospitalChatbot.Domain.Entities;
using System.Text;
using System.Text.Json;
using RestSharp;

namespace HospitalChatbot.Infrastructure.Services;

public class ClinicService : IClinicService
{
    private readonly HttpClient _httpClient;
    private const string ClinicSelectionApiUrl = "https://cb45c777-9cd1-4079-b906-48fd30e9653c.mock.pstmn.io/clinicselection";

    // Temporary cache for current session only
    private static readonly List<Clinic> _clinics = new();

    public ClinicService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<List<Clinic>> GetAllClinicsAsync()
    {
        // For now, return static list as the API doesn't provide all clinics
        return Task.FromResult(_clinics.Where(c => c.IsActive).ToList());
    }

    public Task<Clinic?> GetClinicByIdAsync(Guid id)
    {
        var clinic = _clinics.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(clinic);
    }

    public async Task<Clinic?> GetClinicByPatientNameAsync(string patientName)
    {
        try
        {
            // Prepare RestSharp client and request with EXACT Postman format
            var client = new RestClient();

            // Create JSON with exact spacing to match Postman examples
            var restBody = $"{{\r\n\"Name\" : \"{patientName}\"\r\n}}";

            Console.WriteLine($"=== CLINIC SELECTION REQUEST ===");
            Console.WriteLine($"Request Body: {restBody}");

            var request = new RestRequest(ClinicSelectionApiUrl, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody(restBody, DataFormat.Json);

            // Call API
            Console.WriteLine($"Calling Clinic Selection API: {ClinicSelectionApiUrl}");
            var response = await client.ExecuteAsync(request);
            Console.WriteLine($"Clinic Selection Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                Console.WriteLine($"Clinic Selection Response: {responseContent}");

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var clinicResponse =
                    JsonSerializer.Deserialize<ClinicSelectionResponse>(responseContent, jsonOptions);

                if (clinicResponse != null)
                {
                    // Find existing clinic
                    var clinic = _clinics.FirstOrDefault(c =>
                        c.Name.Contains(clinicResponse.ClinicName) ||
                        clinicResponse.ClinicName.Contains(c.Name));

                    if (clinic == null)
                    {
                        // Create new clinic
                        clinic = new Clinic
                        {
                            Id = Guid.NewGuid(),
                            Name = clinicResponse.ClinicName,
                            Department = "General",
                            Description = $"Clinic ID: {clinicResponse.ClinicId}",
                            IsActive = true
                        };

                        _clinics.Add(clinic);
                        Console.WriteLine($"New clinic created: {clinic.Name}");
                    }

                    return clinic;
                }
            }
            else
            {
                Console.WriteLine($"Clinic Selection API Error: {response.Content}");
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Clinic Selection Exception: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

}

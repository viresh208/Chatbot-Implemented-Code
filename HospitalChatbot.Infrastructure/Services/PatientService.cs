using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Application.Interfaces;
using HospitalChatbot.Domain.Entities;
using HospitalChatbot.Infrastructure.Data;
using HospitalChatbot.Infrastructure.Models;
using System.Text.Json;
using System.Text;
using RestSharp;
using MongoDB.Driver;


namespace HospitalChatbot.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly HttpClient _httpClient;
    private readonly MongoDbContext _mongoDbContext;
    private const string MockApiUrl = "https://cb45c777-9cd1-4079-b906-48fd30e9653c.mock.pstmn.io/login";

    // In-memory cache for quick lookups during current session
    private static readonly Dictionary<string, Guid> _patientCache = new();

    public PatientService(HttpClient httpClient, MongoDbContext mongoDbContext)
    {
        _httpClient = httpClient;
        _mongoDbContext = mongoDbContext;
    }

    public async Task<AuthenticationResponse> AuthenticatePatientAsync(AuthenticationRequest authRequest)
    {
        try
        {
            // Prepare RestSharp client and request
            var client = new RestClient();
            var restPayload = new
            {
                DateofBirth = authRequest.DateofBirth
            };
            var restBody = JsonSerializer.Serialize(
                restPayload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // Ensures property names are not camelCased
                }
            );

            var request = new RestRequest(MockApiUrl, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody(restBody, DataFormat.Json);

            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine(response.Content);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                Console.WriteLine($"=== PATIENT LOGIN RESPONSE ===");
                Console.WriteLine($"Response Content: {responseContent}");

                var loginResponse = JsonSerializer.Deserialize<PatientLoginResponse>(responseContent, jsonOptions);

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Name))
                {
                    Console.WriteLine($"Deserialized Patient: {loginResponse.Name}");

                    // Parse the DOB from API response
                    var apiDob = DateTime.ParseExact(loginResponse.DateofBirth, "dd/MM/yyyy", null);

                    Guid patientId;

                    // Check in-memory cache first
                    if (_patientCache.TryGetValue(loginResponse.Name, out var cachedId))
                    {
                        patientId = cachedId;
                        Console.WriteLine($"Patient found in cache: {loginResponse.Name} with ID: {patientId}");
                    }
                    else
                    {
                        // Query ConversationLogs to find existing patient ID by name
                        var filter = Builders<ConversationLog>.Filter.Eq(c => c.PatientName, loginResponse.Name);
                        var existingLog = await _mongoDbContext.ConversationLogs
                            .Find(filter)
                            .SortByDescending(c => c.Timestamp)
                            .FirstOrDefaultAsync();

                        if (existingLog != null && existingLog.PatientId.HasValue && existingLog.PatientId.Value != Guid.Empty)
                        {
                            // Found existing patient ID in ConversationLogs
                            patientId = existingLog.PatientId.Value;
                            _patientCache[loginResponse.Name] = patientId;
                            Console.WriteLine($"Existing patient found in ConversationLogs: {loginResponse.Name} with ID: {patientId}");
                        }
                        else
                        {
                            // Create new patient ID
                            patientId = Guid.NewGuid();
                            _patientCache[loginResponse.Name] = patientId;
                            Console.WriteLine($"New patient ID created: {loginResponse.Name} with ID: {patientId}");
                        }
                    }

                    return new AuthenticationResponse
                    {
                        Success = true,
                        PatientId = patientId,
                        PatientName = loginResponse.Name,
                        Message = "Authentication successful!"
                    };
                }
                else
                {
                    Console.WriteLine("Login response is null or has no name");
                }
            }
            else
            {
                Console.WriteLine($"API Error: {response.Content}");
            }

            return new AuthenticationResponse
            {
                Success = false,
                Message = "Patient not found. Please check your date of birth."
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return new AuthenticationResponse
            {
                Success = false,
                Message = $"Authentication failed: {ex.Message}"
            };
        }
    }

    public async Task<Patient?> GetPatientByIdAsync(Guid id)
    {
        // Query ConversationLogs to get patient details by ID
        var filter = Builders<ConversationLog>.Filter.Eq(c => c.PatientId, id);
        var log = await _mongoDbContext.ConversationLogs
            .Find(filter)
            .SortByDescending(c => c.Timestamp)
            .FirstOrDefaultAsync();

        if (log != null && log.PatientId.HasValue)
        {
            return new Patient
            {
                Id = log.PatientId.Value,
                Name = log.PatientName ?? string.Empty,
                DateOfBirth = DateTime.MinValue, // Not stored in logs
                PhoneNumber = string.Empty,
                Email = string.Empty,
                CreatedAt = log.Timestamp
            };
        }

        return null;
    }
}


//http client
//using HospitalChatbot.Application.DTOs;
//using HospitalChatbot.Application.Interfaces;
//using HospitalChatbot.Domain.Entities;
//using System.Text;
//using System.Text.Json;

//namespace HospitalChatbot.Infrastructure.Services;

//public class PatientService : IPatientService
//{
//    private readonly HttpClient _httpClient;
//    private const string MockApiUrl = "https://cb45c777-9cd1-4079-b906-48fd30e9653c.mock.pstmn.io/login";

//    // Temporary cache for current session only - cleared on app restart
//    private static readonly List<Patient> _patients = new();

//    public PatientService(HttpClient httpClient)
//    {
//        _httpClient = httpClient;
//    }

//    public async Task<AuthenticationResponse> AuthenticatePatientAsync(AuthenticationRequest authRequest)
//    {
//        try
//        {
//            // Prepare request payload
//            var payload = new
//            {
//                DateofBirth = authRequest.DateofBirth
//            };

//            var json = JsonSerializer.Serialize(
//                payload,
//                new JsonSerializerOptions
//                {
//                    PropertyNamingPolicy = null // Keep exact property names
//                });

//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            // Send POST request
//            HttpResponseMessage response = await _httpClient.PostAsync(MockApiUrl, content);

//            var responseContent = await response.Content.ReadAsStringAsync();

//            Console.WriteLine($"Response Status: {response.StatusCode}");
//            Console.WriteLine($"Response Content: {responseContent}");

//            if (!response.IsSuccessStatusCode)
//            {
//                return new AuthenticationResponse
//                {
//                    Success = false,
//                    Message = "Patient not found. Please check your date of birth."
//                };
//            }

//            var jsonOptions = new JsonSerializerOptions
//            {
//                PropertyNameCaseInsensitive = true
//            };

//            var loginResponse =
//                JsonSerializer.Deserialize<PatientLoginResponse>(responseContent, jsonOptions);

//            if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.Name))
//            {
//                return new AuthenticationResponse
//                {
//                    Success = false,
//                    Message = "Invalid response from authentication service."
//                };
//            }

//            // Parse DOB returned by API
//            var apiDob = DateTime.ParseExact(
//                loginResponse.DateofBirth,
//                "dd/MM/yyyy",
//                null);

//            // Check if patient already exists
//            var patient = _patients.FirstOrDefault(p =>
//                p.Name == loginResponse.Name &&
//                p.DateOfBirth.Date == apiDob.Date);

//            if (patient == null)
//            {
//                patient = new Patient
//                {
//                    Id = Guid.NewGuid(),
//                    Name = loginResponse.Name,
//                    DateOfBirth = apiDob,
//                    PhoneNumber = loginResponse.MobileNumber,
//                    Email = string.Empty,
//                    CreatedAt = DateTime.UtcNow
//                };

//                _patients.Add(patient);
//                Console.WriteLine($"New patient created: {patient.Name} | ID: {patient.Id}");
//            }

//            return new AuthenticationResponse
//            {
//                Success = true,
//                PatientId = patient.Id,
//                PatientName = patient.Name,
//                Message = "Authentication successful!"
//            };
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Exception: {ex.Message}");
//            Console.WriteLine(ex.StackTrace);

//            return new AuthenticationResponse
//            {
//                Success = false,
//                Message = $"Authentication failed: {ex.Message}"
//            };
//        }
//    }

//    public Task<Patient?> GetPatientByIdAsync(Guid id)
//    {
//        var patient = _patients.FirstOrDefault(p => p.Id == id);
//        return Task.FromResult(patient);
//    }
//}


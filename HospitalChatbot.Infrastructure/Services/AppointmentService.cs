using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Application.Interfaces;
using HospitalChatbot.Domain.Entities;
using HospitalChatbot.Infrastructure.Data;
using HospitalChatbot.Infrastructure.Models;
using System.Text;
using System.Text.Json;
using MongoDB.Driver;

namespace HospitalChatbot.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly HttpClient _httpClient;
    private readonly MongoDbContext _mongoContext;
    private const string SlotSearchApiUrl = "https://cb45c777-9cd1-4079-b906-48fd30e9653c.mock.pstmn.io/slotsearch";
    private static readonly List<Appointment> _appointments = new();

    public AppointmentService(HttpClient httpClient, MongoDbContext mongoContext)
    {
        _httpClient = httpClient;
        _mongoContext = mongoContext;
    }

    public Task<List<TimeSlot>> GetAvailableSlotsAsync(Guid doctorId, DateTime date)
    {
        var slots = new List<TimeSlot>();

        var startOfDay = new TimeSpan(9, 0, 0);   // 9:00 AM
        var endOfDay = new TimeSpan(18, 0, 0);    // 6:00 PM
        var slotDuration = TimeSpan.FromMinutes(10);

        var now = DateTime.Now;

        // Always generate slots from 9 AM
        var startTime = startOfDay;

        // Decide minimum allowed time
        TimeSpan minAllowedTime;

        if (date.Date == now.Date)
        {
            // TODAY → round current time
            minAllowedTime = TimeSpan.FromMinutes(
                Math.Ceiling(now.TimeOfDay.TotalMinutes / 10) * 10
            );

            if (minAllowedTime < startOfDay)
                minAllowedTime = startOfDay;
        }
        else
        {
            // TOMORROW / FUTURE → always 9 AM
            minAllowedTime = startOfDay;
        }

        // Generate slots
        while (startTime + slotDuration <= endOfDay)
        {
            if (startTime >= minAllowedTime)
            {
                slots.Add(new TimeSlot
                {
                    SlotId = $"{date:yyyyMMdd}_{startTime}", // stable ID
                    StartTime = startTime,
                    EndTime = startTime.Add(slotDuration),
                    IsAvailable = true
                });
            }

            startTime = startTime.Add(slotDuration);
        }

        // Mark booked slots unavailable
        var bookedTimes = _appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
            .Select(a => a.TimeSlot.StartTime)
            .ToHashSet();

        foreach (var slot in slots)
        {
            if (bookedTimes.Contains(slot.StartTime))
            {
                slot.IsAvailable = false;
            }
        }

        return Task.FromResult(slots);
    }

    public async Task<List<AppointmentSummaryDto>> GetAppointmentsFromMongoByPatientIdAsync(string patientName)
    {
        var logs = await _mongoContext.AppointmentLogs
            .Find(a => a.PatientName == patientName && a.Status == "Confirmed")
            .ToListAsync();

        return logs.Select(a => new AppointmentSummaryDto
        {
            AppointmentId = a.AppointmentId,
            AppointmentDate = a.AppointmentDate,
            DoctorName = a.DoctorName,
            ClinicName = a.ClinicName,
            StartTime = a.TimeSlot.StartTime,
            EndTime = a.TimeSlot.EndTime,
            Status = a.Status
        }).ToList();
    }

    public async Task<List<TimeSlot>> GetAvailableSlotsByDoctorNameAsync(string doctorName)
    {
        try
        {
            // Match exact Postman format
            var json = $"{{\r\n  \"doctorName\": \"{doctorName}\"\r\n}}";
            Console.WriteLine($"Slot Search Request JSON: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(SlotSearchApiUrl, content);
            Console.WriteLine($"Slot Search Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Slot Search Response: {responseContent}");

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var slotResponses = JsonSerializer.Deserialize<List<SlotSearchResponse>>(responseContent, jsonOptions);

                if (slotResponses != null && slotResponses.Any())
                {
                    var slots = new List<TimeSlot>();
                    foreach (var sr in slotResponses)
                    {
                        // Parse SlotTime (e.g., "05:30 PM")
                        if (TryParseSlotTime(sr.SlotTime, out TimeSpan startTime))
                        {
                            var slot = new TimeSlot
                            {
                                SlotId = sr.SlotId.ToString(),
                                StartTime = startTime,
                                EndTime = startTime.Add(TimeSpan.FromMinutes(30)),
                                IsAvailable = true
                            };
                            slots.Add(slot);
                        }
                    }

                    return slots;
                }
            }

            // Fallback to static slots
            return await GetAvailableSlotsAsync(Guid.Empty, DateTime.Now);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Slot Search Exception: {ex.Message}");
            return await GetAvailableSlotsAsync(Guid.Empty, DateTime.Now);
        }
    }

    private bool TryParseSlotTime(string slotTime, out TimeSpan result)
    {
        try
        {
            // Parse "05:30 PM" format
            var parts = slotTime.Split(' ');
            if (parts.Length == 2)
            {
                var timeParts = parts[0].Split(':');
                if (timeParts.Length == 2)
                {
                    int hour = int.Parse(timeParts[0]);
                    int minute = int.Parse(timeParts[1]);

                    if (parts[1].ToUpper() == "PM" && hour != 12)
                    {
                        hour += 12;
                    }
                    else if (parts[1].ToUpper() == "AM" && hour == 12)
                    {
                        hour = 0;
                    }

                    result = new TimeSpan(hour, minute, 0);
                    return true;
                }
            }

            result = TimeSpan.Zero;
            return false;
        }
        catch
        {
            result = TimeSpan.Zero;
            return false;
        }
    }

    public async Task<Appointment> BookAppointmentAsync(Guid patientId, Guid doctorId, Guid clinicId, DateTime date, string slotId, string reason)
    {
        // Find the slot to get the correct time information
        var availableSlots = await GetAvailableSlotsAsync(doctorId, date);
        var selectedSlot = availableSlots.FirstOrDefault(s => s.SlotId == slotId);

        // If slot not found in available slots, create a default one
        if (selectedSlot == null)
        {
            selectedSlot = new TimeSlot
            {
                SlotId = slotId,
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.Zero,
                IsAvailable = false
            };
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            ClinicId = clinicId,
            AppointmentDate = date,
            TimeSlot = new TimeSlot
            {
                SlotId = selectedSlot.SlotId,
                StartTime = selectedSlot.StartTime,
                EndTime = selectedSlot.EndTime,
                IsAvailable = false
            },
            Status = AppointmentStatus.Confirmed,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _appointments.Add(appointment);
        return appointment;
    }

    public async Task LogAppointmentToMongoDbAsync(
        Appointment appointment,
        string patientName,
        string doctorName,
        string clinicName,
        Guid sessionId)
    {
        try
        {
            var appointmentLog = new AppointmentLog
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = patientName,
                DoctorId = appointment.DoctorId,
                DoctorName = doctorName,
                ClinicId = appointment.ClinicId,
                ClinicName = clinicName,
                AppointmentDate = appointment.AppointmentDate,
                TimeSlot = new TimeSlotMongo
                {
                    Id = int.TryParse(appointment.TimeSlot.SlotId.Replace("slot", ""), out int slotNum) ? slotNum : 0,
                    StartTime = appointment.TimeSlot.StartTime.ToString(@"hh\:mm"),
                    EndTime = appointment.TimeSlot.EndTime.ToString(@"hh\:mm"),
                    IsAvailable = false
                },
                Reason = appointment.Reason,
                SessionId = sessionId,
                Status = appointment.Status.ToString(),
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };

            await _mongoContext.AppointmentLogs.InsertOneAsync(appointmentLog);

            Console.WriteLine($"Appointment logged to MongoDB - ID: {appointmentLog.AppointmentId}, Patient: {patientName}, Doctor: {doctorName}, Clinic: {clinicName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging appointment to MongoDB: {ex.Message}");
        }
    }

    public async Task CancelAppointmentInMongoAsync(Guid appointmentId)
    {
        var filter = Builders<AppointmentLog>.Filter
            .Eq(a => a.AppointmentId, appointmentId);

        var update = Builders<AppointmentLog>.Update
            .Set(a => a.Status, "Cancelled")
            .Set(a => a.UpdatedAt, DateTime.UtcNow);

        await _mongoContext.AppointmentLogs.UpdateOneAsync(filter, update);
    }

    public Task<Appointment?> GetAppointmentByIdAsync(Guid id)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(appointment);
    }

    public Task<List<Appointment>> GetPatientAppointmentsAsync(Guid patientId)
    {
        var appointments = _appointments.Where(a => a.PatientId == patientId).ToList();
        return Task.FromResult(appointments);
    }
}

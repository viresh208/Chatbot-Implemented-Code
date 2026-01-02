using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Application.Interfaces;
using HospitalChatbot.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalChatbot.Application.Services;

public class ChatbotService : IChatbotService
{
    private readonly ISessionService _sessionService;
    private readonly IPatientService _patientService;
    private readonly IClinicService _clinicService;
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentService _appointmentService;
    private readonly IConversationLogService _conversationLogService;
    private readonly ILogger<ChatbotService> _logger;
    private readonly IHandleCancellationListAsync _handleCancellationListAsync;

    public ChatbotService(
        ISessionService sessionService,
        IPatientService patientService,
        IClinicService clinicService,
        IDoctorService doctorService,
        IAppointmentService appointmentService,
        IConversationLogService conversationLogService,
        ILogger<ChatbotService> logger,
        IHandleCancellationListAsync handleCancellationListAsync)
    {
        _sessionService = sessionService;
        _patientService = patientService;
        _clinicService = clinicService;
        _doctorService = doctorService;
        _appointmentService = appointmentService;
        _conversationLogService = conversationLogService;
        _logger = logger;
        _handleCancellationListAsync = handleCancellationListAsync;
    }

    public async Task<Guid> StartNewSessionAsync()
    {
        var session = await _sessionService.CreateSessionAsync();
        return session.Id;
    }

    public async Task<ChatMessageResponse> ProcessMessageAsync(ChatMessageRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionId);

        if (session == null)
        {
            return new ChatMessageResponse
            {
                SessionId = request.SessionId,
                Message = "Session not found. Please start a new conversation.",
                CurrentState = ConversationState.Initial,
                IsCompleted = true
            };
        }

        var response = session.State switch
        {
            ConversationState.Initial => await HandleInitialStateAsync(session),
            ConversationState.AwaitingDateOfBirth => await HandleDateOfBirthInputAsync(session, request.Message),
            ConversationState.Authenticated => await HandleAuthenticatedStateAsync(session),
            ConversationState.AwaitingClinicSelection => await HandleClinicSelectionAsync(session, request.Message),
            ConversationState.AwaitingBookingOrCancelChoice => await HandleBookingOrCancelChoiceAsync(session, request.Message),
            ConversationState.AwaitingDoctorOrSymptom => await HandleDoctorOrSymptomChoiceAsync(session, request.Message),
            ConversationState.AwaitingDoctorSelection => await HandleDoctorSelectionAsync(session, request.Message),
            ConversationState.AwaitingSlotSelection => await HandleSlotSelectionAsync(session, request.Message),
            ConversationState.BookingConfirmed => await HandleBookingConfirmationAsync(session),
            ConversationState.AwaitingCancellationSelection => await HandleCancellationSelectionAsync(session,request.Message),
            _ => new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Something went wrong. Please start over.",
                CurrentState = session.State,
                IsCompleted = true
            }
        };

        // Log conversation to MongoDB
        try
        {
            await _conversationLogService.LogConversationAsync(
                sessionId: session.Id,
                patientId: session.PatientId,
                patientName: session.Context.ContainsKey("PatientName") ? session.Context["PatientName"]?.ToString() : null,
                conversationState: response.CurrentState.ToString(),
                userMessage: request.Message,
                botResponse: response.Message,
                options: response.Options,
                context: session.Context
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging conversation to MongoDB");
        }

        await _sessionService.UpdateSessionAsync(session);
        return response;
    }

    private async Task<ChatMessageResponse> HandleInitialStateAsync(ChatSession session)
    {
        session.State = ConversationState.AwaitingDateOfBirth;
        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = "Welcome to Hospital Booking System! 🏥\n\nTo get started with your appointment booking, please enter your date of birth for verification (format: DD-MM-YYYY).",
            CurrentState = session.State,
            IsCompleted = false
        };
    }

    private async Task<ChatMessageResponse> HandleDateOfBirthInputAsync(ChatSession session, string dobInput)
    {
        //if (!TryParseDateOfBirth(dobInput, out DateTime dob))
        //{
        //    return new ChatMessageResponse
        //    {
        //        SessionId = session.Id,
        //        Message = "Invalid date format. Please enter your date of birth in format DD/MM/YYYY (e.g., 28/12/2002).",
        //        CurrentState = session.State,
        //        IsCompleted = false
        //    };
        //}

        var authResponse = await _patientService.AuthenticatePatientAsync(new AuthenticationRequest
        {
            Name = string.Empty,
            DateofBirth = dobInput
        });

        if (!authResponse.Success)
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = authResponse.Message + "\nPlease try again with your correct date of birth.",
                CurrentState = ConversationState.AwaitingDateOfBirth,
                IsCompleted = false
            };
        }

        session.PatientId = authResponse.PatientId;
        session.Context["DateOfBirth"] = dobInput;
        session.Context["PatientName"] = authResponse.PatientName;
        session.PatientName = authResponse.PatientName;
        session.State = ConversationState.Authenticated;

        return await HandleAuthenticatedStateAsync(session);
    }

    private async Task<ChatMessageResponse> HandleAuthenticatedStateAsync(ChatSession session)
    {
        // Call clinic selection API with patient name
        var patientName = session.Context["PatientName"]?.ToString() ?? string.Empty;
        var clinic = await _clinicService.GetClinicByPatientNameAsync(patientName);

        if (clinic != null)
        {
            // Automatically select the clinic from API
            session.Context["ClinicId"] = clinic.Id;
            session.Context["ClinicName"] = clinic.Name;
            session.State = ConversationState.AwaitingBookingOrCancelChoice;

            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = $"Great! You are verified as {patientName}. " +
          $"You are registered with {clinic.Name}.\n\n" +
          $"Would you like to:\n" +
          $"1. Book an appointment\n" +
          $"2. Cancel appointment\n\n" +
          $"Please type '1' or '2'.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }

        // Fallback to manual clinic selection if API fails
        var clinics = await _clinicService.GetAllClinicsAsync();
        session.State = ConversationState.AwaitingClinicSelection;

        var options = clinics.Select(c => new OptionDto
        {
            Id = c.Id.ToString(),
            Display = $"{c.Name} - {c.Department}",
            Value = c.Id.ToString()
        }).ToList();

        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = "Great! You're verified. Now, please select a clinic from the options below:",
            CurrentState = session.State,
            Options = options,
            IsCompleted = false
        };
    }

    private async Task<ChatMessageResponse> HandleClinicSelectionAsync(ChatSession session, string clinicInput)
    {
        if (!Guid.TryParse(clinicInput, out Guid clinicId))
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Invalid clinic selection. Please select a valid clinic from the list.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }

        var clinic = await _clinicService.GetClinicByIdAsync(clinicId);
        if (clinic == null)
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Clinic not found. Please select a valid clinic.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }

        session.Context["ClinicId"] = clinicId;
        session.Context["ClinicName"] = clinic.Name;
        session.State = ConversationState.AwaitingBookingOrCancelChoice;

        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message =
          $"You've selected {clinic.Name}.\n\n" +
          $"Would you like to:\n" +
          $"1. Book an appointment\n" +
          $"2. Cancel appointment\n\n" +
          $"Please type '1' or '2'.",
            CurrentState = session.State,
            IsCompleted = false
        };
    }

    private async Task<ChatMessageResponse> HandleBookingOrCancelChoiceAsync(ChatSession session, string choice)
    {
        if (choice.Trim() == "1")
        {
            // User wants to book an appointment
            session.State = ConversationState.AwaitingDoctorOrSymptom;

            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Would you like to:\n" +
                         "1. View all doctors in this clinic\n" +
                         "2. Describe your symptoms to get doctor recommendations\n\n" +
                         "Please type '1' or '2'.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }
        //else if (choice.Trim() == "2")
        //{
        //    // User wants to cancel an appointment
        //    return new ChatMessageResponse
        //    {
        //        SessionId = session.Id,
        //        Message = "Appointment cancellation feature is coming soon. Please contact the clinic directly to cancel your appointment.",
        //        CurrentState = session.State,
        //        IsCompleted = true
        //    };
        //}
        else if (choice.Trim() == "2")
        {
            session.State = ConversationState.AwaitingCancellationSelection;
            return await HandleCancellationListAsync(session);
        }

        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = "Please type '1' to book an appointment or '2' to cancel an appointment.",
            CurrentState = session.State,
            IsCompleted = false
        };
    }

    //private async Task<ChatMessageResponse> HandleCancellationListAsync(ChatSession session)
    //{
    //    if (session.PatientId == null)
    //    {
    //        return new ChatMessageResponse
    //        {
    //            SessionId = session.Id,
    //            Message = "I could not found any patient record with this input. Please start again.",
    //            CurrentState = ConversationState.Initial,
    //            IsCompleted = true
    //        };
    //    }

    //    var appointments = await _appointmentService.GetAppointmentsFromMongoByPatientIdAsync(session.PatientId.Value);

    //    if (appointments.Count() == 0)
    //    {
    //        return new ChatMessageResponse
    //        {
    //            SessionId = session.Id,
    //            Message = "You don’t have any active appointments to cancel.",
    //            CurrentState = session.State,
    //            IsCompleted = true
    //        };
    //    }

    //    session.Context["CancellableAppointments"] =
    //    System.Text.Json.JsonSerializer.Serialize(
    //        appointments.Select(a => new
    //        {
    //            a.Id,
    //            a.DoctorName,
    //            a.ClinicName,
    //            a.AppointmentDate,
    //            StartTime = a.TimeSlot.StartTime,
    //            EndTime = a.TimeSlot.EndTime
    //        })
    //    );

    //    var options = appointments.Select(a => new OptionDto
    //    {
    //        Id = a.Id.ToString(),
    //        Display = $"Dr. {a.DoctorName} at {a.ClinicName} on {a.AppointmentDate:dddd, dd MMMM yyyy} " +
    //                  $"from {FormatTimeWithAmPm(a.TimeSlot.StartTime)} to {FormatTimeWithAmPm(a.TimeSlot.EndTime)}",
    //        Value = a.Id.ToString()
    //    }).ToList();

    //    return new ChatMessageResponse
    //    {
    //        SessionId = session.Id,
    //        Message = "Here are your active appointments. Please select one to cancel:",
    //        CurrentState = session.State,
    //        Options = options,
    //        IsCompleted = false
    //    };  
    //}

    private async Task<ChatMessageResponse> HandleCancellationListAsync(ChatSession session)
    {
        var nowDate = DateTime.Today;
        var nowTime = DateTime.Now.TimeOfDay;

        if (session.PatientId == null)
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "I could not find your patient record. Please start again.",
                CurrentState = ConversationState.Initial,
                IsCompleted = true
            };
        }

        var appointments =
            await _appointmentService.GetAppointmentsFromMongoByPatientIdAsync(session.PatientName);

    
        var futureAppointments = appointments
            .Where(a =>
                a.AppointmentDate > nowDate ||
                (a.AppointmentDate == nowDate &&
                 TimeSpan.Parse(a.StartTime) > nowTime)
            )
            .ToList();

        if (!futureAppointments.Any())
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "You don’t have any active appointments to cancel.",
                CurrentState = session.State,
                IsCompleted = true
            };
        }

        // Store cancellable appointments in session
        session.Context["CancellableAppointments"] =
            System.Text.Json.JsonSerializer.Serialize(
                futureAppointments.Select(a => new
                {
                    a.AppointmentId,
                    a.DoctorName,
                    a.ClinicName,
                    a.AppointmentDate,
                    a.StartTime,
                    a.EndTime
                })
            );

        // Build UI options
        var options = futureAppointments.Select(a => new OptionDto
        {
            Id = a.AppointmentId.ToString(),
            Display =
                $"Dr. {a.DoctorName} at {a.ClinicName} on " +
                $"{a.AppointmentDate:dddd, dd MMMM yyyy} " +
                $"from {FormatTimeWithAmPm(TimeSpan.Parse(a.StartTime))} " +
                $"to {FormatTimeWithAmPm(TimeSpan.Parse(a.EndTime))}",
            Value = a.AppointmentId.ToString()
        }).ToList();

        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = "Here are your active appointments. Please select one to cancel:",
            CurrentState = session.State,
            Options = options,
            IsCompleted = false
        };
    }

    private async Task<ChatMessageResponse> HandleCancellationSelectionAsync(
    ChatSession session,
    string selection)
    {
        // WhatsApp sends appointmentId (GUID)
        if (!Guid.TryParse(selection, out Guid appointmentId))
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Invalid selection. Please select a valid appointment.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }

        // Get stored appointments
        var json = session.Context["CancellableAppointments"]?.ToString();
        var appointments = JsonSerializer.Deserialize<List<CancellableAppointmentDto>>(json!);

        // Find the selected appointment
        var appointment = appointments?
            .FirstOrDefault(a => a.AppointmentId == appointmentId);

        if (appointment == null)
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Appointment not found.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }

        // Cancel appointment
        await _appointmentService.CancelAppointmentInMongoAsync(appointment.AppointmentId);

        session.State = ConversationState.Completed;

        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = "Your appointment has been cancelled successfully.",
            CurrentState = session.State,
            IsCompleted = true
        };
    }

    private async Task<ChatMessageResponse> HandleDoctorOrSymptomChoiceAsync(ChatSession session, string choice)
    {
        var clinicName = session.Context["ClinicName"]?.ToString() ?? string.Empty;

        if (choice.Trim() == "1")
        {
            // Use doctor search API with clinic name
            var doctors = await _doctorService.GetDoctorsByClinicNameAsync(clinicName);
            return await ShowDoctorSelectionAsync(session, doctors);
        }
        else if (choice.Trim() == "2")
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Please describe your symptoms (e.g., 'fever', 'chestPain', 'headech'):",
                CurrentState = ConversationState.AwaitingDoctorOrSymptom,
                IsCompleted = false
            };
        }
        else if (choice.Length > 2)
        {
            // Use symptoms API
            var doctors = await _doctorService.GetDoctorsBySymptomsAsync(choice);

            if (doctors.Count == 0)
            {
                return new ChatMessageResponse
                {
                    SessionId = session.Id,
                    Message = "No doctors found matching your symptoms. Let me show you all available doctors in this clinic.",
                    CurrentState = session.State,
                    IsCompleted = false
                };
            }

            session.Context["Symptoms"] = choice;
            return await ShowDoctorSelectionAsync(session, doctors);
        }

        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = "Please type '1' to view all doctors or '2' to describe your symptoms.",
            CurrentState = session.State,
            IsCompleted = false
        };
    }

    private async Task<ChatMessageResponse> ShowDoctorSelectionAsync(ChatSession session, List<Doctor> doctors)
    {
        session.State = ConversationState.AwaitingDoctorSelection;

        var options = doctors.Select(d => new OptionDto
        {
            Id = d.Id.ToString(),
            Display = $"Dr. {d.Name} - {d.Specialization}",
            Value = d.Id.ToString()
        }).ToList();

        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = "Here are the available doctors. Please select one:",
            CurrentState = session.State,
            Options = options,
            IsCompleted = false
        };
    }

    private async Task<ChatMessageResponse> HandleDoctorSelectionAsync(ChatSession session, string doctorInput)
    {
        if (!Guid.TryParse(doctorInput, out Guid doctorId))
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Invalid doctor selection. Please select a valid doctor from the list.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }

        var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
        if (doctor == null)
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Doctor not found. Please select a valid doctor.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }

        session.Context["DoctorId"] = doctorId;
        session.Context["DoctorName"] = doctor.Name;

        // Dates
        var today = DateTime.Now.Date;
        var tomorrow = today.AddDays(1);
        var dayAfterTomorrow = today.AddDays(2);
        var dayAfterTomorrow2 = today.AddDays(3);
        var dayAfterTomorrow3 = today.AddDays(4);
        var dayAfterTomorrow4 = today.AddDays(5);
        var dayAfterTomorrow5 = today.AddDays(6);

        // Slots container
        var allSlotsWithDates = new List<(DateTime date, string label, TimeSlot slot)>();

        // Generate slots PER DATE (this is the fix)
        await AddSlotsForDateAsync(doctorId, today, "Today", allSlotsWithDates);
        await AddSlotsForDateAsync(doctorId, tomorrow, "Tomorrow", allSlotsWithDates);
        await AddSlotsForDateAsync(doctorId, dayAfterTomorrow, dayAfterTomorrow.ToString("dddd"), allSlotsWithDates);
        await AddSlotsForDateAsync(doctorId, dayAfterTomorrow2, dayAfterTomorrow2.ToString("dddd"), allSlotsWithDates);
        await AddSlotsForDateAsync(doctorId, dayAfterTomorrow3, dayAfterTomorrow3.ToString("dddd"), allSlotsWithDates);
        await AddSlotsForDateAsync(doctorId, dayAfterTomorrow4, dayAfterTomorrow4.ToString("dddd"), allSlotsWithDates);
        await AddSlotsForDateAsync(doctorId, dayAfterTomorrow5, dayAfterTomorrow5.ToString("dddd"), allSlotsWithDates);

        // Store slots in session for later selection
        session.Context["AvailableSlotsWithDates"] =
            System.Text.Json.JsonSerializer.Serialize(
                allSlotsWithDates.Select(s => new
                {
                    Date = s.date,
                    DateLabel = s.label,
                    SlotId = s.slot.SlotId,
                    StartTime = s.slot.StartTime,
                    EndTime = s.slot.EndTime
                }).ToList()
            );

        session.State = ConversationState.AwaitingSlotSelection;

        // Build UI options
        var options = allSlotsWithDates.Select((s, index) => new OptionDto
        {
            Id = index.ToString(),
            Display = $"{s.label} - {FormatTimeWithAmPm(s.slot.StartTime)} - {FormatTimeWithAmPm(s.slot.EndTime)}",
            Value = index.ToString()
        }).ToList();

        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = $"You've selected Dr. {doctor.Name}.\n\nAvailable time slots:",
            CurrentState = session.State,
            Options = options,
            IsCompleted = false
        };
    }

    private async Task AddSlotsForDateAsync(
    Guid doctorId,
    DateTime date,
    string label,
    List<(DateTime date, string label, TimeSlot slot)> result)
    {
        var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, date);

        var morning = slots.FirstOrDefault(s => s.StartTime.Hours >= 9 && s.StartTime.Hours <= 12 && s.IsAvailable);
        var afternoon = slots.FirstOrDefault(s => s.StartTime.Hours > 12 && s.StartTime.Hours <= 16 && s.IsAvailable);
        var evening = slots.FirstOrDefault(s => s.StartTime.Hours > 16 && s.StartTime.Hours <= 20 && s.IsAvailable);

        if (morning != null) result.Add((date, label, morning));
        if (afternoon != null) result.Add((date, label, afternoon));
        if (evening != null) result.Add((date, label, evening));
    }

    private async Task<ChatMessageResponse> HandleSlotSelectionAsync(ChatSession session, string slotIndexStr)
    {
        if (string.IsNullOrWhiteSpace(slotIndexStr))
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Invalid slot selection. Please select a valid time slot.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }

        // Parse the slot index
        if (!int.TryParse(slotIndexStr, out int slotIndex))
        {
            return new ChatMessageResponse
            {
                SessionId = session.Id,
                Message = "Invalid slot selection. Please select a valid time slot.",
                CurrentState = session.State,
                IsCompleted = false
            };
        }

        // Retrieve available slots with dates from session context
        DateTime appointmentDate = DateTime.Now.AddDays(1).Date;
        TimeSpan startTime = TimeSpan.Zero;
        TimeSpan endTime = TimeSpan.Zero;
        string slotId = string.Empty;
        string dateLabel = "Tomorrow";

        if (session.Context.ContainsKey("AvailableSlotsWithDates"))
        {
            var slotsJson = session.Context["AvailableSlotsWithDates"]?.ToString();
            if (!string.IsNullOrEmpty(slotsJson))
            {
                var slots = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(slotsJson);
                if (slots != null && slotIndex >= 0 && slotIndex < slots.Count)
                {
                    var selectedSlot = slots[slotIndex];
                    appointmentDate = selectedSlot.GetProperty("Date").GetDateTime();
                    dateLabel = selectedSlot.GetProperty("DateLabel").GetString() ?? dateLabel;
                    slotId = selectedSlot.GetProperty("SlotId").GetString() ?? string.Empty;

                    // Parse TimeSpan from string
                    var startTimeStr = selectedSlot.GetProperty("StartTime").GetString();
                    var endTimeStr = selectedSlot.GetProperty("EndTime").GetString();
                    TimeSpan.TryParse(startTimeStr, out startTime);
                    TimeSpan.TryParse(endTimeStr, out endTime);
                }
            }
        }

        var patientId = session.PatientId ?? Guid.Empty;
        var doctorId = Guid.Parse(session.Context["DoctorId"]?.ToString() ?? string.Empty);
        var clinicId = Guid.Parse(session.Context["ClinicId"]?.ToString() ?? string.Empty);
        var reason = session.Context.ContainsKey("Symptoms") ? session.Context["Symptoms"]?.ToString() ?? "General checkup" : "General checkup";

        var appointment = await _appointmentService.BookAppointmentAsync(
            patientId, doctorId, clinicId, appointmentDate, slotId, reason);

        session.Context["AppointmentId"] = appointment.Id;
        session.State = ConversationState.BookingConfirmed;

        var doctorName = session.Context["DoctorName"]?.ToString();
        var clinicName = session.Context["ClinicName"]?.ToString();
        var patientName = session.Context["PatientName"]?.ToString();

        // Use the stored slot times for display
        var displayStartTime = startTime != TimeSpan.Zero ? startTime : appointment.TimeSlot.StartTime;
        var displayEndTime = endTime != TimeSpan.Zero ? endTime : appointment.TimeSlot.EndTime;

        // Log appointment confirmation details
        _logger.LogInformation(
            "Appointment Confirmed - ID: {AppointmentId}, Patient: {PatientName} (ID: {PatientId}), " +
            "Doctor: Dr. {DoctorName} (ID: {DoctorId}), Clinic: {ClinicName} (ID: {ClinicId}), " +
            "Date: {AppointmentDate:yyyy-MM-dd}, Time: {StartTime} - {EndTime}, Reason: {Reason}, Session: {SessionId}",
            appointment.Id, patientName, patientId, doctorName, doctorId, clinicName, clinicId,
            appointmentDate, FormatTimeWithAmPm(displayStartTime), FormatTimeWithAmPm(displayEndTime), reason, session.Id);

        // Log appointment to MongoDB
        try
        {
            await _appointmentService.LogAppointmentToMongoDbAsync(
                appointment,
                patientName ?? "Unknown",
                doctorName ?? "Unknown",
                clinicName ?? "Unknown",
                session.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging appointment to MongoDB");
        }

        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = $"🎉 Appointment Confirmed!\n\n" +
                     $"📋 Booking Details:\n" +
                     $"- Appointment ID: {appointment.Id}\n" +
                     $"- Doctor: Dr. {doctorName}\n" +
                     $"- Clinic: {clinicName}\n" +
                     $"- Date: {appointmentDate:dddd, dd MMMM yyyy}\n" +
                     $"- Time: {FormatTimeWithAmPm(displayStartTime)} - {FormatTimeWithAmPm(displayEndTime)}\n\n" +
                     $"Thank you for booking with us! You will receive a confirmation email shortly.",
            CurrentState = session.State,
            IsCompleted = true,
            Data = new { AppointmentId = appointment.Id }
        };
    }

    private async Task<ChatMessageResponse> HandleBookingConfirmationAsync(ChatSession session)
    {
        return new ChatMessageResponse
        {
            SessionId = session.Id,
            Message = "Your appointment has been confirmed. Have a great day!",
            CurrentState = session.State,
            IsCompleted = true
        };
    }

    private string FormatTimeWithAmPm(TimeSpan time)
    {
        var dateTime = DateTime.Today.Add(time);
        return dateTime.ToString("hh:mm tt");
    }

    private bool TryParseDateOfBirth(string input, out DateTime dob)
    {
        dob = DateTime.MinValue;

        var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "MM/dd/yyyy" };

        if (DateTime.TryParseExact(input.Trim(), formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out dob))
        {
            if (dob > DateTime.Now.AddYears(-120) && dob < DateTime.Now)
            {
                return true;
            }
        }

        return false;
    }
}

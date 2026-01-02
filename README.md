# Hospital Booking Chatbot - .NET Application

A comprehensive hospital appointment booking system with an intelligent chatbot interface built using .NET 9, clean architecture principles, and a modern web UI.

## Features

- **Conversational Chatbot Interface**: Natural language interaction for booking appointments
- **Patient Authentication**: Secure login using name and date of birth
- **Clinic Selection**: Browse and select from multiple hospital clinics
- **Doctor Selection**: Choose doctors by clinic or get recommendations based on symptoms
- **Intelligent Symptom Matching**: Bot suggests appropriate doctors based on described symptoms
- **Slot Booking**: View and book available appointment time slots
- **Real-time Conversation State Management**: Maintains context throughout the booking flow
- **Modern Web UI**: Responsive, animated chatbot interface

## Architecture

The application follows **Clean Architecture** principles with clear separation of concerns:

```
HospitalChatbot/
├── HospitalChatbot.Domain/          # Core business entities
│   └── Entities/
│       ├── Patient.cs
│       ├── Doctor.cs
│       ├── Clinic.cs
│       ├── Appointment.cs
│       └── ChatSession.cs
│
├── HospitalChatbot.Application/     # Business logic & interfaces
│   ├── DTOs/
│   │   ├── ChatMessageRequest.cs
│   │   ├── ChatMessageResponse.cs
│   │   └── AuthenticationRequest.cs
│   ├── Interfaces/
│   │   ├── IChatbotService.cs
│   │   ├── IPatientService.cs
│   │   ├── IClinicService.cs
│   │   ├── IDoctorService.cs
│   │   ├── IAppointmentService.cs
│   │   └── ISessionService.cs
│   └── Services/
│       └── ChatbotService.cs        # Core chatbot orchestration logic
│
├── HospitalChatbot.Infrastructure/  # Data access & implementation
│   └── Services/
│       ├── SessionService.cs        # In-memory session management
│       ├── PatientService.cs        # Dummy patient data
│       ├── ClinicService.cs         # Dummy clinic data
│       ├── DoctorService.cs         # Dummy doctor data with symptom matching
│       └── AppointmentService.cs    # Dummy appointment booking
│
└── HospitalChatbot.API/             # Web API & UI
    ├── Controllers/
    │   ├── ChatbotController.cs     # Main chatbot endpoint
    │   ├── PatientsController.cs    # Patient authentication
    │   ├── ClinicsController.cs     # Clinic management
    │   ├── DoctorsController.cs     # Doctor search & selection
    │   └── AppointmentsController.cs # Appointment booking
    ├── wwwroot/
    │   ├── index.html              # Chatbot UI
    │   ├── styles.css              # Modern, animated styles
    │   └── app.js                  # Frontend chatbot logic
    └── Program.cs                  # DI configuration & startup
```

## Conversation Flow

1. **Initial State** → Welcome message
2. **Name Input** → User provides full name
3. **Date of Birth** → User provides DOB for authentication
4. **Authentication** → System verifies patient credentials
5. **Clinic Selection** → Display available clinics
6. **Doctor/Symptom Choice** → User chooses to view all doctors or describe symptoms
7. **Doctor Selection** → User selects from available/recommended doctors
8. **Slot Selection** → User picks an available time slot
9. **Booking Confirmed** → Final confirmation with appointment details

## API Endpoints

### Chatbot
- `POST /api/chatbot/start` - Start a new chat session
- `POST /api/chatbot/message` - Send message and get response

### Patients
- `POST /api/patients/authenticate` - Authenticate patient
- `GET /api/patients/{id}` - Get patient details

### Clinics
- `GET /api/clinics` - Get all clinics
- `GET /api/clinics/{id}` - Get clinic by ID

### Doctors
- `GET /api/doctors/clinic/{clinicId}` - Get doctors by clinic
- `GET /api/doctors/symptoms?symptoms={text}` - Get doctors by symptoms
- `GET /api/doctors/{id}` - Get doctor by ID

### Appointments
- `GET /api/appointments/slots/{doctorId}?date={date}` - Get available slots
- `POST /api/appointments/book` - Book an appointment
- `GET /api/appointments/{id}` - Get appointment details

## Dummy Test Data

### Patients
| Name | Date of Birth | ID |
|------|--------------|-----|
| John Doe | 1990-05-15 | 11111111-1111-1111-1111-111111111111 |
| Jane Smith | 1985-08-20 | 22222222-2222-2222-2222-222222222222 |
| Michael Johnson | 1992-03-10 | 33333333-3333-3333-3333-333333333333 |

### Clinics
- City General Hospital (General Medicine)
- Heart Care Center (Cardiology)
- Pediatric Clinic (Pediatrics)
- Skin & Allergy Center (Dermatology)

### Doctors
- Dr. Sarah Williams - General Physician (fever, cold, cough, headache)
- Dr. Robert Chen - Cardiologist (chest pain, heart palpitations)
- Dr. Emily Brown - Pediatrician (child care, vaccination)
- Dr. David Martinez - Dermatologist (skin rash, acne, allergy)
- Dr. Lisa Anderson - Internal Medicine (diabetes, thyroid, fatigue)

## Running the Application

### Prerequisites
- .NET 9 SDK

### Build & Run

1. Navigate to the project directory:
```bash
cd HospitalChatbot
```

2. Build the solution:
```bash
dotnet build
```

3. Run the API:
```bash
cd HospitalChatbot.API
dotnet run
```

4. Open your browser and navigate to:
```
https://localhost:5001
```
or
```
http://localhost:5000
```

5. Access Swagger documentation at:
```
https://localhost:5001/swagger
```

## Technology Stack

- **Backend**: .NET 9, ASP.NET Core Web API
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **Architecture**: Clean Architecture with DDD principles
- **API Documentation**: Swagger/OpenAPI
- **State Management**: In-memory (Dictionary-based for demo)
- **Dependency Injection**: Built-in ASP.NET Core DI container

## Design Patterns Used

1. **Clean Architecture**: Separation of Domain, Application, Infrastructure, and API layers
2. **Repository Pattern**: Abstracted data access through service interfaces
3. **Dependency Injection**: Loose coupling and testability
4. **State Pattern**: Conversation state management in chatbot
5. **DTO Pattern**: Data transfer objects for API communication
6. **Strategy Pattern**: Different doctor selection strategies (by clinic/by symptoms)

## Key Features of the Architecture

### 1. Separation of Concerns
- **Domain Layer**: Pure business entities with no dependencies
- **Application Layer**: Business logic and orchestration
- **Infrastructure Layer**: Implementation details (data access)
- **API Layer**: HTTP endpoints and UI

### 2. Dependency Inversion
- All dependencies point inward toward the domain
- Infrastructure and API depend on abstractions in Application layer

### 3. Testability
- Service interfaces allow easy mocking
- Business logic isolated from infrastructure concerns
- Each layer can be tested independently

### 4. Extensibility
- Easy to add new conversation states
- Simple to swap in-memory storage with actual database
- New services can be added without modifying existing code

### 5. State Management
- Conversation context preserved throughout the flow
- Session-based tracking of user progress
- Context dictionary for flexible data storage

## Future Enhancements

- Integrate with actual database (Entity Framework Core)
- Add authentication & authorization (JWT)
- Implement real-time notifications (SignalR)
- Add appointment cancellation & rescheduling
- Email/SMS confirmations
- Payment integration
- Medical history tracking
- Prescription management
- Multi-language support
- Voice input/output capabilities

## License

MIT License - Feel free to use this project for learning and development purposes.

## Developer Notes

This is a demonstration project showcasing:
- Modern .NET development practices
- Clean architecture implementation
- Conversational AI design patterns
- RESTful API design
- Responsive web UI development

The dummy data services can be easily replaced with actual database implementations by creating new service classes that implement the existing interfaces.

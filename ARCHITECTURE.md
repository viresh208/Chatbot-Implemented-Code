# Architecture Documentation

## Overview

This document describes the architecture of the Hospital Booking Chatbot system, built using Clean Architecture principles with .NET 9.

## Architecture Layers

### 1. Domain Layer (HospitalChatbot.Domain)

**Purpose**: Contains core business entities and value objects with no external dependencies.

**Components**:
- `Patient`: Represents a patient entity
- `Doctor`: Represents a doctor with specialization and symptom mappings
- `Clinic`: Represents a hospital clinic/department
- `Appointment`: Represents a booking with time slot details
- `ChatSession`: Maintains conversation state and context
- `ConversationState`: Enum defining all possible conversation states
- `AppointmentStatus`: Enum for appointment lifecycle

**Key Principles**:
- No dependencies on other layers
- Pure business logic
- Framework-agnostic
- Contains only entities and enums

### 2. Application Layer (HospitalChatbot.Application)

**Purpose**: Contains business logic, interfaces, and orchestration services.

**Components**:

#### DTOs (Data Transfer Objects)
- `ChatMessageRequest`: Input from client
- `ChatMessageResponse`: Output to client with state and options
- `AuthenticationRequest/Response`: Patient verification data
- `OptionDto`: Represents user choice options

#### Interfaces
- `IChatbotService`: Main chatbot orchestration
- `ISessionService`: Session management
- `IPatientService`: Patient authentication
- `IClinicService`: Clinic operations
- `IDoctorService`: Doctor search and recommendations
- `IAppointmentService`: Appointment booking

#### Services
- `ChatbotService`: Core implementation containing:
  - State machine logic
  - Conversation flow management
  - Input validation
  - Response generation
  - Context management

**Key Principles**:
- Defines interfaces (contracts)
- Contains business rules
- No dependency on infrastructure
- Orchestrates domain entities

### 3. Infrastructure Layer (HospitalChatbot.Infrastructure)

**Purpose**: Implements interfaces defined in Application layer with concrete implementations.

**Components**:
- `SessionService`: In-memory session storage using Dictionary
- `PatientService`: Dummy patient data with authentication logic
- `ClinicService`: Static clinic data
- `DoctorService`: Doctor data with intelligent symptom matching
- `AppointmentService`: Slot generation and booking logic

**Key Principles**:
- Implements Application layer interfaces
- Contains all external concerns (data access)
- Can be swapped without affecting business logic
- Uses in-memory storage for demo (easily replaceable with DB)

### 4. API Layer (HospitalChatbot.API)

**Purpose**: Exposes HTTP endpoints and hosts the web UI.

**Components**:

#### Controllers
- `ChatbotController`: Main chatbot endpoints
- `PatientsController`: Patient authentication endpoints
- `ClinicsController`: Clinic data endpoints
- `DoctorsController`: Doctor search endpoints
- `AppointmentsController`: Appointment management endpoints

#### Web UI (wwwroot)
- `index.html`: Chatbot interface structure
- `styles.css`: Modern, animated UI styling
- `app.js`: Frontend logic and API integration

#### Configuration
- `Program.cs`:
  - Dependency injection setup
  - CORS configuration
  - Static files middleware
  - Swagger configuration

**Key Principles**:
- Thin layer, delegates to Application services
- RESTful API design
- Proper HTTP status codes
- SPA hosting with fallback routing

## Data Flow

### Chatbot Conversation Flow

```
User Action → API Controller → ChatbotService → State Handler → Domain Services → Response
     ↓                                                                              ↑
     └──────────────────────────── JSON Response ←───────────────────────────────┘
```

### Detailed Flow Example (Clinic Selection):

1. **User clicks clinic option** (Frontend)
2. **POST /api/chatbot/message** (API Layer)
3. **ChatbotController receives request**
4. **Calls IChatbotService.ProcessMessageAsync()** (Application Layer)
5. **ChatbotService:**
   - Retrieves session from ISessionService
   - Checks current state (AwaitingClinicSelection)
   - Validates clinic ID
   - Calls IClinicService.GetClinicByIdAsync()
   - Updates session context and state
   - Returns ChatMessageResponse
6. **Controller returns JSON to frontend**
7. **Frontend renders bot message and doctor options**

## State Machine Design

The chatbot uses a state machine pattern to manage conversation flow:

```
Initial
  ↓
AwaitingName
  ↓
AwaitingDateOfBirth
  ↓
Authenticated (Auto-transitions to clinic selection)
  ↓
AwaitingClinicSelection
  ↓
AwaitingDoctorOrSymptom
  ↓ (branches)
  ├─→ View all doctors → AwaitingDoctorSelection
  └─→ Describe symptoms → Symptom matching → AwaitingDoctorSelection
      ↓
AwaitingSlotSelection
  ↓
BookingConfirmed
  ↓
Completed
```

### State Handlers

Each state has a corresponding handler method:
- `HandleInitialStateAsync()`
- `HandleNameInputAsync()`
- `HandleDateOfBirthInputAsync()`
- `HandleAuthenticatedStateAsync()`
- `HandleClinicSelectionAsync()`
- `HandleDoctorOrSymptomChoiceAsync()`
- `HandleDoctorSelectionAsync()`
- `HandleSlotSelectionAsync()`
- `HandleBookingConfirmationAsync()`

## Dependency Injection Configuration

```csharp
// All services registered as Scoped
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
```

**Why Scoped?**
- Services maintain state per HTTP request
- Session data is request-specific
- Follows ASP.NET Core best practices for web APIs

## Design Patterns Implemented

### 1. Clean Architecture Pattern
- Dependency Rule: Inner layers don't depend on outer layers
- Domain at the center, Infrastructure at the edges

### 2. Repository Pattern
- Abstracted data access through service interfaces
- Easy to swap implementations

### 3. Dependency Inversion Principle
- High-level modules depend on abstractions
- Low-level modules implement abstractions

### 4. State Pattern
- Conversation state management
- Each state has specific behavior

### 5. Strategy Pattern
- Different doctor selection strategies:
  - By clinic
  - By symptoms (intelligent matching)

### 6. DTO Pattern
- Separation between API contracts and domain entities
- Prevents over-posting and under-posting

### 7. Service Layer Pattern
- Business logic encapsulated in services
- Controllers remain thin

## Security Considerations

### Current Implementation
- Basic authentication using name + DOB
- CORS enabled for development
- No sensitive data stored

### Production Recommendations
- Implement JWT authentication
- Add rate limiting
- Enable HTTPS only
- Implement proper logging
- Add input sanitization
- Use secure session storage (Redis)
- Implement CSRF protection
- Add API versioning

## Scalability Considerations

### Current Limitations (Demo)
- In-memory session storage (lost on restart)
- Static dummy data
- Single instance only

### Production Recommendations
1. **Session Storage**:
   - Use Redis or distributed cache
   - Implement session persistence

2. **Database**:
   - Replace dummy services with Entity Framework Core
   - Use SQL Server, PostgreSQL, or similar

3. **Caching**:
   - Cache clinic and doctor data
   - Use memory cache or Redis

4. **Load Balancing**:
   - Stateless API design
   - Session affinity or distributed sessions

5. **Message Queue**:
   - Async appointment confirmations
   - Email/SMS notifications

## Testing Strategy

### Unit Tests (Not included but recommended)
- Test each service in isolation
- Mock dependencies using interfaces
- Test state transitions
- Test validation logic

### Integration Tests
- Test API endpoints
- Test database interactions
- Test authentication flow

### E2E Tests
- Test complete booking flow
- Test UI interactions

## Extension Points

### Easy to Add:
1. **New Conversation States**
   - Add to ConversationState enum
   - Create handler method
   - Add to state switch

2. **New Services**
   - Define interface in Application
   - Implement in Infrastructure
   - Register in DI container

3. **New Entities**
   - Add to Domain layer
   - Create corresponding DTOs
   - Add service interface and implementation

4. **Database Integration**
   - Install EF Core packages
   - Create DbContext
   - Replace dummy services with repository implementations
   - No changes to Application or Domain layers

### Example: Adding Prescription Feature

1. **Domain**: Add `Prescription` entity
2. **Application**: Add `IPrescriptionService` interface
3. **Infrastructure**: Implement `PrescriptionService`
4. **API**: Add `PrescriptionsController`
5. **DI**: Register in Program.cs

## Performance Optimizations

### Current
- Minimal allocations
- Fast in-memory lookups
- No database round trips

### Potential Improvements
1. **Response Caching**
   - Cache clinic/doctor lists
   - Use ETags for conditional requests

2. **Async/Await**
   - Already implemented for scalability
   - Non-blocking I/O operations

3. **Connection Pooling**
   - When database is added
   - Configure appropriate pool size

4. **Compression**
   - Enable response compression middleware
   - Reduce bandwidth usage

## Monitoring and Logging

### Recommended Additions
1. **Structured Logging**
   - Serilog or NLog
   - Log conversation states
   - Track errors and exceptions

2. **Application Insights**
   - Track API performance
   - Monitor user behavior
   - Alert on errors

3. **Health Checks**
   - API health endpoint
   - Database connectivity
   - External service dependencies

## Conclusion

This architecture provides:
- ✅ Clear separation of concerns
- ✅ Testability at all layers
- ✅ Easy to maintain and extend
- ✅ Ready for production with minimal changes
- ✅ Follows SOLID principles
- ✅ Industry best practices

The clean architecture approach ensures that business logic remains independent of frameworks, UI, and external services, making the codebase resilient to change and easy to test.

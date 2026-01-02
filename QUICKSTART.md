# Quick Start Guide

## Getting Started in 5 Minutes

### Step 1: Build the Solution
```bash
cd c:\Users\User\Desktop\Voicebot-Dummy
dotnet build
```

### Step 2: Run the Application
```bash
cd HospitalChatbot.API
dotnet run
```

### Step 3: Open Your Browser
Navigate to: `https://localhost:5001` or `http://localhost:5000`

### Step 4: Start Chatting!
1. Click the "Start Chat" button
2. Follow the conversation flow

## Test the Complete Flow

### Test User 1: John Doe
1. **Name**: `John Doe`
2. **Date of Birth**: `1990-05-15` or `15/05/1990`
3. Select a clinic (e.g., City General Hospital)
4. Choose option: `1` (view all doctors) or `2` (describe symptoms)
5. If symptoms, try: `headache and fever`
6. Select a doctor
7. Pick a time slot
8. ✅ Appointment confirmed!

### Test User 2: Jane Smith
1. **Name**: `Jane Smith`
2. **Date of Birth**: `1985-08-20` or `20/08/1985`
3. Select: Heart Care Center
4. Choose: `2` (symptoms)
5. Describe: `chest pain`
6. Select: Dr. Robert Chen (Cardiologist)
7. Pick a slot
8. ✅ Done!

### Test User 3: Michael Johnson
1. **Name**: `Michael Johnson`
2. **Date of Birth**: `1992-03-10` or `10/03/1992`
3. Select: Pediatric Clinic
4. Choose: `1` (view all doctors)
5. Select: Dr. Emily Brown
6. Pick a slot
7. ✅ Booking confirmed!

## API Testing with Swagger

Visit: `https://localhost:5001/swagger`

### Test Sequence:

#### 1. Start Session
```
POST /api/chatbot/start
Response: { "sessionId": "guid-here" }
```

#### 2. Send First Message (Gets Welcome)
```
POST /api/chatbot/message
Body:
{
  "sessionId": "your-session-id",
  "message": ""
}
```

#### 3. Provide Name
```
POST /api/chatbot/message
Body:
{
  "sessionId": "your-session-id",
  "message": "John Doe"
}
```

#### 4. Provide DOB
```
POST /api/chatbot/message
Body:
{
  "sessionId": "your-session-id",
  "message": "1990-05-15"
}
```

#### 5. Select Clinic (use ID from options)
```
POST /api/chatbot/message
Body:
{
  "sessionId": "your-session-id",
  "message": "c1111111-1111-1111-1111-111111111111"
}
```

...and so on!

## Direct API Endpoints

### Get All Clinics
```bash
curl https://localhost:5001/api/clinics
```

### Authenticate Patient
```bash
curl -X POST https://localhost:5001/api/patients/authenticate \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","dateOfBirth":"1990-05-15"}'
```

### Get Doctors by Clinic
```bash
curl https://localhost:5001/api/doctors/clinic/c1111111-1111-1111-1111-111111111111
```

### Get Doctors by Symptoms
```bash
curl "https://localhost:5001/api/doctors/symptoms?symptoms=headache"
```

### Get Available Slots
```bash
curl "https://localhost:5001/api/appointments/slots/d1111111-1111-1111-1111-111111111111?date=2025-12-24"
```

### Book Appointment
```bash
curl -X POST https://localhost:5001/api/appointments/book \
  -H "Content-Type: application/json" \
  -d '{
    "patientId": "11111111-1111-1111-1111-111111111111",
    "doctorId": "d1111111-1111-1111-1111-111111111111",
    "clinicId": "c1111111-1111-1111-1111-111111111111",
    "appointmentDate": "2025-12-24",
    "slotId": "slot1",
    "reason": "General checkup"
  }'
```

## Common Issues & Solutions

### Issue: Port already in use
**Solution**: Change port in Properties/launchSettings.json or run:
```bash
dotnet run --urls "http://localhost:5500;https://localhost:5501"
```

### Issue: CORS error
**Solution**: Already configured for development. For production, update CORS policy in Program.cs

### Issue: Session not found
**Solution**: Sessions are in-memory. If app restarts, start a new chat session.

### Issue: Patient not found
**Solution**: Use one of the test patients listed above with exact name and DOB.

## Project Structure Quick Reference

```
HospitalChatbot/
├── HospitalChatbot.Domain/          # Entities (Patient, Doctor, Appointment)
├── HospitalChatbot.Application/     # Business Logic (ChatbotService)
├── HospitalChatbot.Infrastructure/  # Data Services (Dummy implementations)
└── HospitalChatbot.API/             # Controllers + Web UI
    └── wwwroot/                     # HTML/CSS/JS
```

## Development Tips

### Hot Reload
```bash
dotnet watch run
```

### View Logs
Console shows all HTTP requests and responses

### Debugging
1. Open in Visual Studio or VS Code
2. Set breakpoints in ChatbotService.cs
3. Press F5

## Next Steps

1. ✅ Explore the web UI
2. ✅ Test with Swagger
3. ✅ Review the code architecture
4. ✅ Read ARCHITECTURE.md for deep dive
5. ✅ Modify dummy data in Infrastructure services
6. ✅ Add new conversation states
7. ✅ Integrate with real database

## Support

For issues or questions:
- Check README.md for detailed documentation
- Review ARCHITECTURE.md for design details
- Inspect browser console for frontend errors
- Check API console for backend logs

Happy coding! 🎉

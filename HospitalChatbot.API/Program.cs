using HospitalChatbot.Application.Interfaces;
using HospitalChatbot.Application.Services;
using HospitalChatbot.Infrastructure.Services;
using HospitalChatbot.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        options.ListenAnyIP(int.Parse(port));
    });
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MongoDB Context
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddHttpClient();

// Configure HttpClient for all services that need external API calls
builder.Services.AddHttpClient<IPatientService, PatientService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "HospitalChatbot/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IClinicService, ClinicService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "HospitalChatbot/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IDoctorService, DoctorService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "HospitalChatbot/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IAppointmentService, AppointmentService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "HospitalChatbot/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IConversationLogService, ConversationLogService>();
builder.Services.AddScoped<IHandleCancellationListAsync, HandleCancellationListAsyncService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();

app.MapFallbackToFile("index.html");

app.Run();

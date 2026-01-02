using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalChatbot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;

    public ChatbotController(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartSession()
    {
        var sessionId = await _chatbotService.StartNewSessionAsync();
        return Ok(new { SessionId = sessionId });
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
    {
        var response = await _chatbotService.ProcessMessageAsync(request);
        return Ok(response);
    }
}

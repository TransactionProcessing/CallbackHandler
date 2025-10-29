using System;
using Microsoft.AspNetCore.Mvc;

namespace CallbackHandler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public DiagnosticsController(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    [HttpGet("verify")]
    public IActionResult Verify()
    {
        // If this executes, DI and middleware pipelines are valid.
        return this.Ok(new
        {
            Status = "OK",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        });
    }
}
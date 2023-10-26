using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }
    
    
    [HttpPost]
    [Route("uploadWordFile")]
    public void PostFile(IFormFile uploadedFile)
    {
        string readText = WordFileExtractor.UploadAndExtractWordFile(uploadedFile);
        Console.WriteLine("\"Original\":   " + readText);


        var anonimizator = new TextAnonimizator();
        string processedText = anonimizator.Anonimize(readText);
        Console.WriteLine("\"Anonymized\": " + processedText);
    }
}
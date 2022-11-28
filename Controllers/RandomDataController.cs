using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using RandomDataAppApi.Models;
using RandomDataAppApi.Services;
using System.Text;

namespace RandomDataAppApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RandomDataController : ControllerBase
{
    private readonly RandomDataService _randomDataService;

    public RandomDataController(RandomDataService randomDataService) =>
        _randomDataService = randomDataService;

    [HttpGet(Name = "GetRandomData")]
    public async Task<List<RandomUserData>> Get([FromQuery] DataQueryDTO query)
    {
        List<RandomUserData> data = await _randomDataService.GetRandomUserData(query);
        return data;
    }

    [HttpPost("ExportToCSV")]
    public ActionResult ExportToCSV(List<RandomUserData> randomUserData)
    {
        string csvData = _randomDataService.RandomUserDataToCSV(randomUserData);
        return File(Encoding.UTF8.GetBytes(csvData), "text/csv");
    }
}

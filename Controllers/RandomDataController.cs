using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using RandomDataAppApi.Models;

namespace RandomDataAppApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RandomDataController : ControllerBase
{
  private static readonly string[] Summaries = new[]
  {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

  // private readonly ILogger _logger;

  // public RandomDataController(ILogger logger)
  // {
  //   _logger = logger;
  // }

  public class Element
  {
    [JsonPropertyName("group")]
    public string Group { get; set; }
    [JsonPropertyName("position")]
    public int Position { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("small")]
    public string Sign { get; set; }
    [JsonPropertyName("molar")]
    public double Molar { get; set; }
    [JsonPropertyName("electrons")]
    public IList<int> Electrons { get; set; }
  }


  [HttpGet(Name = "GetRandomData")]
  public async Task<ActionResult<List<Element>>> GetAboba([FromQuery] DataQueryDTO query)
  {
    using (StreamReader stream = new StreamReader("./Controllers/aboba.json"))
    {
      string json = stream.ReadToEnd();
      var list = JsonSerializer.Deserialize<List<Element>>(json);
      var chunks = list.Chunk(query.Limit).ToList();

      Console.WriteLine(query.Limit);
      Console.WriteLine(query.Page);
      Console.WriteLine(query.Region);
      Console.WriteLine(query.ErrorsCount);
      Console.WriteLine(query.Seed);


      Console.WriteLine(chunks[0]);

      IList<Element> result = new List<Element>();

      for (int i = 0; i < query.Page; i += 1)
      {
        result = result.Concat(chunks[i]).ToList();
      }

      return result.ToList();

    }

  }
}

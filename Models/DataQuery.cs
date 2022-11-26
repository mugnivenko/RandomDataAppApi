namespace RandomDataAppApi.Models;

public class DataQueryDTO
{
  public int Page { get; set; }
  public int Limit { get; set; }
  public string Region { get; set; } = default!;
  public double ErrorsCount { get; set; }
  public int Seed { get; set; }
}
using System.Text.Json.Serialization;

namespace RandomDataAppApi.Models;

public class FirstNames
{
    [JsonPropertyName("male")]
    public string[]? Male { get; set; }

    [JsonPropertyName("female")]
    public string[]? Female { get; set; }
}

public class Phone
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("areaCodes")]
    public string[]? AreaCodes { get; set; }

    [JsonPropertyName("lastNumbersMask")]
    public string? LastNumbersMask { get; set; }
}

public class PostalCode
{
    [JsonPropertyName("from")]
    public int From { get; set; }

    [JsonPropertyName("to")]
    public int To { get; set; }
}

public abstract class JsonFileData
{
    [JsonPropertyName("alphabet")]
    public string[]? Alphabet { get; set; }

    [JsonPropertyName("lastNames")]
    public string[]? LastNames { get; set; }

    [JsonPropertyName("firstNames")]
    public FirstNames? FirstNames { get; set; }

    [JsonPropertyName("phone")]
    public Phone? Phone { get; set; }
}

public class USAJsonFileData : JsonFileData
{
    public class USAAddress
    {
        [JsonPropertyName("street")]
        public string[]? Street { get; set; }

        [JsonPropertyName("city")]
        public string[]? City { get; set; }

        [JsonPropertyName("region")]
        public string[]? Region { get; set; }

        [JsonPropertyName("zipCode")]
        public PostalCode? ZipCode { get; set; }
    }

    [JsonPropertyName("address")]
    public USAAddress? Address { get; set; }

}

public class SpainJsonFileData : JsonFileData
{
    public class RegionWithPostalCodePrefix
    {
        [JsonPropertyName("postalCodePrefix")]
        public string? PostalCodePrefix { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class SpainAddress
    {
        [JsonPropertyName("street")]
        public string[]? Street { get; set; }

        [JsonPropertyName("locality")]
        public string[]? Locality { get; set; }

        [JsonPropertyName("lobuildingcality")]
        public string[]? Building { get; set; }

        [JsonPropertyName("region")]
        public RegionWithPostalCodePrefix[]? Region { get; set; }
    }

    [JsonPropertyName("address")]
    public SpainAddress? Address { get; set; }
}

public class SwedenJsonFileData : JsonFileData
{
    public class SwedenAddress
    {
        [JsonPropertyName("street")]
        public string[]? Street { get; set; }

        [JsonPropertyName("urbanAreaName")]
        public string[]? UrbanAreaName { get; set; }

        [JsonPropertyName("urbanAreaType")]
        public string[]? UrbanAreaType { get; set; }

        [JsonPropertyName("postalCode")]
        public PostalCode? PostalCode { get; set; }

        [JsonPropertyName("municipality")]
        public string[]? Municipality { get; set; }

        [JsonPropertyName("landskap")]
        public string[]? Landskap { get; set; }
    }

    [JsonPropertyName("address")]
    public SwedenAddress? Address { get; set; }
}

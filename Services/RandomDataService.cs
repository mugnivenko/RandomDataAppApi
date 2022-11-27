using CsvHelper;
using Newtonsoft.Json;
using RandomDataAppApi.Models;

namespace RandomDataAppApi.Services;

public class RandomDataService
{

    private async Task<T> ReadFile<T>(string fileName)
    {
        using (StreamReader reader = new StreamReader($"./data/{fileName}.json"))
        {
            string json = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    private Guid GenerateGuid(Random rnd)
    {
        byte[] bytes = new Byte[16];
        rnd.NextBytes(bytes);
        return new Guid(bytes);
    }

    private string GenerateFullName(
        Random rnd,
        string[] lastNames,
        FirstNames firstNames)
    {
        string lastName = lastNames[rnd.Next(lastNames.Length)];
        string firstName = rnd.Next(2) switch
        {
            0 => firstNames.Female[rnd.Next(firstNames.Female.Length)],
            1 => firstNames.Male[rnd.Next(firstNames.Male.Length)],
            _ => string.Empty,
        };
        return $"{firstName} {lastName}";
    }

    private string GeneratePhone(Random rnd, Phone phone, int maxValue)
    {
        string areaCode = phone.AreaCodes[rnd.Next(phone.AreaCodes.Length)];
        int aboba = rnd.Next(maxValue);
        string a = aboba.ToString(phone.LastNumbersMask);
        return $"{phone.Code} {areaCode} {a}";
    }

    private string DeleteRandomSymbol(Random rnd, string value) =>
        value.Remove(rnd.Next(value.Length), 1);


    private string AddRandomSymbol(Random rnd, string value, string[] alphabet) =>
        value.Insert(rnd.Next(value.Length), alphabet[rnd.Next(alphabet.Length)]);

    private string SwipeNeighborhoodSymbols(Random rnd, string value)
    {
        char randomChar = value[rnd.Next(value.Length)];
        int index = value.IndexOf(randomChar);
        if (index == value.Length - 1)
        {
            char firstChar = value.First();
            char lastChar = value.Last();
            return $"{lastChar}{value.Substring(1, value.Length - 1)}{firstChar}";
        }
        string valueWithoutChar = value.Remove(index, 1);
        return valueWithoutChar.Insert(index + 1, randomChar.ToString());
    }

    private string GenerateErrorByType(Random rnd, string value, int initialLength, string[] alphabet)
    {
        if (value.Length <= initialLength * 0.5)
        {
            return AddRandomSymbol(rnd, value, alphabet);
        }
        if (value.Length >= initialLength * 1.5)
        {
            return DeleteRandomSymbol(rnd, value);
        }
        return rnd.Next(3) switch
        {
            0 => SwipeNeighborhoodSymbols(rnd, value),
            1 => AddRandomSymbol(rnd, value, alphabet),
            2 => DeleteRandomSymbol(rnd, value),
            _ => value,
        };

    }


    private RandomUserData GenerateErrors(
        RandomUserData randomUserData,
        Random rnd,
        double errorsCount,
        string[] alphabet)
    {
        int neededErrorsCount = (int)Math.Truncate(errorsCount);
        double decimalPart = errorsCount - Math.Truncate(errorsCount);
        if (rnd.Next(0, 101) <= decimalPart * 100)
        {
            neededErrorsCount += 1;
        }
        foreach (var _ in Enumerable.Range(0, neededErrorsCount))
        {
            int value = rnd.Next(3);

            int initialAddressLength = randomUserData.Address.Length;
            int initialFullNameLength = randomUserData.FullName.Length;
            int initialPhoneLength = randomUserData.Phone.Length;


            if (value == 0)
            {
                randomUserData.Address = GenerateErrorByType(rnd, randomUserData.Address, initialAddressLength, alphabet);
                continue;
            }

            if (value == 1)
            {
                randomUserData.FullName = GenerateErrorByType(rnd, randomUserData.FullName, initialFullNameLength, alphabet);
                continue;
            }

            if (value == 2)
            {
                randomUserData.Phone = GenerateErrorByType(rnd, randomUserData.Phone, initialPhoneLength, alphabet);
                continue;
            }
        }

        return randomUserData;
    }


    private List<RandomUserData> GenerateData<T>(
        DataQueryDTO query,
        T fileData,
        Func<Random, string> addressFn,
        int phoneMaxValue) where T : JsonFileData
    {
        List<RandomUserData> generatedUsers = new List<RandomUserData>();

        foreach (var num in Enumerable.Range(1, query.Limit))
        {
            int number = num + (query.Page * query.Limit);
            Random rnd = new Random(number + query.Seed);

            RandomUserData userData = new RandomUserData()
            {
                Number = number,
                Id = GenerateGuid(rnd),
                FullName = GenerateFullName(rnd, fileData.LastNames, fileData.FirstNames),
                Address = addressFn(rnd),
                Phone = GeneratePhone(rnd, fileData.Phone, phoneMaxValue)
            };

            RandomUserData userDataWithErrors = GenerateErrors(userData, rnd, query.ErrorsCount, fileData.Alphabet);

            generatedUsers.Add(userDataWithErrors);
        }

        return generatedUsers;
    }

    private (int house, int apartment) GetUSAAddressAttributes(Random rnd, PostalCode code)
    {
        int house = rnd.Next(501);
        int apartment = rnd.Next(251);
        return (house, apartment);
    }

    private string GenerateCommonUSAAddressPart(Random rnd, USAJsonFileData.USAAddress address)
    {
        string city = address.City[rnd.Next(address.City.Length)];
        var region = address.Region[rnd.Next(address.Region.Length)];
        int zipCode = rnd.Next(address.ZipCode.From - 1, address.ZipCode.To + 1);
        return $"{city}, {region} {zipCode}";
    }

    private Func<Random, string> GenerateUSAAddress(USAJsonFileData.USAAddress address) => (Random rnd) =>
        {
            string commonPart = GenerateCommonUSAAddressPart(rnd, address);
            string street = address.Street[rnd.Next(address.Street.Length)];
            var (house, apartment) = GetUSAAddressAttributes(rnd, address.ZipCode);
            return rnd.Next(2) switch
            {
                0 => $"{house} {street} Apartment #{apartment}, {commonPart}",
                1 => $"{house} {street} {commonPart}",
                _ => string.Empty,
            };
        };

    private async Task<List<RandomUserData>> GetUSAData(DataQueryDTO query)
    {
        var fileData = await ReadFile<USAJsonFileData>(query.Region);
        return GenerateData<USAJsonFileData>(query, fileData, GenerateUSAAddress(fileData.Address), 10000000);
    }

    private (int premise, int floor, int stairwell, string door) GetSpainAddressAttributes(Random rnd)
    {
        int premise = rnd.Next(51);
        int floor = rnd.Next(6);
        int stairwell = rnd.Next(11);
        int doorNumber = rnd.Next(1, 11);
        char doorLetter = (char)('A' + doorNumber);
        string door = rnd.Next(2) == 0 ? $"{doorNumber}{doorLetter}" : $"{doorLetter}";
        return (premise, floor, stairwell, door);
    }

    private string GenerateCommonSpainAddressPart(Random rnd, SpainJsonFileData.SpainAddress address)
    {
        string locality = address.Locality[rnd.Next(address.Locality.Length)];
        var regionWithPostalCodePrefix = address.Region[rnd.Next(address.Region.Length)];
        string postalCode = $"{regionWithPostalCodePrefix.PostalCodePrefix}{rnd.Next(1000)}";
        return $"{postalCode} {locality}, {regionWithPostalCodePrefix.Name}";
    }

    private Func<Random, string> GenerateSpainAddress(SpainJsonFileData.SpainAddress address) => (Random rnd) =>
    {
        string commonPart = GenerateCommonSpainAddressPart(rnd, address);
        string street = address.Street[rnd.Next(address.Street.Length)];
        string building = address.Building[rnd.Next(address.Building.Length)];
        var (premise, floor, stairwell, door) = GetSpainAddressAttributes(rnd);
        return rnd.Next(3) switch
        {
            0 => $"{street} {premise}, Esc {stairwell}, {floor}º, {door}, {commonPart}",
            1 => $"{building}, {street} {premise}, {floor}º, {door}, {commonPart}",
            2 => $"{building} {commonPart}",
            _ => string.Empty,
        };
    };

    private async Task<List<RandomUserData>> GetSpainData(DataQueryDTO query)
    {
        var fileData = await ReadFile<SpainJsonFileData>(query.Region);
        return GenerateData<SpainJsonFileData>(query, fileData, GenerateSpainAddress(fileData.Address), 100000000);
    }

    private (int house, int postalCode) GetSwedenAddressAttributes(Random rnd, PostalCode code)
    {
        int house = rnd.Next(101);
        int postalCode = rnd.Next(code.From - 1, code.To + 1);
        return (house, postalCode);
    }

    private Func<Random, string> GenerateSwedenAddress(SwedenJsonFileData.SwedenAddress address) => (Random rnd) =>
    {
        string street = address.Street[rnd.Next(address.Street.Length)];
        string area = address.UrbanAreaName[rnd.Next(address.UrbanAreaName.Length)];
        string areaType = address.UrbanAreaType[rnd.Next(address.UrbanAreaType.Length)];
        string landskap = address.Landskap[rnd.Next(address.Landskap.Length)];
        string municipality = address.Municipality[rnd.Next(address.Municipality.Length)];
        var (house, postalCode) = GetSwedenAddressAttributes(rnd, address.PostalCode);
        return rnd.Next(2) switch
        {
            0 => $"{street} {house}, {postalCode} {area} {municipality} kommun",
            1 => $"{house} {street}, {postalCode} {areaType} {area}, {landskap} län {municipality} kommun",
            _ => string.Empty,
        };
    };
    private async Task<List<RandomUserData>> GetSwedenData(DataQueryDTO query)
    {
        var fileData = await ReadFile<SwedenJsonFileData>(query.Region);
        return GenerateData<SwedenJsonFileData>(query, fileData, GenerateSwedenAddress(fileData.Address), 10000000);
    }

    public async Task<List<RandomUserData>> GetRandomUserData(DataQueryDTO query) =>
        query.Region switch
        {
            "USA" => await GetUSAData(query),
            "Spain" => await GetSpainData(query),
            "Sweden" => await GetSwedenData(query),
            _ => throw new ArgumentException("Invalid value for command", nameof(query.Region)),
        };

    public string RandomUserDataToCSV(List<RandomUserData> randomUserData)
    {
        using (TextWriter writer = new StringWriter())
        {
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture))
            {
                csv.WriteRecords(randomUserData);
            }

            return writer.ToString();
        }
    }

}
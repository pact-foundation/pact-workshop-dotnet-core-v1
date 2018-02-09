using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            string dateTimeToValidate = "05/01/2018";
            string baseUri = "http://localhost:9000";

            if(args.Length <= 1)
            {
                Console.WriteLine("-------------------");
                Console.WriteLine($"Running consumer with args: dateTimeToValidate = {dateTimeToValidate}, baseUri = {baseUri}");
                Console.WriteLine("To use with your own parameters:");
                WriteoutUsageInstructions();
                Console.WriteLine("-------------------");
            }
            else
            {
                dateTimeToValidate = args[0];
                baseUri = args[1];
                Console.WriteLine("-------------------");
                Console.WriteLine($"Running consumer with args: dateTimeToValidate = {dateTimeToValidate}, baseUri = {baseUri}");
                Console.WriteLine("-------------------");
            }

            Console.WriteLine("Validating date...");
            ValidateDateTimeUsingProviderApi(dateTimeToValidate, baseUri).GetAwaiter().GetResult();
            Console.WriteLine("...Date validation complete. Goodbye.");
        }

        static private async Task ValidateDateTimeUsingProviderApi(string dateTimeToValidate, string baseUri)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(baseUri)})
            {
                try
                {
                    string response = await client.GetStringAsync($"/api/provider?validDateTime={dateTimeToValidate}");
                    Console.WriteLine(response);
                }
                catch (System.Exception ex)
                {
                    throw new Exception("There was a problem connecting to Provider API.", ex);
                }
            }
        }

        static private void WriteoutUsageInstructions()
        {
            Console.WriteLine("Usage: dotnet run [DateTime To Validate] [Provider Api Uri]");
            Console.WriteLine("Usage Example: dotnet run 01/01/2018 http://localhost:9000");
        }
    }
}

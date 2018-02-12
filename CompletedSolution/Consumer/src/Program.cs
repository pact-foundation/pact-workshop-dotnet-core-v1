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
                WriteoutArgsUsed(dateTimeToValidate, baseUri);
                WriteoutUsageInstructions();
                Console.WriteLine("-------------------");
            }
            else
            {
                dateTimeToValidate = args[0];
                baseUri = args[1];
                Console.WriteLine("-------------------");
                WriteoutArgsUsed(dateTimeToValidate, baseUri);
                Console.WriteLine("-------------------");
            }

            Console.WriteLine("Validating date...");
            var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi(dateTimeToValidate, baseUri).GetAwaiter().GetResult();
            var resultContentText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine(resultContentText);
            Console.WriteLine("...Date validation complete. Goodbye.");
        }

        static private void WriteoutArgsUsed(string datetimeArg, string baseUriArg)
        {
            Console.WriteLine($"Running consumer with args: dateTimeToValidate = {datetimeArg}, baseUri = {baseUriArg}");
        }

        static private void WriteoutUsageInstructions()
        {
            Console.WriteLine("To use with your own parameters:");
            Console.WriteLine("Usage: dotnet run [DateTime To Validate] [Provider Api Uri]");
            Console.WriteLine("Usage Example: dotnet run 01/01/2018 http://localhost:9000");
        }
    }
}

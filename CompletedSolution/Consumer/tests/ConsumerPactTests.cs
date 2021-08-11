using System;
using System.Net;
using System.Net.Http;
using Xunit;
using Consumer;
using PactNet;
using PactNet.Native;
using Xunit.Abstractions;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace tests
{
    public class ConsumerPactTests
    {
        private IPactBuilderV3 pact;

        public ConsumerPactTests(ITestOutputHelper output)
        {
            var Config = new PactConfig
            {
                PactDir = @"..\..\..\..\..\pacts",
                LogDir = @".\pact_logs",
                Outputters = new[] { new XUnitOutput(output) },
                DefaultJsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };

            pact = Pact.V3("Consumer", "Provider", Config).UsingNativeBackend();
        }

        [Fact]
        public async void ItHandlesInvalidDateParam()
        {
            // Arange
            var invalidRequestMessage = "validDateTime is not a date or time";
            pact.UponReceiving("A invalid GET request for Date Validation with invalid date parameter")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Get, "/api/provider")
                    .WithQuery("validDateTime", "lolz")
                .WillRespond()
                    .WithStatus(HttpStatusCode.BadRequest)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new { message = invalidRequestMessage });

            // Act & Assert
            await pact.VerifyAsync(async ctx => {
                var response = await ConsumerApiClient.ValidateDateTimeUsingProviderApi("lolz", ctx.MockServerUri);
                var body = await response.Content.ReadAsStringAsync();
                Assert.Contains(invalidRequestMessage, body);
            });
        }

        [Fact]
        public async void ItHandlesEmptyDateParam()
        {
            // Arrange
            var invalidRequestMessage = "validDateTime is required";
            pact.UponReceiving("A invalid GET request for Date Validation with empty string date parameter")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Get, "/api/provider")
                    .WithQuery("validDateTime", "")
                .WillRespond()
                    .WithStatus(HttpStatusCode.BadRequest)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new { message = invalidRequestMessage });

            // Act & Assert
            await pact.VerifyAsync(async ctx => {
                var response = await ConsumerApiClient.ValidateDateTimeUsingProviderApi(String.Empty, ctx.MockServerUri);
                var body = await response.Content.ReadAsStringAsync();
                Assert.Contains(invalidRequestMessage, body);
            });
        }

        [Fact]
        public async void ItHandlesNoData()
        {
            var validDate = "04/04/2018";

            pact.UponReceiving("A valid GET request for Date Validation")
                .Given("There is no data")
                .WithRequest(HttpMethod.Get, "/api/provider")
                .WithQuery("validDateTime", validDate)
            .WillRespond()
                .WithStatus(HttpStatusCode.NotFound);

            // Act & Assert
            await pact.VerifyAsync(async ctx => {
                var response = await ConsumerApiClient.ValidateDateTimeUsingProviderApi(validDate, ctx.MockServerUri);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async void ItParsesADateCorrectly()
        {
            var expectedDateString = "04/05/2018";
            var expectedDateParsed = DateTime.Parse(expectedDateString).ToString("dd-MM-yyyy HH:mm:ss");

            pact.UponReceiving("A valid GET request for Date Validation")
                .Given("There is data")
                .WithRequest(HttpMethod.Get, "/api/provider")
                .WithQuery("validDateTime", expectedDateString)
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new { test = "NO", validDateTime = expectedDateParsed });

            // Act & Assert
            await pact.VerifyAsync(async ctx => {
                var response = await ConsumerApiClient.ValidateDateTimeUsingProviderApi(expectedDateString, ctx.MockServerUri);
                var body = await response.Content.ReadAsStringAsync();
                Assert.Contains(expectedDateParsed, body);
            });
        }
    }
}

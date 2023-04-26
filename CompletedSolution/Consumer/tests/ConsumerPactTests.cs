using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using Consumer;
using System.Collections.Generic;
using PactNet;
using PactNet.Matchers;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace tests
{
    public class ConsumerPactTests
    {
        private IPactBuilderV3 pact;

        public ConsumerPactTests(ITestOutputHelper output)
        {
            var Config = new PactConfig
            {
                PactDir = Path.Join("..", "..", "..", "..", "..", "pacts"),
                Outputters = new[] { new XunitOutput(output) },
                LogLevel = PactLogLevel.Debug
            };

            pact = Pact.V3("Consumer", "Provider", Config).WithHttpInteractions();
        }

        [Fact]
        public async Task ItHandlesInvalidDateParam()
        {
            // Arrange
            var invalidRequestMessage = "validDateTime is not a date or time";
            pact.UponReceiving("A invalid GET request for Date Validation with invalid date parameter")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Get, "/api/provider")
                    .WithQuery("validDateTime", "lolz")
                .WillRespond()
                    .WithStatus(HttpStatusCode.BadRequest)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new { message = invalidRequestMessage });

            // Act
            await pact.VerifyAsync(async ctx =>
            {
                var result = await ConsumerApiClient.ValidateDateTimeUsingProviderApi("lolz", ctx.MockServerUri.ToString());
                Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                Assert.Contains(invalidRequestMessage, result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            });
        }

        [Fact]
        public async Task ItHandlesEmptyDateParam()
        {

            // Arrange
            var invalidRequestMessage = "validDateTime is required";
            pact.UponReceiving("A invalid GET request for Date Validation with empty string date parameter")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Get, "/api/provider")
                    .WithQuery("validDateTime", String.Empty)
                .WillRespond()
                    .WithStatus(HttpStatusCode.BadRequest)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new { message = invalidRequestMessage });

            // Act
            await pact.VerifyAsync(async ctx =>
            {
                var result = await ConsumerApiClient.ValidateDateTimeUsingProviderApi(String.Empty, ctx.MockServerUri.ToString());
                Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                Assert.Contains(invalidRequestMessage, result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            });
        }

        [Fact]
        public async Task ItHandlesNoData()
        {

            // Arrange
            pact.UponReceiving("A valid GET request for Date Validation")
                    .Given("There is no data")
                    .WithRequest(HttpMethod.Get, "/api/provider")
                    .WithQuery("validDateTime", "04/04/2018")
                .WillRespond()
                    .WithStatus(HttpStatusCode.NotFound);

            // Act
            await pact.VerifyAsync(async ctx =>
            {
                var result = await ConsumerApiClient.ValidateDateTimeUsingProviderApi("04/04/2018", ctx.MockServerUri.ToString());
                Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            });
        }

        [Fact]
        public async Task ItParsesADateCorrectly()
        {
            var expectedDateString = "04/05/2018";
            var expectedDateParsed = DateTime.Parse(expectedDateString).ToString("dd-MM-yyyy HH:mm:ss");

            // Arrange
            pact.UponReceiving("A valid GET request for Date Validation")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Get, "/api/provider")
                    .WithQuery("validDateTime", expectedDateString)
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new
                    {
                        test = "NO",
                        validDateTime = expectedDateParsed
                    });
            // Act
            await pact.VerifyAsync(async ctx =>
            {
                var result = await ConsumerApiClient.ValidateDateTimeUsingProviderApi(expectedDateString, ctx.MockServerUri.ToString());
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                Assert.Contains(expectedDateParsed, result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            });
        }
    }
}

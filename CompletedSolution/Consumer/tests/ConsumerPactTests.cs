using System;
using Xunit;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using Consumer;
using System.Collections.Generic;

namespace tests
{
    public class ConsumerPactTests : IClassFixture<ConsumerPactClassFixture>
    {
        private IMockProviderService _mockProviderService;
        private string _mockProviderServiceBaseUri;

        public ConsumerPactTests(ConsumerPactClassFixture fixture)
        {
            _mockProviderService = fixture.MockProviderService;
            _mockProviderService.ClearInteractions(); //NOTE: Clears any previously registered interactions before the test is run
            _mockProviderServiceBaseUri = fixture.MockProviderServiceBaseUri;
        }

        [Fact]
        public void ItHandlesInvalidDateParam()
        {
            // Arange
            var invalidRequestMessage = "validDateTime is not a date or time";
            _mockProviderService.Given("There is data")
                                .UponReceiving("A invalid GET request for Date Validation with invalid date parameter")
                                .With(new ProviderServiceRequest 
                                {
                                    Method = HttpVerb.Get,
                                    Path = "/api/provider",
                                    Query = "validDateTime=lolz"
                                })
                                .WillRespondWith(new ProviderServiceResponse {
                                    Status = 400,
                                    Headers = new Dictionary<string, object>
                                    {
                                        { "Content-Type", "application/json; charset=utf-8" }
                                    },
                                    Body = new 
                                    {
                                        message = invalidRequestMessage
                                    }
                                });

            // Act
            var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi("lolz", _mockProviderServiceBaseUri).GetAwaiter().GetResult();
            var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Assert
            Assert.Contains(invalidRequestMessage, resultBodyText);
        }

        [Fact]
        public void ItHandlesEmptyDateParam()
        {
            // Arrange
            var invalidRequestMessage = "validDateTime is required";
            _mockProviderService.Given("There is data")
                                .UponReceiving("A invalid GET request for Date Validation with empty string date parameter")
                                .With(new ProviderServiceRequest 
                                {
                                    Method = HttpVerb.Get,
                                    Path = "/api/provider",
                                    Query = "validDateTime="
                                })
                                .WillRespondWith(new ProviderServiceResponse {
                                    Status = 400,
                                    Headers = new Dictionary<string, object>
                                    {
                                        { "Content-Type", "application/json; charset=utf-8" }
                                    },
                                    Body = new 
                                    {
                                        message = invalidRequestMessage
                                    }
                                });

            // Act
            var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi(String.Empty, _mockProviderServiceBaseUri).GetAwaiter().GetResult();
            var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Assert
            Assert.Contains(invalidRequestMessage, resultBodyText);
        }

        [Fact]
        public void ItHandlesNoData()
        {
            // Arrange
            _mockProviderService.Given("There is no data")
                                .UponReceiving("A valid GET request for Date Validation")
                                .With(new ProviderServiceRequest 
                                {
                                    Method = HttpVerb.Get,
                                    Path = "/api/provider",
                                    Query = "validDateTime=04/04/2018"
                                })
                                .WillRespondWith(new ProviderServiceResponse {
                                    Status = 404
                                });

            // Act
            var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi("04/04/2018", _mockProviderServiceBaseUri).GetAwaiter().GetResult();
            var resultStatus = (int)result.StatusCode;

            // Assert
            Assert.Equal(404, resultStatus);
        }

        [Fact]
        public void ItParsesADateCorrectly()
        {
            var expectedDateString = "04/05/2018";
            var expectedDateParsed = DateTime.Parse(expectedDateString).ToString("dd-MM-yyyy HH:mm:ss");

            // Arrange
            _mockProviderService.Given("There is data")
                                .UponReceiving("A valid GET request for Date Validation")
                                .With(new ProviderServiceRequest 
                                {
                                    Method = HttpVerb.Get,
                                    Path = "/api/provider",
                                    Query = $"validDateTime={expectedDateString}"
                                })
                                .WillRespondWith(new ProviderServiceResponse {
                                    Status = 200,
                                    Headers = new Dictionary<string, object>
                                    {
                                        { "Content-Type", "application/json; charset=utf-8" }
                                    },
                                    Body = new 
                                    {
                                        test = "NO",
                                        validDateTime = expectedDateParsed
                                    }
                                });

            // Act
            var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi(expectedDateString, _mockProviderServiceBaseUri).GetAwaiter().GetResult();
            var resultBody = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Assert
            Assert.Contains(expectedDateParsed, resultBody);
        }
    }
}

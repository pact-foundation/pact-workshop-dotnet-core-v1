using System;
using Xunit;
using PactNet;
using PactNet.Mocks.MockHttpService;

namespace tests
{
    // This class is responsible for setting up a shared
    // mock server for Pact used by all the tests.
    // XUnit can use a Class Fixture for this.
    // See: https://goo.gl/hSq4nv
    public class ConsumerPactClassFixture : IDisposable
    {
        public IPactBuilder PactBuilder { get; private set; }
        public IMockProviderService MockProviderService { get; private set; }

        public int MockServerPort { get { return 9222; } }
        public string MockProviderServiceBaseUri { get { return String.Format("http://localhost:{0}", MockServerPort); } }

        public ConsumerPactClassFixture()
        {
            // Using Spec version 2.0.0 more details at https://goo.gl/UrBSRc
            PactBuilder = new PactBuilder(new PactConfig { SpecificationVersion = "2.0.0", PactDir = @"..\pacts", LogDir = @"c:\temp\logs" });

            PactBuilder.ServiceConsumer("Consumer")
                       .HasPactWith("Provider");

            MockProviderService = PactBuilder.MockService(MockServerPort);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // This will save the pact file once finished.
                    PactBuilder.Build();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
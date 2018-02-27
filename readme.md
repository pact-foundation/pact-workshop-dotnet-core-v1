# Example .NET Core Project for Pact Workshop

When writing a lot of small services, testing the interactions between these becomes a major headache.
That's the problem Pact is trying to solve.

Integration tests typically are slow and brittle, requiring each component to have its own environment to run the tests in.
With a micro-service architecture, this becomes even more of a problem. They also have to be 'all-knowing' and this makes them
difficult to keep from being fragile.

After J. B. Rainsberger's talk [Integrated Tests Are A Scam](https://www.youtube.com/watch?v=VDfX44fZoMc) people have been thinking
how to get the confidence we need to deploy our software to production without having a tiresome integration test suite that does
not give us all the coverage we think it does.

PactNet is a .NET implementation of Pact that allows you to define a pact between service consumers and providers. It provides a DSL for
service consumers to define the request they will make to a service producer and the response they expect back. This expectation is
used in the consumer's specs to provide a mock producer and is also played back in the producer specs to ensure the producer actually
does provide the response that the consumer expects.

This allows you to test both sides of an integration point using fast unit tests.

# Prerequisites

This workshop while written with .NET Core is not specifically about it so in-depth knowledge of .NET Core is not required if you can
write code in any other language you should be fine.

However before taking part in this workshop please make sure you have:

* [.NET Core SDK](https://www.microsoft.com/net/download/)
* An account at [Github.com](www.github.com)!
* A text editor/IDE that supports .NET Core. Check out [VSCode](https://code.visualstudio.com/)

# Workshop Steps

## Step 1 - Fork the Repo & Explore the Code!

Create a fork of [pact-workshop-dotnet-core-v1](https://github.com/tdshipley/pact-workshop-dotnet-core-v1) and familiarise yourself with
its contents. There are two main folders to be aware of:

### CompletedSolution

This folder contains a complete sample solution for the workshop so if you get stuck at any point or are unsure what to do next take a look
in here and you will see all the completed code for guidance.

Within the folder is a Consumer project in the **Consumer/src** folder which is a simple .NET Core console application that connects to the
Provider project which is in the **Provider/src** folder and is an ASP.NET Core Web API. Both projects also have a **tests/** folder which
is where the [Pact](https://docs.pact.io/) tests for both projects exist.

### YourSolution

This folder follows the same structure as the *CompletedSolution/* folder except for the *tests/* folders are empty! During this workshop you
will be creating the test projects using [Pact](https://docs.pact.io/) to test both the *Consumer* project and the *Provider* project.

## Step 2 - Understanding The Consumer Project

The *Consumer* is a .NET Core console application which validates date & time strings by making requests to our *Provider* API. Take a look
at the code. You might notice before we can run the project successfully we need the Provider API running locally.

### Step 2.1 - Start the Provider API Locally

Using the command line navigate to:

```
[RepositoryRoot]/YourSolution/Provider/src/
```

Once in the Provider */src/* directory first do a ```dotnet restore``` at the command line to pull down the dependencies required for the project.
Once that has completed run ```dotnet run``` this will start your the Provider API. Now check that everything is working O.K. by navigating to
the URL below in your browser:

```
http://localhost:9000/api/provider?validDateTime=05/01/2018
```

If your request is successful you should see in your browser:

```
{"test":"NO","validDateTime":"05-01-2018 00:00:00"}
```

If you see the above leave the Provider API running then you are ready to try out the consumer.

#### NB: Potential Error

If you get a **404** error check that the path ```[RepositoryRoot]/YourSolution/data``` exists with a text file in it called **somedata.txt** in it. We will
talk about this file later on.

### Step 2.2 - Execute the Consumer

With the Provider API running open another command line instance and navigate to:

```
[RepositoryRoot]/YourSolution/Consumer/src/
```

Once in the directory run another ```dotnet restore``` to pull down the dependencies for the Consumer project. Once this is completed at the command line
type in ```dotnet run``` you should see output:

```
MyPc:src thomas.shipley$ dotnet run
-------------------
Running consumer with args: dateTimeToValidate = 05/01/2018, baseUri = http://localhost:9000
To use with your own parameters:
Usage: dotnet run [DateTime To Validate] [Provider Api Uri]
Usage Example: dotnet run 01/01/2018 http://localhost:9000
-------------------
Validating date...
{"test":"NO","validDateTime":"05-01-2018 00:00:00"}
...Date validation complete. Goodbye.
```

If you see output similar to above in your command line then the consumer is now running successfully! If you want to now you can experiment with passing in
parameters different to the defaults.

## Step 3 - Testing the Consumer Project with Pact

Now we have tested the Provider API and Consumer run successfully on your machine we can start to create our Pact tests. Pact files are **Consumer Driven**
that is to say, they work by the *Consumer* defining in there Pact tests first what they expect from a provider which can be verified by the *Provider*.
So let's follow this convention and create our *Consumer* tests first.

### Step 3.1 - Creating a Test Project for Consumer with XUnit

Pact cannot execute tests on its own it needs a test runner project. For this workshop, we will be using [XUnit](https://xunit.github.io/) to create the project
navigate to ```[RepositoryRoot]/YourSolution/Consumer/tests``` and run:

```
dotnet new xunit
```

This will create an empty XUnit project with all the references you need... expect Pact. Depending on what OS you are completing this workshop on you will need
to run one of the following commands:

```
# Windows
dotnet add package PactNet.Windows --version 2.2.1

# OSX
dotnet add package PactNet.OSX --version 2.2.1

# Linux
dotnet add package PactNet.Linux.x64 --version 2.2.1
# Or...
dotnet add package PactNet.Linux.x86 --version 2.2.1
```

Finally you will need to add a reference to the Consumer Client project src code. So again
at the same command line type and run the command:

```
dotnet add reference ../src/consumer.csproj
```

This will allow you to access public code from the Consumer Client project which you will
need to do to test the code!

Once this command runs successfully you will have in ```[RepositoryRoot]/YourSolution/Consumer/tests``` an empty .NET Core XUnit Project with Pact
and we can begin to setup Pact!

#### NB - Multiple OS Environments

When using Pact tests for your production projects you might want to support multiple OSes. You can with .NET Core specify different packages in your
**.csproj** file based on the operating system but for the purpose of this workshop this is unnecessary. Other language implementations do not always
require OS based packages.

### Step 3.2 - Configuring the Mock HTTP Pact Server on the Consumer

Pact works by placing a mock HTTP server between the consumer and provider(s) in an application to handle mocked provider interactions on the consumer
side and replay this actions on the provider side to verify them. So before we can write Pact tests we need to setup and configure this mock server.
This server will be used for all the tests in our Consumer test project.

XUnit shares common resources in a few different ways. For this workshop, we shall create a [Class Fixture](https://xunit.github.io/docs/shared-context.html)
which will share our mock HTTP server between our consumer tests. Start by creating a file and class called ```ConsumerPactClassFixture.cs``` in the root of
the Consumer test project (```[RepositoryRoot]/YourSolution/Consumer/tests```). It should look like:

```csharp
using System;
using Xunit;

namespace tests
{
    // This class is responsible for setting up a shared
    // mock server for Pact used by all the tests.
    // XUnit can use a Class Fixture for this.
    // See: https://goo.gl/hSq4nv
    public class ConsumerPactClassFixture
    {
    }
}
```

#### Step 3.2.1 - Setup using PactBuilder

The [PactBuilder](https://github.com/pact-foundation/pact-net/blob/master/PactNet/PactBuilder.cs) is the class used to build out the configuration we
need for Pact which defines among other things where to find our mock HTTP server.

First, at the top of your class add some properties which will be used to store your instance of PactBuilder and store Mock HTTP Server properties:

```csharp
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
    public class ConsumerPactClassFixture
    {
        public IPactBuilder PactBuilder { get; private set; }
        public IMockProviderService MockProviderService { get; private set; }

        public int MockServerPort { get { return 9222; } }
        public string MockProviderServiceBaseUri { get { return String.Format("http://localhost:{0}", MockServerPort); } }
    }
}
```

Above we have setup some properties which ultimately say our Mock HTTP Server will be hosted at ```http://localhost:9222```. With that in place the next
step is to add a constructor to start the other properties starting with PactBuilder:

```csharp
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
    public class ConsumerPactClassFixture
    {
        public IPactBuilder PactBuilder { get; private set; }
        public IMockProviderService MockProviderService { get; private set; }

        public int MockServerPort { get { return 9222; } }
        public string MockProviderServiceBaseUri { get { return String.Format("http://localhost:{0}", MockServerPort); } }

        public ConsumerPactClassFixture()
        {
            // Using Spec version 2.0.0 more details at https://goo.gl/UrBSRc
            var pactConfig = new PactConfig
            {
                SpecificationVersion = "2.0.0",
                PactDir = @"..\..\..\..\..\pacts",
                LogDir = @".\pact_logs"
            };

            PactBuilder = new PactBuilder(pactConfig);

            PactBuilder.ServiceConsumer("Consumer")
                       .HasPactWith("Provider");
        }
    }
}
```

The constructor is doing a couple of things right now:

* It creates a [PactConfig](https://github.com/pact-foundation/pact-net/blob/master/PactNet/PactConfig.cs) object which allows us to specify:
  * The Pact files will be generated and overwritten too (```[RepositoryRoot]/pacts```).
  * The Pact Log files will be written to the executing directory.
  * The project will follow [Pact Specification](https://github.com/pact-foundation/pact-specification) 2.0.0
* Define the name of our Consumer project (Consumer) which will be used in other Pact Test projects.
  * Define the relationships our Consumer project has with others. In this case, just one called "Provider" this name will map to the same name used in the
  Provider Project Pact tests.

The final thing it needs to do is create an instance of our Mock HTTP service using the now created configuration:

```csharp
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
    public class ConsumerPactClassFixture
    {
        public IPactBuilder PactBuilder { get; private set; }
        public IMockProviderService MockProviderService { get; private set; }

        public int MockServerPort { get { return 9222; } }
        public string MockProviderServiceBaseUri { get { return String.Format("http://localhost:{0}", MockServerPort); } }

        public ConsumerPactClassFixture()
        {
            // Using Spec version 2.0.0 more details at https://goo.gl/UrBSRc
            var pactConfig = new PactConfig
            {
                SpecificationVersion = "2.0.0",
                PactDir = @"..\..\..\..\..\pacts",
                LogDir = @".\pact_logs"
            };

            PactBuilder = new PactBuilder(pactConfig);

            PactBuilder.ServiceConsumer("Consumer")
                       .HasPactWith("Provider");

            MockProviderService = PactBuilder.MockService(MockServerPort);
        }
    }
}
```

By adding the line ```MockProviderService = PactBuilder.MockService(MockServerPort);``` to the constructor we have created our Mock HTTP Server with
our specific configuration. We are nearly ready to start mocking out Provider interactions but (in my best Columbo voice) there is [just one more
thing](https://www.youtube.com/watch?v=biW9BbWJtQU).

#### Step 3.2.2 Tearing Down the Pact Mock HTTP Server & Generating the Pact File

If the tests were to use the Class Fixture above as is right now the Mock Server might be left running once the tests have finished and worse no Pact file
would be created - so we wouldn't be able to verify our mocks with the Provider API!


It is always a good idea in your tests to teardown any resources used in them at end of the test run. However [XUnit doesn't implement teardown methods](http://mrshipley.com/2018/01/10/implementing-a-teardown-method-in-xunit/) so instead we can implement the IDisposable interface to handle the clean up
of the Mock HTTP Server which will at the same time generate our Pact file. To do this update your ConsumerPactClassFixture class to conform to IDisposable
and clean up the server using ```PactBuilder.Build()```:

```csharp
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
            var pactConfig = new PactConfig
            {
                SpecificationVersion = "2.0.0",
                PactDir = @"..\..\..\..\..\pacts",
                LogDir = @".\pact_logs"
            };

            PactBuilder = new PactBuilder(pactConfig);

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
```

The ```PactBuilder.Build()``` method will teardown the Mock HTTP Server it uses for tests and generates the Pact File used for verifying mocks with
providers. It will always overwrite the Pact file with the results of the latest test run.

### Step 3.3 - Creating Your First Pact Test for the Consumer Client

With the class fixture created to manage the Mock HTTP Server update the test class added
by the ```dotnet create xunit``` command to be named ```ConsumerPactTests``` and update
the file name to match. With that done update the class to conform to the IClassFixture
interface and create an instance of your class fixture in the constructor.

```csharp
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
    }
}
```

With an instance of our Mock HTTP Server in our test class, we can add the first test. 
All the Pact tests added during this workshop will follow the same three steps:

1. Mock out an interaction with the Provider API.
2. Interact with the mocked out interaction using our Consumer code.
3. Assert the result is what we expected.

For the first test, we shall check that if we pass an invalid date string to our Consumer
that the Provider API will return a ```400``` response and a message explaining why the
request is invalid.

#### Step 3.3.1 - Mocking an Interaction with the Provider

Create a test in ```ConsumerPactTests``` called ```ItHandlesInvalidDateParam()``` and
using the code below mock out our HTTP request to the Provider API which will return a
```400```:

```csharp
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
}
```

The code above uses the ```_mockProviderService``` to setup our mocked response using Pact.
Breaking it down by the different method calls:

* ```Given("")```

This workshop will talk more about the Given method when writing the Provider API Pact test
but for now, it is important to know that the Given method manages the state that your test
requires to be in place before running. In our example, we require the Provider API to
have some data. The Provider API Pact test will parse these given statements and map
them to methods which will execute code to setup the required state(s).

* ```UponReceiving("")```

When this method executes it will add a description of what the mocked HTTP request
represents to the Pact file. It is important to be accurate here as this message is what
will be shown when a test fails to help a developer understand what went wrong.

* ```With(ProviderServiceRequest)```

Here is where the configuration for your mocked HTTP request is added. In our example
we have added what *Method* the request is (Get) the *Path* the request is made to 
(api/provider/) and the query parameters which in this test is our invalid date time
string (validDateTime=lolz).

* ```WillRespondWith(ProviderServiceResponse)```

Finally, in this method, we define what we expect back from the Provider API for our mocked
request. In our case a ```400``` HTTP Code and a message in the body explaining what the
failure was. 

All the methods above on running the test will generate a *Pact file* which will be used
by the Provider, API to make the same requests against the actual API to ensure the responses
match the expectations of the Consumer.

#### Step 3.3.2 - Completing Your First Consumer Test

With the mocked response setup the rest of the test can be treated like any other test
you would write; perform an action and assert the result:

```csharp
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
```

With the updated test above it will make a request using our Consumer client and get the
mocked interaction back which we assert on to confirm the error message is the one we
expect.

Now all that is left to do is run your test. From the
```[RepositoryRoot]/YourSolution/Consumer/tests/``` directory run the ```dotnet test```
command at the command line. If successful you should see some output like this:

```
YourPC:tests thomas.shipley$ dotnet test
Build started, please wait...
Build completed.

Test run for pact-workshop-dotnet-core-v1/YourSolution/Consumer/tests/bin/Debug/netcoreapp2.0/tests.dll(.NETCoreApp,Version=v2.0)
Microsoft (R) Test Execution Command Line Tool Version 15.3.0-preview-20170628-02
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
[xUnit.net 00:00:00.8359010]   Discovering: tests
[xUnit.net 00:00:00.9334030]   Discovered:  tests
[xUnit.net 00:00:00.9399270]   Starting:    tests
[2018-02-15 11:24:24] INFO  WEBrick 1.3.1
[2018-02-15 11:24:24] INFO  ruby 2.2.2 (2015-04-13) [x86_64-darwin13]
[2018-02-15 11:24:24] INFO  WEBrick::HTTPServer#start: pid=57443 port=9222
[xUnit.net 00:00:03.4053200]   Finished:    tests

Total tests: 1. Passed: 1. Failed: 0. Skipped: 0.
Test Run Successful.
Test execution time: 1.4248 Seconds
```

If you now navigate to ```[RepositoryRoot]/pacts``` you will see the pact file your test
generated. Take a moment to have a look at what it contains which is a JSON representation
of the mocked our requests your test made.

With your Consumer Pact Test passing and your new Pact file we can now create the Provider
Pact test which will validate your mocked responses match actual responses from the
Provider API.

## Step 4 - Testing the Provider Project with Pact

Navigate to the ```[RepositoryRoot]/YourSolution/Provider/tests``` directory in your
command line and create another new XUnit project by running the command
```dotnet create xunit```. Once again you will also need to add the correct version of
the PactNet package using one of the command line commands below:

```
# Windows
dotnet add package PactNet.Windows --version 2.2.1

# OSX
dotnet add package PactNet.OSX --version 2.2.1

# Linux
dotnet add package PactNet.Linux.x64 --version 2.2.1
# Or...
dotnet add package PactNet.Linux.x86 --version 2.2.1
```

Finally your Provider Pact Test project will need to run its own web server during tests
which will be covered in more detail in the next step but for now, let's get the
```Microsoft.AspNetCore.All``` package which we will need to run this server. Run the 
command below to add it to your project:

```
dotnet add package Microsoft.AspNetCore.All --version 2.0.3
```

With all the packages added to our Provider API test project, we are ready to move onto
the next step; creating an HTTP Server to manage test environment state.

### Step 4.1 - Creating a Provider State HTTP Server

The Pact tests for the Provider API will need to do two things:

1. Manage the state of the Provider API as dictated by the Pact file.
2. Communicate with the Provider API to verify that the real responses for HTTP requests
defined in the Pact file match the mocked ones.

For the first point, we need to create an HTTP API used exclusively by our tests to manage
the transitions in the state. The first step is to create a simple web api that is started
when your test run starts.

#### Step 4.1.1 - Creating a Basic Web API to Manage Provider State

First, navigate to your new Provider Tests project
(```[RepositoryRoot]/YourSolution/Provider/tests/```) and create a file and corresponding
class called ```TestStartup.cs```. In which we will create a basic Web API using the code
below:

``` csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using tests.Middleware;
using Microsoft.AspNetCore.Hosting;

namespace tests
{
    public class TestStartup
    {
        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<ProviderStateMiddleware>();
            app.UseMvc();
        }
    }
}
```

When you created the class above you might have noticed that the compiler has found a
compilation error because we haven't created the ProviderStateMiddleware class yet.

#### Step 4.1.2 - Creating a The Provider State Middleware

When creating a Pact test for a Provider your test needs its own API. The reason for
this is so it can manage the state of your API based on what the Pact file needs for each
request. This might be actions like ensuring a user is in the database or a user has
permission to access a resource.

Above we took the first step to create this API for our tests to access but currently
it both doesn't compile and even if we removed the ```app.UseMiddleware``` line it 
wouldn't do anything. We need to create a way for the API to manage the states required
by our tests. We will do this by creating a piece of middleware (similar to a controller)
that handles requests to the path ```/provider-states```.

##### Step 4.1.2.1 - Creating the ProviderState Class

First create a new folder at ```[RepositoryRoot]/YourSolution/Provider/tests/Middleware```
and create a file and corresponding class called ```ProviderState.cs``` and add the
following code:

```csharp
namespace tests.Middleware
{
    public class ProviderState
    {
        public string Consumer { get; set; }
        public string State { get; set; }
    }
}
```

This is a simple class which represents the data sent to the ```/provider-states``` path.
The first property will store the name of *Consumer* who is requesting the state change.
Which in our case is **Consumer**. The second property stores the state we want the
Provider API to be in.

With this class in place, we can create the middleware class.

##### Step 4.1.2.2 - Creating the ProviderStateMiddleware Class

Again at ```[RepositoryRoot]/YourSolution/Provider/tests/Middleware``` create a file and corresponding class called ```ProviderStateMiddleware.cs```. For now add the following code:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Newtonsoft.Json;

namespace tests.Middleware
{
    public class ProviderStateMiddleware
    {
        private const string ConsumerName = "Consumer";
        private readonly RequestDelegate _next;
        private readonly IDictionary<string, Action> _providerStates;

        public ProviderStateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value == "/provider-states")
            {
                this.HandleProviderStatesRequest(context);
                await context.Response.WriteAsync(String.Empty);
            }
            else
            {
                await this._next(context);
            }
        }

        private void HandleProviderStatesRequest(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            if (context.Request.Method.ToUpper() == HttpMethod.Post.ToString().ToUpper() &&
                context.Request.Body != null)
            {
                string jsonRequestBody = String.Empty;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    jsonRequestBody = reader.ReadToEnd();
                }

                var providerState = JsonConvert.DeserializeObject<ProviderState>(jsonRequestBody);

                //A null or empty provider state key must be handled
                if (providerState != null && !String.IsNullOrEmpty(providerState.State) &&
                    providerState.Consumer == ConsumerName)
                {
                    _providerStates[providerState.State].Invoke();
                }
            }
        }
    }
}
```

The code above gives us a way to handle requests to the ```/provider-states``` path and
based on the ```ProviderState.State``` requested run some associated code but in the code
above the ```_providerStates``` is empty so let's update the constructor to set up two states
and the associated code. The states to be added are:

1. "There is data"

This state will create a text file called ```somedata.txt``` at
```[RepositoryRoot]/YourSolution/data```. This state is currently used by our Consumer
Pact test.

2. "There is no data"

This state will delete the text file ```somedata.txt``` at
```[RepositoryRoot]/YourSolution/data``` if it exists. This state is not currently used
by our Consumer Pact test but could be if some more test cases were added ;).

The code for this looks like:

```csharp
public class ProviderStateMiddleware
{
    private const string ConsumerName = "Consumer";
    private readonly RequestDelegate _next;
    private readonly IDictionary<string, Action> _providerStates;

    public ProviderStateMiddleware(RequestDelegate next)
    {
        _next = next;
        _providerStates = new Dictionary<string, Action>
        {
            {
                "There is no data",
                RemoveAllData
            },
            {
                "There is data",
                AddData
            }
        };
    }

    private void RemoveAllData()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../../../data");
        var deletePath = Path.Combine(path, "somedata.txt");

        if (File.Exists(deletePath))
        {
            File.Delete(deletePath);
        }
    }

    private void AddData()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../../../data");
        var writePath = Path.Combine(path, "somedata.txt");

        if (!File.Exists(writePath))
        {
            File.Create(writePath);
        }
    }
```

Now we have initialised our ```_providerStates``` field with the two states which map to
```AddData()``` and ```RemoveAllData()``` respectively. Now if our Consumer Pact test
contains the step:

```csharp
    _mockProviderService.Given("There is data");
```

When setting up a mock request our Provider API Pact test will map this to the
```AddData()``` method and create the ```somedata.txt``` file if it does not already exist.
If the mock defines the Given step as:

```csharp
    _mockProviderService.Given("There is no data");
```

Then the ```RemoveAllData()``` method will be called and if the ```somedata.txt``` file
exists it will be deleted.

With this code in place the ```ProviderStateMiddleware``` class should be completed and
look like:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Newtonsoft.Json;

namespace tests.Middleware
{
    public class ProviderStateMiddleware
    {
        private const string ConsumerName = "Consumer";
        private readonly RequestDelegate _next;
        private readonly IDictionary<string, Action> _providerStates;

        public ProviderStateMiddleware(RequestDelegate next)
        {
            _next = next;
            _providerStates = new Dictionary<string, Action>
            {
                {
                    "There is no data",
                    RemoveAllData
                },
                {
                    "There is data",
                    AddData
                }
            };
        }

        private void RemoveAllData()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../../../data");
            var deletePath = Path.Combine(path, "somedata.txt");

            if (File.Exists(deletePath))
            {
                File.Delete(deletePath);
            }
        }

        private void AddData()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../../../data");
            var writePath = Path.Combine(path, "somedata.txt");

            if (!File.Exists(writePath))
            {
                File.Create(writePath);
            }
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value == "/provider-states")
            {
                this.HandleProviderStatesRequest(context);
                await context.Response.WriteAsync(String.Empty);
            }
            else
            {
                await this._next(context);
            }
        }

        private void HandleProviderStatesRequest(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            if (context.Request.Method.ToUpper() == HttpMethod.Post.ToString().ToUpper() &&
                context.Request.Body != null)
            {
                string jsonRequestBody = String.Empty;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    jsonRequestBody = reader.ReadToEnd();
                }

                var providerState = JsonConvert.DeserializeObject<ProviderState>(jsonRequestBody);

                //A null or empty provider state key must be handled
                if (providerState != null && !String.IsNullOrEmpty(providerState.State) &&
                    providerState.Consumer == ConsumerName)
                {
                    _providerStates[providerState.State].Invoke();
                }
            }
        }
    }
}
```

#### Step 4.1.2 - Starting the Provider States API When the Pact Tests Start

Now we have a Provider States API we need to start it when our Provider Pact tests start.
To do this first rename the provided test class when you created the XUnit project to
```ProviderApiTests.cs``` and include the code below:

```csharp
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using PactNet;
using PactNet.Infrastructure.Outputters;
using tests.XUnitHelpers;
using Xunit;
using Xunit.Abstractions;

namespace tests
{
    public class ProviderApiTests : IDisposable
    {
        private string _providerUri { get; }
        private string _pactServiceUri { get; }
        private IWebHost _webHost { get; }
        private ITestOutputHelper _outputHelper { get; }

        public ProviderApiTests(ITestOutputHelper output)
        {
            _outputHelper = output;
            _providerUri = "http://localhost:9000";
            _pactServiceUri = "http://localhost:9001";

            _webHost = WebHost.CreateDefaultBuilder()
                .UseUrls(_pactServiceUri)
                .UseStartup<TestStartup>()
                .Build();

            _webHost.Start();
        }

        [Fact]
        public void EnsureProviderApiHonoursPactWithConsumer()
        {
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _webHost.StopAsync().GetAwaiter().GetResult();
                    _webHost.Dispose();
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
```

Reading the code above when the ```EnsureProviderApiHonoursPactWithConsumer()``` test is
run using the ```dotnet test``` command at the command line the constructor will create
a new HTTP server with our Provider States API and store it in the ```_webHost``` property.
Once stored it will start the server so now our test once written can send requests to
```http://localhost:9001/provider-states``` to manipulate the state of our Provider API.

There are two other things to note:

* The Class implements IDisposable to ensure our Provider States API server is stopped
when the test is completed.
* The test requires a running instance of the Provider API server to verify the
responses match those expected in the Pact file. This server is not started by the tests.

### Step 4.2 - Creating the Provider API Pact Test

With our Provider States API in place and managed by our test when it is run we can
complete our test. Update the ```EnsureProviderApiHonoursPactWithConsumer()``` test
to:

```csharp
[Fact]
public void EnsureProviderApiHonoursPactWithConsumer()
{
    // Arrange
    var config = new PactVerifierConfig
    {

        // NOTE: We default to using a ConsoleOutput,
        // however xUnit 2 does not capture the console output,
        // so a custom outputter is required.
        Outputters = new List<IOutput>
                        {
                            new XUnitOutput(_outputHelper)
                        },

        // Output verbose verification logs to the test output
        Verbose = true
    };

    //Act / Assert
    IPactVerifier pactVerifier = new PactVerifier(config);
    pactVerifier.ProviderState($"{_pactServiceUri}/provider-states")
        .ServiceProvider("Provider", _providerUri)
        .HonoursPactWith("Consumer")
        .PactUri(@"..\..\..\..\..\pacts\consumer-provider.json")
        .Verify();
}
```

The **Act/Assert** part of this test creates a new
[PactVerifier](https://github.com/pact-foundation/pact-net/blob/master/PactNet/PactVerifier.cs)
instance which first uses a call to ```ProviderState``` to know where our Provider States
API is hosted. Next, the ```ServiceProvider``` method takes the name of the Provider being
verified in our case **Provider** and a URI to where it is hosted. Then the
```HonoursPactWith()``` method tells Pact the name of the consumer that generated the Pact
which needs to be verified with the Provider API - in our case **Consumer**.  Finally, in
our workshop, we point Pact directly to the Pact File (instead of hosting elsewhere) and 
call ```Verify``` to test that the mocked request and responses in the Pact file for our
Consumer and Provider match the real responses from the Provider API.

However there is one last step - the test currently doesn't compile as the
```XUnitOutput``` class does not exist - so we will create it.

### Step 4.2.1 - Creating the XUnitOutput Class

As noted by the comment in ```ProviderApiTests``` XUnit doesn't capture the output we want
to show in the console to tell us if a test run as passed or failed. So first create the
folder ```[RepositoryRoot]/YourSolution/Provider/tests/XUnitHelpers``` and inside create
the file ```XUnitOutput.cs``` and the corresponding class which should look like:

```csharp
using PactNet.Infrastructure.Outputters;
using Xunit.Abstractions;

namespace tests.XUnitHelpers
{
    public class XUnitOutput : IOutput
    {
        private readonly ITestOutputHelper _output;

        public XUnitOutput(ITestOutputHelper output)
        {
            _output = output;
        }

        public void WriteLine(string line)
        {
            _output.WriteLine(line);
        }
    }
}
```

This class will ensure the output from Pact is displayed in the console. How this works
is beyond the scope of this workshop but you can read more at
[Capturing Output](https://xunit.github.io/docs/capturing-output.html).

### Step 4.3 - Running Your Provider API Pact Test

Now we have a test in the Consumer Project which creates our Pact file based on its mock
requests to the Provider API and we have a Pact test in the Provider API which consumes
this Pact file to verify the mocks match the actual responses we should run the Provider
tests!

### Step 4.3.1 - Start Your Provider API Locally

In the command line navigate to ```[RepositoryRoot]/YourSolution/Provider/src``` and run
the command below to start the server:

```
dotnet run
```

This should show ouput similar to:

```
YourPC:src thomas.shipley$ dotnet run
Using launch settings from /Users/thomas.shipley/code/thomas/pact-workshop-dotnet-core-v1/YourSolution/Provider/src/Properties/launchSettings.json...
Hosting environment: Development
Content root path: /Users/thomas.shipley/code/thomas/pact-workshop-dotnet-core-v1/YourSolution/Provider/src
Now listening on: http://localhost:9000
Application started. Press Ctrl+C to shut down.
```

If you see the output above leave that server running and move on to the next step!

### Step 4.3.2 - Run your Provider API Pact Test

First, confirm you have a Pact file at ```[RepositoryRoot]/YourSolution/pacts``` called
consumer-provider.json.

Next, create another command line window and navigate to
```[RepositoryRoot]/YourSolution/Provider/tests``` and to run the tests type in and execute
the command below:

```
dotnet test
```

Once you run this command and it completes you will hopefully see some output which looks like:

```
YourPC:tests thomas.shipley$ dotnet test
Build started, please wait...
Build completed.

Test run for /Users/thomas.shipley/code/thomas/pact-workshop-dotnet-core-v1/YourSolution/Provider/tests/bin/Debug/netcoreapp2.0/tests.dll(.NETCoreApp,Version=v2.0)
Microsoft (R) Test Execution Command Line Tool Version 15.3.0-preview-20170628-02
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
[xUnit.net 00:00:03.1234490]   Discovering: tests
[xUnit.net 00:00:03.2294800]   Discovered:  tests
[xUnit.net 00:00:03.2992030]   Starting:    tests
info: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[0]
      User profile is available. Using '/Users/thomas.shipley/.aspnet/DataProtection-Keys' as key repository; keys will not be encrypted at rest.
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
      Request starting HTTP/1.1 POST http://localhost:9001/provider-states application/json 74
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[2]
      Request finished in 24.308ms 200 
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
      Request starting HTTP/1.1 POST http://localhost:9001/provider-states application/json 80
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[2]
      Request finished in 1.849ms 200 
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
      Request starting HTTP/1.1 POST http://localhost:9001/provider-states application/json 74
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[2]
      Request finished in 0.476ms 200 
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
      Request starting HTTP/1.1 POST http://localhost:9001/provider-states application/json 74
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[2]
      Request finished in 0.217ms 200 
[xUnit.net 00:00:06.8562100]   Finished:    tests

Total tests: 1. Passed: 1. Failed: 0. Skipped: 0.
Test Run Successful.
Test execution time: 7.9642 Seconds
```

Hopefully, you see the above output which means your Pact Provider test was successful!
At this point, you now have a working local example of a Pact test suite that tests
both the Consumer and Provider sides of an application but a few test cases are
missing...

## Step 5 - Missing Consumer Pact Test Cases

The Consumer Pact test suite only has one test in it. But there are a few test cases
which could also be implemented:

* It handles an empty date parameter.
* It handles having no data in the data folder.
* It parses a date correctly.

For the final step of this workshop take some time to update your Consumer Pact tests
to implement one or all of the test cases above. Once done generate a new Pact file
by running your Consumer Pact tests and validate your Pact file against the Provider
API. 

If you are struggling take a look at
```[RepositoryRoot]/CompletedSolution/Consumer/tests``` which contains the solutions
to each test case. But perhaps give it a go first!

# Copyright Notice & Licence 

This workshop is a port of the [Ruby Project for Pact Workshop](https://github.com/DiUS/pact-workshop-ruby-v2) with some
minor modifications. It is covered under the same Apache License 2.0 as the original Ruby workshop.

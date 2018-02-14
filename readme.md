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

This folder contains a complete sample solution for the workshop so if you get stuck at any point or are unsure what to do next take a look]
in here and you will see all the completed code for guidance.

Within the folder is a Consumer project in the **Consumer/src** folder which is a simple .NET Core console application that connects to the
Provider project which is in the **Provider/src** folder and is an ASP.NET Core Web API. Both projects also have a **tests/** folder which
is where the [Pact](https://docs.pact.io/) tests for both projects exist.

### YourSolution

This folder follows the same structure as the *CompletedSoultion/* folder except for the *tests/* folders are empty! During this workshop you
will be creating the test projects using [Pact](https://docs.pact.io/) to test both the *Consumer* project and the *Provider* project.

## Step 2 - Understanding The Consumer Project

The *Consumer* is a .NET Core console application which validates date & time strings by making requests to our *Provider* API. Take a look
at the code. You might notice before we can run the project successfully we need the Provider API running locally.

### Step 2.1 - Start the Provider API Locally

Using the command line navigate to:

```
[ProjectRoot]/YourSolution/Provider/src/
```

Once in the Provider */src/* directory first do a ```dotnet restore``` at the command line to pull down the dependencies required for the project.
Once that has completed run ```dotnet run``` this will start your the Provider API. Now check that everything is working O.K. by navigating to
the URL below in your browser:

```
http://http://localhost:9000/api/provider?validDateTime=05/01/2018
```

If your request is successful you should see in your browser:

```
{"test":"NO","validDateTime":"05-01-2018 00:00:00"}
```

If you see the above leave the Provider API running then you are ready to try out the consumer.

#### Potential Error

If you get a **404** error check that the path ```./YourSoultion/data``` exists with a text file in it called **somedata.txt** in it. We will
talk about this file later on.

### Step 2.2 - Execute the Consumer

With the Provider API running open another command line instance and navigate to:

```
[ProjectRoot]/YourSolution/Consumer/src/
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

# Copyright Notice & Licence 

This workshop is a port of the [Ruby Project for Pact Workshop](https://github.com/DiUS/pact-workshop-ruby-v2) with some
minor modifications. It is covered under the same Apache License 2.0 as the original Ruby workshop.
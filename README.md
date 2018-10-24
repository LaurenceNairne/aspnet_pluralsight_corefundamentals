# ASP.NET Core Fundamentals (Pluralsight course)

This repo contains my work on the above course. I'm using this readme to document the points I've learned from completing the course for me to come back to - also to verify that I've understood the concepts and details.

It's worth mentioning that this course uses ASP.NET Core 2.0 (not ASP.NET Framework, or Core 1.0) and that I began with an empty project - so no MVC service to begin with, though I believe I'll be adding that in later down the line.

## Program.cs

Program.cs is one of two classes present when creating a new ASP.NET Core Web Application. Like a standard console application, it has a `Main()` method that is used to launch the app. In this case, it calls the `BuildWebHost()` to return an `IWebHost` implementation, then calls `Run()` to get the WebHost running.

### BuildWebHost

This method creates a webhost builder object, sets it to use a `Startup` object for dependencies, then builds it all.

#### WebHost.CreateDefaultBuilder()

- A webhost builder is an object that knows how to setup our web server environment.
- Sets up our Kestrel (codenamed server that comes packaged with ASP.NET Core) server which will listen for HTTP connections
- Sets up IIS Express integration (not really gone into depth with this as it's apparently useful for creating Intranet apps for users inside a company firewall on Windows - for passing credentials to the Kestrel server)
- Sets up default logging when building
- Creates an object that implements the `IConfiguration` interface which can be accessed throughout the app and allows us to retrieve config information via the interface

### IConfiguration service

- Reads information from a few sources:
-- applicationsettings.json file
-- User secrets file
-- Environment variables
-- Command line arguments
- Matching variables in any of the above sources will be overwritten by the later source (e.g. a `greeting` variable in applicationsettings.json will be overwritten by a `greeting` variable in Environment variables

## Startup.cs

The `Startup` class is constructed from two main parts: a `ConfigureServices()` method, and a `Configure()` method. The former handles the registration of the services that will be utilised by the application. The latter handles the implementation of those services.

### Registering services

There are a whole host of services that come as standard with ASP.NET. These can be registered by using calling `services.AddService_Type` where 'Service_Type' is replaced. If we wish to add custom services (once we've set them up of course), we can use one of three options:

```CSharp
services.AddSingleton<Service_Type>(); //Only make one instance for the entire application
services.AddTransient<Service_Type>(); //Create a new instance every time a user needs to access the service
services.AddScoped<Service_Type>(); // Create a new instance for every HTTP request
```




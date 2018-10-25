# ASP.NET Core Fundamentals (Pluralsight course)

This repo contains my work on the above course. I'm using this readme to document the points I've learned from completing the course for me to come back to - also to verify that I've understood the concepts and details.

It's worth mentioning that this course uses ASP.NET Core 2.0 (not ASP.NET Framework, or Core 1.0) and that I began with an empty project - so no MVC service to begin with, though I believe I'll be adding that in later down the line.

## Program.cs

Program.cs is one of two classes present when creating a new ASP.NET Core Web Application. Like a standard console application, it has a `Main()` method that is used to launch the app. In this case, it calls the `BuildWebHost()` to return an `IWebHost` implementation, then calls `Run()` to get the WebHost running.

The `UseStartup<Class_Name>()` method registers the startup logic and instantiates an object of the class provided to it. It then invokes the two methods `ConfigureServices()` and `Configure()`.

### BuildWebHost()

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
## Middleware

In ASP.NET Core, middleware defines how an application responds to HTTP requests and how we display error information.

- The order in which middleware appears in the `Configure()` method is significant.
- HTTP request arrives at the server (e.g POST/reviews)
- Each piece of middleware is an object with a specific role
- They work in a bi-directional Pipeline design patter:
- HTTP request moves through middleware objects until it gets to something that can provide a response (usually through a router)
- If nothing found, error returned
- Request flows in through middlewares, response flows back out in the opposite direction
- The HTML response (with 200 OK status code) exits the server, over the network to the client waiting for it

## Working with environments and environment variables

- ASP.NET Core understands the concept of runtime environments
- The current environment can be scrutinised and set in the `Startup.Configure()` method
- In this way, we can show different content depending on the environment we're currently in
- As an example, the default Startup class contains a conditional statement that checks if we're currently in the development environment, and shows a developer exception page when thre's an error if we are
- ASP.NET has present boolean values to check against `IsDevelopment`, `IsStaging` and `IsProduction`
- We can use custom environments using the `IsEnvironment("Environment_Name")` boolean
- We can set environment using the `EnvironmentName` property.

We can use multiple profiles to run our application. By default in VS2017, we get two profiles: IIS Express, and our default project profile. There are more differences between the two, but at this point, it's worth mentioning that we can use different runtime environments between profiles.

This can be set in two places (they're actually the same but one is essentially a UI on top of the other:

- launchSettings.json which can be found as a child of Properties
- Just double-click Properties and the UI will open
 
We can also access different `appsettings.json` files depending on the environment we're running in.To do so, we just need to create a new appsettings file where we suffix the filename with the corresponding environment name. For example `appsettings.Development.json`. As long as an environment exists by the name appended to it, that file will be used when necessary.

## Serving static files

In order to use static files (such as pages, stylesheets, js files, etc), we need to implement a middleware by invoking `app.UseStaticFiles()`. This allows us to serve static content as required. It inspects the HTTP request, and when concerned it searches the filesystem inside the `wwwroot` folder for a file by the given name in the request. If found, it will stream it back to the user. If it doesn't find it, it will invoke the next middleware.

When concerning HTML pages, we can set a default page that loads when we sent a request for the root directory by invoking the `app.UseDefaultFiles()`middleware. This looks at an incoming request, if it is for a directory like wwwroot, it will check for a default file. By default, `index.html` is the file this middleware looks for. If a custom file is required, a `DefaultFilesOptions` object can be brought in to provide a custom filename.

It's important to note that `UseDefaultFiles()` does not serve anything back to the user. All it is doing is looking to see if there is a default file avaiable in wwwroot, and if it finds one, it will update the request path which will be sent on towards `UseStaticFiles()` which handles the stream back to the user. This is critical because it needs to come **before** `UseStaticFiles()` in `Startup.Configure()`.

Finally, `app.UseFileServer()` will install both of the above middlewares to cut down on keystrokes.

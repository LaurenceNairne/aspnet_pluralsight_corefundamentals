# ASP.NET Core Fundamentals (Pluralsight course)
This repo contains my work on the above course. I'm using this readme to document the points I've learned from completing the course for me to come back to - also to verify that I've understood the concepts and details.

It's worth mentioning that this course uses ASP.NET Core 2.0 (not ASP.NET Framework, or Core 1.0) and that I began with an empty project - so no MVC service to begin with, ~~though I believe I'll be adding that in later down the line~~ but this has been added manually.

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

### Services 

### Registering services
There are a whole host of services that come as standard with ASP.NET. These can be registered by using calling `services.Add[Service_Name]` where 'Service_Name' is replaced with the required service name. If we wish to add custom services (once we've created them up of course), we can use one of three options:

```CSharp
services.AddSingleton<Service_Type>(); //Only make one instance for the entire application
services.AddTransient<Service_Type>(); //Create a new instance every time a user needs to access the service
services.AddScoped<Service_Type>(); // Create a new instance for every HTTP request
```

#### Creating services
When creating a new service, it's good practice to provide an interface which the Startup class will use along side the implementation of the interface. This provides a great deal of flexibility because we can have different implementations of the interface and we don't need to change anything at the configuration end (every implementation of an interface must contain an implementation of all of it's methods), except the used implementation when registering the service in the `ConfigureServices()` method. We use the...

```CSharp
services.AddSingleton<TService, TImplementation>();
```
...form to pull in the interface and the desired implementation. We simply need to change the value for `TImplementation` when necessary. This is useful for stuff like data source switching. Currently I have a registered service `IRestaurantData` which is implemented by an `InMemoryRestaurantData` class - this class simply contains some hard coded data entries for testing purposes. When our app is ready for primetime, we can create a new implementation that points to a database of real data. We can even use different implementations dependent on the current running environment.

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

Finally, `app.UseFileServer()` will install both of the above middlewares to cut down on keystrokes. For MVC, it's best to just use `UseStaticFiles()` because we only want to respond with a static file if the request explicitly matches a filepath.

## MVC Framework
MVC design pattern separates concerns into three categories: Models, Views and Controllers. ASP.NET has a service to implement this design pattern.

So at a top level, a controller receives the request and works out how to handle it. It instantiates a model object responsible for holding the information that the user has requested. In complex models, this could be several classes and therefore several objects. If we're building an API layer, the controller can return the model serialised as JSON, XML or some other data type required. If we're needing to render something to a HTML web page, the controller can select a view to render the model to. The View will receive the information from the model and use it to construct the HTML page.

Before MVC can be used, we need to add it as a service in `Startup.ConfigureServices()`. It's a known service, so `service.AddMvc()` is all that's required so long as the NuGet dependency is present in the project.

In this project, `UseStaticFiles()` invokes `UseMvc()` if a static file is not requested. ASP.NET MVC has conventions to map specific parts of a URL in a HTTP request to methods in a controller class, which MVC instantiates.

So we want an incoming request to be directed to a specific controller. We could have implemented `app.UseMvcWithDefaultRoute()`which means a `HomeController` class will receive a request to the root of the app by default. If this class contains an `Index()` method, this will be the default action used to determine the response returned to the view. However, using `app.UseMvc()` is more flexible, but requires more setup. To specify controller manually, we need to use routing.

### Controllers
A controller simply has to create a model object (from a given model class) and decide what to do with it when a request is received.

Within ASP.NET MVC there is a base Controller class that most controllers derive from. It contains a lot of useful methods - many of which return objects that derive from `IActionResult` interface. One such method is `Controller.View()`, which creates a `ViewResult` object that takes in a model to be rendered by the view on a HTML page.

It's worth noting that there is a separation between the controller deciding **what** will be written into the HTTP response, and the writing and sending of the response. That is, nothing is sent back to the client immediately. In the controller, it creates the IActionResult datastructure which informs MVC what to do. Later in the processing pipeline, MVC carries out that instruction.

This makes testing easier, as we can test controllers and their actions without having a web server set up - we only need to test that our instructions are being sent to MVC, not that it resulted in the correct HTTP response.

In the background, the `IActionResult` is handling content negotiation (checking which content formats the request will accept in the accept header). If we're just returning the model to the client (i.e. not via a view to a HTML page), then the `IActionResult` will instruct MVC what format the model data should be serialized to.

#### Accessing services in controllers
We can use dependency injection to access services in a controller. We do this by creating a constructor of the controller with a required parameter that matches the service. In this project's case, we're using Interfaces matched to an implementation when adding a custom service, so the required parameter is the interface itself, not the implementation (so we can swap out for a different implementation later).

**For example:**

```CSharp
Startup.cs

public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IGreeter, Greeter>();
    services.AddScoped<IRestaurantData, InMemoryRestaurantData>();
    services.AddMvc();
}
-----------------------------------------------------------
HomeController.cs

private IRestaurantData _restaurantData
private IGreeter _greeter

public HomeController(
    IRestaurantData restaurantData,
    IGreeter greeter)
{
    _restaurantData = restaurantData;
    _greeter = greeter;
}
```

In the above, we are registering the services as normal in the Startup class. We then have a constructor that requires a `IRestaurantData` and `IGreeter`. When MVC has to send the request to the HomeController, it will see the constructor, see it's dependencies and will check with the service provider for a definition of these services. On finding them, it will request instances (of the implementation classes of those services in the registration) to populate those required parameters, and in our case we then assign their values to our own private properties. We then use these properties in our actions as required.

### Models
A model in its simplest form is a class containing some properties. Controllers instantiate them appropriately when they receive a corresponding request. They can come in slightly different forms depending on how they are supposed to be used. The author of the course (Scott Allen) separates models into two main archetypes: **Entity Models** and **View Models**.

An Entity Model is an object that we persist into our database and it resembles the database schema. A View Model is an object to carry information between the controller and the view (also known as a Data Transfer Object). It is not persisted into the database, but it does copy information from entities and transfer information back into entities. 

A standard Entity Model might look something like this:

```CSharp
public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; }
}   
```

For View Models, he breaks it down into two further subdivisions: **Output Models** and **Input Models**

#### Output View Models
These can provide a reference to several sources of data in one place. This is useful for when we want to then provide a model to a `ViewResult`. When working with a view, we then provide this to the `@model` directive and can then pull in everything we need as before.

**For Example:**

```CSharp
HomeIndexViewModel.cs

public class HomeIndexViewModel
{
    public IEnumerable<Restaurant> Restaurants { get; set; }
    public string CurrentMessage { get; set; }
}
```
```
HomeController.cs

...

public IActionResult Index()
{
    var model = new HomeIndexViewModel();
    model.Restaurants = _restaurantData.GetAll();
    model.CurrentMessage = _greeter.GetMessageOfTheDay();

    return View(model);
}
```

In the above, we've provided a ViewModel with an `IEnumerable` of the type `Restaurant` called `Restaurants`. In other words, this property will hold a collection of `Restaurant` objects. We also provide a `CurrentMessage` property that expects a string type value. In the `HomeController` we then create a model object and assign to it a new `HomeIndexViewModel` object. We then assign values to the `Restaurants` and `CurrentMessage` properties by pulling them from the respective services `IRestaurantData` and `IGreeter`, then provide this model to the `ViewResult` when returning `View(model)`.

At face value, this might seem like a lot of steps to do something fairly straight forward, but it makes it easier to test by separating concerns into different places in the source code. We handle the definition of what a restaurant is separately from the creation, editing and deletion of restaurants, and then separate the collecting of existing restaurants from the handling of how we then present this collection back to the user. If we need to change something, so long as we're smart about it we can find where to make the change with minimal effort.

#### Input View Models
An input view model takes some form of input from the user which is used by a view to decide how to render a page. A simple form of this would be providing a detailed view of a single restaurant from the collection. To achieve this, we'd need to have an action that pulled in some identifying parameter that would tell MVC which restaurant we wanted to view on the new page. When an action has a parameter, MVC will do everything it can to populate it with a value. It will first look for a matching property in the routing table, and also checks any query strings in the request as well (but routing will take precendence).

Luckily, our restaurants all come with an ID property associated with them, and when we set up our convention-based routing (see the section on [routing](#routing)) we provided the option to have an `{id?}` in the request URL. We just need to create a view that matches the name of the action it's associated with, and pull in a generated model object assigned with the restaurant data at the given id.

**To illustrate this:**

```CSharp
HomeController.cs

public IActionResult Details(int id)
{
    var model = _restaurantData.Get(id);
    if (model == null)
    {
        return RedirectToAction(nameof(Index));
    }
    return View(model);
}
```
```CSharp
public class InMemoryRestaurantData : IRestaurantData
{
    private List<Restaurant> _restaurants;

    {...}

    public Restaurant Get(int id)
    {
        return _restaurants.FirstOrDefault(r => r.Id == id);
    }
```

```cshtml
Details.cshtml

@model OdeToFood.Models.Restaurant

<h1>@Model.Name</h1>
<div>...details...</div>
<a asp-action="Index" asp-controller="Home">Home</a>
```
```cshtml
Index.html

@model OdeToFood.ViewModels.HomeIndexViewModel

<...>
<body>
    <h1>@Model.CurrentMessage</h1>
    <table>
        @foreach (var restaurant in Model.Restaurants)
        {
            <tr>
                <td>@restaurant.Id</td>
                <td>@restaurant.Name</td>
                <td>
                    <a asp-action="Details" asp-route-id="@restaurant.Id">Explore</a>                   
                </td>
            </tr>
        }
    </table>
    <a asp-action="Create">Create Restaurant</a>
</body>
</html>
```
In the above, our `Detail` action assigns the value of the Restaurant at the given id by invoking the `Get(int id)` method on an implementation of `IRestaurantData`. Our LINQ query is just saying give me the first item where the restaurant ID matches the ID in the request.

We perform a null check on the model (because the `FirstOrDefault()` method returns `null` if it doesn't find a match), then return the corresponding view with the generated model as a parameter.

In the detail we simply display the given model's name, some static text, and then provide a link back to the Index view which renders our home page.

Finally, on the `Index` view, we add a column to the table with our restaurants, and populate it with a link for each restaurant using tag helpers (see section on [tag helpers](#tag-helpers)). We use the `asp-route` tag helper to tell MVC what the name of the element to use is (in this case it is `id`) and then assign `restaurant.Id` as it's value. This means that when the link for a restaurant is clicked, the request will have the URL `/Details/{Id of restaurant}` and MVC will be able to invoke the correct action with the correct value in the `id` parameter.

### Views
A view is a file on a file system by default. When a controller returns a ViewResult, MVC looks in the file system for a file by the name of the action it was returned from, and executes the view which produces the HTML. This HTML is sent back to the client to be rendered.

As standard, in ASP.NET MVC a view must live inside a Views folder, and must either live in a subfolder by the controller name, or in a 'Shared' folder which will hold views shared across multiple controllers. For example, the home page view would live in "Views/Home/" and the file name would be "index.cshtml" to match the action name on the controller. 

If MVC doesn't find a file at the directory it expects, it will throw an error.

The `.cshtml` format is a Razor View. Razor is a markup syntax for embedding server-based code into our webpages and combines Razor markup, C# and HTML. `@` symbols either prefix C# code or prefix Razor reserved keywords. Razor then evaluates this and renders it as HTML.

When we return the ViewResult in the controller, we can also provide a model object to inform the construction of the view. 

**For example:**

```CSharp
public IActionResult Index()
{
    var model = new Restaurant
    {
        Id = 1,
        Name = "Scott's Pizza Place"
    };

    return View(model);
}
```

We can then reference this model in the view file to pull properties into HTML elements to be rendered. In the example below, we are pulling in the ` Model.Name` and `Model.Id` property from our created model object. It's important to note the line that reads `@model OdeToFood.Models.Restaurant;` in the beginning of our file. This Razor directive allows us to use IntelliSense to grab the properties on our model. Without it we could still type in and use the properties, which would work, but it would mean we'd risk typos. So it's an efficiency measure if nothing else. 

```cshtml
@model OdeToFood.Models.Restaurant;

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body>
    <h1>@Model.Name</h1>
    <div>The ID value is @Model.Id</div>
</body>
</html>
```

#### Tag Helpers

Tag helpers are just a way of writing C# code that follows the visual syntactic style of HTML elements to make them easier to read in-line. Before these were introduced, the norm was to use HTML Helpers that would follow the form of `@Html.*HelperName*`. 

**For example:**

```CSharp
@Html.ActionLink(
                 string TextToDisplay, 
                 string ActionToInvoke, 
                 string ControllerToInstantiate, 
                 TModel RootValue)
```
This isn't all too visually appealing and looks out of place in a file resembling HTML for the most part. The same functionality as the above using Tag Helpers would look like so:

```HTML
<a asp-action="ActionName" 
   asp-controller="ControllerName" 
   asp-route-[route_element_name]="ValueAtGivenRouteElement">
     TextToDisplay
</a>
```

Both of the above options use the routing table, so if the format of the URL changes, they will still work (which is infinitely better than having a static link to a fixed URL in a view).

In the case of the tag helper method, we see the last attribute `asp-route-[route_element_name]`. The last part is replace with whatever part of your route gives an identifier for the view you want to be rendered. In our case, it would be a page with the details of a specific restaurant on it.

In order to use Tag Helpers, our project must have a Razor View Imports page. These files tell MVC and the Razor View engine how our views should behave and the various capabilities that views should have. In this file, we need to add an `@TagHelpers *, Microsoft.AspNetCore.Mvc.TagHelpers`

**Tag helpers I've used so far:**

- `asp-action` Used in an <a> tag to define the name of the action
- `asp-controller` Used in an <a> tag to define the controller if not the same as the one that instantiated the `ViewResult`
- `asp-for` Used in form elements to connect a particular input by the user to a property in the currently associated model
- `asp-item` Used in select form elements to provide a list of options of a type that matches the connected model property
- `asp-route-*route_element_name*` Used in an <a> tag to define the element of the route that the given action should use as its parameter (*route_element_name* is replaced with the actual name of the element in the request that should be used as the value)

### Routing
This concerns how we get a HTTP request to the correct controller and how to invoke a public method within it. There are two types of routing used in ASP.NET - that I'm aware of so far (and they can be used in tandem):

#### Convention-based routing
This option defines templates for how MVC should get a controller and action name from a URL in `Startup.Configure()`.

If we were to leave `app.UseMvc()` as it is, it wouldn't know how to handle any requests. It requires an overload to take an `Action<IRouteBuilder>`. In this case, this takes the form of a private `ConfigureRoutes(IRouteBuilder routeBuilder)` method. It is in this that we will define our routing template.

Inside this method we use our routeBuilder to configure a `MapRoute()`. We provide a friendly name for the route - in this case `"Default"` - and then the template specifics - in this case `"{controller=Home}/{action=Index}"`. This is literally saying "When a controller class name appears in the request URL straight after the root, followed by a forward slash and then a recognised public method name, instantiate the controller and invoke the method".

There are a couple more things worth mentioning here. In our example, we have `=Home` following the controller and `=Index` following the action. These are defaults - i.e. we're explicitly defining the work handled by `UseMvcWithDefaultRoute()` in that we are saying, "if no controller and action is defined in the request URL, instantiate this controller and invoke this action by default". The difference here is that we are manually setting what those defaults are, so they could be any controller and action we wanted.

Secondly, when the controller name appears in the URL, it does not require "Controller" part. So `HomeController` can appear as "Home" in the URL and MVC will append this with "Controller" when processing it. Further reading states that a Controller file __must__ be named as "SomethingController" by convention. The official word is that this is to avoid class name clashes between controllers and models - I'm not convinced this should have been enforced, but I'm not too bothered about that. Plus naming is hard enough as it is anyway.

Final point, in the example below, there is a third parameter that the template can accept in the request URL. That is an "id", but this is followed by a "?", meaning it's an optional parameter and is not required. This gives us the option to do things like query the database for a given ID, etc.

**Example:**

```CSharp
public class Startup
    {
        ...

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env)
        {
            app.UseStaticFiles();
            app.UseMvc(ConfigureRoutes);
        }

        private void ConfigureRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute(
                "Default", 
                "{controller=Home}/{action=Index}/{id?}");
        }
    }
```

#### Attribute-based routing
This option applies C# attributes to the controllers (classes) and actions (public methods) themselves which lets MVC know when to call a specific action.

While this can be used in combination with the convention-based routing, it gives more flexibility and overrides the convention when used on a specific controller.

A route attribute takes the form of `[Route("")]`, and what is entered between the quotes defines the expected value in the URL in the appropriate place. This could be a literal string like "about", but this may get confusing if we were to change the class name. To keep the two aligned, we can also use a token like `[controller]`, which tells MVC to just use the controller name as the expected value.

There's a couple of ways to achieve the required result, with their own strengths and weaknesses. Firstly, you can set an attribute on the controller and each action separately. MVC knows that the route at the controller level will be the first element in the URL and that any attributes on actions within the class will follow on. 

This method is useful as it allows us to control what each action should appear as in the URL at a granular level. We can again use a string here with the explicit value, or use an `[action]` token to make it just match the action name. A major benefit of this option is that you can set a default action by leaving the route blank in its route attribute (`[Route("")]`), and as long as attributes have been set on the other actions, this will always default to your chosen action if the URL doesn't contain anything beyond the controller part of the path. Finally, we could choose to make a single action be invoked from a custom route like `[Route("details/[action]")]`, while the rest are simply `[Route("[action]")]`. So lots of flexibility.

The drawback of this method is that you need to write attributes for every action that you have (there could be loads) and you're wasting keystrokes if you intend to handle them all in the same way (and if you do not want a default).

**Example where route is defined in a combination of controller level and action level:**

```CSharp
[Route("[controller]")]
public class AboutController
{
    [Route("")]
    public string Phone()
    {
        return "+44 07777 777 777";
    }

    [Route("[action]")]
    public string Address()
    {
        return "123 Fake Street,\nMadeupville,\nNowhere";
    }
}
```
We can also define the routing above the controller class declaration. Doing this means it sits in a single place and we know all actions will follow the same URL format. We do this by appending a forward slash and then an action token (`[action]`) to the route. We can still add in fixed elements before and in-between the controller and action like `[Route("pages/[controller]/smallpages/[action]")]`, but I'm not sure it makes sense to.

This does not allow us to provide a default action - or at least as far as I can tell. I've tried a few things but none seemed to work.

**Example where route is defined entirely on the controller:**
```CSharp
[Route("[controller]/[action]")]
public class AboutController
{
    public string Phone()
    {
        return "+44 07777 777 777";
    }

    public string Address()
    {
        return "123 Fake Street,\nMadeupville,\nNowhere";
    }
}
```


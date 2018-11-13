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
- registers some services upfront that we can use before we get to `ConfigureServices` in the `Startup` class.

### IConfiguration service
- Reads information from a few sources:
-- applicationsettings.json file
-- User secrets file
-- Environment variables
-- Command line arguments
- Matching variables in any of the above sources will be overwritten by the later source (e.g. a `greeting` variable in applicationsettings.json will be overwritten by a `greeting` variable in Environment variables

## Startup.cs
The `Startup` class is constructed from two main parts: a `ConfigureServices()` method, and a `Configure()` method. The former handles the registration of the services that will be utilised by the application. The latter handles the implementation of those services.

`ConfigureServices()` is a non injectible class - so we cannot add further injected dependecies to it like we can with `Configure()`.

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
```
```CSharp
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
#### Layout Views
This is a special type of view that does not return from a controller action. It exists to provide a template for our rendered views, with all of the html payload and consistent sections that we'll want across the application like header, footer, sidebar and so on.

The obvious benefit of using this structure is that we don't need to duplicate effort across all pages and can focus specifically on bespoke elements in each of our rendered views. Where rendered views live in a folder that matches the controller action name that they are returned from, special views like this live in a 'Shared' folder so that it's accessible throughout the application.

```cshtml
<!DOCTYPE html>

<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title>@ViewBag.Title</title>
</head>
<body>
    <div>
        @RenderBody()
    </div>
    <footer>
        @RenderSection("footer", required:true)
    </footer>
</body>
</html>
```
In the above example, we have a fairly simple layout that contains a few features worth noting. First and foremost, we must have a single `@RenderBody()` call in our layout. This tells the engine where the bulk of the rendered view in question will sit in the overall markup.

In the <title> element, we have this call to `@ViewBag.Title`. We are expecting that it will have a value provided in the view in question when it comes to rendering it to a page - we'll see this more clearly when we look at how a view can connect to a layout. We use this because a layout should not have a hardcoded title.
 
Finally, We have a call to `@RenderSection("footer", required:true)` in the <footer> element. This tells the engine to render any markup included inside a "footer" section in this part of the page. Again we'll see how this is implemented when we get to seeing how a view connects to the layout. The second parameter in this method will tell the engine whether a footer section needs to be there or not.
 
```cshtml
@model OdeToFood.ViewModels.HomeIndexViewModel
@{
    ViewBag.Title = "Home";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<h1>All Restaurants</h1>
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
@section footer {
    @Model.CurrentMessage
}
```
At the top of our index view, we include a C# snippet to set the `ViewBag.Title` discussed earlier and to set the layout view to use when rendering the page. We have to set the title before instructing the view to use the layout, else our title field will not be populated and would likely throw an exception.

At the bottom of the file we add our `@section footer {}` which holds all our markup that we want to be rendered when we call `@RenderSection()` in the layout view. In our case, footer will be rendered in the footer element.

#### _ViewStart
A _ViewStart file is processed before any views are rendered to a page. Because of this, we can use them to set default layout views for multiple views in one place. It works on the basis that the default for any views at the same level, or below this file in the Views folder hierarchy will receive the default layout unless it's overriden.

We can override the default in two ways:

- We can add further _ViewStart files closer to views in the hierarchy that we want to have a different layout
- We can add a layout to a view specifically or set it to `null`

#### _ViewImports
We already touched on _ViewImports previously when we set up tag helpers (see section on [tag helpers](#tag-helpers)). We can also set up using directives in this file to ensure that we have access to namespaces we'll need for our views. 

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
- `asp-validation-for` Used in a span element on a form to do a validation check against validation requirements in the referenced model, if errors are found, the error string will be displayed in the span.

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

#### Route constraints
There are occasions where it's appropriate to have two actions with the same name. For example, if we're providing the functionality to enter some data into a form that will be stored in a database somewhere, we might have a Create action that takes the user to the Create view, and we might have a Create action that deals with the storing of the data entered by the user.

In such cases, we're dealing with one action that is a HTTP GET, and one that is a HTTP POST. If we left the two actions as they are, we'd receive an error because MVC doesn't know which of the two to invoke. To solve this, we have two attributes: `[HttpGet]` and `[HttpPost]`. When one is used, we're saying "this action can only support this type of request".

### Input validation
When users enter information into a form in a page view, we want to be sure they are entering information correctly. It may be that they're entering a password that requires specific character types, or a text field that has a max and min character count. It could also be that the field is simply "required" and can't be left empty.

In ASP.NET, form validation is a three part process: setting validation attributes on properties in a model, validating the user inputs on the form against those attributes and informing the user if there are problems in the page view. 

In the below example we have a model that has a required `Name` field that cannot contain more than 100 characters.

```csharp
public class RestaurantEditModel
{
    [Required, MaxLength(100)]
    public string Name { get; set; }
    public CuisineOrigin Cuisine { get; set; }
}
```

MVC has a ModelState class which holds information about the transaction of model binding between the model and the action in question. It contains any errors that are flagged during this process, prompted by the validation attributes on properties within the model. Given the above model, we can check if the user has entered any value for the `Name` property, and that it's less than 100 characters. We then decide what to do if either of these requirements are not met during the binding process. This is done using `ModelState.IsValid`. It returns true if no errors are found, and false if one or more is.

In the below example on our controller, our POST Create action does a check on `IsValid`, carries out our submission behaviour if true, and just returns the view again if not.

```CSharp
[HttpPost]
public IActionResult Create(RestaurantEditModel model)
{
    if (ModelState.IsValid)
    {
        var newRestaurant = new Restaurant
        {
            Name = model.Name,
            Cuisine = model.Cuisine
        };

        newRestaurant = _restaurantData.Add(newRestaurant);

        return RedirectToAction(nameof(Details), new { id = newRestaurant.Id });
    }
    else
    {
        return View();
    }
}
```

Finally, in the Create view itself we then include `<span>` elements that contain the `asp-validation-for` tag helpers for those fields that we've set validation requirements on in the model. The Razor View engine will know if errors are returned, and will display those errors in that `<span>`.

```cshtml
@using OdeToFood.Models
@model Restaurant

<h1>Create</h1>
<form  method="post">
    <div>
        <label asp-for="Name"></label>
        <input asp-for="Name" />
        <span asp-validation-for="Name"></span>
    </div>
    <div>
        <label asp-for="Cuisine"></label>
        <select asp-for="Cuisine"
                asp-items="@Html.GetEnumSelectList<CuisineOrigin>()"></select>
    </div>
    <input type="submit" name="Save" value="Save" />
</form>
```

## Razor Pages
While similar in premise, Razor Pages are not part of the MVC Framework. Rather than a HTTP request going to a controller, it will go directly to a page with a parent folder name and file name that matches the request route.

A Razor Page is just a `.cshtml` file like a standard view. How it differs, is with the `@page` directive. This tells the Razor engine to treat it differently from a normal view.

From it's similarity with a view, a Razor Page can still make use of [_ViewStart](#_viewstart), [_ViewImports](#_viewimports) and [_Layout](#layout-views) views. 

Where a Razor View can be supplied with a model when an action is called on a controller, a Razor Page has it's own model. This model can handle dependency injection, using directives and all manner of logic operations within `OnGet()` and `OnPost()` methods. Whenever a GET request arrives to the page, the `OnGet()` method will be invoked, and whenever a POST request is sent from the page (like a form submission), the `OnPost()` method is invoked.

It is possible to put all using directives and dependencies in our pages, but this isn't a good idea - we are then trying to do too much in one place. By separating this to make the page model handle logic, dependencies and namespace usage, and the page handle what actually displays and how, we keep a clean separation of concerns and it allows us to test the model independently from the page and browser.

It's worth mentioning that projects that are primarily API driven will suit MVC better than Razor Pages, but the latter is a more streamlined approach to HTML heavy applications. Generally speaking, a project will use one or the other as it can get confusing to have a mix of both. I am following a course though, so we currently have a mix of the two.

### Editing a database item via a Razor Page
We so far have the ability to create new restaurants. Now we need to add the ability to edit an existing one. There's a lot of similarities between the two processes, but this time we need to access and present the values of the existing retaurant in the form when the request comes in.

We're currently in the Razor Pages part of the course, so this is going to be set up using a Razor Page, but it shouldn't be a great deal different if we were to retrofit it to work with the standard MVC framework. We'd just need to switch out the PageModel for actions on the HomeController (like we have with Create) and make use standard views instead of pages.

The first thing we need to do is amend our `IRestaurantData` service with an `Update()` method. We will then add an implementation for this method in our `SqlRestaurantData` class.

```csharp
public Restaurant Update(Restaurant restaurant)
{
    _context.Attach(restaurant).State = 
        EntityState.Modified;
    _context.SaveChanges();
    return restaurant;
}
```
This method requires a `Restaurant` entity when invoked. We `Attach()` the entity to our `OdeToFoodDbContext`, which makes it begin tracking the entity, but without any further actions, it wouldn't change anything in the database if we called `SaveChanges()` because it is set to `EntityState.Unchanged`. We therefore manually set the state to `EntityState.Modified`, then call `SaveChanges()` to ensure that the database gets updated with the modified entity. Finally we return the `Restaurant` entity.

```csharp
public class EditModel : PageModel
    {
        private IRestaurantData _restaurantData;

        [BindProperty]
        public Restaurant Restaurant { get; set; }

        public EditModel(IRestaurantData restaurantData)
        {
            _restaurantData = restaurantData;
        }

        public IActionResult OnGet(int id)
        {
            Restaurant = _restaurantData.Get(id);
            if (Restaurant == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _restaurantData.Update(Restaurant);
                return RedirectToAction("Details", "Home", new { id = Restaurant.Id });
            }

            return Page();
        }
    }
```
In our Edit page model, we have a private `IRestaurantData` property that we can access throughout the class, a `Restaurant` property that is bound to an entity received in the request and a constructor that expects an implementation of `IRestaurantData` to be passed in when a request hits. Inside this constructor we assign the value of the received `IRestaurantData` to our private property of matching type. We then have our `OnGet()` and our `OnPost()` methods which will handle the expected requests.

In our `OnGet()` method, we need to pull in an id parameter as we did with our `HomeController` Details action (see section on [Input ViewModels](#input-view-models)). This parameter will be looked for in the routing information received in the request, or in a query string on the URL (with priority given to the routing data). We'll see how we can enforce the presence of this ID in the route when we look at the Edit page itself. We set the value of our `Restaurant` property to that of the `Restaurant` at the given id in the `OdeToFoodDbContext` - or rather, in our database which is connected to it. We do this using the `SqlRestaurantData.Get(int id)` method. We perform a null check, returning the user to the index page if no such ID is found in the database, or returning a `PageResult` built by the `Page()` method (provided by the base `PageModel` class if one is found. This will render the expected edit page

In our `OnPost()` we check against validation errors using `ModelState.IsValid`, which will return false if any exceptions are found. If it returns false, we simply return the same page and the Razor Page will handle rendering the validation errors. If it returns true, we call the `Update()` method on our `IRestaurantData` property, passing in our bound `Restaurant` property as a parameter, then return a redirect to the Details view for this restaurant.

In our Edit page we have a similar setup to the Create form (see section on [Input Validation](#input-validation)). We have to include the `@page` directive (because we're working with Razor Page), and we insist on a parameter of `"{id}"`. We'll get an exception if one is not provided. We pull in our `EditModel` class so we can read the values from the `Restaurant` entity. 

In our form, we need to add a hidden input field for `Restaurant.Id`. I haven't yet worked out _why_ we need to include this field, but I know that commenting it out gives us a `System.InvalidOperationException` where the full description reads:

`'The property 'Id' on entity type 'Restaurant' has a temporary value while attempting to change the entity's state to 'Modified'. Either set a permanent value explicitly or ensure that the database is configured to generate values for this property.'`

Presumably, this input field provides some permanence to the `Id` property in our submitted POST request, but I'm just not clear on why (seeing as we already have an id that exists in the database). If I were to hazard a guess, I'd say it is because of the below statement in the Microsoft EF6 documentation:

`When you change the state to Modified all the properties of the entity will be marked as modified and all the property values will be sent to the database when SaveChanges is called.`

I imagine that we're telling Entity Framework all properties on our `Restaurant` entity have been modified, but we're not providing a modified value for our `Id` property. Now we **do not** want the user to submit this value, but we **do** need to submit something for it. As our database is setting values for this property,  we just submit it without a new value.

All other elements are identical to the Create form, but we pull in the properties from the above entity, rather than before where we simply provided a property name to bind our values to when creating a brand new `Restaurant` entity.

## Entity Framework
N.B. We're using EF Core in this project.

From the docs, EF Core is a lightweight, extensible and cross-platform version of the popular Entity Framework data access technology. It's essentially a shortcut to data access, serving as an object-relational mapper (ORM) in this capacity.

We start by creating a class that derives from the base `DbContext` provided by EF. A `DbContext` provides access to a single database. This can be remapped to different databases in the application's lifetime, but it logically maps to a specific database with a schema that the `DbContext` class understands.

In our project we have an `OdeToFoodDbContext`:

```csharp
public class OdeToFoodDbContext : DbContext
{
    public OdeToFoodDbContext(DbContextOptions options) 
        : base(options)
    {

    }

    public DbSet<Restaurant> Restaurants { get; set; }
}
```
We create a constructor that takes in a `DbContextOptions` object to allow it to connect to different databases. This object can give us several things, but importantly it handles the connection strings to the database. We pass these options to the base `DbContext` which will handle looking at the options and setting up connections.

We then provide a `DbSet<T>` for each Entity Model that should map to a table in the database. In our case we just have `Restaurant`. By default, naming our `DbSet<T>` "Restaurants" will prompt EF to look for a table by the same name in the database. This can be reconfigured.

At this point we could create an instance of this context class inside a controller or register our context as a service in Startup to be injected in whatever components require it. In our case we just abstract it away behind an implementation of our `IRestaurantData` service we set up previously.

```csharp
public class SqlRestaurantData : IRestaurantData
{
    private OdeToFoodDbContext _context;

    public SqlRestaurantData(OdeToFoodDbContext context)
    {
        _context = context;
    }

    public Restaurant Add(Restaurant restaurant)
    {
        _context.Restaurants.Add(restaurant);
        _context.SaveChanges();
        return restaurant;
    }

    public Restaurant Get(int id)
    {
        return _context.Restaurants.FirstOrDefault(r => r.Id == id);
    }

    public IEnumerable<Restaurant> GetAll()
    {
        return _context.Restaurants.OrderBy(r => r.Name);
    }
}
```
As with our previous in memory implementation of `IRestaurantData`, we're contractually obliged to implement all of its methods. We create a constructor that pulls in a `OdeToFoodDbContext` as it's parameter, then inside this we assign the value of the parameter to our own private `OdeToFoodDbContext` property for use elsewhere in the class.

When implementing our `Add()` method, we call the `Add()` method on our `DbSet<Restaurant>`. This prompts EF to track it for inserting into the database later. It will not be added until we call the `SaveChanges()` method on our context object. This separation allows us to do things like batching several additions into a single insertion operation.

By default, an `Id` property on the entity model will become an `Id` column in our database table and with this column, the Ids can be generated automatically. When the new model object is inserted into the database table, EF will grab the generated id value and assign this to the `Id` property on the model object. That way, when we return the the object at the end of the `Add()` method, it's `Id` prop is not null.

### Configuration
In our startup class, we're currently registering our `IRestaurantData` service with the `InMemoryRestaurantData` implementation. We need to amend this to use our new `SqlRestaurantData` implementation. We also need to alter the registration method we use. Currently we're employing `AddSingleton<T>` - i.e. create a single instance and keep it throughout the app runtime, but DbContext is not thread safe so we should create a new instance for each new HTTP request using `AddScoped<T>` which ensures the DbContext is only used on a single logical thread.

- **Before:** `services.AddSingleton<IRestaurantData, InMemoryRestaurantData>()`
- **After:** `services.AddScoped<IRestaurantData, SqlRestaurantData>()`

We also need to register an Entity Framework service to make the DbContext actually do something.

`services.AddDbContext<OdeToFoodDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString("OdeToFood")))`

In the above, we provide the name of our DbContext derived class `OdeToFoodDbContext` that we want to execute and we provide the options that we want to use to initialise that class. We instruct the service to use SQL Server with an extension method found in `Microsoft.EntityFrameworkCore` namespace, and provide a connection string. We provide this connection string via our _appsettings.json_ file using an instance of the `IConfiguration` service. `GetConnectionString()` is a shortcut method that will look in our config file for a group named "ConnectionStrings" and then for a property with the key we provide as a parameter - in our case "OdeToFood".

For reference: a connection string tells Entity Framework what server to go to and what database to use.

### Setting up a database with .Net CLI

One way of creating a database is via the .NET CLI (dotnet). First we need to add migrations to our project. A migration is C# code that can be executed to create a database and database schema. It is generated by Entity Framework, which looks first at existing databases. If none exist it starts from scratch, looking to our DbContext and entities to formulate the schema. If we add more entities and update existing ones, we can create further migrations and add them to the project to ensure our database schema is in sync.

Using the command... 
```bash 
dotnet ef migration add <name_of_class>
``` 
...on the command line (when current directory is your project directory) adds a migrations folder and our migration to the project. A migration looks something like this:

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Restaurants",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Cuisine = table.Column<int>(nullable: false),
                Name = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Restaurants", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Restaurants");
    }
}
```
We can see from the above that the injected migrationBuilder is creating a table with a matching name to the `Restaurants` entity. Within this method the columns are set up, and the `Id` column is set as the `IdentityColumn`, which ensures the database generates IDs when a new entry is added to the table.

Using the command... 
```bash 
dotnet ef database update
``` 
...applies any migrations to our database - naturally this only works if there are migrations in our project.

By default if Entity Framework finds an `Id` property on an entity, it'll make it the primary key in the database table.

## POST - Redirect - GET Pattern

This design pattern is pretty straight forward. We begin with a HTTP POST which is comprised of some submitted input data from the user. When the user submits this data it is posted back to the server, then we redirect the user to a different URL and retrieve this data in a HTTP GET to render it to the view.

As an example of bad practice, we might be inclined to simply return the Details view of the new restaurant immediately once the user clicks the save button like so:

```CSharp
public IActionResult Create(RestaurantEditModel model)
{
    if (ModelState.IsValid)
    {
        var newRestaurant = new Restaurant();
        newRestaurant.Name = model.Name;
        newRestaurant.Cuisine = model.Cuisine;

        newRestaurant = _restaurantData.Add(newRestaurant);
        
        return View("Details", newRestaurant);
    }
    else
    {
        return View();
    }
}
```
The trouble with this is that we end up returning the view to the current request - i.e. the URL to which we're returning the view is "/Home/Create". The biggest problem with this approach is that if we then refresh the page, the submitted data is then resubmitted creating a duplicate entry. Most modern browsers give a warning when a HTTP POST is about to be resent, but this should be rectified on the application side to prevent this issue in the first place.

We can instead amend the return statement to the following:

```CSharp
return RedirectToAction(nameof(Details), new { id = newRestaurant.Id });
```
This is MVC's simple approach to implementing the `GET` part of the design pattern. `RedirectToAction` takes in an action name and an object for the route value (this determines the id part of the redirect URL). We've been using `Details()` to handle a detail view of each restaurant, so we provide this as the action, then create an anonymous object with a property `id` and assign it the value of `newRestaurant.Id`. Simply put, the URL will end up as /Home/Details/*IdOfNewRestaurant*.

## Security concerns

For want of a better place to put these parts, I'm just jotting all security related items in this section.

### Mass assignment/Overposting
This is an attack where values are set on properties in the server that a developer does not expect. It's often seen in sites built on the MVC design pattern during model binding, where a form provides fields for a user to enter values for a set of properties bound to a model. If the model contains other fields that are not included in the form, malicious users could still access those properties and provide unwanted values.

**For example:**

```CSharp
public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public CuisineOrigin Cuisine { get; set; }
}
```
```CSharp
[HttpPost]
public IActionResult Create(Restaurant model)
{
    if (ModelState.IsValid)
    {
        var newRestaurant = new Restaurant();
        newRestaurant.Name = model.Name;
        newRestaurant.Cuisine = model.Cuisine;

        newRestaurant = _restaurantData.Add(newRestaurant);
        
        return View("Details", newRestaurant);
    }
    else
    {
        return View();
    }
}
```
```cshtml
@using OdeToFood.Models
@model Restaurant

<h1>Create</h1>
<form  method="post">
    <div>
        <label asp-for="Name"></label>
        <input asp-for="Name" />
        <span asp-validation-for="Name"></span>
    </div>
    <div>
        <label asp-for="Cuisine"></label>
        <select asp-for="Cuisine"
                asp-items="@Html.GetEnumSelectList<CuisineOrigin>()">
        </select>
    </div>
    <input type="submit" name="Save" value="Save" />
</form>
```
In the above, we have a `Restaurant` model with three properties: Id, Name and Cuisine. We then have a HttpPost `Create` action on a controller that takes in a Restaurant object and handles adding the submission to the server. Finally we have our view that will present two form fields to the user: Name and Cuisine.

By normal standards, the user will only be able to fill in those two fields, which is great because we want the Id to be automatically generated to ensure it's unique. However, we could manipulate the HTML or use a tool to add a value for that Id property regardless of what's available in the page.

To avoid this issue, there are many solutions. My favourite currently is separating logic into a binding/input model and a view/output model.

It makes logical sense too if we look at it from a separation of concerns point of view. The output model contains all properties that should be displayed, and the input model contains only those properties that are required for binding. The obvious drawback to this option is that it requires duplication of effort and any changes need to be performed in two places. We could amend this to make the input model a base class that the output model could derive from, in which case we only need to add the extra properties that should not be edited.

N.B. I've seen criticism of this approach and various suggestions of the best way to approach the problem. Further reading here: https://andrewlock.net/preventing-mass-assignment-or-over-posting-in-asp-net-core/

### Cross-site forgery requests

In it's simplest terms, cross-site forgery requests happen when a user is signed into a legitimate site (e.g. their bank account), then they access a malicious site which attempts to send a request to the former site using the user's authenticated session. There are many ways to deal with this (and some specific to different frameworks), but I'm just going to talk about the one I've encountered so far.

From ASP.NET Core 2.0, whenever a form exists on a Razor View with `method="post"` (all forms should be posted), `FormTagHelper` injects a hidden `RequestVerificationToken` into a child input element.

**For example:**

```html
<form method="post">
    <...>
    </...>
    <input type="submit" name="Save" value="Save">
    <input name="__RequestVerificationToken" type="hidden" value="CfDJ8NrAkS ... s2-m9Yw">
</form>
```
Prior to 2.0, anti-forgery tokens were auto-generated when using the IHtmlHelper.BeginForm method and they can be added explicitly using the `@Html.AntiForgeryToken` helper as a child of a form element.

We can validate this token in a number of ways. Before we get to that, it's important to note that ASP.NET doesn't automagically generate anti-forgery tokens for safe requests (GET, HEAD, OPTIONS, TRACE). This could leave us with a predicament if we were to use a `[ValidateAntiForgeryToken]` attribute. If we use it at controller (class) level, it would reject all of our safe requests because it doesn't find a token on them. If we're using it on individual actions, this leaves us open to missing a POST action and leaves us exposed to a CSRF attack. Now we could use this attribute at the class level, then apply an `[IgnoreAntiForgeryToken]` attribute to each safe action that doesn't need validation, but that also leaves us at the risk of missing one, or adding it to a POST request accidentally.

Thankfully, reading the Microsoft docs, their recommended approach is to use an `[AutoValidateAntiforgeryToken]` attribute on the controller in question. It checks all POST requests for a token, but ignores all safe requests as if we'd correctly assigned them with the `[IgnoreAntiForgeryToken]` attribute.

It's very important to note that this only works if all of our forms requiring input from the user use POST methods. If they use safe methods, it will be overlooked in the validation and we're open to attack.

A final note to say that it's possible to add these tokens at a global application level, but I'm not yet sure how.

## Research topics to follow on from this project
- Unit of Work and Repository design patterns in relation to DbContext
- Differences between `IEnumerable<T>` and `IQueryable<T>` with regards to large datasets
- Treating DbContext as a unit of work

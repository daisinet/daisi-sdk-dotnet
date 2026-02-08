# Daisi's .Net SDK
This is the .Net 10 SDK for interacting with the DAISI network.

Documentation available on [our website](https://daisi.ai/Learn/SDK).

Quickstart Guide is [here](https://daisi.ai/Learn/SDK/dotnet/QuickStart).

Nuget install with Package Manager Console:

```
Install-Package Daisi.SDK
```

### Project - Daisi.SDK
Core functionality across most of the Daisi .Net codebase. Almost every project references this either directly or indirectly. Use this for gaining access to all of the clients that communicate with Orcs and Hosts.

### Project - Daisi.SDK.Razor
A set of Razor components that are useful when building out new apps in the Daisi network. DaisiChat is available here and can allow you to easily add a chat window to any .Net Razor project.

### Project - Daisi.SDK.Web
Middleware and other website related projects that are not components. This is useful for adding DAISI authentication and authorization to your .Net web applications.

### Project - Daisi.SDK.Tests
Unit testing project for the SDK. Coverage is very light. We could use some help here as it's not a strength existing on the team at this time.

# Examples
## Daisi.Console.Chat
The example Console app is meant to show the bare minimum needed to get started. Should give a simple Basic thinking chat in a console window. This was moved into this project from it's own repo to make it easier to keep it up to date with the SDK changes.
You will need a secret key as provided by the [Daisi Manager](https://manager.daisi.ai). 
View our SDK documentation [on our website](https://daisi.ai/Learn/SDK).

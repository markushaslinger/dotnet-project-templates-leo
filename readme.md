# HTL Leonding .NET/C# Project Templates

Used to create projects for assignments [@HTL Leonding](https://www.htl-leonding.at/).

## Installation

>  `dotnet new install HTLLeonding.Utility.Templates`

### Updates

Running the install command above again will update the templates package to the latest version.

## Usage

Do not forget to update referenced NuGet packages (and potentially .NET version) in the projects after creation!

### `leoconsole`

Console Application

- Main project + unit test project
    - With solution file
    - Optional `--tests false` flag can be added to _not_ generate a unit test project
- Includes .gitignore file
- Includes .editorconfig file

> Syntax: `dotnet new leoconsole -n <ASSIGNMNET_NAME> -o . [--tests false]`

> Example: `dotnet new leoconsole -n VendingMachine -o .`

### `leominiapi`

Minimal WebAPI (REST) Application

- Main project, with:
    - Basic libraries
    - JSON Serializer Configuration
    - Prepared DI setup
    - A single demo endpoint
    - OpenApi
- Meant for development/container internal use (no HTTPS)
- Includes a `http` requests file with sample requests
- Includes .gitignore file
- Includes .editorconfig file
- By default binds to port 5200

> Syntax: `dotnet new leominiapi -n <ASSIGNMENT_NAME> -o .`

> Example: `dotnet new leominiapi -n PoolGuard -o .`

### `leoblazorpages`

Blazor WASM Application for GitHub Pages deployment

- A simple, single project Blazor WASM template
- Set up to be easily deployed to GitHub Pages with very little manual configuration
    - Pages has to be enabled in GitHub, instructions are in the template readme
- Demonstrates:
    - Dynamic content and events (buttons)
    - Loading data via HTTP request and showing it in a table
    - CSS isolation
- Includes .gitignore
- Includes .editorconfig file
- By default binds to port 5250
- HTTPS config removed on purpose (will be HTTPS on GH Pages) to keep the first steps simpler
- **If the repository is owned by an organization (classrooms!) the organization admin has to enable read & write access for actions in the organization settings, otherwise the bot is not allowed to create the `gh-pages` branch!**

> Syntax: `dotnet new leoblazorpages -n <ASSIGNMENT_NAME> -o .`

> Example: `dotnet new leoblazorpages -n HerbalGarden -o .`
# HTL Leonding .NET/C# Project Templates

Used to create projects for assignments.

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
- Includes .gitignore file

> Syntax: `dotnet new leoconsole -n <ASSIGNMNET_NAME> -o .`

> Example: `dotnet new leoconsole -n VendingMachine -o .`
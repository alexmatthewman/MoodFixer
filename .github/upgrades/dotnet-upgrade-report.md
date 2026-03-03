# .NET 10 Upgrade Report

## Project target framework modifications

| Project name              | Old Target Framework | New Target Framework | Commits  |
|:--------------------------|:--------------------:|:--------------------:|----------|
| AIRelief\\AIRelief.csproj |       net9.0         |       net10.0        | 2db7f245 |

## NuGet Packages

| Package Name                                         | Old Version | New Version | Commit Id |
|:-----------------------------------------------------|:-----------:|:-----------:|-----------|
| Microsoft.AspNetCore.Authentication.Google           |   9.0.8     |   10.0.3    | 0af9904f  |
| Microsoft.AspNetCore.Authentication.MicrosoftAccount |   9.0.8     |   10.0.3    | 0af9904f  |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore    |   9.0.8     |   10.0.3    | 0af9904f  |
| Microsoft.AspNetCore.Identity.UI                     |   9.0.8     |   10.0.3    | 0af9904f  |
| Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation    |   9.0.8     |   10.0.3    | 0af9904f  |
| Microsoft.EntityFrameworkCore                        |   9.0.8     |   10.0.3    | 0af9904f  |
| Microsoft.EntityFrameworkCore.Sqlite                 |   9.0.8     |   10.0.3    | 0af9904f  |
| Microsoft.EntityFrameworkCore.SqlServer              |   9.0.8     |   10.0.3    | 0af9904f  |
| Microsoft.EntityFrameworkCore.Tools                  |   9.0.8     |   10.0.3    | 0af9904f  |
| Microsoft.VisualStudio.Web.CodeGeneration.Design     |   9.0.0     |   10.0.2    | 0af9904f  |

## All commits

| Commit ID | Description                                  |
|:----------|:---------------------------------------------|
| f48735b7  | Upgrade plan committed                       |
| 2db7f245  | Update AIRelief.csproj to target .NET 10.0   |
| 0af9904f  | Update NuGet package versions in AIRelief.csproj |

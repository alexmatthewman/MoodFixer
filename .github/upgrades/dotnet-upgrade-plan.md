# .NET 10 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 10 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10 upgrade.
3. Upgrade AIRelief/AIRelief.csproj to .NET 10.

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

No projects are excluded from this upgrade.

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects that need version updates.

| Package Name                                          | Current Version | New Version | Description                             |
|:------------------------------------------------------|:---------------:|:-----------:|:----------------------------------------|
| Microsoft.AspNetCore.Authentication.Google            |   9.0.8         |  10.0.3     | Recommended for .NET 10                 |
| Microsoft.AspNetCore.Authentication.MicrosoftAccount  |   9.0.8         |  10.0.3     | Recommended for .NET 10                 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore     |   9.0.8         |  10.0.3     | Recommended for .NET 10                 |
| Microsoft.AspNetCore.Identity.UI                      |   9.0.8         |  10.0.3     | Recommended for .NET 10                 |
| Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation     |   9.0.8         |  10.0.3     | Recommended for .NET 10                 |
| Microsoft.EntityFrameworkCore                         |   9.0.8         |  10.0.3     | Recommended for .NET 10                 |
| Microsoft.EntityFrameworkCore.Sqlite                  |   9.0.8         |  10.0.3     | Recommended for .NET 10                 |
| Microsoft.EntityFrameworkCore.SqlServer               |   9.0.8         |  10.0.3     | Recommended for .NET 10                 |
| Microsoft.EntityFrameworkCore.Tools                   |   9.0.8         |  10.0.3     | Recommended for .NET 10                 |
| Microsoft.VisualStudio.Web.CodeGeneration.Design      |   9.0.0         |  10.0.2     | Recommended for .NET 10                 |

### Project upgrade details

#### AIRelief\AIRelief.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.Authentication.Google should be updated from `9.0.8` to `10.0.3` (*recommended for .NET 10*)
  - Microsoft.AspNetCore.Authentication.MicrosoftAccount should be updated from `9.0.8` to `10.0.3` (*recommended for .NET 10*)
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore should be updated from `9.0.8` to `10.0.3` (*recommended for .NET 10*)
  - Microsoft.AspNetCore.Identity.UI should be updated from `9.0.8` to `10.0.3` (*recommended for .NET 10*)
  - Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation should be updated from `9.0.8` to `10.0.3` (*recommended for .NET 10*)
  - Microsoft.EntityFrameworkCore should be updated from `9.0.8` to `10.0.3` (*recommended for .NET 10*)
  - Microsoft.EntityFrameworkCore.Sqlite should be updated from `9.0.8` to `10.0.3` (*recommended for .NET 10*)
  - Microsoft.EntityFrameworkCore.SqlServer should be updated from `9.0.8` to `10.0.3` (*recommended for .NET 10*)
  - Microsoft.EntityFrameworkCore.Tools should be updated from `9.0.8` to `10.0.3` (*recommended for .NET 10*)
  - Microsoft.VisualStudio.Web.CodeGeneration.Design should be updated from `9.0.0` to `10.0.2` (*recommended for .NET 10*)

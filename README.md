# dotnetCampus.UpdateAllDotNetTools

Archive : This feature has been officially implemented by dotnet. Please use the `dotnet tool update --all` command to update all tools. See https://github.com/dotnet/sdk/pull/38996

---

The dotnet tool that can update all dotnet tools

| Build | NuGet |
| -- | -- |
|![](https://github.com/dotnet-campus/dotnetCampus.UpdateAllDotNetTools/workflows/.NET%20Core/badge.svg)|[![](https://img.shields.io/nuget/v/dotnetCampus.UpdateAllDotNetTools.svg)](https://www.nuget.org/packages/dotnetCampus.UpdateAllDotNetTools)|

## Installation

```
dotnet tool install --global dotnetCampus.UpdateAllDotNetTools 
```

## Usage

```
dotnet updatealltools
```

Or update all tools to prerelease version by:

```
dotnet UpdateAllTools --prerelease
```

# OpenTKAnalyzer
Analyzer for OpenTK (unofficial)

## Status
[![Issues](https://img.shields.io/github/issues/occar421/opentkanalyzer.svg?style=flat-square)](https://github.com/occar421/OpenTKAnalyzer/issues)
[![Release](https://img.shields.io/github/release/occar421/opentkanalyzer.svg?style=flat-square)](https://github.com/occar421/opentkanalyzer/releases/latest)
[![NuGet Version](https://img.shields.io/nuget/v/OpenTKAnalyzer.svg?style=flat-square)](https://www.nuget.org/packages/OpenTKAnalyzer/)
[![NuGet Donwloads](https://img.shields.io/nuget/dt/OpenTKAnalyzer.svg?style=flat-square)](https://www.nuget.org/packages/OpenTKAnalyzer/)
[![License](https://img.shields.io/github/license/occar421/opentkanalyzer.svg?style=flat-square)](https://github.com/occar421/OpenTKAnalyzer/blob/master/LICENSE)  
master:[![Build status(master)](https://img.shields.io/appveyor/ci/occar421/opentkanalyzer/master.svg?style=flat-square)](https://ci.appveyor.com/project/occar421/opentkanalyzer/branch/master)  
develop:[![Build status(develop)](https://img.shields.io/appveyor/ci/occar421/opentkanalyzer/develop.svg?style=flat-square)](https://ci.appveyor.com/project/occar421/opentkanalyzer/branch/develop)

## Description
This analyzer improves your OpenTK code (fewer bug and guide).  

## Versioning
In appveyor.yml line 1, version is defined. And stage(alpha, beta etc...) is before_build script.
### format
<dl>
    <dt>nupkg</dt>
    <dd>[major].[minor](.[revision])(-[stage])</dd>
    <dd>1.2-alpha, 3.4.5-beta, 6.7</dd>
	<dt>assembly</dt>
	<dd>[major].[minor].[revision].[build]</dd>
    <dd>1.2.0.1, 3.4.5.2, 6.7.0.3</dd>
</dl>
Change them when we make release branch.  
Publish to nuget runs only on master branch.

## Development Envirionment
Visual Studio Community 2015 (on Windows 10 Pro)
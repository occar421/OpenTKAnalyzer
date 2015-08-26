# OpenTKAnalyzer
Analyzer for OpenTK (unofficial)  
This project is just started. Your ideas and pull requests(of course issues) are WELCOME!

## Status
[![Issues](https://img.shields.io/github/issues/occar421/opentkanalyzer.svg?style=flat-square)](https://github.com/occar421/OpenTKAnalyzer/issues)
[![Release](https://img.shields.io/github/release/occar421/opentkanalyzer.svg?style=flat-square)](https://github.com/occar421/opentkanalyzer/releases/latest)
[![NuGet Version](https://img.shields.io/nuget/v/OpenTKAnalyzer.svg?style=flat-square)](https://www.nuget.org/packages/OpenTKAnalyzer/)
[![NuGet Donwloads](https://img.shields.io/nuget/dt/OpenTKAnalyzer.svg?style=flat-square)](https://www.nuget.org/packages/OpenTKAnalyzer/)
[![License](https://img.shields.io/github/license/occar421/opentkanalyzer.svg?style=flat-square)](https://github.com/occar421/OpenTKAnalyzer/blob/master/LICENSE)

|master|develop|
|---|---|
|[![Build status(master)](https://img.shields.io/appveyor/ci/occar421/opentkanalyzer/master.svg?style=flat-square)](https://ci.appveyor.com/project/occar421/opentkanalyzer/branch/master)|[![Build status(develop)](https://img.shields.io/appveyor/ci/occar421/opentkanalyzer/develop.svg?style=flat-square)](https://ci.appveyor.com/project/occar421/opentkanalyzer/branch/develop)|

## Description
This analyzer improves your OpenTK code (fewer bug and guide).  
Currently, this contains analyzers below.
### 0.1.0-alpha
+ GL.Begin and GL.End comformity.
+ Prevent mistakes of degree value and radian value in OpenTK Math Library.
+ GL.PushMatrix and GL.PopMatrix conformity. Other Push Pop functions are supported.
+ Check lighting enabling by GL.Enable.
+ Check GL.Light param(third argument) type is correct. 

## Versioning
In appveyor.yml line 1, version is defined. And stage(alpha, beta or release) is line 3.
### format
<dl>
    <dt>nupkg</dt>
    <dd>[major].[minor].[revision](-[stage])</dd>
    <dd>sample: 1.2.0-alpha, 3.4.5-beta, 6.7.8</dd>
	<dt>assembly</dt>
	<dd>[major].[minor].[revision].[build]</dd>
    <dd>sample: 1.2.0.1, 3.4.5.2, 6.7.8.3</dd>
</dl>
Change them and NuGet's release note when we make release branch.

## Development Envirionment
Visual Studio Community 2015 (on Windows 10 Pro)  
Powered by AppVeyor, every pull request to develop branch is built automatically.
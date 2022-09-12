// Addins
#tool "nuget:?package=NUnit.ConsoleRunner"

var solutionPath = "../src";

// Arguments
var target = Argument("target", "BuildAndTest");
var buildType = Argument("configuration", "Release");
var buildVersion = Argument("suffix", "");

// Environment variables
var nugetApiKey = EnvironmentVariable("NUGET_API_KEY");
string versionSuffix = null;

// Targets
Task("Clean")
    .Does(() => {
        DotNetClean(solutionPath);
    });

Task("Build")
    .IsDependentOn("Clean")
    .Does(() => 
    {
        var buildSettings = new DotNetBuildSettings
        {
            Configuration = buildType,
            VersionSuffix = versionSuffix
        };

        Information("NuGet version suffix: " + buildSettings.VersionSuffix);
        DotNetBuild(solutionPath, buildSettings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() => {
        DotNetTest(solutionPath);
    });

Task("Publish")
    .IsDependentOn("Test")
    .Does(() => 
    {
        var pkgPath = GetFiles($"./**/bin/Release/*{versionSuffix ?? string.Empty}.nupkg").First();
        
        if (string.IsNullOrEmpty(nugetApiKey))
        {
            Error("NuGet API key not set");
            return;
        }

        NuGetPush(pkgPath, new NuGetPushSettings
        {
            ApiKey = nugetApiKey,
            Source = "https://api.nuget.org/v3/index.json"
        });
    });

RunTarget(target);
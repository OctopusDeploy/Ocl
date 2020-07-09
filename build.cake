//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#module nuget:?package=Cake.DotNetTool.Module&version=0.4.0
#tool "dotnet:?package=GitVersion.Tool&version=5.3.5"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var artifactsDir = "./artifacts/";
var localPackagesDir = "../LocalPackages";
GitVersion gitVersionInfo;
string nugetVersion;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
    gitVersionInfo = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json,
        NoFetch = true
    });

    if(BuildSystem.IsRunningOnTeamCity)
        BuildSystem.TeamCity.SetBuildNumber(gitVersionInfo.NuGetVersion);

    nugetVersion = gitVersionInfo.NuGetVersion;

    Information("Building Octopus.Hcl v{0}", nugetVersion);
    Information("Informational Version {0}", gitVersionInfo.InformationalVersion);
});

Teardown(context =>
{
    Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() => {
		CleanDirectory(artifactsDir);
		CleanDirectories("./source/**/bin");
		CleanDirectories("./source/**/obj");
		CleanDirectories("./source/**/TestResults");
	});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => RunTarget("_Restore"));

Task("_Restore")
    .Does(() => {
		DotNetCoreRestore("./source");
    });

Task("Build")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does(() => RunTarget("_Build"));

Task("_Build")
    .Does(() => {
		 DotNetCoreBuild("./source", new DotNetCoreBuildSettings
		{
			NoRestore = true,
			Configuration = configuration,
			ArgumentCustomization = args => args.Append($"/p:Version={nugetVersion}")
		});
	});

Task("Test")
    .IsDependentOn("Build")
    .Does(() => RunTarget("_Test"));

Task("_Test")
    .Does(() => {
        DotNetCoreTest("./source", new DotNetCoreTestSettings
			{
				NoRestore = true,
				NoBuild = true,
				Configuration = configuration,
			});
	});

Task("CopyToArtifacts")
    .IsDependentOn("Test")
    .Does(() => RunTarget("_CopyToArtifacts"));

Task("_CopyToArtifacts")
    .Does(() => {
		CreateDirectory(artifactsDir);
		CopyFiles($"./source/**/*.nupkg", artifactsDir);
	});

Task("CopyToLocalPackages")
    .IsDependentOn("CopyToArtifacts")
    .WithCriteria(BuildSystem.IsLocalBuild)
    .Does(() => RunTarget("_CopyToLocalPackages"));

Task("_CopyToLocalPackages")
    .Does(() => {
		CreateDirectory(localPackagesDir);
		CopyFiles($"./source/**/*.nupkg", localPackagesDir);
	});


Task("Default")
    .IsDependentOn("CopyToArtifacts")
    .IsDependentOn("CopyToLocalPackages");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);

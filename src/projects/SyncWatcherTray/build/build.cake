#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin nuget:?package=Cake.Git
#r "..\..\tools\CakeExtensions\build\Extensions\CakeExtensions.dll"

string target = Argument<string>("target", "Default");
string outputDir = Argument<string>("output", @".\publish");
string buildNumber = Argument<string>("buildNumber", "666");
string gitVersion = Argument<string>("gitVersion", "v1.1-80-gdb791cd"); //may be db791cd if no tags present
string gitBranch = Argument<string>("gitBranch", "develop");
bool isMultiStage = Argument<bool>("multistage", false);
string buildConfiguration = Argument<string>("buildconfig", "Release");

//make outputdir absolute
outputDir = System.IO.Path.GetFullPath(outputDir);

List<(string, string, PlatformTarget)> solutions = new List<(string, string, PlatformTarget)>
{
	(@"..\..\..\solutions\Projects.sln", buildConfiguration, PlatformTarget.x64),
};

const string ASSEMBLIES_FILE = "assemblies.txt";
string[] assemblyInfoFiles = new []{ @"..\..\..\solutions\VersionInfo.cs"};

string nsiScript = @".\build.nsi";

//get version number
GetVersionNumber(gitVersion, gitBranch, buildNumber, out string shortVersion, out string longVersion);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Restore-NuGet-Packages")
    .WithCriteria(!isMultiStage)
    .DoesForEach(solutions, (_solution) =>
{
	string solutionFile = _solution.Item1;
    NuGetRestore(solutionFile);
});

Task("Update-Version")
    .WithCriteria(!isMultiStage)
	.DoesForEach(assemblyInfoFiles, (_assemblyInfoFile) =>
{
	CreateAssemblyInfo(_assemblyInfoFile, new AssemblyInfoSettings {
		Version = shortVersion,
		FileVersion = shortVersion,
		InformationalVersion = longVersion,
		Copyright = string.Format("Copyright (c) mkramers 2017 - {0}", DateTime.Now.Year)
	});
});

Task("Build")
    .WithCriteria(!isMultiStage)
    .DoesForEach(solutions, (_solution) =>
{
	string solutionFile = _solution.Item1;
	string configuration = _solution.Item2;
	PlatformTarget platform = _solution.Item3;

	Information($"Solution: {solutionFile} with Configuration: {configuration}");

	MSBuildSettings settings = new MSBuildSettings
	{
		Verbosity = Verbosity.Minimal,
		Configuration = configuration,
		MaxCpuCount = 0,
		PlatformTarget = platform,
	};
	settings.WithTarget("Build");

    MSBuild(solutionFile, settings);
});

Task("Build-Installer")
    .Does(() =>
{
	//make sure this exists before we start creating artifacts
	EnsureDirectoryExists(outputDir);

	//our long version will be used in the installer file output, so make it filesystem-safe
	string cleanLongVersion = ReplaceIllegalChars(longVersion, ".");

	Dictionary<string, string> defines = new Dictionary<string, string>();
	defines.Add("PUBLISH_DIR", outputDir);
	defines.Add("SHORTVERSION", shortVersion);
	defines.Add("LONGVERSION", cleanLongVersion);

	MakeNSISSettings nsisSettings = new MakeNSISSettings
	{
		NoConfig = true,
		Defines = defines,
	};

	MakeNSIS(nsiScript, nsisSettings);
});

Task("Restore-Version")
    .WithCriteria(!isMultiStage)
	.Does(() =>
{
	var filePaths = assemblyInfoFiles.Select(_assemblyInfoFile => new FilePath(_assemblyInfoFile)).ToArray();
	GitCheckout(@"..\..\..\..\", filePaths);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Update-Version")
    .IsDependentOn("Build")
    .IsDependentOn("Build-Installer")
    .IsDependentOn("Restore-Version");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

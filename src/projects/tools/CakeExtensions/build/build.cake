#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin nuget:?package=Cake.Git
#r ".\Extensions\CakeExtensions.dll"

string target = Argument<string>("target", "Default");
string outputDir = Argument<string>("output", @".\publish");
string buildNumber = Argument<string>("buildNumber", "666");
string gitVersion = Argument<string>("gitVersion", "v1.1-80-gdb791cd"); //may be db791cd if no tags present
string gitBranch = Argument<string>("gitBranch", "develop");
string buildConfiguration = Argument<string>("buildconfig", "CakeExtensionsRelease");

PlatformTarget platformTarget = PlatformTarget.MSIL;

//make outputdir absolute
outputDir = System.IO.Path.GetFullPath(outputDir);

List<(string, string)> solutions = new List<(string, string)>
{
	(@"..\..\..\..\solutions\Projects.sln", buildConfiguration),
};

const string ASSEMBLIES_FILE = "assemblies.txt";
string[] assemblyInfoFiles = System.IO.File.ReadAllLines(ASSEMBLIES_FILE);

//get version number
GetVersionNumber(gitVersion, gitBranch, buildNumber, out string shortVersion, out string longVersion);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Restore-NuGet-Packages")
    .DoesForEach(solutions, (_solution) =>
{
	string solutionFile = _solution.Item1;
    NuGetRestore(solutionFile);
});

Task("Update-Version")
	.DoesForEach(assemblyInfoFiles, (_assemblyInfoFile) =>
{
	Information($"Updating {_assemblyInfoFile} to v{shortVersion} ({longVersion})");
	CreateAssemblyInfo(_assemblyInfoFile, new AssemblyInfoSettings {
		Version = shortVersion,
		FileVersion = shortVersion,
		InformationalVersion = longVersion,
		Copyright = string.Format("Copyright (c) mkramers 2017 - {0}", DateTime.Now.Year)
	});
});

Task("Inspect")
    .DoesForEach(solutions, (_solution) =>
{
	//temp skip
	return;
	
	string solutionFile = _solution.Item1;
	string configuration = _solution.Item2;
	string shortname= System.IO.Path.GetFileNameWithoutExtension(solutionFile);

	Information($"Inspecting sln: {solutionFile} configuration: {configuration} platform= {platformTarget}");

	Dictionary<string, string> msBuildProperties = new Dictionary<string, string>();
	msBuildProperties.Add("configuration", configuration);
	msBuildProperties.Add("platform", platformTarget.ToString());

	InspectCode(solutionFile, new InspectCodeSettings {
		SolutionWideAnalysis = true,
		MsBuildProperties = msBuildProperties,
		OutputFile = Directory(outputDir) + File($"{shortname}_inspectcode-output.xml"),
		ThrowExceptionOnFindingViolations = true
	});
});

Task("Build")
    .DoesForEach(solutions, (_solution) =>
{
	string solutionFile = _solution.Item1;
	string configuration = _solution.Item2;

	Information($"Solution: {solutionFile} with Configuration: {configuration}");

	MSBuildSettings settings = new MSBuildSettings
	{
		Verbosity = Verbosity.Minimal,
		Configuration = configuration,
		MaxCpuCount = 0,
		PlatformTarget = platformTarget,
	};
	settings.WithTarget("Clean,Rebuild");

    MSBuild(solutionFile, settings);
});

Task("Publish")
	.Does(() =>
{
	//hardcoded for any cpu Release!
	
	const string cakeExtensionsFileName = "CakeExtensions.dll";
	
	string inputFilePath = System.IO.Path.Combine(@"..\bin\Release", cakeExtensionsFileName);
	
	string outputFilePath = System.IO.Path.Combine(outputDir, cakeExtensionsFileName);
	
	Information($"Copying {inputFilePath} to {outputFilePath}...");
	
	EnsureDirectoryExists(outputDir);
	
	CopyFile(inputFilePath, outputFilePath);
});


Task("Restore-Version")
	.Does(() =>
{
	var filePaths = assemblyInfoFiles.Select(_assemblyInfoFile => new FilePath(_assemblyInfoFile)).ToArray();
	GitCheckout(@"..\..\..\..\..\", filePaths);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Update-Version")
    .IsDependentOn("Inspect")
    .IsDependentOn("Build")
    .IsDependentOn("Publish");
    //.IsDependentOn("Restore-Version");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

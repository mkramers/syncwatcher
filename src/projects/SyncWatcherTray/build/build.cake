#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var buildNumber = Argument<string>("buildNumber", "0");
var changeListNumber = Argument<string>("changeList", "0");
var outputDir = Argument<string>("output", "./publish");
var assemblyInfoFileList = Argument<string>("assemblyInfoFileList", "assemblyInfoFileList.txt");
var archiveDir = Argument<string>("archive", "./archive");
var isQuickMode = Argument<bool>("quick", false); 

//////////////////////////////////////////////////////////////////////
//PROJECT
//////////////////////////////////////////////////////////////////////

var solutionFile = "../../solutions/syncwatcher/syncwatcher.sln";
var projectDir = "../../projects/syncwatchertray/bin";
var nsisScript = "./buildInstaller.nsi";
var installerFilename = "syncwatcher.exe";

var version = "1.0";		
var fullVersion = string.Concat(version + "." + buildNumber);
var semVersion = string.Concat(fullVersion + "." + changeListNumber);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

Information("\n==========\n");

// Define directories.
var buildDir = Directory(projectDir) + Directory(configuration);

var cleanDirs = new []{outputDir, buildDir};

Information($"Reading assemblyinfo files list from {assemblyInfoFileList}...");

var assemblyInfoFiles = System.IO.File.ReadAllLines(assemblyInfoFileList);

Information($"Found {assemblyInfoFiles.Length} files");

var stages = new Dictionary<string, string>();
stages.Add(buildDir, "bin");

//////////////////////////////////////////////////////////////////////
// TASKS	
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
}).DoesForEach(cleanDirs, (dir) => 
{ 
	Information($"Cleaning: {dir}");
	if (DirectoryExists(dir))
	{	
		DeleteDirectory(dir, new DeleteDirectorySettings {
			Recursive = true,
			Force = true
		});
	}
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionFile);
});

Task("Update-Version")
    .IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
	{		
	})
	.DoesForEach(assemblyInfoFiles, _assemblyInfoFile =>
	{
		var file = _assemblyInfoFile;
						
		Information($"Updating file: {file} with version: {semVersion}...");
		
		var assemblyInfo = ParseAssemblyInfo(file);	
		
		var wasReadOnly = false;
		System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
		if (fileInfo.IsReadOnly)
		{
			Information($"\tSetting \"{file}\" readonly=false");
			fileInfo.IsReadOnly = false;	
			wasReadOnly = true;			
		}
		
		Information($"\tWriting assembly file to \"{file}\"");
		CreateAssemblyInfo(file, new AssemblyInfoSettings {
			Version = fullVersion,
			FileVersion = fullVersion,
			InformationalVersion = semVersion,
		});
		
		fileInfo = new System.IO.FileInfo(file);
		if (wasReadOnly)
		{
			Information($"\tRestoring \"{file}\" readonly=true");
			fileInfo.IsReadOnly = true;		
		}
	});

Task("Inspect")
    .IsDependentOn("Update-Version")
    .Does(() =>
{
	if (isQuickMode)
	{
		Information("Skipping inspect due to quickmode=true");
		return;
	}
	
	var msBuildProperties = new Dictionary<string, string>();
	msBuildProperties.Add("configuration", configuration);
	msBuildProperties.Add("platform", "AnyCPU");

	InspectCode(solutionFile, new InspectCodeSettings {
		SolutionWideAnalysis = true,
		MsBuildProperties = msBuildProperties,
		OutputFile = Directory(outputDir) + File("inspectcode-output.xml"),
		ThrowExceptionOnFindingViolations = true
	});
});

Task("Build")
    .IsDependentOn("Update-Version")
    .Does(() =>
{
	var settings = new MSBuildSettings
	{
		Verbosity = Verbosity.Minimal,
		Configuration = configuration,		
	};
		
    MSBuild(solutionFile, settings);
});

Task("Stage")
    .IsDependentOn("Build")
	.Does(() => {})
    .DoesForEach(stages, stage =>
	{
		var source = stage.Key;
		var destination = System.IO.Path.Combine(outputDir, stage.Value);
		
		Information($"Staging directory \"{source}\" to \"{destination}\"");
		CopyDirectory(source, destination);	
	});

Task("Build-Installer")
    .IsDependentOn("Build")
    .Does(() =>
	{
		MakeNSIS(nsisScript);
	});

Task("Post-Clean")
    .IsDependentOn("Build-Installer")
    .Does(() =>
	{
		var destination = System.IO.Path.Combine(archiveDir, semVersion);
		
		if (!DirectoryExists(destination))
		{
			CreateDirectory(destination);
		}
		
		var destinationFile = System.IO.Path.Combine(destination, installerFilename);
		
		MoveFile(installerFilename, destinationFile);
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Post-Clean");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#r ".\Extensions\CakeExtensions.dll"

string target = Argument<string>("target", "Default");
string outputDir = Argument<string>("output", @".\publish");
bool isQuickMode = Argument<bool>("quick", true);
string buildNumber = Argument<string>("buildNumber", "666");
string gitVersion = Argument<string>("gitVersion", "v1.1-80-gdb791cd"); //may be db791cd if no tags present
string gitBranch = Argument<string>("gitBranch", "develop");
bool isMultiStage = Argument<bool>("multistage", false);
string fpsInstallerPath = Argument<string>("fpspath", "");
string buildConfiguration = Argument<string>("buildconfig", "All_Release");

//make outputdir absolute
outputDir = System.IO.Path.GetFullPath(outputDir);

List<(string, string, PlatformTarget)> solutions = new List<(string, string, PlatformTarget)>
{
	(@"..\..\..\Solutions\Projects.sln", buildConfiguration, PlatformTarget.x64),
};

const string ASSEMBLIES_FILE = "assemblies.txt";
string[] assemblyInfoFiles = System.IO.File.ReadAllLines(ASSEMBLIES_FILE);

string nsiScript = @".\buildInstaller.nsi";
const string fpsNsiScript = @"..\..\..\Shared\PacsWindowsService\build\pacsServiceInstaller.nsi";

//used for storing assemblyinfo files in memory throughout the script
Dictionary<string, MemoryStream> fileMemory = null;

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
	.Does(() =>
{			
	fileMemory = UpdateAssemblyInfoFiles(assemblyInfoFiles, shortVersion, longVersion);
});

Task("Inspect")
    .WithCriteria(!isMultiStage)
    .DoesForEach(solutions, (_solution) =>
{
	if (isQuickMode)
	{
		Information("Skipping inspect...");
		return;
	}
	
	string solutionFile = _solution.Item1;
	string configuration = _solution.Item2;
	string platform = _solution.Item3.ToString();
	string shortname= System.IO.Path.GetFileNameWithoutExtension(solutionFile);

	Information($"Inspecting sln: {solutionFile} configuration: {configuration} platform= {platform}");

	Dictionary<string, string> msBuildProperties = new Dictionary<string, string>();
	msBuildProperties.Add("configuration", configuration);
	msBuildProperties.Add("platform", platform);

	InspectCode(solutionFile, new InspectCodeSettings {
		SolutionWideAnalysis = true,
		MsBuildProperties = msBuildProperties,
		OutputFile = Directory(outputDir) + File($"{shortname}_inspectcode-output.xml"),
		ThrowExceptionOnFindingViolations = true
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

	//build pacs installer first if running standalone
	if (!isMultiStage)
	{		
		if (!string.IsNullOrWhiteSpace(fpsInstallerPath))
		{
			throw new Exception("fpspath should not be defined in standalone!");
		}
				
		//use the default installer path
		fpsInstallerPath = $@"{outputDir}\fps_{cleanLongVersion}.exe";

		Dictionary<string, string> fpsInstallerDefines = new Dictionary<string, string>(defines);
		fpsInstallerDefines.Add("OUT_FILE", fpsInstallerPath);

		MakeNSISSettings pacsInstallerSettings = new MakeNSISSettings
		{
			NoConfig = true,
			Defines = fpsInstallerDefines,
		};
		
		MakeNSIS(fpsNsiScript, pacsInstallerSettings);
	}
	
	defines.Add("FPS_INSTALLER_PATH", fpsInstallerPath);

	MakeNSISSettings bxInstallerSettings = new MakeNSISSettings
	{
		NoConfig = true,
		Defines = defines,
	};
	
	MakeNSIS(nsiScript, bxInstallerSettings);
});

Task("Restore-Version")
    .WithCriteria(!isMultiStage)
	.Does(() =>
{
	RestoreAssemblyInfoFiles(fileMemory);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Update-Version")
    .IsDependentOn("Inspect")
    .IsDependentOn("Build")    
    .IsDependentOn("Build-Installer")
    .IsDependentOn("Restore-Version");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

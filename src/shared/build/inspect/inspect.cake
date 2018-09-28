#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

string target = Argument<string>("target", "Default");
string solutionFilePath = Argument<string>("solution", "");
string outputDir = Argument<string>("output", @".\publish");

Task("Inspect")
    .Does(() =>
{
	string shortname= System.IO.Path.GetFileNameWithoutExtension(solutionFilePath);

	Information($"Inspecting sln: {solutionFilePath}");

	InspectCode(solutionFilePath, new InspectCodeSettings {
		SolutionWideAnalysis = true,
		OutputFile = Directory(outputDir) + File($"{shortname.ToLower()}.inspect.xml"),
		ThrowExceptionOnFindingViolations = false,
	});
});

Task("Default")
    .IsDependentOn("Inspect");

RunTarget(target);
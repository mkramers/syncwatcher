
string solutionFilePath = Argument<string>("solution", "");

Task("Inspect")
    .DoesForEach(solutions, (_solution) =>
{
	string shortname= System.IO.Path.GetFileNameWithoutExtension(solutionFilePath);

	Information($"Inspecting sln: {solutionFile}");

	InspectCode(solutionFilePath, new InspectCodeSettings {
		SolutionWideAnalysis = true,
		OutputFile = Directory(outputDir) + File($"{shortname.ToLower()}.inspect.xml"),
		ThrowExceptionOnFindingViolations = false,
	});
});

Task("Default")
    .IsDependentOn("Inspect");

RunTarget(target);
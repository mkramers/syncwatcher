using System;
using System.Diagnostics;
using System.IO;

class Program
{
	static void Main(string[] _args)
	{
		var outputFile = ""; 
		
		if (_args.Length < 1)
		{
			Error("Invalid arguments. Expected output path name!");
			return;
		}
		
		outputFile = _args[0];
		Debug.WriteLine($"Starting version finder using output file: {outputFile}");
		
		var tempFile = "out.txt"; 
		
		if (File.Exists(tempFile))
		{
			Debug.WriteLine($"Cleaning up {tempFile}");
			File.Delete(tempFile);
		}		
		if (File.Exists(outputFile))
		{
			Debug.WriteLine($"Cleaning up {outputFile}");
			File.Delete(outputFile);
		}
		
		Debug.WriteLine("Starting process");
		
		string command = $"/C powershell.exe Get-VSSetupInstance > {tempFile}";
		var process = Process.Start("CMD.exe", command);
		process.WaitForExit();
		
		if (!File.Exists(tempFile))
		{
			Error($"Can't find output file {tempFile}!");
			return;
		}		
		
		var fileLines = File.ReadAllLines(tempFile);
		if (fileLines.Length >= 6)
		{
			var pathLine = fileLines[5];
			var split = pathLine.Split(':');
			if (split.Length >= 2)
			{
				Console.WriteLine($"Whole line: {pathLine}");
				for (var i = 1; i < split.Length; i++)
				{
					Console.WriteLine($"Adding: {split[i]}");
				}
				
				var path = split[1];
				for (var i = 2; i < split.Length; i++)
				{
					path += $":{split[i]}";
				}
				
				path = Path.Combine(path, @"MSBuild\15.0\Bin\MSBuild.exe");
				path = path.Trim();
				
				Debug.WriteLine($"Found path: {path}");
				File.WriteAllText(outputFile, path);
			}			
		}
		
		Debug.WriteLine("Cleaning up");
		// File.Delete(tempFile);
	}
	
	static void Error(string _message)
	{		
        Debug.WriteLine(_message);
		Debug.WriteLine("Press any key to coninue");
        Console.ReadKey();
	}
}
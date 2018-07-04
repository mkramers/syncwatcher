using System;
using CommandLine;
using P4Commands;

namespace BuildTool.CommandLineDemo
{
    public class Test
    {
        public static int Do(string[] _args)
        {
            var result = Parser.Default.ParseArguments<P4GetChangelistOptions>(_args);
            var output = result.MapResult(GetChangelist, _ => MakeError());

            PrintIfNotEmpty(output);

            return output.Equals(MakeError()) ? 1 : 0;
        }

        private static void PrintIfNotEmpty(string _text)
        {
            if (_text.Length == 0)
            {
                return;
            }
            Console.WriteLine(_text);
        }

        private static string GetChangelist(P4GetChangelistOptions _opts)
        {
            var description = _opts.Description;
            var outputFile = _opts.OutputFile;

            Console.WriteLine($"Creating changelist \"{_opts.Description}\"...");

            var success = Commands.TryWriteChangelistToFile(description, outputFile, out var changelist);

            var message = success ? $"Created changelist #{changelist}" : "Failed to generate changelist!";
            return message;
        }

        private static string MakeError()
        {
            return "\n\n";
        }
    }
}
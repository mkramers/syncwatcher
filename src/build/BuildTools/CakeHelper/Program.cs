using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace CakeHelper
{
    internal class Program
    {
        private static void Main()
        {
            const string destinationDir = "../../stage";
            const string stagesFile = "../../stages.json";

            DoStaging(stagesFile, destinationDir);
            Console.ReadKey();
        }

        private static void DoStaging(string _stagesFile, string _destinationDir)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_stagesFile));
            Debug.Assert(!string.IsNullOrWhiteSpace(_destinationDir));

            //CreateTestStagesFile(_stagesFile);

            var output = File.ReadAllText(_stagesFile);
            var stageItems = JsonConvert.DeserializeObject<List<StageItem>>(output);

            //debug - create the files on the disk
            foreach (var item in stageItems)
            {
                CreateStageItemSource(item);
            }
            
            //execute stages
            foreach (var item in stageItems)
            {
                ExectuteStageItem(item, _destinationDir);
            }
        }

        private static void CreateTestStagesFile(string _stagesFile)
        {
            var stageItems = GetTestStageItems();

            //serialize items
            var output = JsonConvert.SerializeObject(stageItems);
            File.WriteAllText(_stagesFile, output);
        }

        private static void CreateStageItemSource(StageItem _item)
        {
            Debug.Assert(_item != null);

            var isFile = _item.IsFile;

            var source = _item.Source;
            
            if (isFile)
            {
                foreach (var file in source)
                {
                    var parentDir = Path.GetDirectoryName(file);
                    if (!Directory.Exists(parentDir))
                    {
                        Directory.CreateDirectory(parentDir);
                    }

                    if (!File.Exists(file))
                    {
                        File.WriteAllText(file, "hello file");
                    }
                }
            }
            else
            {
                foreach (var dir in source)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
            }
        }

        private static void ExectuteStageItem(StageItem _item, string _desinationDir)
        {
            Debug.Assert(_item != null);
            
            var source = _item.Source;
            var destinationDir = Path.Combine(_desinationDir, _item.DestinationDirectory);

            var isFile = _item.IsFile;
            if (isFile)
            {
                foreach (var file in source)
                {
                    Console.WriteLine($"File: {file}.");

                    var fileName = Path.GetFileName(file);
                    Debug.Assert(fileName != null);

                    var destination = Path.Combine(destinationDir, fileName);
                    Console.WriteLine($"\tDestination: {destination}");

                    var parentDir = Path.GetDirectoryName(destination);
                    Console.WriteLine($"\tParent dir: {parentDir}");
                    if (!Directory.Exists(parentDir))
                    {
                        Console.WriteLine("\t\tCreating parent dir");
                        Directory.CreateDirectory(parentDir);
                    }

                    File.Copy(file, destination, true);
                }
            }
            else
            {
                foreach (var dir in source)
                {
                    var dirName = new DirectoryInfo(dir).Name;
                    var destination = Path.Combine(destinationDir, dirName);

                    Console.WriteLine($"Copy directory {dir} to {destination}");
                    Copy(dir, destination);
                }
            }
        }

        private static List<StageItem> GetTestStageItems()
        {
            var items = new List<StageItem>
            {
                new StageItem
                {
                  Source  = new List<string>
                  {
                      "../../test/file.txt",
                      "../../test/file1.txt",
                      "../../test/file2.txt",
                      "../../test/file3.txt",
                      "../../test/a/file.txt",
                      "../../test/a/file1.txt",
                      "../../test/a/file2.txt",
                      "../../test/b/file.txt",
                      "../../test/b/dummer.txt",
                      "../../test/b/dum.txt",
                  },
                  DestinationDirectory = "test",
                  IsFile = true,
                },
                new StageItem
                {
                    Source  = new List<string>
                    {
                        "../../test/bigDir",
                        "../../test/biggerDir",
                    },
                    DestinationDirectory = "big",
                    IsFile = false,
                },
            };

            return items;
        }

        public static void Copy(string _sourceDirectory, string _targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(_sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(_targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo _source, DirectoryInfo _target)
        {
            Directory.CreateDirectory(_target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in _source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(_target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in _source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    _target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }

    public class StageItem
    {
        public StageItem()
        {
        }

        public List<string> Source { get; set; }
        public string DestinationDirectory { get; set; }
        public bool IsFile { get; set; }
    }
}

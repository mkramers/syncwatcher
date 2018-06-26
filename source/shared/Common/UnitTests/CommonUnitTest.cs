using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace UnitTests
{
    [TestClass]
    public class CommonUnitTests
    {
        [TestMethod]
        public void TestGetFileLinesSplit()
        {
            //create the test data and write it to file
            string path = "test.txt";
            char seperator = ' ';
            List<string[]> strings = new List<string[]>
            {
                new [] { "a", "b", "c" },
                new []  { "d", "e", "f" },
                new [] { "g", "h", "i" },
            };
            List<string> combined = new List<string>();
            strings.ForEach(line =>
            {
                string newLine = "";
                line.ToList().ForEach(item => newLine += item + seperator);
                combined.Add(newLine);
            });
            combined.Remove(combined.Last());
            File.WriteAllLines(path, combined);

            //test the function
            List<string[]> result = Utilities.GetFileLinesSplit(path, seperator);

            //compare and report result
            bool isEqual = false;
            if (strings.Count == result.Count)
            {
                for (int i = 0; i < strings.Count; i++)
                {
                    isEqual = strings[i].SequenceEqual(result[i]);

                    if (!isEqual)
                    {
                        break;
                    }
                }
            }
            Assert.IsFalse(isEqual);
        }
    }
}

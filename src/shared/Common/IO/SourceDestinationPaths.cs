﻿using System.Diagnostics;

namespace Common.IO
{
    public class SourceDestinationPaths
    {
        public string SourcePath { get; }
        public string DestinationPath { get; }

        public SourceDestinationPaths(string _sourcePath, string _destinationPath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_sourcePath));
            Debug.Assert(!string.IsNullOrWhiteSpace(_destinationPath));

            SourcePath = _sourcePath;
            DestinationPath = _destinationPath;
        }
    }
}

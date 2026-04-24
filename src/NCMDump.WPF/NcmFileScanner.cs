using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace NCMDump.WPF
{
    internal static class NcmFileScanner
    {
        /// <summary>
        /// Scans file paths and directories, adding any .ncm files found to the collection.
        /// Uses a HashSet for O(1) duplicate detection.
        /// </summary>
        public static void ScanPaths(string[] paths, ObservableCollection<NCMConvertMissionStatus> collection, int maxDepth = 16)
        {
            var knownPaths = new HashSet<string>(collection.Select(x => x.FilePath), StringComparer.OrdinalIgnoreCase);

            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    WalkDirectory(new DirectoryInfo(path), collection, knownPaths, maxDepth, 0);
                }
                else if (File.Exists(path))
                {
                    if (path.EndsWith(".ncm", StringComparison.OrdinalIgnoreCase) && knownPaths.Add(path))
                        collection.Add(new NCMConvertMissionStatus(path, ConvertStatus.Await));
                }
            }
        }

        /// <summary>
        /// Recursively walks a directory and adds all .ncm files to the collection.
        /// </summary>
        private static void WalkDirectory(DirectoryInfo dir, ObservableCollection<NCMConvertMissionStatus> collection, HashSet<string> knownPaths, int maxDepth, int currentDepth)
        {
            if (currentDepth >= maxDepth)
                return;

            foreach (DirectoryInfo sub in dir.EnumerateDirectories())
            {
                WalkDirectory(sub, collection, knownPaths, maxDepth, currentDepth + 1);
            }

            foreach (FileInfo file in dir.EnumerateFiles())
            {
                if (file.FullName.EndsWith(".ncm", StringComparison.OrdinalIgnoreCase) && knownPaths.Add(file.FullName))
                    collection.Add(new NCMConvertMissionStatus(file.FullName, ConvertStatus.Await));
            }
        }
    }
}

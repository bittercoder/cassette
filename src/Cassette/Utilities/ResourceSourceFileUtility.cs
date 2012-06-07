using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cassette.Utilities
{
    public static class ResourceSourceFileUtility
    {
        public static string TryFindSourceFileForResource(Assembly assembly, string resourceName)
        {
            string path = assembly.CodeBase;

            Uri uri = new Uri(path);

            string fileName = uri.LocalPath;

            if (!File.Exists(fileName)) return null;

            string directory = Path.GetDirectoryName(fileName);

            if (directory == null) return null;

            string parentDirectory = System.IO.Directory.GetParent(directory).FullName;

            string parentOfParentDirectory = Directory.GetParent(parentDirectory).FullName;

            string[] candidatePaths = new string[] { directory, parentDirectory, parentOfParentDirectory };

            var candidateResourceNames = GetCandidateResourceNames(assembly, resourceName).ToList();

            foreach (var candidatePath in candidatePaths)
            {
                foreach (var candidateResourceName in candidateResourceNames)
                {
                    var pathToTest = Path.Combine(candidatePath, candidateResourceName.Replace("/", "\\"));

                    if (File.Exists(pathToTest))
                    {
                        return pathToTest;
                    }
                }
            }

            return null;
        }

        static IEnumerable<string> GetCandidateResourceNames(Assembly assembly, string resourceName)
        {
            resourceName = resourceName.Replace("~/", "").Replace("/", Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture));

            yield return resourceName;

            var assemblyName = assembly.GetName().Name;

            if (resourceName.StartsWith(assemblyName))
            {
                resourceName = resourceName.Substring(assemblyName.Length + 1);
            }

            var splitResourceName = resourceName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (splitResourceName.Length > 2)
            {
                for (int i = 2; i < splitResourceName.Length; i++)
                {
                    var pathPart = string.Join(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture),
                                               splitResourceName.Reverse().Skip(i - 1).Reverse().ToArray());

                    var filePart = "." + string.Join(".", splitResourceName.Skip(splitResourceName.Length - (i - 1)).ToArray());

                    var periodsToPathSeperatorsName = pathPart + filePart;

                    yield return periodsToPathSeperatorsName;
                }
            }
        }
    }
}
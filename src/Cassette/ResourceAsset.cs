using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Cassette.IO;
using Cassette.Utilities;
using Cassette.Web.Jasmine;

namespace Cassette
{
    /// <summary>
    /// Embedded resource asset, with support for fetching asset from on-disk (to support editing javascript files while an application is running, to
    /// speed up development).
    /// </summary>
    public class ResourceAsset : AssetBase
    {
        readonly Assembly assembly;
        readonly Lazy<byte[]> hash;
        readonly Bundle parentBundle;
        readonly List<AssetReference> references = new List<AssetReference>();
        readonly IFile resourceFile;
        readonly string resourceName;

        public ResourceAsset(string resourceName, Assembly assembly, Bundle parentBundle)
        {
            this.resourceName = ResolveResourceName(resourceName, assembly);
            resourceFile = new ResourceFile("~/" + this.resourceName);
            this.assembly = assembly;
            this.parentBundle = parentBundle;
            this.hash = new Lazy<byte[]>(ComputeHash);
        }

        public override byte[] Hash
        {
            get { return hash.Value; }
        }

        public override IFile SourceFile
        {
            get { return resourceFile; }
        }

        public override IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        static string ResolveResourceName(string resourceName, Assembly assembly)
        {
            if (resourceName.Contains("/"))
            {
                if (resourceName.StartsWith("~/"))
                {
                    resourceName = resourceName.Substring(2);
                }

                resourceName = assembly.GetName().Name + "." + resourceName.Replace("/", ".");
            }
            return resourceName;
        }

        byte[] ComputeHash()
        {
            using (SHA1 sha1 = SHA1.Create())
            using (Stream stream = OpenStream())
            {
                return sha1.ComputeHash(stream);
            }
        }

        public override void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void AddReference(string assetRelativePath, int lineNumber)
        {
            if (assetRelativePath.IsUrl())
            {
                AddUrlReference(assetRelativePath, lineNumber);
            }
            else
            {
                string appRelativeFilename;

                if (assetRelativePath.StartsWith("~"))
                {
                    appRelativeFilename = assetRelativePath;
                }
                else if (assetRelativePath.StartsWith("/"))
                {
                    appRelativeFilename = "~" + assetRelativePath;
                }
                else
                {
                    string subDirectory = SourceFile.Directory.FullPath;
                    appRelativeFilename = PathUtilities.CombineWithForwardSlashes(
                        subDirectory,
                        assetRelativePath
                        );
                }

                appRelativeFilename = PathUtilities.NormalizePath(appRelativeFilename);

                AddBundleReference(appRelativeFilename, lineNumber);
            }
        }

        void AddBundleReference(string appRelativeFilename, int lineNumber)
        {
            AssetReferenceType type = parentBundle.ContainsPath(appRelativeFilename)
                                          ? AssetReferenceType.SameBundle
                                          : AssetReferenceType.DifferentBundle;

            references.Add(new AssetReference(appRelativeFilename, this, lineNumber, type));
        }

        void AddUrlReference(string url, int sourceLineNumber)
        {
            references.Add(new AssetReference(url, this, sourceLineNumber, AssetReferenceType.Url));
        }

        public override void AddRawFileReference(string relativeFilename)
        {
            if (relativeFilename.StartsWith("/"))
            {
                relativeFilename = "~" + relativeFilename;
            }
            else if (!relativeFilename.StartsWith("~"))
            {
                relativeFilename = PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(
                    SourceFile.Directory.FullPath,
                    relativeFilename
                                                                   ));
            }

            bool alreadyExists = references.Any(r => r.Path.Equals(relativeFilename, StringComparison.OrdinalIgnoreCase));

            if (alreadyExists) return;

            references.Add(new AssetReference(relativeFilename, this, -1, AssetReferenceType.RawFilename));
        }

        protected override Stream OpenStreamCore()
        {
            string file = ResourceSourceFileUtility.TryFindSourceFileForResource(assembly, resourceName);

            if (file != null)
            {
                Trace.Source.TraceEvent(TraceEventType.Information, 0, "Using source file for embedded resource: {0}, path: {1}", this.resourceName, this.resourceFile);
                return File.OpenRead(file);
            }

            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
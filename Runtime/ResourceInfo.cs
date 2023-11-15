using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HamerSoft.BetterResources
{
    public class ResourceInfo
    {
        public string Guid { get; internal set; }
        public string Name { get; internal set; }
        public string ResourcesPath { get; internal set; }
        public string FullPath { get; internal set; }
        public string FileExtension { get; internal set; }
        public string Package { get; internal set; }
        public Type Type { get; internal set; }
        public bool IsInPackage => !string.IsNullOrWhiteSpace(Package);
        public HashSet<Type> Components { get; }

        internal ResourceInfo(string guid, string path, string packageName,
            IEnumerable<Type> components)
        {
            Guid = guid;
            FileExtension = Path.GetExtension(path);
            ResourcesPath = path.Replace(FileExtension, "");
            FullPath = path;
            Name = Path.GetFileNameWithoutExtension(path);
            Package = packageName;
            Components = new HashSet<Type>(components);
            Type = Components.First();
        }
    }
}
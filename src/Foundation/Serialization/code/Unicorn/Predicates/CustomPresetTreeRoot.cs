using System.Collections.Generic;
using Rainbow.Storage;

namespace Unicorn.Predicates
{
    public class CustomPresetTreeRoot : PresetTreeRoot
    {
        public CustomPresetTreeRoot(string name, string path, string databaseName, string template) : base(name, path, databaseName)
        {
            EnsureTemplate = template;
        }

        public string EnsureTemplate { get; protected set; }
    }
}
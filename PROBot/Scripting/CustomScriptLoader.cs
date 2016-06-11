using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System.IO;

namespace PROBot.Scripting
{
    internal class CustomScriptLoader : ScriptLoaderBase
    {
        private string _path;

        public CustomScriptLoader(string path)
        {
            _path = path;
        }

        public override object LoadFile(string file, Table globalContext)
        {
            return File.ReadAllText(Path.Combine(_path, file));
        }

        public override bool ScriptFileExists(string name)
        {
            return File.Exists(Path.Combine(_path, name));
        }
    }
}

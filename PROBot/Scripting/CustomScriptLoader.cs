using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace PROBot.Scripting
{
    internal class CustomScriptLoader : ScriptLoaderBase
    {
        //HashSet used, so that dulplicate entries are filtered by default
        private readonly HashSet<string> _baseDirs = new HashSet<string>();

        private readonly string _proShineSystemVar = "lua_path";

        public CustomScriptLoader(string scriptDir)
        {
            //for readability added every string by hand
            //1. relative path | script dir is root then
            _baseDirs.Add(scriptDir);
            //2. absolute path | if given an exact location on disk e.g.: C:\ultrascript.lua
            _baseDirs.Add("");
            //3. lua_path | using system variables as root keys
            if (Environment.GetEnvironmentVariable(_proShineSystemVar) != null)
            {
                var luaPaths = Environment.GetEnvironmentVariable(_proShineSystemVar);
                foreach (var luaPath in luaPaths.Split(';'))
                    _baseDirs.Add(luaPath);
            }
        }

        public override object LoadFile(string relPath, Table globalContext)
        {
            return File.ReadAllText(GetPath(relPath));
        }

        public override bool ScriptFileExists(string relPath)
        {
            return GetPath(relPath) != null;
        }

        /// <summary>
        ///     Returns an absolute path if every combination of baseDirs + relative
        ///     return only one existing file.
        /// </summary>
        /// <param name="relPath">
        ///     The reference one uses to import another lua script. Relative in
        ///     terms of script path, lua path or it is an absolute path itself.
        /// </param>
        /// <returns>
        ///     The file path if unique or null, if multiple files are accessible
        ///     via the <paramref name="relPath" /> param.
        /// </returns>
        public string GetPath(string relPath)
        {
            //return path, only if a single match exists:
            //base directories are unique, but constructed directories could reference multiple files e.g.:
            //1. opening ...workspace\ultrascript.lua makes rel_path = "ultrascript.lua"
            //2. lua_path = "C:\" 
            //==> reference could either point to workspace\ultrascript.lua or C:\ultrascript.lua
            try
            {
                //building paths
                var dirs = new HashSet<string>();
                foreach (var baseDir in _baseDirs)
                    dirs.Add(BuildPath(baseDir, relPath));

                //check uniqueness | added as function for future enrichment.
                //if path would point to a dir: hasOneMatch = e => Directory.Exists(e);
                Func<string, bool> hasOneMatch = e => File.Exists(e);
                return dirs.Single(hasOneMatch);
            }

            //Single() throws exception if zero or multiple matches exist
            catch (InvalidOperationException ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return null;
            }
        }

        /// <summary>
        ///     Generates the file path, while making some lua reference/interpreter
        ///     specific changes.
        /// </summary>
        /// <param name="baseDir">
        ///     The root path, eiter script dir, absolut or lua path.
        /// </param>
        /// <param name="relPath">
        ///     The reference one uses to import another lua script. Relative in
        ///     terms of script path, lua path or it is an absolute path itself.
        /// </param>
        /// <returns>
        ///     Either combined path of <paramref name="baseDir" /> and
        ///     <paramref name="relPath" /> or "" if exception occurs.
        /// </returns>
        private string BuildPath(string baseDir, string relPath)
        {
            try
            {
                var relPathMod = GetCleanPath(relPath);
                var combine = Path.Combine(baseDir, relPathMod);
                return Path.GetFullPath(combine);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                //has to return "" for dirs.Single() to work, don't return null
                return "";
            }
        }

        /// <summary>
        ///     Searches for differences between lua and Windows specific referening
        ///     and makes it so that <paramref name="relPath" /> can be used with
        ///     Path.Combine() function.
        /// </summary>
        /// <param name="relPath">
        ///     The reference one uses to import another lua script. Relative in
        ///     terms of script path, lua path or it is an absolute path itself.
        /// </param>
        /// <attention>
        ///     Probably more cases, that need handeling.
        /// </attention>
        private string GetCleanPath(string relPath)
        {
            return relPath.Replace("//", "..");
        }
    }
}
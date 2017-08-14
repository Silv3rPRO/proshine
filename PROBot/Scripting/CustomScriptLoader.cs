using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PROBot.Scripting
{
    internal class CustomScriptLoader : ScriptLoaderBase
    {
        //HashSet used, so that dulplicate entries are filtered by default
        private HashSet<string> _baseDirs = new HashSet<string>();
        private string proShineSystemVar = "lua_path";

        public CustomScriptLoader(string scriptDir)
        {
            //for readability added every string by hand
            //1. relative path | script dir is root then
            _baseDirs.Add(scriptDir);
            //2. absolute path | if given an exact location on disk e.g.: C:\ultrascript.lua
            _baseDirs.Add("");
            //3. lua_path | using system variables as root keys
            if (Environment.GetEnvironmentVariable(proShineSystemVar) != null)
            {
                string luaPaths = Environment.GetEnvironmentVariable(proShineSystemVar);
                foreach (var lua_path in luaPaths.Split(';'))
                    _baseDirs.Add(lua_path);
            }
        }

        public override object LoadFile(string relPath, Table globalContext)
        {
            return File.ReadAllText(getPath(relPath));
        }

        public override bool ScriptFileExists(string relPath)
        {
            return getPath(relPath) != null;
        }

        /// <summary>
        /// Returns an absolute path if every combination of baseDirs + relative return only one existing file. 
        /// </summary>
        /// <param name="relPath">
        /// The reference one uses to import another lua script. Relative in terms of script path, lua 
        /// path or it is an absolute path itself.
        /// </param>
        /// <returns>The file path if unique or null, if multiple files are accessible via the relPath param.</returns>
        public string getPath(string relPath)
        {
            //return path, only if a single match exists:
            //base directories are unique, but constructed directories could reference multiple files e.g.:
            //1. opening ...workspace\ultrascript.lua makes rel_path = "ultrascript.lua"
            //2. lua_path = "C:\" 
            //==> reference could either point to workspace\ultrascript.lua or C:\ultrascript.lua
            try
            {
                //building paths
                HashSet<string> dirs = new HashSet<string>();
                foreach (var baseDir in _baseDirs)
                    dirs.Add(buildPath(baseDir, relPath));

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
        /// Generates the file path, while making some lua reference/interpreter specific changes.
        /// </summary>
        /// <param name="baseDir">The root path, eiter script dir, absolut or lua path.</param>
        /// <param name="relPath">
        /// The reference one uses to import another lua script. Relative in terms of script path, lua 
        /// path or it is an absolute path itself.
        /// </param>
        /// <returns>Either combined path of baseDir and relPath or "" if exception occurs.</returns>
        private string buildPath(string baseDir, string relPath)
        {
            try
            {
                string relPathMod = getCleanPath(relPath);
                string combine = Path.Combine(baseDir, relPathMod);
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
        /// Searches for differences between lua and Windows specific referening and makes it so that
        /// relPath can be used with Path.Combine() function.
        /// </summary>
        /// <param name="relPath">
        /// The reference one uses to import another lua script. Relative in terms of script path, lua 
        /// path or it is an absolute path itself.
        /// </param>
        ///<attention>Probably more cases, that need handeling.</attention>
        private string getCleanPath(string relPath)
        {
            return relPath.Replace("//", "..");
        }
    }
}

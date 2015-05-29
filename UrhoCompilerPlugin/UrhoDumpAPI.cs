using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UrhoCompilerPlugin
{
    public static class UrhoDumpAPI
    {
        /// <summary>
        /// Invokes the ScriptCompiler to generate documentation and API dumps
        /// Used after a successful compilation
        /// 
        /// The IDBBuilderActivity will respond to the updated dump header and rebuild on its own
        /// </summary>
        public static void CreateDumps(string path, string sourceTree)
        {

            string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            dir = System.IO.Path.Combine(dir, "bin");
            string parentDir = System.IO.Directory.GetParent(path).ToString();

            // Check for a source tree
            if (sourceTree != null && sourceTree.Length > 0)
                parentDir = sourceTree;

            // Quote spaces if necessary
            if (parentDir.Contains(' '))
                parentDir = String.Format("\"{0}\"", parentDir);

            Thread thread = new Thread(delegate()
            {
                //Thread thread = new Thread(delegate() {
                Process pi = new Process();
                pi.StartInfo.FileName = System.IO.Path.Combine(dir, "ScriptCompiler.exe");
                pi.StartInfo.Arguments = " -dumpapi " + parentDir + " ScriptAPI.dox dump.h";
                pi.StartInfo.UseShellExecute = false;
                pi.StartInfo.CreateNoWindow = true;
                pi.StartInfo.WorkingDirectory = dir;
                pi.Start();
                pi.WaitForExit();
            });
            thread.Start();
        }
    }
}

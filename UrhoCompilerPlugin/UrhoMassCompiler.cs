using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UrhoCompilerPlugin
{
    /// <summary>
    /// Compiler for Urho3D that compiles ALL scripts in the directory of the target script
    /// 
    /// Use cases include compiling all examples, compiling all scripts to check for include order issues, etc
    /// </summary>
    public class UrhoMassCompiler : PluginLib.ICompilerService
    {
        bool pushedError = false;
        int errorsPushed = 0;
        PluginLib.ICompileHelper compileErrorPublisher;
        PluginLib.IErrorPublisher errorPublisher;

        public string Name { get { return "Urho3D All Files"; } }

        public void CompileFile(string file, PluginLib.ICompileHelper compileErrorPublisher, PluginLib.IErrorPublisher errorPublisher)
        {
            pushedError = false;
            this.compileErrorPublisher = compileErrorPublisher;
            this.errorPublisher = errorPublisher;
            errorsPushed = 0;
            try
            {
                string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                path = Path.Combine(path, "bin");
                path = Path.Combine(path, "ScriptCompiler.exe");

                string parentDir = Path.GetDirectoryName(file);
                if (!Directory.Exists(parentDir))
                    return;

                List<string> compileList = new List<string>();
                foreach (string f in Directory.EnumerateFiles(parentDir))
                {
                    if (!File.Exists(f) || !Path.GetExtension(f).Equals(".as"))
                        continue;
                    compileList.Add(f);
                }
                int ct = 0;
                foreach (string f in compileList)
                {
                    // Quote spaces if necessary
                    string cmdLine = f;
                    if (cmdLine.Contains(' '))
                        cmdLine = String.Format("\"{0}\"", cmdLine);

                    // Append include directories, quoting any space containing paths
                    foreach (string s in compileErrorPublisher.GetIncludeDirs())
                        cmdLine = String.Format(file + " {1}{0}{1}", s, s.Contains(' ') ? "\"" : "");

                    ++ct;
                    compileErrorPublisher.PushOutput("────────────────────────────────────────────────────\r\n");
                    compileErrorPublisher.PushOutput(String.Format("Compiling: {0}\r\n", f, ct));
                    compileErrorPublisher.PushOutput(String.Format("{0} of {1}\r\n", ct, compileList.Count));
                    Process pi = new Process();
                    pi.StartInfo.FileName = path;
                    pi.StartInfo.Arguments = cmdLine;
                    pi.EnableRaisingEvents = true;
                    pi.StartInfo.WorkingDirectory = compileErrorPublisher.GetProjectDirectory();
                    pi.StartInfo.UseShellExecute = false;
                    pi.StartInfo.CreateNoWindow = true;
                    pi.StartInfo.RedirectStandardError = true;
                    pi.StartInfo.RedirectStandardOutput = true;
                    pi.ErrorDataReceived += pi_ErrorDataReceived;
                    pi.OutputDataReceived += pi_OutputDataReceived;
                    pi.Start();
                    pi.BeginErrorReadLine();
                    pi.BeginOutputReadLine();
                    pi.WaitForExit();

                    if (pushedError) {
                        compileErrorPublisher.PushOutput(String.Format("Compiling {0} Failed\r\n", f));
                    } else {
                        compileErrorPublisher.PushOutput(String.Format("Compiling {0} Complete\r\n", f));
                    }
                }
                if (errorsPushed == 0)
                    UrhoDumpAPI.CreateDumps(compileErrorPublisher.GetProjectDirectory(), compileErrorPublisher.GetProjectSourceTree());
            }
            catch (Exception ex)
            {
                errorPublisher.PublishError(ex);
            }
        }

        void pi_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (UrhoCompiler.ProcessLine(e.Data, compileErrorPublisher, errorPublisher))
            {
                pushedError = true;
            }
        }

        void pi_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (UrhoCompiler.ProcessLine(e.Data, compileErrorPublisher, errorPublisher))
            {
                pushedError = true;
            }
        }

        public void PostCompile(string file, string sourceTree, PluginLib.IErrorPublisher errorPublisher)
        {
            try
            {
                string path = System.Reflection.Assembly.GetEntryAssembly().Location;
                path = Path.Combine(path, "bin");
                path = Path.Combine(path, "ScriptCompiler.exe");
            }
            catch (Exception ex)
            {
                errorPublisher.PublishError(ex);
            }

        }
    }
}

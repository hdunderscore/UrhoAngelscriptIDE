using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UrhoCompilerPlugin
{
    /// <summary>
    /// Compiler for Urho3D
    /// 
    /// Uses ScriptCompiler.exe in the bin folder to compile a specific file
    /// </summary>
    public class UrhoCompiler : PluginLib.ICompilerService
    {
        bool pushedError = false;
        PluginLib.ICompileHelper compileErrorPublisher;
        PluginLib.IErrorPublisher errorPublisher;

        public string Name { get { return "Urho3D Single File"; } }
        
        public void CompileFile(string file, PluginLib.ICompileHelper compileErrorPublisher, PluginLib.IErrorPublisher errorPublisher)
        {
            pushedError = false;
            this.compileErrorPublisher = compileErrorPublisher;
            this.errorPublisher = errorPublisher;
            try
            {
                string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                path = Path.Combine(path, "bin");
                path = Path.Combine(path, "ScriptCompiler.exe");
                
                Process pi = new Process();
                pi.StartInfo.FileName = path;
                pi.StartInfo.Arguments = file;
                pi.EnableRaisingEvents = true;
                pi.StartInfo.UseShellExecute = false;
                pi.StartInfo.CreateNoWindow = true;
                pi.StartInfo.RedirectStandardOutput = true;
                pi.ErrorDataReceived += pi_ErrorDataReceived;
                pi.Start();
                pi.WaitForExit();

                string str = "";
                while ((str = pi.StandardOutput.ReadLine()) != null)
                {
                    if (ProcessLine(str, compileErrorPublisher, errorPublisher))
                        pushedError = true;
                }
                if (pushedError)
                    compileErrorPublisher.PushOutput(String.Format("Compiling {0} Failed\r\n", file));
                else
                {
                    compileErrorPublisher.PushOutput(String.Format("Compiling {0} Complete\r\n", file));
                    UrhoDumpAPI.CreateDumps(compileErrorPublisher.GetProjectDirectory(),compileErrorPublisher.GetProjectSourceTree());
                }
            } catch (Exception ex)
            {
                errorPublisher.PublishError(ex);
            }
        }

        void pi_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (ProcessLine(e.Data, compileErrorPublisher, errorPublisher))
                pushedError = true;
        }

        public static bool ProcessLine(string str, PluginLib.ICompileHelper compileErrorPublisher, PluginLib.IErrorPublisher errorPublisher)
        {
            compileErrorPublisher.PushOutput(str + "\r\n");
            bool isError = false;
            bool isWarning = false;
            if (str.Contains("ERROR: "))
            {
                str = str.Replace("ERROR: ", "");
                isError = true;
            }
            if (str.Contains("WARNING: "))
            {
                str = str.Replace("WARNING: ", "");
                isWarning = true;
            }
            if (isError || isWarning)
            {
                string[] words = str.Split(' '); //split on spaces
                int colonIndex = words[0].LastIndexOf(':');
                if (colonIndex == -1)
                    return false; //don't count this as an error
                string fileName = words[0].Substring(0, colonIndex);

                string part = "";
                int line = -1;
                int column = -1;
                //move to first number
                ++colonIndex;
                for (; colonIndex < str.Length; ++colonIndex)
                {
                    if (str[colonIndex] == ',')
                    {
                        if (line == -1)
                            line = int.Parse(part);
                        else
                            column = int.Parse(part);
                    }
                    if (str[colonIndex] == ' ')
                        break;
                    part += str[colonIndex];
                }
                string msg = String.Join(" ", words, 1, words.Length - 1); // str.Substring(colonIndex);
                PluginLib.CompileError error = new PluginLib.CompileError
                {
                    File = fileName,
                    Line = line,
                    Message = msg,
                    IsError = isError
                };
                compileErrorPublisher.PublishError(error);
                return isError;
            }
            return false;
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

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
        public string Name { get { return "Urho3D All Files"; } }

        [DllImport("kernel32.dll")]
        static extern bool ReadConsoleW(IntPtr hConsoleInput, [Out] byte[]
           lpBuffer, uint nNumberOfCharsToRead, out uint lpNumberOfCharsRead,
           IntPtr lpReserved);

        static string ReadLine(IntPtr handle)
        {
            const int bufferSize = 1024;
            var buffer = new byte[bufferSize];

            uint charsRead = 0;

            ReadConsoleW(handle, buffer, bufferSize, out charsRead, (IntPtr)0);
            if (charsRead <= 2)
                return "";
            // -2 to remove ending \n\r
            int nc = ((int)charsRead - 2) * 2;
            var b = new byte[nc];
            for (var i = 0; i < nc; i++)
                b[i] = buffer[i];

            var utf8enc = Encoding.UTF8;
            var unicodeenc = Encoding.Unicode;
            return utf8enc.GetString(Encoding.Convert(unicodeenc, utf8enc, b));
        }

        public void CompileFile(string file, PluginLib.ICompileErrorPublisher compileErrorPublisher, PluginLib.IErrorPublisher errorPublisher)
        {
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
                    ++ct;
                    compileErrorPublisher.PushOutput("────────────────────────────────────────────────────\r\n");
                    compileErrorPublisher.PushOutput(String.Format("Compiling: {0}\r\n", f, ct));
                    compileErrorPublisher.PushOutput(String.Format("{0} of {1}\r\n", ct, compileList.Count));
                    Process pi = new Process();
                    pi.StartInfo.FileName = path;
                    pi.StartInfo.Arguments = f;
                    pi.EnableRaisingEvents = true;
                    pi.StartInfo.UseShellExecute = false;
                    pi.StartInfo.CreateNoWindow = true;
                    pi.StartInfo.RedirectStandardOutput = true;
                    pi.Start();
                    pi.WaitForExit();

                    string str = "";
                    bool pushedError = false;
                    while ((str = pi.StandardOutput.ReadLine()) != null)
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
                                Message = msg
                            };
                            if (isError)
                            {
                                pushedError = true;
                                compileErrorPublisher.PublishError(error);
                            }
                            else
                            {
                                compileErrorPublisher.PublishWarning(error);
                            }
                        }
                    }
                    if (pushedError) {
                        compileErrorPublisher.PushOutput(String.Format("Compiling {0} Failed\r\n", f));
                    } else {
                        compileErrorPublisher.PushOutput(String.Format("Compiling {0} Complete\r\n", f));
                    }
                }
            }
            catch (Exception ex)
            {
                errorPublisher.PublishError(ex);
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

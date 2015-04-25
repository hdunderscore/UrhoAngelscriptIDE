using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE {
    [Serializable]
    public class IDESettings : BaseClass {
        string projectPath_ = "";
        string compilerPath_ = "";
        string compiler_ = ""; //The compiler plugin selected, defaults to first detected
        string runExe_ = "";
        string debugExe_ = "";
        string debugParams_ = "";
        string runParams_ = "";
        string sourceTree_ = "";
        string includeDirs_ = "";

        public IDESettings() {
        }

        public static IDESettings GetOrCreate(string aPath) {
            UserData aUserData = UserData.inst();
            IDESettings existing = aUserData.IDESettings.FirstOrDefault(s => s.ProjectPath.Equals(aPath));
            if (existing != null)
                return existing;
            IDESettings ret = new IDESettings { ProjectPath = aPath };
            ret.Parent = aUserData;
            aUserData.IDESettings.Add(ret);
            return ret;
        }

        public string ProjectPath { get { return projectPath_; } set { projectPath_ = value; } }
        public string CompilerPath { get { return compilerPath_; } set { compilerPath_ = value; OnPropertyChanged("CompilerPath"); } }
        public string DebugExe { get { return debugExe_; } set { debugExe_ = value; OnPropertyChanged("DebugExe"); } }
        public string RunExe { get { return runExe_; } set { runExe_ = value; OnPropertyChanged("RunExe"); } }
        public string DebugParams { get { return debugParams_; } set { debugParams_ = value; OnPropertyChanged("DebugParams"); } }
        public string RunParams { get { return runParams_; } set { runParams_ = value; OnPropertyChanged("RunParams"); } }
        public string SourceTree { get { return sourceTree_; } set { sourceTree_ = value; OnPropertyChanged("SourceTree"); } }
        public string Compiler { get { return compiler_; } set { compiler_ = value; OnPropertyChanged("Compiler"); } }
        public string IncludePaths { get { return includeDirs_; } set { includeDirs_ = value; OnPropertyChanged("IncludePaths"); } }

        public string[] GetIncludeDirectories()
        {
            if (IncludePaths != null)
            {
                string[] paths = IncludePaths.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length; ++i)
                {
                    paths[i] = paths[i].Trim();
                    // Root the path if need be
                    if (!System.IO.Path.IsPathRooted(paths[i]))
                        paths[i] = System.IO.Path.Combine(IDEProject.inst().ProjectDir, paths[i]);
                    
                }
                return paths;
            }
            return null;
        }
    }
}

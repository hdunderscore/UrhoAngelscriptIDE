using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense.Parsers
{
    public class GLSLParser : AngelscriptParser
    {
        Globals glslGlobs;

        public GLSLParser(Globals globs)
        {
            glslGlobs = globs;
        }

        public override Globals Parse(string path, string fileCode, string[] includePaths)
        {
            Globals ret = new Globals(false);

            // Merge global app registered types into it
            ret.Parent = glslGlobs;

            List<string> existingPaths = new List<string>();
            // Inline #includes
            fileCode = ProcessIncludes(path, fileCode, includePaths, existingPaths);
            // Strip comments
            fileCode = StripComments(fileCode);
            DepthScanner scanner = new DepthScanner();
            scanner.Process(fileCode);
            Parse(new StringReader(fileCode), scanner, ret);
            return ret;
        }

        public override bool ResemblesClass(string line)
        {
            string[] tokens = line.Split(BREAKCHARS);
            // struct is here to cover HLSL
            return Array.IndexOf(tokens, "struct") != -1;
        }

        public override bool ResemblesProperty(string line, Globals globals)
        {
            string[] tokens = line.Split(BREAKCHARS);
            if (tokens.Length >= 2)
            {
                int constIdx = Array.IndexOf(tokens, "const");
                int uniformIdx = Array.IndexOf(tokens, "uniform");

                int termIdx = Math.Max(constIdx, uniformIdx) + 1;

                if (globals.ContainsTypeInfo(tokens[termIdx].Replace("@", "")))
                {
                    if (tokens[termIdx + 1].EndsWith(";"))
                        return true;
                    if (tokens.Length - 1 >= termIdx + 2 && tokens[termIdx + 2].Equals("="))
                        return true;
                }
            }
            return false;
        }
    }
}

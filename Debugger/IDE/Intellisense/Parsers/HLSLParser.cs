using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense.Parsers
{
    public class HLSLParser : AngelscriptParser
    {
        Globals hlslGlobs;
        public HLSLParser(Globals baseGlobs)
        {
            hlslGlobs = baseGlobs;
        }

        public override Globals Parse(string path, string fileCode, string[] includePaths)
        {
            Globals ret = new Globals(false);

            // Merge global app registered types into it
            ret.Parent = hlslGlobs;

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

        public override bool ResemblesFunction(string line)
        {
            int colonPos = line.IndexOf(':');
            if (colonPos == -1)
                colonPos = int.MaxValue;

            int equalsPos = line.IndexOf('=');
            if (equalsPos == -1) // Scale it out to max, this is necessary so the comparison of default params vs RHS assignment works
                equalsPos = int.MaxValue;

            int openPos = line.IndexOf('(');

            if (line.Contains("(") && openPos < equalsPos && openPos < colonPos) // Must be a global/namespace function
                return true;
            return false;
        }

        public override bool ResemblesClass(string line)
        {
            string[] tokens = line.Split(BREAKCHARS);
            // struct is here to cover HLSL
            return Array.IndexOf(tokens, "cbuffer") != -1 || Array.IndexOf(tokens, "struct") != -1;
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

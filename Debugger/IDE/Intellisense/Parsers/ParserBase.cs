using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense.Parsers
{
    public abstract class ParserBase
    {
        protected static readonly string blockComments = @"/\*(.*?)\*/";
        protected static readonly string lineComments = @"//(.*?)\r?\n";
        protected static readonly string strings = @"""((\\[^\n]|[^""\n])*)""";
        protected static readonly string verbatimStrings = @"@(""[^""]*"")+";

        protected static readonly char[] BREAKCHARS = { ' ', ':' };
        protected static readonly char[] SPACECHAR = { ' ' };

        //http://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp
        protected static string StripComments(string fileCode)
        {
            return Regex.Replace(fileCode,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);
        }

        protected static string ProcessIncludes(string filePath, string fileCode, string[] dirs, List<string> existingPaths)
        {
            StringReader rdr = new StringReader(fileCode);
            string line = null;
            StringBuilder sb = new StringBuilder();
            string prefixFmt = "{0}|{1}>{2}"; //Prefix with fileName|line#
            int codeLine = 0;
            while ((line = rdr.ReadLine()) != null)
            {
                ++codeLine;
                if (line.Contains("#include"))
                {
                    string[] parts = line.Trim().Split(' ');
                    foreach (string s in parts)
                    {
                        if (s.Contains('"'))
                        {
                            string fileName = s.Replace("\"", "");
                            foreach (string d in dirs)
                            {
                                string pathCombo = System.IO.Path.Combine(d.Trim(), fileName);
                                if (System.IO.File.Exists(pathCombo))
                                {
                                    if (!existingPaths.Contains(pathCombo))
                                    {
                                        string incCode = System.IO.File.ReadAllText(pathCombo);
                                        sb.AppendLine(ProcessIncludes(pathCombo, incCode, dirs, existingPaths));
                                        existingPaths.Add(pathCombo);
                                        break;
                                    }
                                    else
                                        break;
                                    //\todo Option for error on circular include?
                                    //else
                                    //    throw new Exception(String.Format("Circular include referenced {0}", pathCombo));
                                }
                            }
                            break;
                        }
                    }
                }
                else
                    sb.AppendLine(String.Format(prefixFmt, filePath, codeLine, line));
            }
            return sb.ToString();
        }

        protected static void ExtractLineInfo(ref string lineIn, out string lineOut, out int lineNumOut)
        {
            if (String.IsNullOrWhiteSpace(lineIn))
            {
                lineOut = "";
                lineNumOut = -1;
                return;
            }
            int vertidx = lineIn.IndexOf('|');
            int tailidx = lineIn.IndexOf('>');
            lineOut = lineIn.Substring(0, vertidx);
            string parseLine = lineIn.Substring(vertidx + 1, tailidx - vertidx - 1);
            lineNumOut = int.Parse(parseLine);
            lineIn = lineIn.Substring(tailidx + 1);
        }

        public abstract Globals Parse(string path, string fileCode, string[] includePaths);
        public abstract bool ResemblesClass(string line);
        public abstract bool ResemblesProperty(string line, Globals globals);
        public abstract bool ResemblesFunction(string line);
    }
}

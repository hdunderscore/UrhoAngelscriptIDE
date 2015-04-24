using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense
{
    /// <summary>
    /// A more complete psuedo-Angelscript parser
    /// 
    /// Criteria are:
    /// Parse a full code file extracting to Gobal -> Namespace -> Class -> Class Properties + Class Methods depth
    /// Use DepthScanner to track depth awareness
    /// Inline #include files
    /// </summary>
    public static class AngelscriptParser
    {
        static string blockComments = @"/\*(.*?)\*/";
        static string lineComments = @"//(.*?)\r?\n";
        static string strings = @"""((\\[^\n]|[^""\n])*)""";
        static string verbatimStrings = @"@(""[^""]*"")+";

        static char[] BREAKCHARS = { ' ', ':' };

        public static Globals Parse(String fileCode, string[] includePaths)
        {
            Globals ret = new Globals();

            // Merge global app registered types into it
            ret.MergeIntoThis(IDEProject.inst().GlobalTypes);

            List<string> existingPaths = new List<string>();
            // Inline #includes
            fileCode = ProcessIncludes(fileCode, includePaths, existingPaths);
            // Strip comments
            fileCode = StripComments(fileCode);
            DepthScanner scanner = new DepthScanner();
            scanner.Process(fileCode);
            Parse(new StringReader(fileCode), scanner, ret);
            return ret;
        }

        //http://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp
        static string StripComments(string fileCode)
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

        static string ProcessIncludes(string fileCode, string[] dirs, List<string> existingPaths)
        {
            StringReader rdr = new StringReader(fileCode);
            string line = null;
            StringBuilder sb = new StringBuilder();
            while ((line = rdr.ReadLine()) != null)
            {
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
                                string pathCombo = System.IO.Path.Combine(d, fileName);
                                if (System.IO.File.Exists(pathCombo))
                                {
                                    if (!existingPaths.Contains(pathCombo))
                                    {
                                        string incCode = System.IO.File.ReadAllText(pathCombo);
                                        sb.AppendLine(ProcessIncludes(incCode, dirs, existingPaths));
                                        existingPaths.Add(pathCombo);
                                        break;
                                    }
                                    else
                                        throw new Exception(String.Format("Circular include referenced {0}", pathCombo));
                                }
                            }
                            break;
                        }
                    }
                }
                else
                    sb.AppendLine(line);
            }
            return sb.ToString();
        }

        static void Parse(StringReader rdr, DepthScanner scanner, Globals globals)
        {
            int currentLine = 0;
            ParseNamespace(rdr, globals, scanner, ref currentLine);

            // Resolve incomplete names
            foreach (string key in globals.Properties.Keys)
            {
                if (!globals.Properties[key].IsComplete)
                    globals.Properties[key] = globals.Classes[globals.Properties[key].Name];
                if (globals.Properties[key] is TemplateInst)
                {
                    TemplateInst ti = globals.Properties[key] as TemplateInst;
                    if (!ti.WrappedType.IsComplete)
                        ti.WrappedType = globals.Classes[ti.WrappedType.Name];
                }
            }

            foreach (FunctionInfo f in globals.Functions)
            {
                f.ResolveIncompletion(globals);
            }

            foreach (TypeInfo type in globals.Classes.Values)
            {
                type.ResolveIncompletion(globals);
            }
        }

        static void ParseNamespace(StringReader rdr, Globals globals, DepthScanner scanner, ref int currentLine)
        {
            string l = "";
            int depth = scanner.GetBraceDepth(currentLine);
            while ((l = rdr.ReadLine()) != null)
            {
                ++currentLine;
                if (l.Trim().StartsWith("//"))
                    continue;
                int curDepth = scanner.GetBraceDepth(currentLine-1);
                if (curDepth < depth) // Left our namespace depth
                    return;
                else if (curDepth > depth) // Outside of the desired scanning depth (namespace level)
                    continue;
                string line = l.Trim();
                if (line.Length == 0)
                    continue;
                string[] tokens = line.Split(BREAKCHARS);

                // Class / Interface/ Template type
                if (tokens[0].ToLower().Equals("class") || tokens[0].ToLower().Equals("abstract") || tokens[0].ToLower().Equals("interface") || tokens[0].ToLower().Equals("template"))
                {
                    int abstractIdx = Array.IndexOf(tokens, "abstract");
                    int sharedIdx = Array.IndexOf(tokens, "shared");
                    int templateIdx = Array.IndexOf(tokens, "template");

                    int classTermIdx = Math.Max(abstractIdx, Math.Max(sharedIdx, templateIdx)) + 1;

                    bool isInterface = classTermIdx.Equals("interface");
                    if (templateIdx != -1)
                    {
                        //Resolve template type?
                    }

                    string className = tokens[classTermIdx + 1];
                    TypeInfo ti = new TypeInfo { Name = className, IsTemplate = templateIdx != -1, IsAbstract = abstractIdx != -1 };

                    // Get any baseclasses, baseclass must appear first
                    for (int i = classTermIdx + 2; i < tokens.Length; ++i)
                    {
                        string baseName = tokens[i];
                        if (globals.Classes.ContainsKey(baseName))
                            ti.BaseTypes.Add(globals.Classes[baseName]);
                    }
                    ParseClass(rdr, globals, scanner, ti, ref currentLine);
                } 
                else if (tokens[0].ToLower().Equals("namespace")) // Namespace
                {
                    string nsName = tokens[1];
                    Globals namespaceGlobals = null;
                    if (globals.Namespaces.ContainsKey(nsName)) // Check if the namespace has been encountered before
                        namespaceGlobals = globals.Namespaces[nsName];
                    else
                        namespaceGlobals = new Globals();
                    ParseNamespace(rdr, namespaceGlobals, scanner, ref currentLine);

                    globals.Namespaces[nsName] = namespaceGlobals;
                }
                else if (tokens[0].ToLower().Equals("enum")) // Enumeration
                {
                    ParseEnum(line, rdr, globals, ref currentLine);
                }
                else
                {
                    if (ResemblesFunction(line)) // Global/namespace function
                    {
                        globals.Functions.Add(_parseFunction(line, globals));
                    }
                    else if (ResemblesProperty(line, globals)) // Global/namespace property
                    {
                        string[] parts = l.Replace(";", "").Split(BREAKCHARS);

                        // Globals can't be private/protected
                        int constIdx = Array.IndexOf(parts, "const");
                        int termIdx = constIdx + 1;

                        if (parts[termIdx].Contains('<'))
                        {
                            string templateType = parts[termIdx].Substring(0, parts[termIdx].IndexOf('<'));
                            string containedType = parts[termIdx].Extract('<', '>');
                            TypeInfo wrapped = globals.Classes.FirstOrDefault(t => t.Key.Equals(containedType)).Value;
                            TemplateInst ti = new TemplateInst() { Name = templateType, IsTemplate = true, WrappedType = wrapped != null ? wrapped : new TypeInfo { Name = containedType, IsComplete = false } };
                            globals.Properties[parts[termIdx + 1]] = ti;
                            //if (constIdx != -1)
                            //    globals.ReadonlyProperties.Add(tokens[termIdx + 1]);
                        }
                        else
                        {
                            string pname = parts[termIdx].EndsWith("@") ? parts[termIdx].Substring(0, parts[termIdx].Length - 1) : parts[termIdx]; //handle
                            TypeInfo pType = null;
                            if (globals.Classes.ContainsKey(pname))
                                pType = globals.Classes[pname];
                            if (pType == null)
                            { //create temp type to resolve later
                                pType = new TypeInfo() { Name = pname, IsComplete = false };
                            }
                            globals.Properties[parts[termIdx + 1]] = pType;
                            //if (constIdx != -1)
                            //    globals.ReadonlyProperties.Add(tokens[termIdx + 1]);
                        }
                    }
                }
            }
        }

        static void ParseClass(StringReader rdr, Globals targetGlobals, DepthScanner scanner, TypeInfo targetType, ref int currentLine)
        {
            int depth = scanner.GetBraceDepth(currentLine);
            string l = "";
            while ((l = rdr.ReadLine()) != null)
            {
                ++currentLine;
                int curDepth = scanner.GetBraceDepth(currentLine-1);
                if (curDepth < depth)
                    return;
                else if (curDepth > depth) //We do not go deeper than class
                    continue;
                if (ResemblesFunction(l))
                {
                    targetType.Functions.Add(_parseFunction(l, targetGlobals));
                }
                else if (ResemblesProperty(l, targetGlobals))
                {
                    string[] tokens = l.Replace(";","").Split(BREAKCHARS);
                    
                    int constIdx = Array.IndexOf(tokens, "const");
                    int privateIdx = Array.IndexOf(tokens, "private");
                    int protectedIdx = Array.IndexOf(tokens, "protected");

                    int termIdx = Math.Max(constIdx, Math.Max(privateIdx, protectedIdx)) + 1;

                    if (tokens[termIdx].Contains('<'))
                    {
                        string templateType = tokens[termIdx].Substring(0, tokens[termIdx].IndexOf('<'));
                        string containedType = tokens[termIdx].Extract('<', '>');
                        TypeInfo wrapped = targetGlobals.Classes.FirstOrDefault(t => t.Key.Equals(containedType)).Value;
                        TemplateInst ti = new TemplateInst() { Name = templateType, IsTemplate = true, WrappedType = wrapped != null ? wrapped : new TypeInfo { Name = containedType, IsComplete = false } };
                        targetType.Properties[tokens[termIdx+1]] = ti;
                        if (constIdx != -1)
                            targetType.ReadonlyProperties.Add(tokens[termIdx + 1]);
                        if (protectedIdx != -1)
                            targetType.ProtectedProperties.Add(tokens[termIdx + 1]);
                        else if (privateIdx != -1)
                            targetType.PrivateProperties.Add(tokens[termIdx + 1]);
                    }
                    else
                    {
                        string pname = tokens[termIdx].EndsWith("@") ? tokens[termIdx].Substring(0, tokens[termIdx].Length - 1) : tokens[termIdx]; //handle
                        TypeInfo pType = null;
                        if (targetGlobals.Classes.ContainsKey(pname))
                            pType = targetGlobals.Classes[pname];
                        if (pType == null)
                        { //create temp type to resolve later
                            pType = new TypeInfo() { Name = pname, IsComplete = false };
                        }
                        targetType.Properties[tokens[termIdx + 1]] = pType;
                        if (constIdx != -1)
                            targetType.ReadonlyProperties.Add(tokens[termIdx + 1]);
                        if (protectedIdx != -1)
                            targetType.ProtectedProperties.Add(tokens[termIdx + 1]);
                        else if (privateIdx != -1)
                            targetType.PrivateProperties.Add(tokens[termIdx + 1]);
                    }
                }
            }
        }

        static void ParseEnum(string line, StringReader rdr, Globals globals, ref int currentLine) {
            string[] nameparts = line.Split(' ');
            string enumName = nameparts[1];
            List<string> enumValues = new List<string>();
            while ((line = rdr.ReadLine()) != null) {
                ++currentLine;
                if (line.Equals("};")) {
                    EnumInfo ret = new EnumInfo { Name = enumName };
                    ret.Values.AddRange(enumValues);
                    globals.Classes[enumName] = ret;
                    return;
                } else if (line.Contains(',')) {
                    int sub = line.IndexOf(',');
                    enumValues.Add(line.Substring(0, sub));
                }
            }
        }

        static FunctionInfo _parseFunction(string line, Globals globals)
        {
            int firstParen = line.IndexOf('(');
            int lastParen = line.LastIndexOf(')');
            string baseDecl = line.Substring(0, firstParen);
            string paramDecl = line.Substring(firstParen, lastParen - firstParen + 1); //-1 for the ;
            string[] nameParts = baseDecl.Split(' ');
            TypeInfo retType = null;

            //TODO: split the name parts
            if (globals.Classes.ContainsKey(nameParts[0]))
            {
                retType = globals.Classes[nameParts[0]];
            }
            else if (nameParts[0].Contains('<'))
            {
                string wrappedType = nameParts[0].Extract('<', '>');
                string templateType = nameParts[0].Replace(string.Format("<{0}>", wrappedType), "");
                TypeInfo wrapped = globals.Classes.FirstOrDefault(t => t.Key.Equals(wrappedType)).Value;
                TemplateInst ti = new TemplateInst() { Name = nameParts[0], IsTemplate = true, WrappedType = wrapped != null ? wrapped : new TypeInfo { Name = wrappedType, IsComplete = false } };
                retType = ti;
            }
            else
            {
                retType = new TypeInfo() { Name = nameParts[0], IsPrimitive = false };
            }
            return new FunctionInfo { Name = nameParts[1], ReturnType = retType, Inner = paramDecl };
        }

        static bool ResemblesFunction(string line)
        {
            if (line.StartsWith("funcdef")) //don't scan func defs??
                return false;
            if (line.StartsWith("import")) //don't scan imports??
                return false;
            int equalsPos = line.IndexOf('=');
            if (equalsPos == -1) // Scale it out to max, this is necessary so the comparison of default params vs RHS assignment works
                equalsPos = int.MaxValue;

            if (line.Contains("(") && line.IndexOf('(') < equalsPos) // Must be a global/namespace function
                return true;
            return false;
        }

        static bool ResemblesProperty(string line, Globals globals)
        {
            string[] tokens = line.Split(BREAKCHARS);
            if (tokens.Length >= 2)
            {
                int constIdx = Array.IndexOf(tokens, "const");
                int privateIdx = Array.IndexOf(tokens, "private");
                int protectedIdx = Array.IndexOf(tokens, "protected");

                int termIdx = Math.Max(constIdx, Math.Max(privateIdx, protectedIdx)) + 1;

                if (globals.Classes.ContainsKey(tokens[termIdx].Replace("@","")))
                {
                    if (tokens[termIdx+1].EndsWith(";"))
                        return true;
                    if (tokens.Length-1 >= termIdx+2 && tokens[termIdx+2].Equals("="))
                        return true;
                }
            }
            return false;
        }
    }
}

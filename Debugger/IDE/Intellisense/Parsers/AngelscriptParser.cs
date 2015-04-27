using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense.Parsers
{
    /// <summary>
    /// A more complete psuedo-Angelscript parser
    /// 
    /// Criteria are:
    /// Parse a full code file extracting to Gobal -> Namespace -> Class -> Class Properties + Class Methods depth
    /// Use DepthScanner to track depth awareness
    /// Inline #include files
    /// </summary>
    public class AngelscriptParser : ParserBase
    {
        public override Globals Parse(string path, string fileCode, string[] includePaths)
        {
            Globals ret = new Globals(false);

            // Merge global app registered types into it
            ret.Parent = IDEProject.inst().GlobalTypes;

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

        protected void Parse(StringReader rdr, DepthScanner scanner, Globals globals)
        {
            int currentLine = 0;
            ParseNamespace(rdr, globals, scanner, ref currentLine, false);

            // Resolve incomplete names
            foreach (string key in globals.GetPropertyNames())
            {
                TypeInfo t = globals.GetProperty(key, true) as TypeInfo;
                if (t != null)
                {
                    if (!t.IsComplete)
                        globals.AddProperty(key, globals.GetTypeInfo(t.Name), -1, "");
                    if (t is TemplateInst)
                    {
                        TemplateInst ti = t as TemplateInst;
                        if (!ti.WrappedType.IsComplete)
                            ti.WrappedType = globals.GetTypeInfo(ti.WrappedType.Name);
                    }
                }
            }

            foreach (FunctionInfo f in globals.GetFunctions(null, true))
            {
                f.ResolveIncompletion(globals);
            }

            foreach (TypeInfo type in globals.TypeInfo)
            {
                type.ResolveIncompletion(globals);
            }
        }

        protected void ParseNamespace(StringReader rdr, Globals globals, DepthScanner scanner, ref int currentLine, bool asNamespace)
        {
            string l = "";
            int depth = scanner.GetBraceDepth(currentLine);
            bool hitDeeper = false;
            while ((l = rdr.ReadLine()) != null)
            {
                string defName = "";
                int lineNumber = 0;
                ExtractLineInfo(ref l, out defName, out lineNumber);
                
                ++currentLine;
                if (l.Trim().StartsWith("//"))
                    continue;
                if (l.Trim().StartsWith("#"))
                    continue;
                int curDepth = scanner.GetBraceDepth(currentLine-1);
                if (!asNamespace)
                {
                    if (curDepth < depth) // Left our namespace depth
                        return;
                    else if (curDepth > depth) // Outside of the desired scanning depth (namespace level)
                        continue;
                }
                else
                {
                    if (curDepth > depth)
                        hitDeeper = true;
                    if (curDepth == depth && hitDeeper)
                        return;
                    else if (curDepth > depth + 1) //We do not go deeper than the namespace
                        continue;
                }

                string line = l.Trim();
                if (line.Length == 0)
                    continue;
                string[] tokens = line.Split(BREAKCHARS);

                // Class / Interface/ Template type
                if (ResemblesClass(line))
                {
                    int abstractIdx = Array.IndexOf(tokens, "abstract");
                    int sharedIdx = Array.IndexOf(tokens, "shared");
                    int templateIdx = Array.IndexOf(tokens, "template");
                    int mixinIdx = Array.IndexOf(tokens, "mixin");
                    int finalIdx = Array.IndexOf(tokens, "final");

                    int classTermIdx = Math.Max(abstractIdx, Math.Max(sharedIdx, Math.Max(templateIdx, Math.Max(mixinIdx, finalIdx)))) + 1;

                    bool isInterface = tokens[classTermIdx].Equals("interface");
                    if (templateIdx != -1)
                    {
                        //Resolve template type?
                    }

                    string className = tokens[classTermIdx + 1];
                    TypeInfo ti = new TypeInfo { Name = className, 
                        IsTemplate = templateIdx != -1, IsShared = sharedIdx != -1,
                        IsAbstract = abstractIdx != -1, IsMixin = mixinIdx != -1,
                        IsInterface = isInterface,
                        SourceLine = lineNumber, SourceFile = defName };

                    // Get any baseclasses, baseclass must appear first
                    for (int i = classTermIdx + 2; i < tokens.Length; ++i)
                    {
                        string baseName = tokens[i];
                        if (globals.ContainsTypeInfo(baseName))
                            ti.BaseTypes.Add(globals.GetTypeInfo(baseName));
                    }
                    ParseClass(rdr, globals, scanner, ti, ref currentLine);
                    globals.AddTypeInfo(className, ti);
                } 
                else if (tokens[0].ToLower().Equals("namespace")) // Namespace
                {
                    string nsName = tokens[1];
                    Globals namespaceGlobals = null;
                    if (globals.ContainsNamespace(nsName)) // Check if the namespace has been encountered before
                        namespaceGlobals = globals.GetNamespace(nsName);
                    else
                        namespaceGlobals = new Globals(false);
                    namespaceGlobals.Parent = globals;
                    ParseNamespace(rdr, namespaceGlobals, scanner, ref currentLine, true);
                    namespaceGlobals.Name = nsName;
                    globals.AddNamespace(nsName, namespaceGlobals);
                }
                else if (tokens[0].ToLower().Equals("enum")) // Enumeration
                {
                    ParseEnum(line, rdr, globals, ref currentLine);
                }
                else
                {
                    if (ResemblesFunction(line)) // Global/namespace function
                    {
                        try
                        {
                            FunctionInfo fi = _parseFunction(rdr, line, globals, lineNumber, defName);
                            globals.AddFunction(fi);
                        }
                        catch (Exception ex) { }
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
                            TypeInfo wrapped = globals.GetTypeInfo(containedType);
                            TemplateInst ti = new TemplateInst() { Name = templateType, IsTemplate = true, WrappedType = wrapped != null ? wrapped : new TypeInfo { Name = containedType, IsComplete = false, SourceLine = lineNumber, SourceFile = defName } };
                            globals.AddProperty(parts[termIdx + 1], ti, lineNumber, defName);
                            //if (constIdx != -1)
                            //    globals.ReadonlyProperties.Add(tokens[termIdx + 1]);
                        }
                        else
                        {
                            string pname = parts[termIdx].EndsWith("@") ? parts[termIdx].Substring(0, parts[termIdx].Length - 1) : parts[termIdx]; //handle
                            TypeInfo pType = null;
                            if (globals.ContainsTypeInfo(pname))
                                pType = globals.GetTypeInfo(pname);
                            if (pType == null)
                            { //create temp type to resolve later
                                pType = new TypeInfo() { Name = pname, IsComplete = false, SourceLine = lineNumber, SourceFile = defName };
                            }
                            globals.AddProperty(parts[termIdx + 1], pType, lineNumber, defName);
                            //if (constIdx != -1)
                            //    globals.ReadonlyProperties.Add(tokens[termIdx + 1]);
                        }
                    }
                }
            }
        }

        protected void ParseClass(StringReader rdr, Globals targetGlobals, DepthScanner scanner, TypeInfo targetType, ref int currentLine)
        {
            int depth = scanner.GetBraceDepth(currentLine);
            string l = "";
            bool hitDeeper = false;
            while ((l = rdr.ReadLine()) != null)
            {
                string defName = "";
                int lineNumber = 0;
                ExtractLineInfo(ref l, out defName, out lineNumber);
                l = l.Trim();

                ++currentLine;
                int curDepth = scanner.GetBraceDepth(currentLine-1);
                if (curDepth > depth)
                    hitDeeper = true;
                if (curDepth == depth && hitDeeper)
                    return;
                else if (curDepth > depth + 1) //We do not go deeper than class
                    continue;
                if (ResemblesFunction(l))
                {
                    targetType.Functions.Add(_parseFunction(rdr, l, targetGlobals, lineNumber, defName));
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
                        TypeInfo wrapped = targetGlobals.GetTypeInfo(containedType);
                        TemplateInst ti = new TemplateInst() { Name = templateType, IsTemplate = true, WrappedType = wrapped != null ? wrapped : new TypeInfo { Name = containedType, IsComplete = false, SourceLine = lineNumber, SourceFile = defName } };
                        targetType.Properties[tokens[termIdx+1]] = ti;
                        targetType.PropertyLines[tokens[termIdx + 1]] = lineNumber;
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
                        if (targetGlobals.ContainsTypeInfo(pname))
                            pType = targetGlobals.GetTypeInfo(pname);
                        if (pType == null)
                        { //create temp type to resolve later
                            pType = new TypeInfo() { Name = pname, IsComplete = false, SourceLine = lineNumber, SourceFile = defName };
                        }
                        targetType.Properties[tokens[termIdx + 1]] = pType;
                        targetType.PropertyLines[tokens[termIdx + 1]] = lineNumber;
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

        protected void ParseEnum(string line, StringReader rdr, Globals globals, ref int currentLine) {
            string[] nameparts = line.Split(' ');
            string enumName = nameparts[1];
            List<string> enumValues = new List<string>();
            while ((line = rdr.ReadLine()) != null) {
                string defName = "";
                int lineNumber = 0;
                ExtractLineInfo(ref line, out defName, out lineNumber);

                ++currentLine;
                if (line.Equals("};")) {
                    EnumInfo ret = new EnumInfo { Name = enumName };
                    ret.Values.AddRange(enumValues);
                    globals.AddTypeInfo(enumName, ret);
                    return;
                } else if (line.Contains(',')) {
                    int sub = line.IndexOf(',');
                    enumValues.Add(line.Substring(0, sub));
                }
            }
        }

        protected FunctionInfo _parseFunction(StringReader rdr, string line, Globals globals, int lineNumber, string defName)
        {
            int firstParen = line.IndexOf('(');
            int lastParen = line.LastIndexOf(')');
            while (lastParen == -1)
            {
                string l = rdr.ReadLine();
                string d = "";
                int ln = 0;
                ExtractLineInfo(ref l, out d, out ln);
                l = l.Trim();
                if (l.StartsWith("#")) //#if's in shaders
                    continue;
                line += l;
                lastParen = line.LastIndexOf(')');
            }
            string baseDecl = line.Substring(0, firstParen);
            string paramDecl = line.Substring(firstParen, lastParen - firstParen + 1); //-1 for the ;
            string[] nameParts = baseDecl.Split(SPACECHAR, StringSplitOptions.RemoveEmptyEntries);
            TypeInfo retType = null;

            int sharedIdx = Array.IndexOf(nameParts, "shared");
            int importIdx = Array.IndexOf(nameParts, "import");
            int funcdefIdx = Array.IndexOf(nameParts, "funcdef");

            // Return type is at this index
            int startIdx = Math.Max(sharedIdx, Math.Max(importIdx, funcdefIdx)) + 1;

            //TODO: split the name parts
            if (globals.ContainsTypeInfo(nameParts[startIdx]))
            {
                retType = globals.GetTypeInfo(nameParts[startIdx]);
            }
            else if (nameParts[startIdx].Contains('<'))
            {
                string wrappedType = nameParts[startIdx].Extract('<', '>');
                string templateType = nameParts[startIdx].Replace(string.Format("<{0}>", wrappedType), "");
                TypeInfo wrapped = globals.GetTypeInfo(wrappedType);
                TemplateInst ti = new TemplateInst() { Name = nameParts[startIdx], IsTemplate = true, WrappedType = wrapped != null ? wrapped : new TypeInfo { Name = wrappedType, IsComplete = false, SourceLine = lineNumber, SourceFile = defName } };
                retType = ti;
            }
            else
            {
                retType = new TypeInfo() { Name = nameParts[startIdx], IsPrimitive = false, SourceLine = lineNumber, SourceFile = defName };
            }
            if (funcdefIdx == -1)
            {
                return new FunctionInfo { Name = nameParts[startIdx+1], ReturnType = retType, Inner = paramDecl, 
                    IsImport = importIdx != -1, IsShared = sharedIdx != -1,
                    SourceLine = lineNumber, SourceFile = defName };
            } 
            else
            {
                return new FuncDefInfo { Name = nameParts[startIdx + 1], ReturnType = retType, Inner = paramDecl,
                    IsImport = importIdx != -1, IsShared = sharedIdx != -1, 
                    SourceLine = lineNumber, SourceFile = defName };
            }
        }

        public override bool ResemblesFunction(string line)
        {
            int equalsPos = line.IndexOf('=');
            if (equalsPos == -1) // Scale it out to max, this is necessary so the comparison of default params vs RHS assignment works
                equalsPos = int.MaxValue;

            if (line.Contains("(") && line.IndexOf('(') < equalsPos) // Must be a global/namespace function
                return true;
            return false;
        }

        public override bool ResemblesProperty(string line, Globals globals)
        {
            string[] tokens = line.Split(BREAKCHARS);
            if (tokens.Length >= 2)
            {
                int constIdx = Array.IndexOf(tokens, "const");
                int privateIdx = Array.IndexOf(tokens, "private");
                int protectedIdx = Array.IndexOf(tokens, "protected");

                int termIdx = Math.Max(constIdx, Math.Max(privateIdx, protectedIdx)) + 1;

                if (globals.ContainsTypeInfo(tokens[termIdx].Replace("@","")))
                {
                    if (tokens[termIdx+1].EndsWith(";"))
                        return true;
                    if (tokens.Length-1 >= termIdx+2 && tokens[termIdx+2].Equals("="))
                        return true;
                }
            }
            return false;
        }

        public override bool ResemblesClass(string line)
        {
            string[] tokens = line.Split(BREAKCHARS);
            // struct is here to cover HLSL
            return Array.IndexOf(tokens, "class") != -1 || Array.IndexOf(tokens, "interface") != -1 || Array.IndexOf(tokens, "struct") != -1;
        }
    }
}

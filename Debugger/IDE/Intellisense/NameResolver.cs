﻿using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense {

    //Works with DepthScanner data to start from current position,
    //scan lines upwards looking for lines containing the term
    public class NameResolver {
        static readonly char[] SPACECHAR = { ' ' };
        static readonly char[] BREAKCHARS = { ' ', '.', ',', '*','/','%','(','{','}',')',';','=','[',']','\t','\r','\n'};
        DepthScanner scanner_;
        Globals globals_;

        public NameResolver(Globals globals, DepthScanner aScanner) {
            scanner_ = aScanner;
            globals_ = globals;
        }

        public void GetNameMatch(TextDocument aDoc, int line, string text, ref List<string> suggestions) {
            int depth = scanner_.GetBraceDepth(line);
            do {
                string[] lineCodes = aDoc.GetText(aDoc.Lines[line]).Split(BREAKCHARS, StringSplitOptions.RemoveEmptyEntries);
                foreach (string lineCode in lineCodes) {
                    if (lineCode.Length > 3 && lineCode.StartsWith(text)) { //contains our current text
                        int startidx = lineCode.IndexOf(text);
                        if (!suggestions.Contains(lineCode))
                            suggestions.Add(lineCode);
                    }
                }
                --line;
            } while (depth > 0 && line > 0); //function depth may be on the first 0 scanning up, same with class def
        }

        public TypeInfo GetClassType(TextDocument aDoc, int line, string text) {
            if (globals_ == null)
                return null;
            --line; //subtract one for how AvalonEdit stores text versus reports its position
            int startLine = line;
            if (text.Equals("this")) { //easy case
                int depth = scanner_.GetBraceDepth(line);
                do {
                    string lineCode = aDoc.GetText(aDoc.Lines[line]);
                    if (lineCode.Contains("class ")) {
                        string[] parts = lineCode.Trim().Split(SPACECHAR, StringSplitOptions.RemoveEmptyEntries);
                        if (parts[0].Equals("shared") && globals_.ContainsTypeInfo(parts[2]))
                            return globals_.GetTypeInfo(parts[2]);
                        else if (globals_.ContainsTypeInfo(parts[1]))
                            return globals_.GetTypeInfo(parts[1]);
                        else
                            break;
                    }
                    depth = scanner_.GetBraceDepth(line);
                    --line;
                } while (depth > 0 && line > 0); //class def may be on last line

                //unkonwn class
                int curDepth = depth;
                string[] nameparts = aDoc.GetText(aDoc.Lines[line]).Trim().Split(SPACECHAR, StringSplitOptions.RemoveEmptyEntries);
                string className = "";
                if (nameparts[0].Equals("shared"))
                    className = nameparts[2];
                else
                    className = nameparts[3];
                //TODO get baseclasses
                TypeInfo tempType = new TypeInfo() { Name = className };
                ++line;
                do {
                    depth = scanner_.GetBraceDepth(line);
                    if (depth == curDepth+1) {
                        string lineCode = aDoc.GetText(aDoc.Lines[line]);
                        string[] words = aDoc.GetText(aDoc.Lines[line]).Trim().Split(SPACECHAR, StringSplitOptions.RemoveEmptyEntries);
                        if (words != null && words.Length > 1) {
                            if (words[1].Contains("(")) { //function
                                if (globals_.ContainsTypeInfo(words[0])) {

                                }
                            } else {
                                string rettype = FilterTypeName(words[0]);
                                string propname = FilterTypeName(words[1]);
                                if (globals_.ContainsTypeInfo(rettype)) {
                                    tempType.Properties[propname] = globals_.GetTypeInfo(rettype);
                                }
                            }
                        }
                    }
                    ++line;
                } while (line < startLine);
                return tempType;
            }

            //SCOPE block for depth
            {
                int depth = scanner_.GetBraceDepth(line);
                bool indexType = false;
                if (text.Contains('[')) {
                    indexType = true;
                    text = text.Substring(0, text.IndexOf('['));
                }
                do {
                    string lineCode = aDoc.GetText(aDoc.Lines[line]).Trim();   
                    if (lineCode.Contains(text)) {
                        int endidx = lineCode.IndexOf(text);
                        int idx = endidx;
                        bool okay = true;
                        bool hitSpace = false;
                        while (idx > 0) { //scan backwards to find the typename
                            if (!char.IsLetterOrDigit(lineCode[idx]) && lineCode[idx] != ' ' && lineCode[idx] != '@' && lineCode[idx] != '&') {
                                okay = false;
                                ++idx;
                                break;
                            }
                            if (!hitSpace && lineCode[idx] == ' ')
                                hitSpace = true;
                            else if (lineCode[idx] == ' ')
                            {
                                break;
                            }
                            --idx;
                        }
                        if (idx < 0) idx = 0;
                        string substr = endidx - idx > 0 ? FilterTypeName(lineCode.Substring(idx, endidx - idx).Trim()) : "";
                        if (substr.Length > 0) { //not empty
                            if (substr.StartsWith(">")) {//TEMPLATE DEFINITION
                                if (!indexType)
                                    substr = lineCode.Substring(0, lineCode.IndexOf('<'));
                                else {
                                    int start = lineCode.IndexOf('<');
                                    int end = lineCode.IndexOf('>');
                                    substr = lineCode.Substring(start+1, end - start - 1);
                                    substr = substr.Replace("@", "");
                                }
                            }
                            if (globals_.ContainsTypeInfo(substr)) {
                                //Found a class
                                return globals_.GetTypeInfo(substr);
                            }
                        }
                    }
                    --line;
                    depth = scanner_.GetBraceDepth(line);
                } while (depth >= 0 && line > 0);
                return null;
            }
        }

        string FilterTypeName(string input)
        {
            return input.Replace("@", "").Replace("&in", "").Replace("&", "").Replace(";","");
        }
    }
}

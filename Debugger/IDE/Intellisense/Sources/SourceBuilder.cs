using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense.Sources
{
    public static class SourceBuilder
    {
        public static bool HandlesExtension(string ext)
        {
            if (ext.ToLowerInvariant().Equals(".as"))
                return true;
            else if (ext.ToLowerInvariant().Equals(".glsl"))
                return true;
            else if (ext.ToLowerInvariant().Equals(".hlsl"))
                return true;
            return false;
        }

        public static IntellisenseSource GetSourceForExtension(string ext)
        {
            if (ext.ToLowerInvariant().Equals(".as"))
                return new AngelscriptSource();
            else if (ext.ToLowerInvariant().Equals(".glsl"))
                return new GLSLSource();
            else if (ext.ToLowerInvariant().Equals(".hlsl"))
                return new HLSLSource();
            return null;
        }
    }
}

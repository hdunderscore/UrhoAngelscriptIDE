using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLib
{
    public interface ICompileErrorPublisher
    {
        void PublishError(CompileError error);
        void PublishWarning(CompileError warning);
        void PushOutput(string text);
    }
}

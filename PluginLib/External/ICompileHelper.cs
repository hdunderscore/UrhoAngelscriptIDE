﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLib
{
    public interface ICompileHelper
    {
        void PublishError(CompileError error);
        void PushOutput(string text);
        string GetProjectDirectory();
        string GetProjectSourceTree();
    }
}

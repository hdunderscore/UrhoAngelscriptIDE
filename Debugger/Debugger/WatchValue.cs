using Debugger.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.Debug {
    
    /// <summary>
    /// A 'stringified' as dot path expression
    /// 
    /// Resolves against the locals/this/globals JWrapper trees
    /// </summary>
    public class WatchValue : BaseClass {
        string path_ = "";
        string evaluatedAs_= "";

        public void Update() {
            try {
                if (path_.Trim().Length == 0) {
                    Value = "{Nothing to evaluate}";
                    return;
                }
                if (path_.StartsWith("this."))
                    Evaluate(Debugger.Debug.SessionData.inst().ThisData);
                else if (path_.StartsWith("global."))
                    Evaluate(Debugger.Debug.SessionData.inst().GlobalData);
                else
                    Evaluate(Debugger.Debug.SessionData.inst().LocalData);
            }
            catch (Exception ex) {
                Value = ex.Message;
            }
        }

        public string Variable { get { return path_; } 
            set { 
                path_ = value;
                Update();
                OnPropertyChanged("Variable");
            } 
        }
        public string Value { 
            get { return evaluatedAs_; } 
            set { 
                evaluatedAs_ = value;
                if (Debugger.Net.DebugClient.inst() != null)
                {
                    JWrapper root = null;
                    if (path_.StartsWith("this."))
                        root = Debugger.Debug.SessionData.inst().ThisData;
                    else if (path_.StartsWith("global."))
                        root = Debugger.Debug.SessionData.inst().GlobalData;
                    else
                        root = Debugger.Debug.SessionData.inst().LocalData;

                    JWrapper end = root.ResolveDotPath(Variable.Replace("this.","").Replace("global.","").Split('.'));
                    if (end is JLeaf)
                    {
                        string tildePath = end.GetTildePath();
                        Json.JWrapper topLevel = end.GetTopMost();
                        ((JLeaf)end).Value = value;
                        if (topLevel.Name.Equals("This"))
                        {
                            // The userdata int is stored as "Depth" but need the index here
                            int val = Debug.SessionData.inst().CallStack.Count - 1;
                            tildePath = String.Format("{0}~{1}", val - ((int)topLevel.UserData), tildePath);
                            Debugger.Net.DebugClient.inst().SetThisValue(tildePath, value);
                        }
                        else if (topLevel.Name.Equals("Globals"))
                        {
                            Debugger.Net.DebugClient.inst().SetGlobalValue(tildePath, value);
                        }
                        else
                            Debugger.Net.DebugClient.inst().SetStackValue(tildePath, value);
                    }
                    
                }
                OnPropertyChanged("Value"); 
            } 
        }
        public JLeaf lastResolvedAs;

        public bool Evaluate(JWrapper root) {
            if (root == null) {
                Value = "{Impossible to evaluate while not debugging}";
                return false;
            }
            JWrapper end = root.ResolveDotPath(Variable.Replace("this.","").Replace("global.","").Split('.'));
            if (end is JLeaf) {
                lastResolvedAs = end as JLeaf;
                Value = lastResolvedAs.Value;
                OnPropertyChanged("IsEditable");
                return true;
            } else {
                Value = "{Unable to resolve value; Likely doesn't exist in scope}";
                OnPropertyChanged("IsEditable");
                return false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense
{
    public interface PossiblyIncomplete
    {
        void ResolveIncompletion(Globals globs);
    }

    public interface TypeImageSource
    {
        string ImgSource { get; }
    }

    public class PropInfo : PossiblyIncomplete, TypeImageSource
    {
        public string Name { get; set; }
        public bool ReadOnly { get; set; }
        public bool Protected { get; set; }
        public bool IsReference { get; set; }
        public TypeInfo Container { get; set; } //only relevant to the master class list
        public TypeInfo Type { get; set; }
        public TypeInfo WrappedType { get; set; } //only relevant when we're a template

        public string SourceFile { get; set; }
        public int SourceLine { get; set; }
        public bool CanGoToDef { get { return SourceLine > 0; } }

        public string ImgSource
        {
            get
            {
                if (ReadOnly)
                    return "/Images/all/roproperty.png";
                else if (Protected)
                    return "/Images/all/proproperty.png";
                return "/Images/all/property.png";
            }
        }

        public void ResolveIncompletion(Globals globs)
        {
            if (!Type.IsComplete && globs.ContainsTypeInfo(Type.Name))
                Type = globs.GetTypeInfo(Type.Name);
            if (WrappedType != null && !WrappedType.IsComplete && globs.ContainsTypeInfo(WrappedType.Name))
                WrappedType = globs.GetTypeInfo(WrappedType.Name);
        }
    }

    public class NamespaceInfo
    {
        public string Name { get; set; }
        public Globals Globals { get; set; }
    }

    public abstract class BaseTypeInfo
    {

        public abstract BaseTypeInfo ResolvePropertyPath(Globals globals, params string[] path);
    }

    public class TypeInfo : BaseTypeInfo, PossiblyIncomplete, TypeImageSource
    {
        public TypeInfo()
        {
            Properties = new Dictionary<string, TypeInfo>();
            BaseTypeStr = new List<string>();
            BaseTypes = new List<TypeInfo>();
            Functions = new List<FunctionInfo>();
            ReadonlyProperties = new List<string>();
            ProtectedProperties = new List<string>();
            PrivateProperties = new List<string>();
            IsComplete = true; //default we'll assume complete
            IsPrimitive = false;
            SourceLine = 0;
            PropertyLines = new Dictionary<string, int>();
        }

        public override BaseTypeInfo ResolvePropertyPath(Globals globals, params string[] path)
        {
            string str = path[0];
            if (str.Contains('('))
            {
                string content = str.Substring(0, str.IndexOf('('));
                FunctionInfo fi = Functions.FirstOrDefault(f => f.Name.Equals(content));
                if (fi != null)
                {
                    if (str.Contains('[') && fi.ReturnType is TemplateInst)
                        return ((TemplateInst)fi.ReturnType).WrappedType;
                    else if (fi.ReturnType is TemplateInst)
                    {
                        return globals.GetTypeInfo(fi.ReturnType.Name.Substring(0, fi.ReturnType.Name.IndexOf('<')));
                    }
                    return fi.ReturnType;
                }
            }
            else if (str.Contains('['))
            {
                string content = str.Extract('[', ']');
                str = str.Replace(string.Format("[{0}]", content), "");
                TemplateInst ti = Properties[str] as TemplateInst;
                if (ti != null && path.Length > 1)
                {
                    TypeInfo t = ti.WrappedType;
                    return t.ResolvePropertyPath(globals, path.SubArray(1, path.Length - 1));
                }
                if (ti == null)
                    return null;
                else if (ti.WrappedType == null)
                    return ti;
                return globals.GetTypeInfo(ti.WrappedType.Name);
            }
            else if (Properties.ContainsKey(path[0]))
            {
                BaseTypeInfo ti = Properties[path[0]];
                if (ti is TemplateInst)
                    ti = globals.GetTypeInfo(((TemplateInst)ti).Name);
                if (path.Length > 1)
                    ti = ti.ResolvePropertyPath(globals, path.SubArray(1, path.Length - 1));
                return ti;
            } return null;
        }

        public string Description
        {
            get
            {
                if (IsTemplate)
                    return "template ";
                if (IsPrimitive)
                    return "prim ";
                if (IsAbstract && IsShared)
                    return "shared abstract ";
                else if (IsAbstract)
                    return "abstract ";
                if (IsInterface && IsShared)
                    return "shared interface ";
                else if (IsInterface)
                    return "interface ";

                if (IsMixin) //can mixin's be shared?
                    return "mixin ";
                if (IsShared)
                    return "shared class ";
                return "class ";
            }
        }

        // Type traits
        public bool IsAbstract { get; set; }
        public bool IsTemplate { get; set; }
        public bool IsPrimitive { get; set; }
        public bool IsComplete { get; set; }
        public bool IsInterface { get; set; }
        public bool IsShared { get; set; }
        public bool IsMixin { get; set; }
        public List<string> BaseTypeStr { get; private set; }
        public List<TypeInfo> BaseTypes { get; private set; }
        public string Name { get; set; }

        // Property Data
        //\todo outsource to PropInfo?
        public Dictionary<string, TypeInfo> Properties { get; private set; }
        public Dictionary<string, int> PropertyLines { get; private set; }
        public List<string> ReadonlyProperties { get; private set; }
        public List<string> ProtectedProperties { get; private set; }
        public List<string> PrivateProperties { get; private set; } //Not the same as reaonly

        // Function Data
        public List<FunctionInfo> Functions { get; private set; }

        // Type definition
        public string SourceFile { get; set; }
        public int SourceLine { get; set; }
        public bool CanGoToDef { get { return SourceLine > 0; } }

        public List<object> PropertyUI
        {
            get
            {
                List<object> ret = new List<object>();
                if (BaseTypes.Count > 0)
                    ret.Add(new TypeList(BaseTypes.ToArray()));
                foreach (string key in Properties.Keys)
                    ret.Add(new PropInfo { Name = key, Type = Properties[key], ReadOnly = PrivateProperties.Contains(key) || ReadonlyProperties.Contains(key), Protected = ProtectedProperties.Contains(key), SourceLine = PropertyLines[key], SourceFile = SourceFile });
                foreach (FunctionInfo f in Functions)
                    ret.Add(f);
                return ret;
            }
        }

        public void ResolveIncompletion(Globals globs)
        {
            foreach (FunctionInfo f in Functions)
                f.ResolveIncompletion(globs);
            List<string> keys = new List<string>(Properties.Keys);
            foreach (string key in keys)
            {
                if (!Properties[key].IsComplete)
                {
                    Properties[key] = globs.GetTypeInfo(Properties[key].Name);
                }
            }
        }

        public string ImgSource
        {
            get
            {
                if (IsMixin)
                    return "/Images/all/mixin.png";
                else if (IsInterface && IsShared)
                    return "Images/all/sharedinterface.png";
                else if (IsInterface)
                    return "/Images/all/interface.png";
                else if (IsShared)
                    return "/Images/all/sharedclass.png";
                return "/Images/all/class.png";
            }
        }
    }

    public class TypeList : TypeImageSource
    {
        public TypeList(params TypeInfo[] info)
        {
            Types = new List<TypeInfo>();
            Types.AddRange(info);
        }

        public List<TypeInfo> Types { get; private set; }

        public string ImgSource
        {
            get { return "/Images/all/supertypes.png"; }
        }
    }

    public class FunctionInfo : BaseTypeInfo, PossiblyIncomplete, TypeImageSource
    {
        public FunctionInfo()
        {
            Params = new List<TypeInfo>();
        }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsProtected { get; set; }
        public bool IsShared { get; set; }
        public bool IsImport { get; set; }
        public TypeInfo ReturnType { get; set; }
        public string Inner { get; set; }
        public List<TypeInfo> Params { get; set; }
        public bool IsConst { get; set; }
        public string SourceFile { get; set; }
        public int SourceLine { get; set; }
        public bool CanGoToDef { get { return SourceLine > 0; } }

        public override BaseTypeInfo ResolvePropertyPath(Globals globals, params string[] path)
        {
            return ReturnType;
        }

        public void ResolveIncompletion(Globals globs)
        {
            if (globs.ContainsTypeInfo(ReturnType.Name))
                ReturnType = globs.GetTypeInfo(ReturnType.Name);
            if (ReturnType is TemplateInst)
            { //not found in globals
                ((TemplateInst)ReturnType).WrappedType = globs.GetTypeInfo(((TemplateInst)ReturnType).WrappedType.Name);
            }
            for (int i = 0; i < Params.Count; ++i)
            {
                if (!Params[i].IsComplete && globs.ContainsTypeInfo(Params[i].Name))
                    Params[i] = globs.GetTypeInfo(Params[i].Name);
            }
        }

        public string ImgSource
        {
            get
            {
                if (Name.StartsWith("op"))
                    return "/Images/all/opmethod.png";
                if (this is FuncDefInfo) //Bad bad bad
                {
                    if (IsShared)
                        return "/Images/all/sharedevent.png";
                    else if (IsImport) //\todo Is this scenario possible?
                        return "/Images/all/sharedimportmethod.png";
                    return "/Images/all/event.png";
                }
                if (IsShared)
                    return "/Images/all/sharedmethod.png";
                else if (IsImport)
                    return "/Images/all/importmethod.png";
                else if (IsPrivate)
                    return "/Images/all/primethod.png";
                else if (IsProtected)
                    return "/Images/all/promethod.png";
                return "/Images/all/method.png";
            }
        }
    }

    public class FuncDefInfo : FunctionInfo
    {

    }

    public class EnumInfo : TypeInfo, TypeImageSource
    {
        public EnumInfo()
        {
            Values = new List<string>();
        }
        public List<string> Values { get; private set; }

        public new string Description
        {
            get
            {
                if (IsShared)
                    return "shared enum ";
                return "enum ";
            }
        }

        public new string ImgSource
        {
            get
            {
                if (IsShared)
                    return "/Images/all/sharedenum.png";
                return "/Images/all/enum.png";
            }
        }
    }

    public class TemplateInfo : TypeInfo
    {
    }

    public class TemplateInst : TypeInfo
    {
        public TypeInfo WrappedType { get; set; }
    }

    public class Globals : BaseTypeInfo
    {
        public Globals(bool createPrimitives)
        {
            Properties = new Dictionary<string, TypeInfo>();
            Functions = new List<FunctionInfo>();
            Classes = new Dictionary<string, TypeInfo>();
            Namespaces = new Dictionary<string, Globals>();
            ReadonlyProperties = new List<string>();
            PropertyLines = new Dictionary<string, int>();
            PropertyFiles = new Dictionary<string, string>();

            if (createPrimitives)
            {
                Classes["void"] = new TypeInfo() { Name = "void", IsPrimitive = true };
                Classes["int"] = new TypeInfo() { Name = "int", IsPrimitive = true };
                Classes["uint"] = new TypeInfo() { Name = "uint", IsPrimitive = true };
                Classes["float"] = new TypeInfo() { Name = "float", IsPrimitive = true };
                Classes["double"] = new TypeInfo() { Name = "double", IsPrimitive = true };
                Classes["bool"] = new TypeInfo() { Name = "bool", IsPrimitive = true };

                //extended types
                Classes["int8"] = new TypeInfo() { Name = "int8", IsPrimitive = true };
                Classes["int16"] = new TypeInfo() { Name = "int16", IsPrimitive = true };
                Classes["int64"] = new TypeInfo() { Name = "int64", IsPrimitive = true };
                Classes["uint8"] = new TypeInfo() { Name = "uint8", IsPrimitive = true };
                Classes["uint16"] = new TypeInfo() { Name = "uint16", IsPrimitive = true };
                Classes["uint64"] = new TypeInfo() { Name = "uint64", IsPrimitive = true };
            }
        }

        // Present usage only wants classes
        public void MergeIntoThis(Globals other)
        {
            foreach (string k in other.Classes.Keys)
            {
                if (!Classes.ContainsKey(k))
                    Classes[k] = IDEProject.inst().GlobalTypes.Classes[k];
            }
        }

        public Globals Parent { get; set; }
        public string Name { get; set; } // Only applies if this is a Namespace
        Dictionary<string, Globals> Namespaces { get; set; }
        Dictionary<string, TypeInfo> Classes { get; set; }
        Dictionary<string, TypeInfo> Properties { get; set; }
        Dictionary<string, int> PropertyLines { get; set; }
        Dictionary<string, string> PropertyFiles { get; set; }

        public List<string> ReadonlyProperties { get; private set; }

        // Used For UI
        IEnumerable<string> ClassKeys { get { return Classes.Keys; } }
        public List<TypeInfo> TypeInfo { get { return new List<TypeInfo>(Classes.Values); } }
        List<FunctionInfo> Functions { get; set; }
        public List<object> UIView
        {
            get
            {
                List<object> ret = new List<object>();
                foreach (string key in Properties.Keys)
                    ret.Add(new PropInfo { Name = key, Type = Properties[key], SourceLine = PropertyLines[key], SourceFile = PropertyFiles[key] });
                foreach (FunctionInfo fi in Functions)
                    ret.Add(fi);
                return ret;
            }
        }

        // 
        public List<object> AllUIView
        {
            get
            {
                List<object> ret = new List<object>();
                // Namespaces first
                foreach (string key in Namespaces.Keys)
                    ret.Add(new NamespaceInfo { Name = key, Globals = Namespaces[key] });
                // Classes next
                foreach (string key in Classes.Keys)
                    ret.Add(Classes[key]);
                // Global functions
                ret.AddRange(Functions);
                // Global properties
                foreach (string key in Properties.Keys)
                    ret.Add(new PropInfo { Name = key, Type = Properties[key], SourceLine = PropertyLines[key], SourceFile = PropertyFiles[key] });
                return ret;
            }
        }

        public bool ContainsTypeInfo(string name)
        {
            if (name.Contains("::"))
            {
                string[] parts = name.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                Globals ns = GetNamespace(name);
                if (ns != null)
                    return ns.ContainsTypeInfo(name);
                if (Parent != null)
                    return Parent.ContainsTypeInfo(name);
                return false;
            }
            else
            {
                if (Classes.ContainsKey(name))
                    return true;
                if (Parent != null)
                    return Parent.ContainsTypeInfo(name);
                return false;
            }
        }

        public TypeInfo GetTypeInfo(string name)
        {
            if (name.Contains("::"))
            {
                string[] parts = name.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                Globals ns = GetNamespace(name);
                if (ns != null)
                    return ns.GetTypeInfo(parts[1]);
                if (Parent != null)
                    return Parent.GetTypeInfo(name);
                return null;
            }
            else
            {
                if (Classes.ContainsKey(name))
                    return Classes[name];
                if (Parent != null)
                    return Parent.GetTypeInfo(name);
                return null;
            }
        }

        public void AddTypeInfo(string name, TypeInfo ti)
        {
            Classes[name] = ti;
        }

        public IEnumerable<string> GetTypeInfoNames()
        {
            List<string> ret = new List<string>();
            ret.AddRange(Classes.Keys);
            if (Parent != null)
                ret.AddRange(Parent.GetTypeInfoNames());
            return ret;
        }

        public bool ContainsFunction(string name)
        {
            if (name.Contains("::"))
            {
                string[] parts = name.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                Globals ns = GetNamespace(name);
                if (ns != null)
                    return ns.ContainsFunction(name);
                if (Parent != null)
                    return Parent.ContainsFunction(name);
                return false;
            }
            else
            {
                if (Functions.FirstOrDefault(f => f.Name == name) != null)
                    return true;
                if (Parent != null)
                    return Parent.ContainsFunction(name);
                return false;
            }
        }

        public FunctionInfo GetFunction(string name)
        {
            if (name.Contains("::"))
            {
                string[] parts = name.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                Globals ns = GetNamespace(name);
                if (ns != null)
                    return ns.GetFunction(parts[1]);
                if (Parent != null)
                    return Parent.GetFunction(name);
                return null;
            }
            else
            {
                if (Classes.ContainsKey(name))
                    return Functions.FirstOrDefault(f => f.Name == name);
                if (Parent != null)
                    return Parent.GetFunction(name);
                return null;
            }
        }

        public void AddFunction(FunctionInfo fi)
        {
            Functions.Add(fi);
        }

        public IEnumerable<FunctionInfo> GetFunctions(string name, bool includeBase)
        {
            List<FunctionInfo> ret = new List<FunctionInfo>();
            foreach (FunctionInfo fi in Functions)
            {
                if (name == null || name.Equals(fi.Name))
                    ret.Add(fi);
            }
            if (Parent != null && includeBase)
                ret.AddRange(Parent.GetFunctions(name, includeBase));
            return ret;
        }

        public bool ContainsNamespace(string ns)
        {
            if (Namespaces.ContainsKey(ns))
                return true;
            if (Parent != null)
                return Parent.ContainsNamespace(ns);
            return false;
        }

        public Globals GetNamespace(string ns)
        {
            if (Namespaces.ContainsKey(ns))
                return Namespaces[ns];
            if (Parent != null)
                return Parent.GetNamespace(ns);
            return null;
        }

        public void AddNamespace(string ns, Globals g)
        {
            Namespaces[ns] = g;
        }

        public bool ContainsProperty(string name)
        {
            if (Properties.ContainsKey(name))
                return true;
            if (Parent != null)
                return Parent.ContainsProperty(name);
            return false;
        }

        public BaseTypeInfo GetProperty(string name, bool includeBase)
        {
            if (Properties.ContainsKey(name))
                return Properties[name];
            if (Parent != null && includeBase)
                return Parent.GetProperty(name, includeBase);
            return null;
        }

        public IEnumerable<string> GetPropertyNames()
        {
            List<string> ret = new List<string>();
            ret.AddRange(Properties.Keys);
            if (Parent != null)
                ret.AddRange(Parent.GetPropertyNames());
            return ret;
        }

        public void AddProperty(string name, TypeInfo ti, int line, string file)
        {
            Properties[name] = ti;
            PropertyLines[name] = line;
            PropertyFiles[name] = file;
        }

        // Left mostly to when used as a namespace
        public override BaseTypeInfo ResolvePropertyPath(Globals globals, params string[] path)
        {
            if (Classes.ContainsKey(path[0]))
                return Classes[path[0]];
            else if (Namespaces.ContainsKey(path[0]))
                return Namespaces[path[0]];
            foreach (FunctionInfo f in Functions)
            {
                if (f.Name.Equals(path[0]))
                    return f;
            }
            return null;
        }
    }
}
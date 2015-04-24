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

    public class PropInfo : PossiblyIncomplete
    {
        public string Name { get; set; }
        public bool ReadOnly { get; set; }
        public bool Protected { get; set; }
        public bool IsReference { get; set; }
        public TypeInfo Container { get; set; } //only relevant to the master class list
        public TypeInfo Type { get; set; }
        public TypeInfo WrappedType { get; set; } //only relevant when we're a template

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
            if (!Type.IsComplete && globs.Classes.ContainsKey(Type.Name))
                Type = globs.Classes[Type.Name];
            if (WrappedType != null && !WrappedType.IsComplete && globs.Classes.ContainsKey(WrappedType.Name))
                WrappedType = globs.Classes[WrappedType.Name];
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

    public class TypeInfo : BaseTypeInfo, PossiblyIncomplete
    {
        public TypeInfo()
        {
            Properties = new Dictionary<string, TypeInfo>();
            BaseTypeStr = new List<string>();
            BaseTypes = new List<TypeInfo>();
            Functions = new List<FunctionInfo>();
            ReadonlyProperties = new List<string>();
            ProtectedProperties = new List<string>();
            IsComplete = true; //default we'll assume complete
            IsPrimitive = false;
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
                        return globals.Classes[fi.ReturnType.Name.Substring(0, fi.ReturnType.Name.IndexOf('<'))];
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
                return globals.Classes[ti.WrappedType.Name];
            }
            else if (Properties.ContainsKey(path[0]))
            {
                BaseTypeInfo ti = Properties[path[0]];
                if (ti is TemplateInst)
                    ti = globals.Classes[((TemplateInst)ti).Name];
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
                if (this is EnumInfo)
                    return "enum ";
                return "class ";
            }
        }

        public bool IsAbstract { get; set; }
        public bool IsTemplate { get; set; }
        public bool IsPrimitive { get; set; }
        public bool IsComplete { get; set; }
        public List<string> BaseTypeStr { get; private set; }
        public List<TypeInfo> BaseTypes { get; private set; }
        public string Name { get; set; }
        public Dictionary<string, TypeInfo> Properties { get; private set; }
        public List<string> ReadonlyProperties { get; private set; }
        public List<string> ProtectedProperties { get; private set; }
        public List<string> PrivateProperties { get; private set; }
        public List<FunctionInfo> Functions { get; private set; }

        public List<object> PropertyUI
        {
            get
            {
                List<object> ret = new List<object>();
                foreach (string key in Properties.Keys)
                    ret.Add(new PropInfo { Name = key, Type = Properties[key], ReadOnly = ReadonlyProperties.Contains(key), Protected = ProtectedProperties.Contains(key) });
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
                    Properties[key] = globs.Classes[Properties[key].Name];
                }
            }
        }
    }

    public class FunctionInfo : BaseTypeInfo, PossiblyIncomplete
    {
        public FunctionInfo()
        {
            Params = new List<TypeInfo>();
        }
        public string Name { get; set; }
        public TypeInfo ReturnType { get; set; }
        public string Inner { get; set; }
        public List<TypeInfo> Params { get; set; }
        public bool IsConst { get; set; }

        public override BaseTypeInfo ResolvePropertyPath(Globals globals, params string[] path)
        {
            return ReturnType;
        }

        public void ResolveIncompletion(Globals globs)
        {
            if (globs.Classes.ContainsKey(ReturnType.Name))
                ReturnType = globs.Classes[ReturnType.Name];
            if (ReturnType is TemplateInst)
            { //not found in globals
                ((TemplateInst)ReturnType).WrappedType = globs.Classes[((TemplateInst)ReturnType).WrappedType.Name];
            }
            for (int i = 0; i < Params.Count; ++i)
            {
                if (!Params[i].IsComplete && globs.Classes.ContainsKey(Params[i].Name))
                    Params[i] = globs.Classes[Params[i].Name];
            }
        }
    }

    public class EnumInfo : TypeInfo
    {
        public EnumInfo()
        {
            Values = new List<string>();
        }
        public List<string> Values { get; private set; }
    }

    public class TemplateInfo : TypeInfo
    {
    }

    public class TemplateInst : TypeInfo
    {
        public TypeInfo WrappedType { get; set; }
    }

    public class Globals
    {
        public Globals()
        {
            Properties = new Dictionary<string, TypeInfo>();
            Functions = new List<FunctionInfo>();
            Classes = new Dictionary<string, TypeInfo>();
            Namespaces = new Dictionary<string, Globals>();
            ReadonlyProperties = new List<string>();

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
        public Dictionary<string, Globals> Namespaces { get; private set; }
        public Dictionary<string, TypeInfo> Classes { get; private set; }
        public Dictionary<string, TypeInfo> Properties { get; private set; }

        public List<string> ReadonlyProperties { get; private set; }
        
        // Used For UI
        public IEnumerable<string> ClassKeys { get { return Classes.Keys; } }
        public List<TypeInfo> TypeInfo { get { return new List<TypeInfo>(Classes.Values); } }
        public List<FunctionInfo> Functions { get; private set; }
        public List<object> UIView
        {
            get
            {
                List<object> ret = new List<object>();
                foreach (string key in Properties.Keys)
                    ret.Add(new PropInfo { Name = key, Type = Properties[key] });
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
                    ret.Add(new PropInfo { Name = key, Type = Properties[key] });
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
                return false;
            }
            else
            {
                return Classes.ContainsKey(name);
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
                return null;
            }
            else
            {
                if (Classes.ContainsKey(name))
                    return Classes[name];
                return null;
            }
        }

        public bool ContainsFunction(string name)
        {
            if (name.Contains("::"))
            {
                string[] parts = name.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                Globals ns = GetNamespace(name);
                if (ns != null)
                    return ns.ContainsFunction(name);
                return false;
            }
            else
            {
                return Functions.FirstOrDefault(f => f.Name == name) != null;
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
                return null;
            }
            else
            {
                if (Classes.ContainsKey(name))
                    return Functions.FirstOrDefault(f => f.Name == name);
                return null;
            }
        }

        public Globals GetNamespace(string ns)
        {
            if (Parent == null)
            {
                if (Namespaces.ContainsKey(ns))
                    return Namespaces[ns];
                return null;
            }
            else
            {
                return Parent.GetNamespace(ns);
            }
        }
    }
}

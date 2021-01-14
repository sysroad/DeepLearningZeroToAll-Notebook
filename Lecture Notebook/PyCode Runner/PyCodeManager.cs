using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace PyCode_Runner
{
    class Code
    {
        public FileInfo fi;
        public string code;
        public bool isModfied;
    }

    class PyCodeManager
    {
        /*
        key : python file name
        value : python code

        python files located under 'PyCode' Folder
         */
        Dictionary<string, Code> Codes { get; } = new Dictionary<string, Code>();

        public string[] CodeNames()
        {
            return Codes.Keys.ToArray();
        }

        public Code GetCode(string codeName)
        {
            if (Codes.TryGetValue(codeName, out var code))
            {
                return code;
            }
            else
            {
                return new Code();
            }
        }

        public void Load(string folder)
        {
            var di = new DirectoryInfo(folder);
            if (di.Exists)
            {
                Codes.Clear();

                foreach (var fi in di.GetFiles("*.py"))
                {
                    var key = fi.Name;
                    var code = File.ReadAllText(fi.FullName);

                    Codes.Add(key, new Code {
                        fi = fi, code = code, isModfied = false 
                    });
                }
            }
        }

        public void Update()
        {
            var iter = Codes.GetEnumerator();
            while (iter.MoveNext())
            {
                var code = iter.Current.Value;

                if (code.isModfied)
                {
                    File.WriteAllText(code.fi.FullName,
                        code.code);
                    code.isModfied = false;
                }
            }
        }

        public Code Create(string filepath)
        {
            return Create(new FileInfo(filepath));
        }

        public Code Create(FileInfo fi)
        {
            if (fi.Exists)
            {
                if (Codes.ContainsKey(fi.Name) == false)
                {
                    var code = new Code
                    {
                        code = File.ReadAllText(fi.FullName),
                        fi = new FileInfo(fi.FullName),
                        isModfied = false
                    };
                    Codes.Add(fi.Name, code);
                    return code;
                }
            }

            return null;
        }

        public void Delete(FileInfo fi)
        {
            Codes.Remove(fi.Name);
        }
    }
}

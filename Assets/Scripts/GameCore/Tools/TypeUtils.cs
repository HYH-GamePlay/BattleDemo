using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Tools{
    public static class TypeUtils{
        public static IEnumerable<Type> GetBaseType(Type baseType){
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(p => p.GetTypes())
                .Where(baseType.IsAssignableFrom).ToList();
        }

        public static Assembly FindAssembly(DirectoryInfo path){
            while (true){
                if (path == null) return null;
                if (!path.Exists) return null;
                var asmdefFile = path.GetFiles().FirstOrDefault(a => a.FullName.EndsWith(".asmdef"));
                if (asmdefFile != null){
                    var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        File.ReadAllText(asmdefFile.FullName));
                    return AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == dic["name"].ToString());
                }

                path = path.Parent;
            }
        }
    }
}
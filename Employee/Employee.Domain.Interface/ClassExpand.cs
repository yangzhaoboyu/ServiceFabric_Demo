using System;
using System.Linq;

namespace Employee.Domain.Interface
{
    public static class ClassExpand
    {
        public static string[] AllAttributes<T>(this T model)
        {
            Type type = model.GetType();
            return type.GetProperties().Select(prop => prop.Name).ToArray();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
namespace SaharokServer
{
    public static class CustomMethods
    {
        public static void MakeClone<T>(T obj, T source)
        {
            List<FieldInfo> FieldsSource = source.GetType().GetFields().ToList();
            FieldsSource.ForEach(field => field.SetValue(obj, field.GetValue(source)));
            var f = true;
        }
    }
}

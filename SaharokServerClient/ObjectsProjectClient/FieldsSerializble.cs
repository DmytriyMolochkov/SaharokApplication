using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ObjectsProjectClient
{
    public static class FieldsSerializble
    {
        public static SerializationInfo GetValue<T>(T source, SerializationInfo info, string[] excludeFields = null)
        {
            List<FieldInfo> Fields = source.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
            Fields.Where(field => !typeof(Delegate).IsAssignableFrom(field.FieldType))
                .Where(field => !excludeFields?.Contains(field.Name) ?? true).ToList()
                .ForEach(field => field.SetValue(source, info.GetValue(field.Name, field.FieldType)));
            return info;
        }

        public static SerializationInfo AddValue<T>(T source, SerializationInfo info, string[] excludeFields = null)
        {
            List<FieldInfo> Fields = source.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
            Fields.Where(field => !typeof(Delegate).IsAssignableFrom(field.FieldType))
                .Where(field => !excludeFields?.Contains(field.Name) ?? true).ToList()
                .ForEach(field => info.AddValue(field.Name, field.GetValue(source)));
            return info;
        }
    }
}

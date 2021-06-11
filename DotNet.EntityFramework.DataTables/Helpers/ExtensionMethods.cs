using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTables.AspNetCore
{
    public static class ExtensionMethods
    {
        public static T GetPropValueExt<T>(this object src, string propName)
        {
            return (T)src.GetType().GetProperty(propName).GetValue(src, null);
        }
        public static void SetPropValueExt(this object src, string PropName, object Value)
        {
            var propertyInfo = src.GetType().GetProperty(PropName);
            propertyInfo.SetValue(src, Value, null);
        }
        public static string GetPropValue(object src, string propName)
        {
            try
            {
                return src.GetType().GetProperty(propName).GetValue(src, null).ToString();
            }
            catch (Exception)
            {

                return "";
            }
        }
        public static object GetPropValueObject(this object src, string propName)
        {
            try
            {
                return src.GetType().GetProperty(propName).GetValue(src, null);
            }
            catch (Exception)
            {

                return "";
            }
        }
    }
}

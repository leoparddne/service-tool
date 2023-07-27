using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ServiceTool.Helper
{
    public static class MapperHelper
    {
        /// <summary>
        /// 将数据映射到指定的对象中
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="obj"></param>
        /// <param name="outObj"></param>
        /// <param name="ignorDesc"></param>
        /// <returns></returns>
        public static TOut AutoMap<TIn, TOut>(TIn obj, TOut outObj, bool ignorDesc = true) where TOut : new()
        {
            return AutoMap(obj, ignorDesc, outObj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="obj"></param>
        /// <param name="ignorDesc">忽略description描述信息</param>
        /// <returns></returns>
        public static TOut AutoMap<TIn, TOut>(TIn obj, bool ignorDesc = true) where TOut : new()
        {
            TOut result = new TOut();
            return AutoMap(obj, ignorDesc, result);
        }

        private static TOut AutoMap<TIn, TOut>(TIn obj, bool ignorDesc, TOut result) where TOut : new()
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();

            //存储源对象属性
            Dictionary<string, PropertyInfo> propertiesDic = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo item in properties)
            {
                propertiesDic.Add(item.Name, item);
            }

            PropertyInfo[] resultProperties = result.GetType().GetProperties();

            foreach (PropertyInfo j in resultProperties)
            {
                try
                {
                    ////自定义属性处理别名
                    DescriptionAttribute desc = (DescriptionAttribute)j.GetCustomAttributes(false).FirstOrDefault(f => f.GetType() == typeof(DescriptionAttribute));
                    if (desc != null && !ignorDesc)
                    {
                        string desName = desc.Description;
                        if (propertiesDic.ContainsKey(desName))
                        {
                            j.SetValue(result, propertiesDic[desName].GetValue(obj));
                            continue;
                        }
                    }
                    else
                    {
                        if (propertiesDic.ContainsKey(j.Name))
                        {
                            j.SetValue(result, propertiesDic[j.Name].GetValue(obj));
                        }
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        j.SetValue(result, Activator.CreateInstance(j.PropertyType));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("转换前后类型不一致");
                    }
                }
            }
            return result;
        }

        public static IList<TOut> AutoMap<TIn, TOut>(this List<TIn> list, bool ignorDesc = true) where TOut : new()
        {
            List<TOut> result = new List<TOut>();
            foreach (TIn item in list)
            {
                try
                {
                    result.Add(AutoMap<TIn, TOut>(item, ignorDesc));
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return result;
        }


        public static object AutoMapByType(object obj, Type outType, bool ignorDesc = false)
        {
            object result = Activator.CreateInstance(outType);

            PropertyInfo[] properties = obj.GetType().GetProperties();

            //存储源对象属性
            Dictionary<string, PropertyInfo> propertiesDic = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo item in properties)
            {
                propertiesDic.Add(item.Name, item);
            }

            PropertyInfo[] resultProperties = outType.GetProperties();

            foreach (PropertyInfo j in resultProperties)
            {
                try
                {
                    ////自定义属性处理别名
                    DescriptionAttribute desc = (DescriptionAttribute)j.GetCustomAttributes(false).FirstOrDefault(f => f.GetType() == typeof(DescriptionAttribute));
                    if (desc != null && !ignorDesc)
                    {
                        string desName = desc.Description;
                        if (propertiesDic.ContainsKey(desName))
                        {
                            j.SetValue(result, propertiesDic[desName].GetValue(obj));
                            continue;
                        }
                    }
                    else
                    {
                        if (propertiesDic.ContainsKey(j.Name))
                        {
                            j.SetValue(result, propertiesDic[j.Name].GetValue(obj));
                        }
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        j.SetValue(result, Activator.CreateInstance(j.PropertyType));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("转换前后类型不一致");
                    }
                }
            }
            return result;
        }
    }
}

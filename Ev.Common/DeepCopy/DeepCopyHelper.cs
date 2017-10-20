/**============================================================
* 命名空间: Ev.Common.DeepCopy
*
* 功 能： N/A
* 类 名： DeepCopyHelper
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 17:19:35 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;
using System.Collections;
using System.Reflection;

namespace Ev.Common.DeepCopy
{
    /// <summary>
    /// DeepCopy helper
    /// </summary>
    public static class DeepCopyHelper
    {
        /// <summary>
        /// Deep copy object
        /// </summary>
        /// <param name="srcobj"></param>
        /// <returns></returns>
        public static object DeepCopyRecursion(object srcobj)
        {
            if (srcobj == null)
            {
                return null;
            }

            Type srcObjType = srcobj.GetType();
            if (srcObjType.IsValueType)
            {
                return srcobj;
            }
            if (srcObjType.IsArray)
            {
                return DeepCopyArray(srcobj as Array);
            }
            if (srcObjType.IsGenericType)
            {
                return DeepCopyGenericType(srcobj);
            }
            if (srcobj is ICloneable)
            {
                return (srcobj as ICloneable).Clone();
            }
            object deepCopiedObj = Activator.CreateInstance(srcObjType);
            BindingFlags bflags = BindingFlags.DeclaredOnly | BindingFlags.Public
                                  | BindingFlags.NonPublic | BindingFlags.Instance;
            MemberInfo[] memberCollection = srcObjType.GetMembers(bflags);
            foreach (MemberInfo member in memberCollection)
            {
                if (member.MemberType == MemberTypes.Field)
                {
                    FieldInfo field = (FieldInfo)member;
                    object fieldValue = field.GetValue(srcobj);
                    field.SetValue(deepCopiedObj, DeepCopyRecursion(fieldValue));
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo property = (PropertyInfo)member;
                    MethodInfo info = property.GetSetMethod(false);
                    if (info != null)
                    {
                        object propertyValue = property.GetValue(srcobj, null);
                        property.SetValue(deepCopiedObj, DeepCopyRecursion(propertyValue), null);
                    }
                }
            }

            return deepCopiedObj;
        }

        /// <summary>
        /// Deep copy generic
        /// </summary>
        /// <param name="srcGeneric"></param>
        /// <returns></returns>
        private static object DeepCopyGenericType(object srcGeneric)
        {
            try
            { 
                IList srcList = srcGeneric as IList;
                if (srcList == null || srcList.Count <= 0)
                {
                    return null;
                }
                IList dstList = Activator.CreateInstance(srcList.GetType()) as IList;
                foreach (object o in srcList)
                {
                    dstList.Add(DeepCopyRecursion(o));
                }

                return dstList;
            }
            catch (Exception)
            {
                try
                {
                    IDictionary srcDictionary = srcGeneric as IDictionary;
                    if (srcDictionary == null || srcDictionary.Count <= 0)
                    {
                        return null;
                    }
                    IDictionary dstDictionary = Activator.CreateInstance(srcDictionary.GetType()) as IDictionary;
                    foreach (object o in srcDictionary.Keys)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        dstDictionary[o] = srcDictionary[o];
                    }
                    return dstDictionary;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Deep copy array
        /// </summary>
        /// <param name="srcArray"></param>
        /// <returns></returns>
        private static Array DeepCopyArray(Array srcArray)
        {
            if (srcArray.Length <= 0)
            {
                return null;
            }
            Array arrayCopied = Array.CreateInstance(srcArray.GetValue(0).GetType(), srcArray.Length);
            for (int i = 0; i < srcArray.Length; i++)
            {
                object o = DeepCopyRecursion(srcArray.GetValue(i));
                arrayCopied.SetValue(o, i);
            }
            return arrayCopied;
        }
    }
}

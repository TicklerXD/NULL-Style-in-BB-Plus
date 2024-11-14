using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DevTools.Extensions;

public static class ReflectionExtensions
{
    [Obsolete("Use BepInEx.Publicizer if you want to access private fields of an assembly.")]
    public static TValue GetValue<TClass, TValue>(string fieldName) where TClass : class =>
        (TValue)AccessTools.Field(typeof(TClass), fieldName).GetValue(InstanceGetter<TClass>.Instance);

    [Obsolete("Use BepInEx.Publicizer if you want to access private fields and methods of an assembly.")]
    public static TValue GetValue<TValue>(this object instance, string fieldName) => Traverse.Create(instance).Field(fieldName).GetValue<TValue>();

    [Obsolete("Use BepInEx.Publicizer if you want to access private fields of an assembly.")]
    public static void SetValue<TClass, TValue>(string fieldName, object setVal) => AccessTools.Field(typeof(TClass), fieldName).SetValue(AccessTools.CreateInstance<TClass>(), setVal);

    [Obsolete("Use BepInEx.Publicizer if you want to access private fields of an assembly.")]
    public static void SetValue<TValue>(this object instance, string fieldName, object setVal) => Traverse.Create(instance).Field(fieldName).SetValue(setVal);

    [Obsolete("Use BepInEx.Publicizer if you want to access private methods of an assembly.")]
    public static TValue Method<TValue>(this object instance, string methodName, params object[] parameters) => Traverse.Create(instance).Method(methodName, parameters).GetValue<TValue>();

    [Obsolete("Use BepInEx.Publicizer if you want to access private methods of an assembly.")]
    public static void Method(this object instance, string methodName, params object[] parameters) => Traverse.Create(instance).Method(methodName, parameters).GetValue();

    /// <summary>
    /// Returns a dictionary with patching classes and corresponding methods for patch method.
    /// </summary>
    /// <param name="methodInfo">Patch method.</param>
    /// <returns>Dictionary with types of patching classes and corresponding methods names.</returns>
    public static Dictionary<Type, string> ExtractPatchInfo(this MethodInfo methodInfo)
    {
        var res = new Dictionary<Type, string>();
        HarmonyPatch[] patchAttributes = methodInfo.GetCustomAttributes(typeof(HarmonyPatch), false) as HarmonyPatch[];

        if (patchAttributes == null || patchAttributes.Length == 0)
            return res;

        foreach (var patchAttribute in patchAttributes)
        {
            if (patchAttribute.info.declaringType != null && patchAttribute.info.methodName != null)
                res.Add(patchAttribute.info.declaringType, patchAttribute.info.methodName);
        }
        return res;
    }
}

public class InstanceGetter<T>
{
    public static T Instance { get => (T)AccessTools.CreateInstance(typeof(T)); }
}

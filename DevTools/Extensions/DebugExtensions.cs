using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DevTools.Extensions;

public static class DebugExtensions
{
    public static T i<T>(this T obj) // Null Check
    {
        if (obj is null)
        {
            StackTrace st = new(true);
            var frame = st.GetFrame(1);

            UnityEngine.Debug.LogError(
               "-------------- Null Reference Exception!!! -----------------\n" +
               frame.GetInfo() + "\n" +
               "-------------- Stack trace: -----------------\n" +
               st.GetStackTrace());           
        }
        return obj;
    }

    public static string GetInfo(this StackFrame f, bool includeColumn = true)
    {
        var m = f.GetMethod();
        var res = $"Method: {m.DeclaringType}.{m.Name}\n";
        try
        {
            #region Get the null element name
            var path = f.GetFileName();
            int lineNum = f.GetFileLineNumber();
            int columnNum = f.GetFileColumnNumber();

                 var file = File.ReadAllLines(path);
                 var targetLine = file[lineNum - 1];
                 int start = columnNum - 1;

                 int isNullIndex = targetLine.IndexOf($".", start);

               var elementName = targetLine.Substring(columnNum - 1, isNullIndex - start);

                var seqNum = targetLine.Substring(0, isNullIndex - elementName.Length).CountSubs(elementName) + 1;
                if (seqNum > 1)
                   elementName += $" (Sequence number: {seqNum})";
            #endregion

            res += $"File: {path}\n" +
                $"Line {lineNum}\n" +
                $"{(includeColumn ? $"Column: {columnNum}" : string.Empty)}\n"
               + $"Element: {elementName}\n";
        }
        catch(Exception e) 
        {
            UnityEngine.Debug.LogWarning(e.Message);
            UnityEngine.Debug.LogWarning(e.StackTrace);
        }
        return res;
    }
    public static string GetStackTrace(this StackTrace st, uint maxFrames = uint.MaxValue)
    {
        if (maxFrames == 0)
            throw new ArgumentOutOfRangeException("Max frames can't be equal to 0!");

        var frames = st.GetFrames().ToList();
        frames = frames.GetRange(2, frames.Count - 2);

        var res = string.Empty;
        maxFrames = (uint)Mathf.Clamp(maxFrames, 1, maxFrames);
        for (int i = 0; i < maxFrames; i++)
            res += frames[i].GetInfo() + "\n";

        return res;
    }

    public static string[] AllNullFields(this object obj)
    {
        System.Collections.Generic.List<string> res = [];
        foreach (FieldInfo data in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (data.GetValue(obj) is null)
                res.Add(data.Name);
        }
        return [..res];
    }
}

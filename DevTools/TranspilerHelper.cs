using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace DevTools.Extensions;

public static class TranspilerHelper
{
    /// <summary>
    /// Extension that acts the same as the original MatchForward method, but using text instructions
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="useEnd"></param>
    /// <param name="matches"></param>
    /// <returns>CodeMatcher with new position</returns>
    public static CodeMatcher MatchForward(this CodeMatcher matcher, bool useEnd, params TextCodeMatch[] matches)
    {
        List<CodeMatch> list = [];
        foreach (var match in matches)
            list.Add(new CodeMatch(x => x.opcode.Equals(match.opcode) && x.OperandIs(match.operand)));

        return matcher.MatchForward(useEnd, [.. list]);
    }
    /// <summary>
    /// Checks whether the operand of an instruction contains a name
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="operand"></param>
    /// <returns>«True» if the operand contains the specified name</returns>
    public static bool OperandIs(this CodeInstruction instruction, string operand) => instruction.operand != null ? instruction.operand.ToString().ToLower().Contains(operand.ToLower()) : operand is null;
    /// <summary>
    /// Checks whether the instruction contains an OpCode and an text operand
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="opCode"></param>
    /// <param name="operand"></param>
    /// <returns>«True» if the instruction contains OpCode and operand with the specified name</returns>
    public static bool Is(this CodeInstruction instruction, OpCode opCode, string operand) =>  instruction.opcode.Equals(opCode) && instruction.OperandIs(operand);
    /// <summary>
    /// Checks whether an operand contains a specified field
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="fieldName"></param>
    /// <returns>«True» if the operand contains a specified field</returns>
    public static bool LoadField(this CodeMatcher matcher, string fieldName) => matcher.Instruction.OperandIs(fieldName);
    /// <summary>
    /// Invoke delegate to all instructions of CodeMatcher
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="act"></param>
    /// <returns>CodeMatcher with modified instructions</returns>
    public static CodeMatcher Action(this CodeMatcher matcher, Action<CodeMatcher> act)
    {
        act(matcher);
        return matcher;
    }
    /// <summary>
    /// Invoke delegate to all instructions of CodeMatcher
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns>CodeMatcher with nop position</returns>
    public static CodeMatcher SetNop(this CodeMatcher matcher) => matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Nop));
    /// <summary>
    /// Gets the current CodeInstruction from the CodeMatcher
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="i"></param>
    /// <returns>Unmodifyed CodeMatcher</returns>
    public static CodeMatcher GetCodeInstruction(this CodeMatcher matcher, out CodeInstruction i)
    {
        i = matcher.Instruction;
        return matcher;
    }
    /// <summary>
    /// Duplicates the current instruction from the CodeMatcher
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="i"></param>
    /// <returns>Unmodifyed CodeMatcher</returns>
    public static CodeMatcher DublicateInstruction(this CodeMatcher matcher, out CodeInstruction i)
    {
        i = new CodeInstruction(matcher.Instruction);
        return matcher;
    }
    /// <summary>
    /// Logs all instructions of the CodeMatcher to the console, starting from the specified position
    /// </summary>
    /// <param name="m"></param>
    /// <param name="ogPos"></param>
    /// <returns>Unmodifyed CodeMatcher</returns>
    public static CodeMatcher DebugLogAllInstructions(this CodeMatcher m, int ogPos = -1)
    {
        if (ogPos < 0)
        {
            ogPos = m.Pos;
            m.Start();
        }

        if (m.IsInvalid)
        {
            m.Advance(ogPos - m.Pos);
            return m;
        }

        Debug.Log($"{m.Pos}: {m.Opcode} >> {m.Operand}");
        m.Advance(1);
        return DebugLogAllInstructions(m, m.Pos - ogPos);
    }
    public static CodeMatcher SetOperand(this CodeMatcher m, object operand)
    {
        m.Instruction.operand = operand;
        return m;
    }
    public static CodeMatcher Set(this CodeMatcher m, OpCode op)
    {
        m.Instruction.opcode = op;
        return m;
    }
}

public class TextCodeMatch(OpCode opCode, string operand = null) // The class, which is basically a modified CodeMatch, which uses an operand not of type object, but its string form
{
    public OpCode opcode = opCode;
    public string operand = operand;
}

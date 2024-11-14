using UnityEngine;
using static UnityEngine.Mathf;

namespace DevTools.Extensions;

public static class MathExtensions
{
    /// <summary>
    /// Determines if the number is positive. Optionally includes zero.
    /// </summary>
    /// <param name="num">The number to check.</param>
    /// <param name="includeZero">Whether to consider zero as positive. Default is true.</param>
    /// <returns>True if the number is positive (or zero if included), false otherwise.</returns>
    public static bool IsPositive(this float num, bool includeZero = true) => includeZero ? num >= 0 : num > 0;
    /// <summary>
    /// Returns the polarity of the number. 1 if the number is positive or 0, -1 if negative.
    /// </summary>
    /// <param name="num">The number to check.</param>
    /// <returns>1 if the number is positive or zero, -1 if negative.</returns>
    public static float GetPolarity(this float num) => num.IsPositive() ? 1f : -1f;
    /// <summary>
    /// Returns the polarity based on a boolean value. 1 if true, -1 if false.
    /// </summary>
    /// <param name="val">The boolean value to check.</param>
    /// <returns>1 if true, -1 if false.</returns>
    public static float GetPolarity(this bool val) => val.ToInt().GetPolarity();
    /// <summary>
    /// Returns the polarity of the object based on whether it's null. 1 if not null, -1 if null.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>1 if the object is not null, -1 if null.</returns>
    public static float GetPolarity(this object obj) => obj != null ? 1f : -1f;
    /// <summary>
    /// Clamps the absolute value of a float to [0,1] and retains its polarity.
    /// </summary>
    /// <param name="num">The number to clamp.</param>
    /// <returns>The clamped value, retaining the original number's polarity.</returns>
    public static float Clamp01(this float num) => Mathf.Clamp01(Abs(num)) * num.GetPolarity();
    /// <summary>
    /// Compares two floating-point numbers for equality.
    /// </summary>
    /// <param name="a">The first float to compare.</param>
    /// <param name="b">The second float to compare.</param>
    /// <returns>True if the two numbers are equal within float.Epsilon, false otherwise.</returns>
    public static bool Compare(this float a, float b) => Abs(a - b) < float.Epsilon;
    /// <summary>
    /// Converts a boolean to an integer. 1 for true, 0 for false.
    /// </summary>
    /// <param name="val">The boolean value to convert.</param>
    /// <returns>1 if true, 0 if false.</returns>
    public static int ToInt(this bool val) => val ? 1 : 0;
}

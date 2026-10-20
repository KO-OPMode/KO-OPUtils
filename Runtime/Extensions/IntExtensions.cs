using System.Runtime.CompilerServices;
using UnityEngine;

public static class IntExtensions
{

    public static bool IsEven(this int value)
    {
        if (value == 0)
            return true;
        return Mathf.Abs(value) % 2 == 0;
    }

    public static bool IsOdd(this int value) => !IsEven(value);

    /// <summary>
    /// Returns true if the value could be used as an index into an array with length <paramref name="arrayLength"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidIndex(this int index, int arrayLength)
    {
        return index >= 0 && index < arrayLength;
    }

    /// <summary>
    /// Turns 0 into -1, and into 1 otherwise. To return symmetric values from an array of length 2 for instance
    /// </summary>
    public static int BinToFlip(this int value) => value == 0 ? -1 : 1;

    public static string ToMinutesAndSeconds(this int seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);

        if (minutes > 0)
        {
            int secondsLeft = seconds - minutes * 60;
            return $"{minutes}m {secondsLeft}s";
        }
        else
            return $"{seconds}s";
    }

    /// <summary>
    /// Returns the number of digits of an integer.
    /// </summary>
    /// <remarks>This will not count the negative sign of a negative number, and 0 counts as 1 digit.</remarks>
    public static int CountDigits(this int value)
    {
        // log10(0) is infinity
        if (value == 0)
            return 1;

        value = Mathf.Abs(value);
        return Mathf.FloorToInt(Mathf.Log10(value)) + 1;
    }

    /// <summary>
    /// Gets the ordinal string of a positive integer.
    /// </summary>
    /// <example>1 = 1st, 112 = 112th, 133 = 133rd</example>
    public static string GetOrdinal(this int value)
    {
        if (value <= 0)
            return value.ToString();

        if (value % 100 is 11 or 12 or 13)
            return value + "th";

        return value + (value % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }
}
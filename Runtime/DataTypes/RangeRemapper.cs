using System;
using UnityEngine;

namespace KOOP
{
    /// <summary>
    /// Small utility struct to serialize remapped ranges in the inspector.
    /// </summary>
    /// <owner>Noé</owner>
    [Serializable]
    public struct RangeRemapper
    {
        [Tooltip("The start of the range of expected input values.")]
        public float InputStart;
        [Tooltip("The end of the range of expected input values.")]
        public float InputEnd;
        [Space]
        [Tooltip("The start of the range to remap values to.")]
        public float RemappedStart;
        [Tooltip("The end of the range to remap values to.")]
        public float RemappedEnd;
        [Space]
        [Tooltip("Should the results be clamped to the remapped range?")]
        public bool ClampOutput;

        public RangeRemapper(float inputStart, float inputEnd, float remappedStart, float remappedEnd, bool clampOutput)
        {
            InputStart = inputStart;
            InputEnd = inputEnd;
            RemappedStart = remappedStart;
            RemappedEnd = remappedEnd;
            ClampOutput = clampOutput;
        }

        /// <summary>
        /// Remaps <paramref name="inputValue"/> to the range of <see cref="RemappedStart"/> and <see cref="RemappedEnd"/>.
        /// </summary>
        /// <param name="inputValue">The value between <see cref="InputStart"/> and <see cref="InputEnd"/> to remap.</param>
        public float Evaluate(float inputValue) => Evaluate(inputValue, null);

        /// <summary>
        /// <inheritdoc cref="Evaluate(float)"/> 
        /// </summary>
        /// <param name="inputValue"><inheritdoc cref="Evaluate(float)"/></param>
        /// <param name="normalizedRemapCurve">A curve that should have a range of 0-1 to skew the remapped output.</param>
        public float Evaluate(float inputValue, AnimationCurve normalizedRemapCurve)
        {
            // do an inverse lerp while avoiding the possible divide by zero error while keeping the value on the correct side of the remapped range
            float inputPercent;
            if (InputStart == InputEnd)
            {
                if (inputValue > InputStart)
                    inputPercent = 1;
                else if (inputValue < InputStart)
                    inputPercent = 0;
                else
                    inputPercent = 0.5f;
            }
            else
            {
                inputPercent = (inputValue - InputStart) / (InputEnd - InputStart);
            }

            if (normalizedRemapCurve != null)
                inputPercent = normalizedRemapCurve.Evaluate(inputPercent);

            return ClampOutput ? Mathf.Lerp(RemappedStart, RemappedEnd, inputPercent) : Mathf.LerpUnclamped(RemappedStart, RemappedEnd, inputPercent);
        }
    }
}
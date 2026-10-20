using System;
using System.Linq;
using System.Reflection;
using ReflectionMagic;
using UnityEditor;
using UnityEngine;

namespace KOOPEditor
{
    /// <summary>
    /// Utilities to complement <see cref="Handles"/>.
    /// </summary>
    /// <owner>Lazlo</owner>
    public static class HandlesUtility
    {
        private static readonly dynamic HandlesDynamic;
        private static readonly object FreeMovePositionHandleParam;
        private static readonly ConstructorInfo PositionHandleParamConstructor;

        /// <summary>
        /// A reimplementation of the internal <seealso cref="Handles.PositionHandleParam.Handle"/> enum.
        /// </summary>
        [Flags]
        public enum PositionHandleAxes
        {
            None = 0,
            X = 1 << 0,
            Y = 1 << 1,
            Z = 1 << 2,
            XY = 1 << 3,
            YZ = 1 << 4,
            XZ = 1 << 5,
            XYZ = 1 << 6,
            All = ~None
        }
        
        static HandlesUtility()
        {
            HandlesDynamic = typeof(Handles).AsDynamicType();
            var positionHandlesParamType = typeof(Handles).GetNestedType("PositionHandleParam", BindingFlags.NonPublic);
            FreeMovePositionHandleParam = positionHandlesParamType.GetField("DefaultFreeMoveHandle", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            PositionHandleParamConstructor = positionHandlesParamType.GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length > 0);
        }

        /// <summary>
        /// Constructs an internal <see cref="Handles.PositionHandleParam"/> with the specified axis flags.
        /// </summary>
        private static object GetPositionHandleParamOnAxes(PositionHandleAxes axes)
        {
            return PositionHandleParamConstructor.Invoke(new object[]
            {
                /* handles: */ (int)axes,
                /* axisOffset: */ Vector3.zero,
                /* axisSize: */ Vector3.one,
                /* planeOffset: */ Vector3.zero,
                /* planeSize: */ Vector3.one * 0.25f,
                /* axesOrientation: */ 0, // Handles.PositionHandleParam.Orientation.Signed
                /* planeOrientation: */ 1 // Handles.PositionHandleParam.Orientation.Camera
            });
        }

        /// <summary>
        /// Whether handles are in free move mode.
        /// Mapped to the internal matching property.
        /// </summary>
        public static bool FreeMoveMode
        {
            get => HandlesDynamic.s_FreeMoveMode;
            set => HandlesDynamic.s_FreeMoveMode = value;
        }

        /// <summary>
        /// Sets <seealso cref="FreeMoveMode"/> based on <seealso cref="Event.current"/>.
        /// </summary>
        public static void UpdateFreeMoveMode()
        {
            var e = Event.current;
            
            // Ctrl/Cmd+Shift = Free Move
            if (e.type == EventType.KeyDown || e.type == EventType.KeyUp)
            {
                FreeMoveMode = e.GetAction() && e.shift;
            }
        }

        /// <summary>
        /// Draws a position handle on the specified axes only.
        /// </summary>
        /// <seealso cref="Handles.PositionHandle(Vector3, Quaternion)"/>
        public static Vector3 PositionHandleOnAxes(PositionHandleAxes axes, Vector3 position, Quaternion rotation)
        {
            UpdateFreeMoveMode();
            
            var param = FreeMoveMode ? FreeMovePositionHandleParam : GetPositionHandleParamOnAxes(axes);
            
            return (Vector3)HandlesDynamic.DoPositionHandle_Internal(Handles.PositionHandleIds.@default, position, rotation, param);
        }
    }
}
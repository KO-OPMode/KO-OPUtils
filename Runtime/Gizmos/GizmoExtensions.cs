using UnityEngine;
using System;
using KOOP.Extensions;
using UnityCollections;

namespace KOOPUtil
{
    /// <summary>
    /// Helper methods for rendering gizmos in editor
    /// </summary>
    public static class GizmoExtensions
    {
        /// <summary>
        /// Sets <see cref="Gizmos.matrix"/> for the duration of the scope.
        /// By default, this multiplies with any existence matrix to allow you to stack transformations.
        /// </summary>
        public struct MatrixScope : IDisposable
        {
            private readonly Matrix4x4 _storedMatrix;

            public MatrixScope(Transform transform, bool stack = true) : this(transform.localToWorldMatrix) { }

            public MatrixScope(Matrix4x4 matrix, bool stack = true)
            {
                _storedMatrix = Gizmos.matrix;
                Gizmos.matrix = stack ? _storedMatrix * matrix : matrix;
            }

            void IDisposable.Dispose()
            {
                Gizmos.matrix = _storedMatrix;
            }
        }

        /// <summary>
        /// Sets Gizmos.color for the duration of the scope
        /// </summary>
        public struct ColorScope : IDisposable
        {
            private readonly Color _storedColor;

            public ColorScope(Color color)
            {
                _storedColor = Gizmos.color;
                Gizmos.color = color;
            }

            void IDisposable.Dispose()
            {
                Gizmos.color = _storedColor;
            }
        }

        public static void DrawWireSquare(Vector3 center, Vector2 size)
        {
            Vector3 f = new(0f, 0f, size.y * 0.5f);
            Vector3 r = new(size.x * 0.5f, 0f, 0f);

            Gizmos.DrawLine(center + f + r, center + f - r);
            Gizmos.DrawLine(center - f + r, center - f - r);
            Gizmos.DrawLine(center + f + r, center - f + r);
            Gizmos.DrawLine(center + f - r, center - f - r);
        }

        const int kDefaultNumberOfPoints = 20;

        [HideInCallstack] public static void DrawWireCircle(Vector3 center, float radius) => DrawWireCircle(center, radius, Vector3.up);
        [HideInCallstack] public static void DrawWireCircle(Vector3 center, float radius, Vector3 normal) => DrawWireCircle(center, radius, normal, kDefaultNumberOfPoints);

        public static void DrawWireCircle(Vector3 center, float radius, Vector3 normal, int numberOfPoints)
        {
            Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
            Vector3 t2 = Vector3.Cross(normal, Vector3.up);
            Vector3 tangent = t1.magnitude > t2.magnitude ? t1 : t2;

            Vector3[] points = new Vector3[numberOfPoints];

            for (int i = 0; i < numberOfPoints; i++)
            {
                points[i] = (center + Quaternion.AngleAxis(18f * i, normal) * tangent * radius);
            }

            for (int i = 0; i < numberOfPoints; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % numberOfPoints]);
            }
        }
    
        /// <summary>
        /// Draws all of the MeshFilter meshes within the given hierarchy
        /// </summary>
        /// <returns>Whether any meshes were found to draw.</returns>
        public static bool DrawMeshes(GameObject root, Vector3 position, Quaternion rotation)
        {
            return DrawMeshesInternal(root, position, Vector3.one, rotation, useWireMesh: false);
        }
    
        /// <summary>
        /// Draws all of the MeshFilter meshes within the given hierarchy
        /// </summary>
        /// <returns>Whether any meshes were found to draw.</returns>
        public static bool DrawMeshes(GameObject root, Vector3 position, Quaternion rotation, float scale)
        {
            return DrawMeshesInternal(root, position, Vector3.one * scale, rotation, useWireMesh: false);
        }
    
        /// <summary>
        /// Draws all of the MeshFilter meshes within the given hierarchy
        /// </summary>
        /// <returns>Whether any meshes were found to draw.</returns>
        public static bool DrawMeshes(GameObject root, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return DrawMeshesInternal(root, position, scale, rotation, useWireMesh: false);
        }
    
        /// <summary>
        /// Draws all of the MeshFilter meshes within the given hierarchy as wireframes
        /// </summary>
        /// <returns>Whether any meshes were found to draw.</returns>
        public static bool DrawMeshes(GameObject root, Matrix4x4 spaceMatrix)
        {
            return DrawMeshesInternal(root, spaceMatrix, useWireMesh: false);
        }
    
        /// <summary>
        /// Draws all of the MeshFilter meshes within the given hierarchy
        /// </summary>
        /// <returns>Whether any meshes were found to draw.</returns>
        public static bool DrawWireMeshes(GameObject root, Vector3 position, Quaternion rotation)
        {
            return DrawMeshesInternal(root, position, Vector3.one, rotation, useWireMesh: true);
        }
    
        /// <summary>
        /// Draws all of the MeshFilter meshes within the given hierarchy
        /// </summary>
        /// <returns>Whether any meshes were found to draw.</returns>
        public static bool DrawWireMeshes(GameObject root, Vector3 position, Quaternion rotation, float scale)
        {
            return DrawMeshesInternal(root, position, Vector3.one * scale, rotation, useWireMesh: true);
        }

        /// <summary>
        /// Draws all of the MeshFilter meshes within the given hierarchy as wireframes
        /// </summary>
        /// <returns>Whether any meshes were found to draw.</returns>
        public static bool DrawWireMeshes(GameObject root, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return DrawMeshesInternal(root, position, scale, rotation, useWireMesh: true);
        }

        /// <summary>
        /// Draws all of the MeshFilter meshes within the given hierarchy as wireframes
        /// </summary>
        /// <returns>Whether any meshes were found to draw.</returns>
        public static bool DrawWireMeshes(GameObject root, Matrix4x4 matrix)
        {
            return DrawMeshesInternal(root, matrix, useWireMesh: true);
        }
    
        private static bool DrawMeshesInternal(
            GameObject root,
            Vector3 position,
            Vector3 scale,
            Quaternion rotation,
            bool useWireMesh)
        {
            if (scale == default)
                scale = Vector3.one;

            if (rotation.Equals(default))
                rotation = Quaternion.identity;
        
            if (scale == default)
                scale = Vector3.one;
        
            var matrix = Matrix4x4.TRS(position, rotation.SafeNormalize(), scale);
            return DrawMeshesInternal(root, matrix, useWireMesh);
        }

        private static bool DrawMeshesInternal(
            GameObject root, 
            Matrix4x4 matrix,
            bool useWireMesh)
        {
            if (root == null)
                return false;
        
            // Remove any position or rotation offset baked into the base prefab
            var rootMatrix = Matrix4x4.TRS(root.transform.localPosition, root.transform.localRotation, Vector3.one);
            matrix *= rootMatrix.inverse;
        
            using var meshFilters = new PooledList<MeshFilter>();
            root.GetComponentsInChildren(false, meshFilters.BackingList);
        
            foreach (var meshFilter in meshFilters)
            {
                RenderMesh(meshFilter.transform, meshFilter.sharedMesh);
            }

            using var skinnedMeshRenderers = new PooledList<SkinnedMeshRenderer>();
            root.GetComponentsInChildren(false, skinnedMeshRenderers.BackingList);
        
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                RenderMesh(skinnedMeshRenderer.transform, skinnedMeshRenderer.sharedMesh);
            }

            void RenderMesh(Transform transform, Mesh mesh)
            {
                if (mesh == null) return;
            
                var objectMatrix = transform.localToWorldMatrix;
                using (new MatrixScope(matrix * objectMatrix))
                {
                    for (var submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
                    {
                        if (useWireMesh)
                        {
                            Gizmos.DrawWireMesh(mesh, submeshIndex);
                        }
                        else
                        {
                            Gizmos.DrawMesh(mesh, submeshIndex);
                        }
                    }
                }
            }

            return meshFilters.Count > 0;
        }

#region Open Source

        /*
        The code in this region is sourced from https://github.com/code-beans/GizmoExtensions
        and is governed by the following license:

        MIT License

        Copyright (c) 2018 Marvin

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.
    */

        /// <summary>
        /// Draws a cube with a given rotation
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="rotation"></param>
        public static void DrawCube(Vector3 center, Vector3 size, Quaternion rotation = default(Quaternion))
        {
            if (rotation.Equals(default(Quaternion)))
                rotation = Quaternion.identity;

            using (new MatrixScope(Matrix4x4.TRS(center, rotation, size)))
            {
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
        }

        /// <summary>
        /// Draws a wire cube with a given rotation
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="rotation"></param>
        public static void DrawWireCube(Vector3 center, Vector3 size, Quaternion rotation = default(Quaternion))
        {
            if (rotation.Equals(default(Quaternion)))
                rotation = Quaternion.identity;

            using (new MatrixScope(Matrix4x4.TRS(center, rotation, size)))
            {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }

        public static void DrawArrow(Vector3 from, Vector3 to, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawLine(from, to);
            var direction = to - from;
            var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawLine(to, to + right * arrowHeadLength);
            Gizmos.DrawLine(to, to + left * arrowHeadLength);
        }

        public static void DrawWireSphere(Vector3 center, float radius, Quaternion rotation = default(Quaternion))
        {
            if (rotation.Equals(default(Quaternion)))
                rotation = Quaternion.identity;

            using (new MatrixScope(Matrix4x4.TRS(center, rotation, Vector3.one)))
            {
                Gizmos.DrawWireSphere(Vector3.zero, radius);
            }
        }


        /// <summary>
        /// Draws an arc with a rotation around the center
        /// </summary>
        /// <param name="center">center point</param>
        /// <param name="radius">radiu</param>
        /// <param name="angle">angle in degrees</param>
        /// <param name="segments">number of segments</param>
        /// <param name="rotation">rotation around the center</param>
        public static void DrawWireArc(Vector3 center, float radius, float angle, int segments = 20, Quaternion rotation = default(Quaternion))
        {
            if (rotation.Equals(default(Quaternion)))
                rotation = Quaternion.identity;

            using (new MatrixScope(Matrix4x4.TRS(center, rotation, Vector3.one)))
            {
                Vector3 from = Vector3.forward * radius;
                var step = Mathf.RoundToInt(angle / segments);
                for (int i = 0; i <= angle; i += step)
                {
                    var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad));
                    Gizmos.DrawLine(from, to);
                    from = to;
                }
            }
        }


        /// <summary>
        /// Draws an arc with a rotation around an arbitraty center of rotation
        /// </summary>
        /// <param name="center">the circle's center point</param>
        /// <param name="radius">radius</param>
        /// <param name="angle">angle in degrees</param>
        /// <param name="segments">number of segments</param>
        /// <param name="rotation">rotation around the centerOfRotation</param>
        /// <param name="centerOfRotation">center of rotation</param>
        public static void DrawWireArc(Vector3 center, float radius, float angle, int segments, Quaternion rotation, Vector3 centerOfRotation)
        {
            if (rotation.Equals(default(Quaternion)))
                rotation = Quaternion.identity;

            using (new MatrixScope(Matrix4x4.TRS(centerOfRotation, rotation, Vector3.one)))
            {
                var deltaTranslation = centerOfRotation - center;
                Vector3 from = deltaTranslation + Vector3.forward * radius;
                var step = Mathf.RoundToInt(angle / segments);
                for (int i = 0; i <= angle; i += step)
                {
                    var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad)) + deltaTranslation;
                    Gizmos.DrawLine(from, to);
                    from = to;
                }
            }
        }

        /// <summary>
        /// Draws an arc with a rotation around an arbitraty center of rotation
        /// </summary>
        /// <param name="matrix">Gizmo matrix applied before drawing</param>
        /// <param name="radius">radius</param>
        /// <param name="angle">angle in degrees</param>
        /// <param name="segments">number of segments</param>
        public static void DrawWireArc(Matrix4x4 matrix, float radius, float angle, int segments)
        {
            using (new MatrixScope(matrix))
            {
                Vector3 from = Vector3.forward * radius;
                var step = Mathf.RoundToInt(angle / segments);
                for (int i = 0; i <= angle; i += step)
                {
                    var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0,
                        radius * Mathf.Cos(i * Mathf.Deg2Rad));
                    Gizmos.DrawLine(from, to);
                    from = to;
                }
            }
        }

        /// <summary>
        /// Draws a wire cylinder face up with a rotation around the center
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <param name="rotation"></param>
        public static void DrawWireCylinder(Vector3 center, float radius, float height, Quaternion rotation = default(Quaternion))
        {
            if (rotation.Equals(default(Quaternion)))
                rotation = Quaternion.identity;

            using (new MatrixScope(Matrix4x4.TRS(center, rotation, Vector3.one)))
            {
                var half = height / 2;

                //draw the 4 outer lines
                Gizmos.DrawLine(Vector3.right * radius - Vector3.up * half, Vector3.right * radius + Vector3.up * half);
                Gizmos.DrawLine(-Vector3.right * radius - Vector3.up * half, -Vector3.right * radius + Vector3.up * half);
                Gizmos.DrawLine(Vector3.forward * radius - Vector3.up * half, Vector3.forward * radius + Vector3.up * half);
                Gizmos.DrawLine(-Vector3.forward * radius - Vector3.up * half, -Vector3.forward * radius + Vector3.up * half);

                //draw the 2 cricles with the center of rotation being the center of the cylinder, not the center of the circle itself
                DrawWireArc(center + Vector3.up * half, radius, 360, 20, rotation, center);
                DrawWireArc(center + Vector3.down * half, radius, 360, 20, rotation, center);
            }
        }

        /// <summary>
        /// Draws a wire cylinder face up with a rotation around the center
        /// </summary>
        public static void DrawWireCylinderFromBase(Vector3 bottom, float radius, float height, Quaternion rotation = default(Quaternion))
        {
            if (rotation.Equals(default(Quaternion)))
                rotation = Quaternion.identity;

            var halfHeight = height * .5f;
            var localUp = rotation * Vector3.up;
            var center = bottom + (localUp * halfHeight);
            DrawWireCylinder(center, radius, height, rotation);
        }

        /// <summary>
        /// Draws a wire capsule face up
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <param name="rotation"></param>
        public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation = default(Quaternion))
        {
            if (rotation.Equals(default(Quaternion)))
                rotation = Quaternion.identity;

            using (new MatrixScope(Matrix4x4.TRS(center, rotation, Vector3.one)))
            {
                var half = height / 2 - radius;

                //draw cylinder base
                DrawWireCylinder(center, radius, height - radius * 2, rotation);

                //draw upper cap
                //do some cool stuff with orthogonal matrices
                var mat = Matrix4x4.Translate(center + rotation * Vector3.up * half) *
                          Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(90, Vector3.forward));
                DrawWireArc(mat, radius, 180, 20);
                mat = Matrix4x4.Translate(center + rotation * Vector3.up * half) * Matrix4x4.Rotate(rotation *
                    Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(90, Vector3.forward));
                DrawWireArc(mat, radius, 180, 20);

                //draw lower cap
                mat = Matrix4x4.Translate(center + rotation * Vector3.down * half) * Matrix4x4.Rotate(rotation *
                    Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward));
                DrawWireArc(mat, radius, 180, 20);
                mat = Matrix4x4.Translate(center + rotation * Vector3.down * half) *
                      Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(-90, Vector3.forward));
                DrawWireArc(mat, radius, 180, 20);
            }
        }

#endregion
    }
}
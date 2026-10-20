using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace KOOP
{
    [AddComponentMenu("KOOP/UI/Simple Linear Gradient")]
    public class SimpleLinearGradient : BaseMeshEffect
    {
        [SerializeField] private Color _colorA = Color.white;
        [SerializeField] private Color _colorB = Color.white;

        [Range(-180f, 180f)]
        [SerializeField] private float _angle = 0f;

		[Range(-1f, 1f)]
		[SerializeField] private float _bias = 0f;

        public Color ColorA { get => _colorA; set { _colorA = value; RefreshGraphic(); } }
        public Color ColorB { get => _colorB; set { _colorB = value; RefreshGraphic(); } }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled)
            {
                return;
            }

            Rect rect = graphic.rectTransform.rect;
            float rads = _angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rads), Mathf.Sin(rads));

            Matrix2x3 localPositionMatrix = LocalPositionMatrix(rect, direction);

            UIVertex vertex = default;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);

                Vector2 matrixedPosition = localPositionMatrix * vertex.position;
                vertex.color *= Color.Lerp(_colorB, _colorA, matrixedPosition.y + _bias);

                vh.SetUIVertex(vertex, i);
            }
        }

        protected virtual void RefreshGraphic()
        {
            graphic.SetVerticesDirty();
        }
        
        private struct Matrix2x3
        {
            public float m00, m01, m02, m10, m11, m12;
            public Matrix2x3(float m00, float m01, float m02, float m10, float m11, float m12)
            {
                this.m00 = m00;
                this.m01 = m01;
                this.m02 = m02;
                this.m10 = m10;
                this.m11 = m11;
                this.m12 = m12;
            }

            public static Vector2 operator *(Matrix2x3 m, Vector2 v)
            {
                float x = (m.m00 * v.x) - (m.m01 * v.y) + m.m02;
                float y = (m.m10 * v.x) + (m.m11 * v.y) + m.m12;
                return new Vector2(x, y);
            }
        }

        private static Matrix2x3 LocalPositionMatrix(Rect rect, Vector2 dir)
        {
            float cos = dir.x;
            float sin = dir.y;
            Vector2 rectMin = rect.min;
            Vector2 rectSize = rect.size;
            float c = 0.5f;
            float ax = rectMin.x / rectSize.x + c;
            float ay = rectMin.y / rectSize.y + c;
            float m00 = cos / rectSize.x;
            float m01 = sin / rectSize.y;
            float m02 = -(ax * cos - ay * sin - c);
            float m10 = sin / rectSize.x;
            float m11 = cos / rectSize.y;
            float m12 = -(ax * sin + ay * cos - c);
            return new Matrix2x3(m00, m01, m02, m10, m11, m12);
        }
    }
}
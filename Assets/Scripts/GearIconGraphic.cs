using UnityEngine;
using UnityEngine.UI;

/// <summary>Small gear-shaped UI mark, built from a mesh so it does not depend on a font glyph.</summary>
public sealed class GearIconGraphic : MaskableGraphic
{
    private const int Segments = 32;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        float innerRadius = radius * 0.43f;
        float[] toothProfile = { 0.82f, 0.82f, 1f, 1f };

        for (int i = 0; i < Segments; i++)
        {
            float angle = (i / (float)Segments) * Mathf.PI * 2f;
            float outer = radius * toothProfile[i % toothProfile.Length];
            float nextAngle = ((i + 1) / (float)Segments) * Mathf.PI * 2f;
            float nextOuter = radius * toothProfile[(i + 1) % toothProfile.Length];

            AddVertex(vh, rect.center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * outer);
            AddVertex(vh, rect.center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * innerRadius);
            AddVertex(vh, rect.center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * nextOuter);
            AddVertex(vh, rect.center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * innerRadius);

            int start = i * 4;
            vh.AddTriangle(start, start + 2, start + 1);
            vh.AddTriangle(start + 2, start + 3, start + 1);
        }
    }

    private void AddVertex(VertexHelper vh, Vector2 position)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = position;
        vh.AddVert(vertex);
    }
}

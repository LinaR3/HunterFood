using UnityEngine;
using UnityEngine.UI;

/// <summary>Flat clipped-corner outline used by the Hunter Food low-poly UI.</summary>
public sealed class AngularFrameGraphic : MaskableGraphic
{
    [Min(1f)] public float thickness = 4f;
    [Range(0.02f, 0.3f)] public float cornerCut = 0.12f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float cut = Mathf.Min(rect.width, rect.height) * cornerCut;
        float inset = Mathf.Min(thickness, Mathf.Min(rect.width, rect.height) * 0.25f);
        Vector2[] outer = CreateOctagon(rect, 0f, cut);
        Vector2[] inner = CreateOctagon(rect, inset, Mathf.Max(0f, cut - inset * 0.35f));

        for (int i = 0; i < 8; i++)
        {
            int next = (i + 1) % 8;
            int first = vh.currentVertCount;
            AddVertex(vh, outer[i]);
            AddVertex(vh, outer[next]);
            AddVertex(vh, inner[next]);
            AddVertex(vh, inner[i]);
            vh.AddTriangle(first, first + 1, first + 2);
            vh.AddTriangle(first, first + 2, first + 3);
        }
    }

    private static Vector2[] CreateOctagon(Rect rect, float inset, float cut)
    {
        float left = rect.xMin + inset;
        float right = rect.xMax - inset;
        float bottom = rect.yMin + inset;
        float top = rect.yMax - inset;
        return new[]
        {
            new Vector2(left + cut, top), new Vector2(right - cut, top),
            new Vector2(right, top - cut), new Vector2(right, bottom + cut),
            new Vector2(right - cut, bottom), new Vector2(left + cut, bottom),
            new Vector2(left, bottom + cut), new Vector2(left, top - cut)
        };
    }

    private void AddVertex(VertexHelper vh, Vector2 position)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = position;
        vh.AddVert(vertex);
    }
}

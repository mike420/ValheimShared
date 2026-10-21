using UnityEngine;
using UnityEngine.UI;

namespace MagicMike.ModSettings;

// Small icons drawn as meshes, so the panel needs no sprites or font glyphs for them

// A circular arrow, for the reset buttons
internal sealed class ResetIcon : MaskableGraphic
{
	private const int Segments = 28;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		Rect rect = GetPixelAdjustedRect();
		Vector2 centre = rect.center;
		float radius = Mathf.Min(rect.width, rect.height) * 0.36f;
		float thickness = radius * 0.3f;

		// An arc most of the way round, anticlockwise, leaving a gap on the right for the arrowhead
		const float start = 60f;
		const float end = 330f;
		for (int i = 0; i < Segments; i++)
		{
			float a0 = Mathf.Lerp(start, end, i / (float)Segments) * Mathf.Deg2Rad;
			float a1 = Mathf.Lerp(start, end, (i + 1) / (float)Segments) * Mathf.Deg2Rad;
			Quad(vh,
				Polar(centre, radius + thickness * 0.5f, a0),
				Polar(centre, radius + thickness * 0.5f, a1),
				Polar(centre, radius - thickness * 0.5f, a1),
				Polar(centre, radius - thickness * 0.5f, a0));
		}

		// Arrowhead at the end of the arc, carrying on anticlockwise into the gap, so it reads as "go back"
		float tipAngle = end * Mathf.Deg2Rad;
		Vector2 onArc = Polar(centre, radius, tipAngle);
		Vector2 along = new Vector2(-Mathf.Sin(tipAngle), Mathf.Cos(tipAngle));
		Vector2 outward = new Vector2(Mathf.Cos(tipAngle), Mathf.Sin(tipAngle));
		float size = thickness * 2.2f;
		Triangle(vh, onArc + along * size, onArc + outward * size * 0.9f, onArc - outward * size * 0.9f);
	}

	private static Vector2 Polar(Vector2 centre, float radius, float angle)
	{
		return centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
	}

	private void Quad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		int index = vh.currentVertCount;
		Vertex(vh, a);
		Vertex(vh, b);
		Vertex(vh, c);
		Vertex(vh, d);
		vh.AddTriangle(index, index + 1, index + 2);
		vh.AddTriangle(index, index + 2, index + 3);
	}

	private void Triangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c)
	{
		int index = vh.currentVertCount;
		Vertex(vh, a);
		Vertex(vh, b);
		Vertex(vh, c);
		vh.AddTriangle(index, index + 1, index + 2);
	}

	private void Vertex(VertexHelper vh, Vector2 position)
	{
		UIVertex vertex = UIVertex.simpleVert;
		vertex.position = position;
		vertex.color = color;
		vh.AddVert(vertex);
	}
}

// A downward-pointing triangle, for the dropdown arrow
internal sealed class DownArrowIcon : MaskableGraphic
{
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		Rect rect = GetPixelAdjustedRect();
		UIVertex vertex = UIVertex.simpleVert;
		vertex.color = color;
		vertex.position = new Vector2(rect.xMin, rect.yMax);
		vh.AddVert(vertex);
		vertex.position = new Vector2(rect.xMax, rect.yMax);
		vh.AddVert(vertex);
		vertex.position = new Vector2(rect.center.x, rect.yMin);
		vh.AddVert(vertex);
		vh.AddTriangle(0, 1, 2);
	}
}

// A cross, for the close and clear buttons
internal sealed class CloseIcon : MaskableGraphic
{
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		Rect rect = GetPixelAdjustedRect();
		Vector2 centre = rect.center;
		float half = Mathf.Min(rect.width, rect.height) * 0.5f;
		float thickness = Mathf.Max(2f, half * 0.28f);
		Bar(vh, centre + new Vector2(-half, -half), centre + new Vector2(half, half), thickness);
		Bar(vh, centre + new Vector2(-half, half), centre + new Vector2(half, -half), thickness);
	}

	private void Bar(VertexHelper vh, Vector2 from, Vector2 to, float thickness)
	{
		Vector2 side = new Vector2(-(to - from).normalized.y, (to - from).normalized.x) * (thickness * 0.5f);
		int index = vh.currentVertCount;
		UIVertex vertex = UIVertex.simpleVert;
		vertex.color = color;
		vertex.position = from + side;
		vh.AddVert(vertex);
		vertex.position = to + side;
		vh.AddVert(vertex);
		vertex.position = to - side;
		vh.AddVert(vertex);
		vertex.position = from - side;
		vh.AddVert(vertex);
		vh.AddTriangle(index, index + 1, index + 2);
		vh.AddTriangle(index, index + 2, index + 3);
	}
}

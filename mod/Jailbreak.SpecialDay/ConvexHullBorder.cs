﻿using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;

namespace Jailbreak.SpecialDay;

public class ConvexHullBorder : IBorder {
  private readonly IList<Vector> hull;
  private readonly IList<Vector> rawPoints;

  public ConvexHullBorder(IList<Vector> points) {
    hull      = ComputeConvexHull(points);
    rawPoints = points;
  }

  public List<Vector> ComputeConvexHull(IList<Vector> points) {
    points = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

    List<Vector> lower = [];
    foreach (var p in points) {
      while (lower.Count >= 2
        && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
        lower.RemoveAt(lower.Count - 1);
      lower.Add(p);
    }

    var upper = new List<Vector>();
    for (var i = points.Count() - 1; i >= 0; i--) {
      var p = points[i];
      while (upper.Count >= 2
        && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
        upper.RemoveAt(upper.Count - 1);
      upper.Add(p);
    }

    lower.RemoveAt(lower.Count - 1);
    upper.RemoveAt(upper.Count - 1);

    return lower.Concat(upper).ToList();
  }

  private static double Cross(Vector o, Vector a, Vector b) {
    return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
  }

  public static float DistanceToSegmentSquared(Vector p, Vector v, Vector w) {
    double l2 = (v.X - w.X) * (v.X - w.X)
      + (v.Y - w.Y) * (v.Y - w.Y);              // length squared of the segment
    if (l2 == 0.0) return p.DistanceSquared(v); // v == w case
    double t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
    t = Math.Max(0, Math.Min(1, t));
    var tmp = new Vector((float)(v.X + t * (w.X - v.X)),
      (float)(v.Y + t * (w.Y - v.Y)));
    return p.DistanceSquared(tmp);
  }

  public bool IsInsiderBorder(Vector point) {
    double x      = point.X, y = point.Y;
    bool   inside = false;
    int    j      = 0;

    var i = 0;
    for (j = hull.Count - 1; i < hull.Count; j = i++) {
      double xi = hull[i].X, yi = hull[i].Y;
      double xj = hull[j].X, yj = hull[j].Y;

      bool intersect = ((yi > y) != (yj > y))
        && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
      if (intersect) inside = !inside;
    }

    return inside;
  }

  public float GetMinDistance(Vector position) {
    if (IsInsiderBorder(position)) return 0;
    var minDistance = float.MaxValue;
    for (var i = 0; i < hull.Count; i++) {
      var v        = hull[i];
      var w        = hull[(i + 1) % hull.Count];
      var distance = DistanceToSegmentSquared(position, v, w);
      if (distance < minDistance) { minDistance = distance; }
    }

    return minDistance;
  }

  public Vector GetCenter() {
    return rawPoints.OrderBy(p => rawPoints.Sum(p.DistanceSquared))
     .ElementAt(rawPoints.Count / 2);
  }

  public IEnumerable<Vector> GetPoints() { return hull; }
}
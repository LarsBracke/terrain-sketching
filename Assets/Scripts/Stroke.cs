using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stroke : MonoBehaviour
{
    private int _strokeDepth = int.MaxValue;
    private List<Vector2> _strokePoints = new List<Vector2>();

    public int PointCount = 0;
    public int PointDistance = 33;

    public int StrokeDepth
    {
        get { return _strokeDepth; }
        set { _strokeDepth = value; }
    }

    public void AddStrokePoint(Vector2 newPoint)
    {
        if (!CanAddPoint(newPoint))
            return;

        _strokePoints.Add(newPoint);
        PointCount = _strokePoints.Count;

        Debug.Log($"Point {newPoint} added to stroke, {PointCount} points present");
    }

    public List<Vector2> GetStrokePoints()
    {
        return _strokePoints;
    }

    public Vector2 GetStrokeXBounds()
    {
        float lowerBound = float.MaxValue;
        float upperBound = float.MinValue;

        foreach (Vector2 point in _strokePoints)
        {
            if (point.x < lowerBound)
                lowerBound = point.x;
            if (point.x > upperBound)
                upperBound = point.x;
        }

        return new Vector2(lowerBound, upperBound);
    }

    public void Undo()
    {
        if (_strokePoints.Count > 0)
            _strokePoints.RemoveAt(_strokePoints.Count - 1);
        PointCount = _strokePoints.Count;
    }

    private bool CanAddPoint(Vector2 newPoint)
    {
        if (_strokePoints.Count == 0)
            return true;
        else
            return
                Vector3.Distance(_strokePoints[_strokePoints.Count - 1], newPoint) < PointDistance;
    }

    private void Awake()
    {
    }

    private void Update()
    { }
}

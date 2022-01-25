using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Stroke
{
    private int _strokeDepth;
    private List<Vector2> _strokePoints;

    public int StrokeDepth
    {
        get { return _strokeDepth; }
        set { _strokeDepth = value; }
    }

    public void AddStrokePoint(Vector2 newPoint)
    {
        _strokePoints.Add(newPoint);
    }

    public void Undo()
    {
        if (_strokePoints.Count > 0)
            _strokePoints.RemoveAt(_strokePoints.Count - 1);
    }

    void Start()
    {
        _strokeDepth = int.MaxValue;
        _strokePoints = new List<Vector2>();
    }

    void Update()
    { }
}

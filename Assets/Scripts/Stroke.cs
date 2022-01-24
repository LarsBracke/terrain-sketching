using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stroke : MonoBehaviour
{
    private int _strokeDepth = int.MaxValue;
    private List<Vector2> _strokePoints = new List<Vector2>();

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
    { }

    void Update()
    { }
}

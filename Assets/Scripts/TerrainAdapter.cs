using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainAdapter : MonoBehaviour
{
    [SerializeField] private Terrain _workingTerrain = null;

    private Sketch _sketch;
    private List<Vector2> _targets;
    private const int _profileLength = 6;

    [Header("Debugging")]
    [SerializeField] private GameObject _debugShape = null;

    public TerrainAdapter(Terrain workingTerrain)
    {
        _workingTerrain = workingTerrain;
    }

    private void Start()
    {
        _sketch = new Sketch();
        _targets = new List<Vector2>();
    }

    private void Update()
    { }

    public void StartSketch()
    {

    }

    public void EndSketch()
    {

    }

    public void RunPPA() // Detecting the terrain features
    {
        TargetRecognition();
        TargetConnection();
    }

    private void TargetRecognition() // Detecting points that could be on a ridge
    {
        int mapWidth = _workingTerrain.terrainData.heightmapWidth;
        int mapHeight = _workingTerrain.terrainData.heightmapHeight;

        for (int indexY = 0; indexY < mapHeight; ++indexY)
        {
            for (int indexX = 0; indexX < mapWidth; ++indexX)
            {
                if (!IsCoordinateValid(_workingTerrain, indexX, indexY, _profileLength))
                    continue;

                if (IsTarget(_workingTerrain, indexX, indexY, _profileLength))
                {
                    Vector2 newTarget = new Vector2(indexX, indexY);
                    _targets.Add(newTarget);
                }
            }
        }

        Debug.Log
            ($"{_targets.Count} targets found during target-recognotion with profile-length {_profileLength}");
        DebugDrawTargets(_targets);
    }

    private void TargetConnection() // Connecting neighboring points
    {

    }

    private void PolygonBreaking()
    {

    }

    private void BranchReduction()
    {

    }

    private bool IsCoordinateValid(Terrain terrain, int x, int y, int profileLength)
    {
        int mapWidth = terrain.terrainData.heightmapWidth;
        int mapHeight = terrain.terrainData.heightmapHeight;

        bool valid =
            (x - profileLength > 0) &&
            (x + profileLength < mapWidth) &&
            (y - profileLength > 0) &&
            (y + profileLength < mapHeight);

        return valid;
    }

    private bool IsTarget(Terrain terrain, int x, int y, int profileLength)
    {
        float centerHeight = terrain.terrainData.GetHeight(x, y);

        // Vertical target check
        float[] neightborHeights = new float[2 * profileLength];
        for (int count = 1; count < profileLength; ++count)
        {
            neightborHeights[count - 1] = terrain.terrainData.GetHeight(x, y + count);
            neightborHeights[count] = terrain.terrainData.GetHeight(x, y - count);
        }
        if (HeightCheck(centerHeight, neightborHeights))
            return true;

        // Horizontal height check
        for (int count = 1; count < profileLength; ++count)
        {
            neightborHeights[count - 1] = terrain.terrainData.GetHeight(x + count, y);
            neightborHeights[count] = terrain.terrainData.GetHeight(x - count, y);
        }
        if (HeightCheck(centerHeight, neightborHeights))
            return true;

        // Northwest --> southeast height check
        for (int count = 1; count < profileLength; ++count)
        {
            neightborHeights[count - 1] = terrain.terrainData.GetHeight(x - count, y + count);
            neightborHeights[count] = terrain.terrainData.GetHeight(x + count, y - count);
        }
        if (HeightCheck(centerHeight, neightborHeights))
            return true;

        // Northeast --> southwest height check
        for (int count = 1; count < profileLength; ++count)
        {
            neightborHeights[count - 1] = terrain.terrainData.GetHeight(x + count, y + count);
            neightborHeights[count] = terrain.terrainData.GetHeight(x - count, y - count);
        }
        if (HeightCheck(centerHeight, neightborHeights))
            return true;

        return false;
    }

    private bool HeightCheck(float centerValue, float[] heights)
    {
        foreach (float height in heights)
        {
            if (height > centerValue)
                return false;
        }

        return true;
    }

    private void DebugDrawTargets(List<Vector2> targets)
    {
        foreach (Vector2 target in targets)
        {
            float heightValue = _workingTerrain.terrainData.GetHeight((int)target.x, (int)target.y);
            Vector3 terrainPos = _workingTerrain.GetPosition();
            Vector3 shapePos = new Vector3(terrainPos.x + target.x, heightValue, terrainPos.y + target.y);

            GameObject debugShapes = new GameObject("DebugShapes");
            GameObject shape = Instantiate(_debugShape, debugShapes.transform);
            shape.transform.position = shapePos;
        }
    }
}
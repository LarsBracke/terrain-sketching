using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainAdapter : MonoBehaviour
{
    private Terrain _workingTerrain = null;

    public TerrainAdapter(Terrain workingTerrain)
    {
        _workingTerrain = workingTerrain;
    }

    private void Start()
    { }

    private void Update()
    { }

    public void RunPPA() // Detecting the terrain features
    {
        TargetRecognition();
    }

    private void TargetRecognition() // Detecting points that could be on a ridge
    {
        int mapWidth = _workingTerrain.terrainData.heightmapWidth;
        int mapHeight = _workingTerrain.terrainData.heightmapHeight;

        int profileLength = 1;
        List<Vector2> targets = new List<Vector2>();

        for (int indexY = 0; indexY < mapHeight; ++indexY)
        {
            for (int indexX = 0; indexX < mapWidth; ++indexX)
            {
                if (!IsCoordinateValid(_workingTerrain, indexX, indexY, profileLength))
                    continue;

                if (IsTarget(_workingTerrain, indexX, indexY, profileLength))
                {
                    Vector2 newTarget = new Vector2(indexX, indexY);
                    targets.Add(newTarget);
                }
            }
        }

        Debug.Log($"{targets.Count} targets found during target-recognotion");
        DebugDrawTargets(targets);
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
 
        }

        Vector3 terrainPos = _workingTerrain.GetPosition();
        GameObject cube = new GameObject();
        cube.transform.position = terrainPos;
    }
}
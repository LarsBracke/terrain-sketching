using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private Terrain _terrain = null;

    public float Scale = 20f;

    public int Width = 256;
    public int Height = 256;

    public int Depth = 36;

    public bool randomize = false;
    public float maxOffsetX = 128f;
    public float maxOffsetY = 128f;
    private float offsetX = 0f;
    private float offsetY = 0f;
    

    void Start()
    {
        if (randomize)
        {
            offsetX = Random.Range(0, maxOffsetX);
            offsetY = Random.Range(0, maxOffsetY);
        }

        _terrain = GetComponent<Terrain>();
        _terrain.terrainData = GenerateTerrain(_terrain.terrainData);
    }

    void Update()
    { }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = Width + 1;
        terrainData.size = new Vector3(Width, Depth, Height);
        terrainData.SetHeights(0, 0, GenerateDepths());
        return terrainData;
    }

    float[,] GenerateDepths()
    {
        float[,] depths = new float[Width, Height];

        for (int indexX = 0; indexX < Width; ++indexX)
        {
            for (int indexY = 0; indexY < Height; ++indexY)
            {
                depths[indexX, indexY] = CalculateDepth(indexX, indexY);
            }
        }

        return depths;
    }

    float CalculateDepth(int x, int y)
    {
        float coordX = (float)x / Width * Scale + offsetX;
        float coordY = (float)y / Height * Scale + offsetX;

        float perlinNoiseCoord = Mathf.PerlinNoise(coordX, coordY);
        return perlinNoiseCoord;
    }
}

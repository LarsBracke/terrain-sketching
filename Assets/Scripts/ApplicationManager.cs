using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] private Terrain _terrain = null;
    [SerializeField] private TerrainAdapter _terrainAdapter = null;

    private void Start()
    {
        if (!_terrain)
            Debug.LogWarning("No terrain found.");

        if (!_terrainAdapter)
            Debug.LogWarning("No terrain-adapter found.");

        _terrainAdapter.WorkingTerrain = _terrain;
        _terrainAdapter.RunPPA();
    }

    private void Update()
    {
        
    }
}

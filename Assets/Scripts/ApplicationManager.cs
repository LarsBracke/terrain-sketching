using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] private TerrainAdapter _terrainAdapter = null;

    private void Start()
    {
        if (!_terrainAdapter)
            Debug.LogWarning("No terrain-adapter found.");

        _terrainAdapter.RunPPA();
    }

    private void Update()
    {
        
    }
}

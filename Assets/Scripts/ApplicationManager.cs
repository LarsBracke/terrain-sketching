using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] private Terrain _terrain = null;

    private void Start()
    {
        if (!_terrain)
            Debug.LogWarning("No terrain found.");

        TerrainAdapter adapter = new TerrainAdapter(_terrain);
        adapter.RunPPA();
    }

    private void Update()
    {
        
    }
}

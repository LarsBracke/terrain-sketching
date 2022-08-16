using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.EventSystems;
using Plane = UnityEngine.Plane;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class TerrainAdapter : MonoBehaviour
{
    [Header("General")] 
    private const float lambda = 0.01f;
    private const float delta = 5.0f;

    [Header("Terrain")]
    [SerializeField] private Terrain _workingTerrain = null;
    private float[,] _terrainBackup;
    private int _terrainWidth;
    private int _terrainHeight;

    [Header("Sketching")]
    private Sketch _sketch;
    private Stroke _currentStroke;
    private Plane _sketchPlane;
    private bool _isSketching;
    [SerializeField] private GameObject _drawDot;
    private TrailRenderer _trailRenderer = null;

    [Header("FeatureDetection")]
    private List<Vector2> _candidateTargets;
    private List<Vector2> _polyBrokenTargets;
    private List<Vector2> _projectedTargets;
    private List<Vector2> _finalTargets;
    private List<Vector2> _finalTargetsProjected;
    private const int _profileLength = 10;

    [Header("Deformation")] 
    private float _lowerCameraBound = 0.0f;
    private float _upperCameraBound = 1000.0f;

    [Header("Debugging")]
    [SerializeField] private GameObject _debugShape = null;

    public TerrainAdapter(Terrain workingTerrain)
    {
        _workingTerrain = workingTerrain;
    }

    private void Awake()
    {
        _sketch = new Sketch();
        _currentStroke = new Stroke();
        _sketchPlane = new Plane((-1)*Camera.main.transform.forward, Camera.main.transform.position + 2*Camera.main.transform.forward);
        _candidateTargets = new List<Vector2>();
        _polyBrokenTargets = new List<Vector2>();
        _projectedTargets = new List<Vector2>();
        _finalTargets = new List<Vector2>();
        _finalTargetsProjected = new List<Vector2>();

        _terrainWidth = _workingTerrain.terrainData.heightmapResolution - 1;
        _terrainHeight = _workingTerrain.terrainData.heightmapResolution - 1;
        _terrainBackup = _workingTerrain.terrainData.GetHeights(0, 0, _terrainWidth, _terrainHeight);

        if (_drawDot)
            _trailRenderer = _drawDot.GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        ToggleSketching();
        Sketching();

        if (Input.GetKeyDown(KeyCode.P))
        {
            ProjectTargets();
            FindFinalTargets();
            DeformTerrain();

            DebugDrawTargets(_finalTargets);
        }
    }

    private void Sketching()
    {
        if (Input.GetKey(KeyCode.Mouse0) && _isSketching)
        {
            // Add point to the stroke
            Vector2 penPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            _currentStroke.AddStrokePoint(penPos); // Stroke will check if the point can be added

            // Set draw dot to last point in the stroke
            Vector3 drawPos =
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
            _drawDot.transform.position = drawPos;

            _trailRenderer.enabled = true;
        }
    }

    public void ToggleSketching()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            _isSketching = !_isSketching;
            Debug.Log($"Toggled sketching from {!_isSketching} to {_isSketching}");

            if (!_isSketching)
                _trailRenderer.enabled = false;
        }
    }

    private void FindFinalTargets()
    {
        Vector2 strokeXBounds = _currentStroke.GetStrokeXBounds();

        for (int index = 0; index < _projectedTargets.Count; ++index)
        {
            Vector2 target = _polyBrokenTargets[index];
            Vector2 projectedTarget = _projectedTargets[index];

            if (projectedTarget.x > strokeXBounds.x && projectedTarget.x < strokeXBounds.y)
            {
                float cameraDistance = Camera.main.WorldToScreenPoint(GetTargetWorldPos(target)).z;
                if (cameraDistance > 0)
                {
                    _finalTargets.Add(target);
                    _finalTargetsProjected.Add(projectedTarget);
                }
            }
        }

        Debug.Log($"found {_finalTargets.Count} targets with bounds {strokeXBounds.x} - {strokeXBounds.y}");
    }

    private void DeformTerrain()
    {
        float[,] displacementMap = new float[_terrainWidth, _terrainHeight];

        var strokePoints = _currentStroke.GetStrokePoints();

        foreach (Vector2 strokePoint in strokePoints)
        {
            Vector2 strokeScreenPos = strokePoint;

            foreach (Vector2 target in _finalTargets)
            {
                Vector3 targetWorldPos = GetTargetWorldPos(target);
                Vector2 targetScreenPos = Camera.main.WorldToScreenPoint(targetWorldPos);

                float strokeDepth = Vector3.Distance(Camera.main.transform.position, targetWorldPos);
                Vector3 strokeWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(strokePoint.x, strokePoint.y, strokeDepth));

                if (Mathf.Abs(strokeScreenPos.x - targetScreenPos.x) < delta)
                {
                    // Height calculations
                    float maxHeight = 36.0f;

                    float terrainHeight = _workingTerrain.terrainData.GetHeight((int)target.x, (int)target.y);
                    float displacement = (strokeWorldPos.y - terrainHeight);

                    displacementMap[(int)target.x, (int)target.y] = displacement;
                }
            }
        }

        // Perform diffusion on the displacement-map
        float[,] diffusedDisplacementMap =  PerformDiffusion(displacementMap);

        // Apply the displacement map to the terrain
        _workingTerrain.terrainData.SetHeights(0, 0, ApplyDisplacementMap(diffusedDisplacementMap));
    }

    private float[,] PerformDiffusion(float[,] displacementMap)
    {
        float[,] diffusedMap = new float[_terrainWidth, _terrainHeight];

        for (int heightIndex = 1; heightIndex < _terrainHeight - 1; ++heightIndex)
        {
            for (int widthIndex = 1; widthIndex < _terrainWidth - 1; ++widthIndex)
            {
                float current = displacementMap[widthIndex, heightIndex];

                float left = displacementMap[widthIndex - 1, heightIndex];
                float right = displacementMap[widthIndex + 1, heightIndex];
                float up = displacementMap[widthIndex, heightIndex + 1];
                float down = displacementMap[widthIndex, heightIndex - 1];

                float diffusedValue = (1 / 4.0f) * (left + right + up + down);

                diffusedMap[heightIndex, widthIndex] = diffusedValue;
            }
        }

        return diffusedMap;
    }

    private float[,] ApplyDisplacementMap(float[,] displacementMap)
    {
        float maxTerrainHeight = 36.0f;

        float[,] terrainHeights = _workingTerrain.terrainData.GetHeights(0, 0, _terrainWidth, _terrainHeight);
        float[,] newHeights = new float[_terrainWidth, _terrainHeight];

        for (int heightIndex = 1; heightIndex < _terrainHeight - 1; ++heightIndex)
        {
            for (int widthIndex = 1; widthIndex < _terrainWidth - 1; ++widthIndex)
            {
                newHeights[widthIndex, heightIndex] =
                    ((terrainHeights[widthIndex, heightIndex] * maxTerrainHeight) + displacementMap[widthIndex, heightIndex]) /
                    maxTerrainHeight;
            }
        }

        return newHeights;
    }


    private Vector3 GetStrokeIntersectionPoint(Vector2 projectedTarget)
    {
        //foreach (Vector2 strokePoint in _currentStroke.GetStrokePoints())
        //{
        //    float diff = Mathf.Abs(strokePoint.x, )
        //}

        return new Vector3();
    }

    private void ProjectTargets()
    {
        foreach (Vector2 target in _polyBrokenTargets)
        {
            Vector3 worldPos = GetTargetWorldPos(target);
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPos);

            _projectedTargets.Add(new Vector2(screenPoint.x, screenPoint.y));
        }

        Debug.Log($"Projected {_projectedTargets.Count} targets");
    }

    public void RunPPA() // Detecting the terrain features
    {
        TargetRecognition();
        //Debug.Log($"{_candidateTargets.Count} targets found during target-recognotion with profile-length {_profileLength}");
        //DebugDrawTargets(_candidateTargets);

        PolygonBreaking();
        //Debug.Log($"{_polyBrokenTargets.Count} targets remaining after poly-breaking");
        //DebugDrawTargets(_polyBrokenTargets);
    }

    private void TargetRecognition() // Detecting points that could be on a ridge
    {
        int mapWidth = _workingTerrain.terrainData.heightmapResolution;
        int mapHeight = _workingTerrain.terrainData.heightmapResolution;

        for (int indexY = 0; indexY < mapHeight; ++indexY)
        {
            for (int indexX = 0; indexX < mapWidth; ++indexX)
            {
                if (!IsCoordinateValid(_workingTerrain, indexX, indexY, _profileLength))
                    continue;

                if (IsTarget(_workingTerrain, indexX, indexY, _profileLength))
                {
                    Vector2 newTarget = new Vector2(indexX, indexY);
                    _candidateTargets.Add(newTarget);
                }
            }
        }
    }

    private void PolygonBreaking() // Breaking polygons (remove least important connection)
    {
        _polyBrokenTargets = new List<Vector2>(_candidateTargets);

        foreach (Vector2 target in _candidateTargets)
        {
            List<Vector2> neighborhood = GetConnectedNeighborhood(target);
            List<Vector2> targetsToRemove = PolyCheck(target, neighborhood);

            foreach (Vector2 targetToRemove in targetsToRemove)
            {
                _polyBrokenTargets.Remove(targetToRemove);
            }
        }
    }

    private void BranchReduction() // Eliminating less important branches
    {

    }

    private bool IsCoordinateValid(Terrain terrain, int x, int y, int profileLength)
    {
        int mapWidth = terrain.terrainData.heightmapResolution;
        int mapHeight = terrain.terrainData.heightmapResolution;

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
        GameObject debugShapes = new GameObject("DebugShapes");

        foreach (Vector2 target in targets)
        {
            float heightValue = _workingTerrain.terrainData.GetHeight((int)target.x, (int)target.y);
            Vector3 terrainPos = _workingTerrain.GetPosition();
            Vector3 shapePos = new Vector3(terrainPos.x + target.x, heightValue, terrainPos.y + target.y);

            GameObject shape = Instantiate(_debugShape, debugShapes.transform);
            shape.transform.position = shapePos;
        }
    }

    private void DebugDrawStroke(Stroke stroke)
    {
        var strokePoints = stroke.GetStrokePoints();
        GameObject debugShapes = new GameObject("DebugShapes");

        foreach (Vector2 strokePoint in strokePoints)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(strokePoint.x, strokePoint.y, 25.0f));
            Debug.Log($"Stroke world pos: {worldPos}");

            GameObject shape = Instantiate(_debugShape, debugShapes.transform);
            shape.transform.position = worldPos;
        }
    }

    private void DebugDrawProjectedTargets(List<Vector2> projectedTargets)
    {
        GameObject debugShapes = new GameObject("DebugShapes");
        debugShapes.transform.position = Camera.main.transform.position;

        foreach (Vector2 target in projectedTargets)
        {
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 worldPos = new Vector3(cameraPos.x + target.x, cameraPos.y + target.y, cameraPos.z);

            GameObject shape = Instantiate(_debugShape, debugShapes.transform);
            shape.transform.position = worldPos;
        }
    }

    private List<Vector2> GetConnectedNeighborhood(Vector2 target)
    {
        List<Vector2> neighborhood = new List<Vector2>();

        Vector2 neighbor = new Vector2(target.x, target.y + 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);
        else
            neighborhood.Add(new Vector2(float.MaxValue, float.MaxValue));

        neighbor = new Vector2(target.x + 1, target.y + 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);
        else
            neighborhood.Add(new Vector2(float.MaxValue, float.MaxValue));

        neighbor = new Vector2(target.x + 1, target.y);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);
        else
            neighborhood.Add(new Vector2(float.MaxValue, float.MaxValue));

        neighbor = new Vector2(target.x + 1, target.y - 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);
        else
            neighborhood.Add(new Vector2(float.MaxValue, float.MaxValue));

        neighbor = new Vector2(target.x, target.y - 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);
        else
            neighborhood.Add(new Vector2(float.MaxValue, float.MaxValue));

        neighbor = new Vector2(target.x - 1, target.y - 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);
        else
            neighborhood.Add(new Vector2(float.MaxValue, float.MaxValue));

        neighbor = new Vector2(target.x - 1, target.y);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);
        else
            neighborhood.Add(new Vector2(float.MaxValue, float.MaxValue));

        neighbor = new Vector2(target.x - 1, target.y + 1);
        if (IsValidNeighbor(neighbor))
            neighborhood.Add(neighbor);
        else
            neighborhood.Add(new Vector2(float.MaxValue, float.MaxValue));

        return neighborhood;
    }

    private bool IsValidNeighbor(Vector2 neighbor)
    {
        if (_candidateTargets.Contains(neighbor) &&
            IsCoordinateValid(_workingTerrain, (int) neighbor.x, (int) neighbor.y, 1))
        {
            return true;
        }

        return false;
    }


    private List<Vector2> PolyCheck(Vector2 target, List<Vector2> neighborhood)
    {
        List<Vector2> targetsToRemove = new List<Vector2>();

        for (int index = 0; index < neighborhood.Count - 1; index +=2)
        {
            bool isPoly =
                !(neighborhood[index].x < float.MaxValue &&
                neighborhood[index + 1].x < float.MaxValue);

            if (isPoly)
            {
                float vertex0Height = _workingTerrain.terrainData.GetHeight((int)target.x, (int)target.y);
                float vertex1Height = _workingTerrain.terrainData.GetHeight((int)neighborhood[index].x, (int)neighborhood[index].y);
                float vertex2Height = _workingTerrain.terrainData.GetHeight((int)neighborhood[index + 1].x, (int)neighborhood[index + 1].y);

                if (vertex0Height < vertex1Height && vertex0Height < vertex2Height)
                    targetsToRemove.Add(target);

                if (vertex1Height < vertex0Height && vertex1Height < vertex2Height)
                    targetsToRemove.Add(neighborhood[index]);

                if (vertex2Height < vertex0Height && vertex2Height < vertex1Height)
                    targetsToRemove.Add(neighborhood[index + 1]);

            }
        }

        return targetsToRemove;
    }

    private Vector3 GetTargetWorldPos(Vector2 target)
    {
        Vector3 terrainWorldPos = _workingTerrain.GetPosition();
        float targetHeight = _workingTerrain.terrainData.GetHeight((int)target.x, (int)target.y);
        Vector3 targetWorldPos = new Vector3(terrainWorldPos.x + target.x, targetHeight, terrainWorldPos.z + target.y);

        return targetWorldPos;
    }
}
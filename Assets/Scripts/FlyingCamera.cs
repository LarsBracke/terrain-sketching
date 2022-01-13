using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCamera : MonoBehaviour
{
    [SerializeField] private GameObject _camera = null;

    [Header("Movement")]
    private const float _movementSpeed = 1f;

    [Header("Rotation")]
    private bool _canRotate = false;
    private const float _rotationSpeedX = 1f;
    private const float _rotationSpeedY = 1f;

    void Start()
    {
        
    }

    void Update()
    {
        if (!_camera)
        {
            Debug.LogWarning("No camera found");
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool detectedMovement = Mathf.Abs(v) > 0.01f || Mathf.Abs(h) > 0.01f;

        if (detectedMovement)
        {
            Vector3 movementVec = (v * _camera.transform.forward) + (h * _camera.transform.right);
            Vector3 displacement = movementVec * _movementSpeed * Time.deltaTime;
            _camera.transform.position += displacement;
        }
    }

    private void HandleRotation()
    {

    }
}

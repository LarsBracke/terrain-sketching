using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCamera : MonoBehaviour
{
    [SerializeField] private GameObject _camera = null;

    [Header("Movement")]
    private const float _movementSpeed = 25f;

    [Header("Rotation")]
    private bool _canRotate = false;
    private const float _sensitivityX = 1f;
    private const float _sensitivityY = 1f;

    void Start()
    { }

    void Update()
    {
        if (!_camera)
        {
            Debug.LogWarning("No camera found");
            return;
        }

        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float z = 0.0f;
        if (Input.GetKeyDown(KeyCode.E))
            z += 1.0f;
        if (Input.GetKeyDown(KeyCode.Q))
            z -= 1.0f;

        bool detectedMovement = 
            Mathf.Abs(v) > 0.01f || Mathf.Abs(h) > 0.01f || Mathf.Abs(z) > 0.01f;

        if (detectedMovement)
        {
            Vector3 movementVec = (v * _camera.transform.forward) + (h * _camera.transform.right) + (z * _camera.transform.up);
            Vector3 displacement = movementVec * _movementSpeed * Time.deltaTime;
            _camera.transform.position += displacement;
        }
    }

    private void HandleRotation()
    {
        if (!Input.GetKey(KeyCode.Mouse1))
            return;

        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(x-y) > 0.01f)
        {
            float xRot = Input.GetAxis("Mouse X") * _sensitivityX;
            float yRot = -(Input.GetAxis("Mouse Y") * _sensitivityY);

            _camera.transform.Rotate(0f, xRot, 0f);
            _camera.transform.Rotate(yRot, 0f, 0f);

            // Locked z-rotation
            _camera.transform.rotation = Quaternion.Euler(_camera.transform.eulerAngles.x, _camera.transform.eulerAngles.y, 1f);
        }
    }
}

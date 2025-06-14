using System;
using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    private Camera mainCamera;

    [SerializeField]
    private float cameraSpeed;
    
    [SerializeField]
    private float zoomSpeed;


    [SerializeField]
    private InputActionAsset inputActions;
    private InputAction movementAction;
    private InputAction zoomAction;

    private Vector3 velocityRef = Vector3.zero;
    private Vector3 targetPosition;

    private bool isMoving = false;

    private void OnEnable()
    {
        inputActions.Enable();
        var map = inputActions.FindActionMap("Player");
        map.Enable();

        movementAction = map.FindAction("Movement");
        zoomAction = map.FindAction("Zoom");
        movementAction.Enable();
        zoomAction.Enable();
    }

    private void OnDisable()
    {
        movementAction.Disable();
        zoomAction.Disable();
    }
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;    
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = movementAction.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, input.y, 0).normalized;
        transform.Translate(movement *(cameraSpeed * Time.deltaTime));

        var z = zoomAction.ReadValue<Vector2>().y;
        mainCamera.orthographicSize -= z * zoomSpeed * Time.deltaTime;
    }
}

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
    private float cameraSmoothTime;

    [SerializeField]
    private InputActionAsset inputActions;
    private InputAction movementAction;

    private Vector3 velocityRef = Vector3.zero;
    private Vector3 targetPosition;

    private bool isMoving = false;

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.FindActionMap("Player").Enable();

        movementAction = InputSystem.actions.FindAction("Movement");
        movementAction.Enable();
        movementAction.performed += OnCameraMove;
        movementAction.canceled  += OnCameraMoveEnd;

    }

    private void OnDisable()
    {
        movementAction.Disable();
        movementAction.performed -= OnCameraMove;
        movementAction.canceled -= OnCameraMoveEnd;

        inputActions.FindActionMap("Player").Disable();
    }

    private void OnCameraMove(InputAction.CallbackContext context)
    {
        isMoving = true;
        StartCoroutine(CameraMove(context.ReadValue<Vector2>()));
    }
    
    private void OnCameraMoveEnd(InputAction.CallbackContext context)
    {
        isMoving = false;
    }

    private IEnumerator CameraMove(Vector2 input)
    {
        while (isMoving)
        {
            Vector3 moveDirection = transform.up * input.y + transform.right * input.x;
            moveDirection.Normalize();

            targetPosition = transform.position + moveDirection * cameraSpeed;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocityRef, cameraSmoothTime);
            yield return null;
        }
    }

 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

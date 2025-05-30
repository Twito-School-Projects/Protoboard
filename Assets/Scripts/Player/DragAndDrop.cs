using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDrop : Singleton<DragAndDrop>
{
    [SerializeField]
    private InputAction mouseClick;

    [SerializeField]
    private float mouseDragSpeed;

    private Camera mainCamera;
    private Vector3 velocity = Vector3.zero;

    private void OnEnable()
    {
        mouseClick.Enable();
        mouseClick.performed += MousePressed;
    }

    private void OnDisable()
    {
        mouseClick.Disable();
        mouseClick.performed -= MousePressed;
    }

    private void MousePressed(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Draggable"))
            {
                StartCoroutine(DragUpdate(hit.collider.gameObject));
            }
        }
    }

    private IEnumerator DragUpdate(GameObject clickedObject)
    {
        float initialDistance = Vector3.Distance(clickedObject.transform.position, mainCamera.transform.position);
        clickedObject.TryGetComponent<IDrag>(out IDrag dragComponent);

        dragComponent?.OnDragStart();
        while (mouseClick.ReadValue<float>() != 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Vector3 direction = ray.GetPoint(initialDistance) - clickedObject.transform.position;
            Vector3 xzDirection = new Vector3(direction.x, 0f, direction.z);

            clickedObject.transform.position = Vector3.SmoothDamp(clickedObject.transform.position, ray.GetPoint(initialDistance), ref velocity, mouseDragSpeed);
            clickedObject.transform.position = new Vector3(clickedObject.transform.position.x, 4, clickedObject.transform.position.z);
            yield return null;
        }
        dragComponent?.OnDragEnd();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}
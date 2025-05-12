using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableUIElement : MonoBehaviour
{
    [SerializeField]
    private InputAction mouseClick;

    [SerializeField]
    private float mouseDragPhysicsSpeed;

    private Camera mainCamera;
    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

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
            if (hit.collider != null)
            {
                StartCoroutine(DragUpdate(hit.collider.gameObject));
                Debug.Log("Mouse clicked on " + gameObject.name);
            }
        }
    }

    private IEnumerator DragUpdate(GameObject clickedObject)
    {
        float initialDistance = Vector3.Distance(clickedObject.transform.position, mainCamera.transform.position);
        clickedObject.TryGetComponent<Rigidbody>(out var rb);

        while (mouseClick.ReadValue<float>() != 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (rb != null)
            {
                Vector3 direction = ray.GetPoint(initialDistance) - clickedObject.transform.position;
                Vector3 xzDirection = new Vector3(direction.x, 0f, direction.z);

                rb.linearVelocity = xzDirection * mouseDragPhysicsSpeed;
                yield return waitForFixedUpdate;
            }
        }
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
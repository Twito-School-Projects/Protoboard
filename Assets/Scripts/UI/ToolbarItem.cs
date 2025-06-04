using UnityEngine;
using UnityEngine.EventSystems;

public class ToolbarItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
    public GameObject componentPrefab;
    private ElectronicComponent component;

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("dragging");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered UI element: " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exited UI element: " + gameObject.name);
    }

    private void Start()
    {
        component = componentPrefab.GetComponent<ElectronicComponent>();
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Mouse is over a UI element
            Debug.Log("Mouse is over " + transform.name);
        }
    }
}
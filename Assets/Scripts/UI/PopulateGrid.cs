using UnityEngine;
using UnityEngine.UI;

public class PopulateGrid : MonoBehaviour
{
    public GameObject itemPrefab;
    public GameObject[] componentPrefabs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Populate();
    }

    private void Populate()
    {
        for (int i = 0; i < componentPrefabs.Length; i++)
        {
            GameObject obj = Instantiate(itemPrefab, transform);

            var toolbarItem = obj.GetComponent<ToolbarItem>();
            var image = obj.GetComponent<Image>();

            var component = componentPrefabs[i].GetComponent<ElectronicComponent>();
            image.sprite = component.imageSprite;
            toolbarItem.componentPrefab = componentPrefabs[i];

            //Toolbar.Instance.items.Add(toolbarItem);
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}
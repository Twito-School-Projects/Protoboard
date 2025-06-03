using NUnit.Framework.Internal.Builders;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PopulateGrid : MonoBehaviour
{
    public GameObject prefab;
    public int numberToCreate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Populate();    
    }

    private void Populate()
    {
        GameObject obj;

        for (int i = 0; i < numberToCreate; i++)
        {
            obj = Instantiate(prefab, transform);
            obj.GetComponent<Image>().color = UnityEngine.Random.ColorHSV();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

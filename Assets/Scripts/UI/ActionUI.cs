﻿using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ActionUI : MonoBehaviour
{
    
    private VisualElement m_Root;
    private Label actionLabel;
    
    private void OnEnable()
    {
        SetupUI();
        WireMaker.WireCreationStarted += OnWireCreationStarted;
        WireMaker.WireCreationCancelled += OnWireCreationCancelled;
        WireMaker.WireCreationEnded += OnWireCreationEnded;
        
        ComponentPlacementSystem.OnPlacementStarted += OnPlacementStarted;
        ComponentPlacementSystem.OnComponentPlaced += OnComponentPlaced;
        ComponentPlacementSystem.OnPlacementCancelled += OnPlacementCancelled;
    }

    private void OnDisable()
    {
        WireMaker.WireCreationStarted -= OnWireCreationStarted;
        WireMaker.WireCreationCancelled -= OnWireCreationCancelled;
        WireMaker.WireCreationEnded -= OnWireCreationEnded;
        
        ComponentPlacementSystem.OnPlacementStarted -= OnPlacementStarted;
        ComponentPlacementSystem.OnComponentPlaced -= OnComponentPlaced;
        ComponentPlacementSystem.OnPlacementCancelled -= OnPlacementCancelled;
    }

    private void OnWireCreationEnded(Hole obj, Hole obj2)
    {
        actionLabel.text = "Created wire from " + obj + " to " + obj2;
    }

    private void OnWireCreationCancelled(Hole obj)
    {
        Cancelled(obj).AsTask().Wait();
    }
    
    private async UniTask Cancelled(Hole obj)
    {
        actionLabel.text = "Cancelled wire creation for some reason";
        await UniTask.Delay(1000);
        actionLabel.text = "";
    }

    private void OnWireCreationStarted(Hole obj)
    {
        actionLabel.text = "Starting wire on " + obj;
    }


    private void SetupUI()
    {
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        actionLabel = m_Root.Q<Label>("ActionIndicator");
        actionLabel.text = "";
    }
    
    private void OnPlacementStarted(ComponentData componentData)
    {
        actionLabel.text = "Placing " + componentData.componentName;
    }
    
    private void OnComponentPlaced(GameObject placedComponent)
    {
        actionLabel.text = "Placed " + placedComponent.name.Split(" ")[0];
    }
    
    private void OnPlacementCancelled(string message)
    {
        actionLabel.text = message;
    }
}

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlsUI : MonoBehaviour
{
    
    private VisualElement m_Root;
    private Label controlsLabel;
    
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
        controlsLabel.text = "";
    }

    private void OnWireCreationCancelled(Hole obj)
    {
        controlsLabel.text = "";

    }

    private void OnWireCreationStarted(Hole obj)
    {
        controlsLabel.text = "Press 'ESC' or 'Q' to cancel";
    }
    
    private void OnPlacementStarted(ComponentData componentData)
    {
        controlsLabel.text = "Press 'ESC' or 'Q' to cancel";
    }
    
    private void OnComponentPlaced(GameObject placedComponent)
    {
        controlsLabel.text = "";
    }
    
    private void OnPlacementCancelled(string message)
    {
        controlsLabel.text = "";
    }
    
    private void SetupUI()
    {
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        controlsLabel = m_Root.Q<Label>("ControlsIndicator");
        controlsLabel.text = "";
    }
}

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MySimpleTarget : MonoBehaviour
{
    [Header("Assign materials")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material hitMaterial;

    private MeshRenderer meshRenderer;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable simple;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        simple = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        if (!simple)
            Debug.LogError("[SimpleTarget] Requires XRSimpleInteractable on the same GameObject.");
    }

    private void Start()
    {
        if (meshRenderer && defaultMaterial)
            meshRenderer.material = defaultMaterial;
    }

    private void OnEnable()
    {
        if (simple != null)
        {
            simple.hoverEntered.AddListener(OnHoverEntered);
            simple.hoverExited.AddListener(OnHoverExited);
        }
    }

    private void OnDisable()
    {
        if (simple != null)
        {
            simple.hoverEntered.RemoveListener(OnHoverEntered);
            simple.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs _)
    {
        if (meshRenderer && hitMaterial)
            meshRenderer.material = hitMaterial;
    }

    private void OnHoverExited(HoverExitEventArgs _)
    {
        if (meshRenderer && defaultMaterial)
            meshRenderer.material = defaultMaterial;
    }
}
using UnityEngine;


public class MySetupRayInteractors : MonoBehaviour
{
    [Header("Assign controller GameObjects from XR Origin")]
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;

    [Header("Ray Tuning")]
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float lineWidth = 0.01f;
    [SerializeField] private string raycastLayerName = "Target";

    private void Start()
    {
        ConfigureOne(leftController);
        ConfigureOne(rightController);
    }

    private void ConfigureOne(GameObject controllerGO)
    {
        if (!controllerGO)
        {
            Debug.LogWarning("[SetupRayInteractors] Missing controller reference.");
            return;
        }

        // Ensure ray exists, start disabled (perf-friendly)
        var ray = controllerGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
        if (!ray) ray = controllerGO.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
        ray.enabled = false;
        ray.maxRaycastDistance = maxDistance;

        // Restrict to the Target layer
        int layer = LayerMask.NameToLayer(raycastLayerName);
        ray.raycastMask = (layer == -1) ? ~0 : (1 << layer);

        // Visual line
        var line = controllerGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();
        if (!line) line = controllerGO.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();
        line.lineWidth = lineWidth;
        line.enabled = true;
    }
}
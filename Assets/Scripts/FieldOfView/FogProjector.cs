using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Projector))]
public class FogProjector : MonoBehaviour
{
    [SerializeField] private Material projectorMaterial = null;
    [SerializeField] private RenderTexture fogTexture = null;

    private Projector projector;
    private static readonly int CurrTexture = Shader.PropertyToID("_CurrTexture");

    private void Awake()
    {
        projector = GetComponent<Projector>();
    }

    private void OnEnable()
    {
        // Projector materials aren't instanced, resulting in the material asset getting changed.
        // Instance it here to prevent us from having to check in or discard these changes manually.
        projector.material = new Material(projectorMaterial);
        projector.material.SetTexture(CurrTexture, fogTexture);
    }

    private void Update()
    {
        Graphics.Blit(fogTexture, fogTexture);
    }
}
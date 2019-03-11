using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
public class HighlightOnHovered : MonoBehaviour, IHoverWatcher
{
    [SerializeField] private Material highlightMaterial = null;
    
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnHovered()
    {
        var mats = new Material[meshRenderer.materials.Length+1];
        meshRenderer.materials.CopyTo(mats, 0);
        mats[mats.Length - 1] = highlightMaterial;
        meshRenderer.materials = mats.ToArray();
    }

    public void OnUnhovered()
    {
        var mats = new Material[meshRenderer.materials.Length-1];
        for (var i = 0; i < mats.Length; i++) mats[i] = meshRenderer.materials[i];
        meshRenderer.materials = mats.ToArray();
    }

    private void OnDestroy()
    {
        if (meshRenderer.materials.Contains(highlightMaterial))
            OnUnhovered();
    }
}
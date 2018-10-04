using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
public class HighlightOnHovered : MonoBehaviour, IHoverWatcher {
    private MeshRenderer mr;

    [SerializeField] private Material highlightMaterial;

    void Awake() {
        mr = this.GetComponent<MeshRenderer>();
    }

    public void OnHovered() {
        List<Material> mats = new List<Material>(mr.materials);
        mats.Add(highlightMaterial);
        mr.materials = mats.ToArray();
    }

    public void OnUnhovered() {
        List<Material> mats = new List<Material>(mr.materials);
        mats.RemoveAt(mats.Count - 1);
        mr.materials = mats.ToArray();
    }

    void OnDestroyed() {
        if (mr.materials.Contains(highlightMaterial))
            OnUnhovered();
    }



}

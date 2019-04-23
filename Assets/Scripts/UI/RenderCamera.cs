using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RenderCamera : MonoBehaviour
{
    [SerializeField] private Camera cam = null;

    public Texture2D CamCapture(GameObject objectToCapture)
    {
        objectToCapture.SetActive(true);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        objectToCapture.layer = 16;
        objectToCapture.transform.Rotate(objectToCapture.GetComponent<Containable>().IconRotation);
        objectToCapture.transform.SetParent(transform, false);
        IHoverWatcher hw = objectToCapture.GetComponent<IHoverWatcher>();
        if (hw != null)
        {
            hw.OnUnhovered();
        }
        ScaleObject(objectToCapture);
        cam.Render();
        Texture2D Image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;
        Destroy(objectToCapture);
        return Image;
        
    }

    private void ScaleObject(GameObject obj)
    {
        var mf = obj.GetComponent<MeshFilter>();
        

        if (mf != null)
        {
            var bounds = mf.mesh.bounds;

            float max = bounds.extents.x;
            if (max < bounds.extents.y)
                max = bounds.extents.y;
            if (max < bounds.extents.z)
                max = bounds.extents.z;

            float scale = 0.75f / max;
            obj.transform.localScale = new Vector3(scale, scale, scale);
            obj.transform.position = this.transform.position + this.transform.forward;
        }
    }
}

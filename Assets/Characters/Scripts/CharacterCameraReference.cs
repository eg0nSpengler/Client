using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCameraReference : MonoBehaviour {

    [SerializeField] Camera characterCamera;

    public Camera GetCamera() {
        return characterCamera;
    }

    public Transform GetCameraTransform() {
        if (!characterCamera) return null;
        return characterCamera.transform;
    }
}

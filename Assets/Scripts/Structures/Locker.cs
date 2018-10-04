using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures.Containers {

    public class Locker : MonoBehaviour, IInteractable {
        [SerializeField] Container container;
        [SerializeField] private Transform door;
        [SerializeField] private Transform dropPosition;
        [SerializeField] private float maxAngle = 100;

        [SerializeField] private float openTime = 1f;
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private bool open = false;

        private Coroutine currentCoroutine;

        private void Awake() {
            container.OnContained += OnPlacedInLocked;
        }

        // TODO: Add logic for permissions
        public bool IsInteractable(GameObject source) {
            return true;
        }

        public void Interact(GameObject source) {
            if (!IsInteractable(source)) {
                return;
            }
            Containable inhand = source.GetComponent<CharacterInventory>().GetInhand();
            if (inhand && open) {
                // Place item inside if we can
                inhand.Container.Remove(inhand);
                container.Contain(inhand);
                return;
            } else {
                // Otherwise toggle the door
                open = !open;
                container.Restricted = !open;

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                }
                currentCoroutine = StartCoroutine(Move());
            }
        }

        private IEnumerator Move() {
            float from = (door.transform.localRotation.eulerAngles.z + 360) % 360;
            float to = open ? maxAngle : 0;
            for (float t = 0; t < openTime; t += Time.deltaTime) {
                float p = openCurve.Evaluate(t / openTime);
                float angle = Mathf.LerpUnclamped(from, to, p);
                door.transform.localRotation = Quaternion.Euler(0, 0, angle);
                yield return null;
            }
            door.transform.localRotation = Quaternion.Euler(0, 0, to);
        }

        private void OnPlacedInLocked(Containable containable) {
            containable.transform.position = dropPosition.transform.position;
        }
    }

}
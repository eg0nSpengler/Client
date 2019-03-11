using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters
{
    public class CharacterModels : MonoBehaviour
    {
        [SerializeField] private float garbageCheckInterval = 10;
        [SerializeField] private SkinnedMeshRenderer[] models = new SkinnedMeshRenderer[0];
        
        private readonly Dictionary<SkinnedMeshRenderer, Transform[]> skeletons = 
            new Dictionary<SkinnedMeshRenderer, Transform[]>();
        private readonly Stack<SkinnedMeshRenderer> removed = new Stack<SkinnedMeshRenderer>();
        
        private Transform[] rootSkeleton;
        
        
        private void Awake()
        {
            if (models.Length < 2) return;

            rootSkeleton = models[0].rootBone.GetComponentsInChildren<Transform>();
            skeletons.Add(models[0], rootSkeleton);
            for (var i = 1; i < models.Length; i++)
            {
                var transforms = models[i].rootBone.GetComponentsInChildren<Transform>();
                if (transforms.Length != rootSkeleton.Length)
                {
                    Debug.LogError($"Incorrect number of bones on {models[i].name}. Is {transforms.Length}. Should be {rootSkeleton.Length}.", models[i]);
                    continue;
                }
                for (var j = 0; j < transforms.Length; j++)
                    transforms[j].SetParent(rootSkeleton[j]);
                skeletons.Add(models[i], transforms);
            }
        }

        public void AddModel(SkinnedMeshRenderer model, bool reparent = true)
        {
            if (reparent) model.transform.parent.SetParent(transform);
            
            var transforms = model.rootBone.GetComponentsInChildren<Transform>();
            if (transforms.Length != rootSkeleton.Length)
            {
                Debug.LogError($"Incorrect number of bones on {model.name}. Is {transforms.Length}. Should be {rootSkeleton.Length}.", model);
                return;
            }
            for (var j = 0; j < transforms.Length; j++)
                transforms[j].SetParent(rootSkeleton[j]);
            skeletons.Add(model, transforms);
        }

        private void OnEnable()
        {
            StartCoroutine(CheckDestroyed());
        }

        private IEnumerator CheckDestroyed()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(garbageCheckInterval);

                foreach (var skeleton in skeletons)
                {
                    if(skeleton.Key != null) continue;
                    foreach (var bone in skeleton.Value)
                        Destroy(bone.gameObject);
                    removed.Push(skeleton.Key);
                }

                while (removed.Count > 0)
                    skeletons.Remove(removed.Pop());
            }
        }
    }
}

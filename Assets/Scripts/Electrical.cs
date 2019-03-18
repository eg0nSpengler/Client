using UnityEngine;

namespace Structures
{
    public class Electrical : MonoBehaviour
    {
        [SerializeField] private bool powered = false;

        public bool Powered => powered;
    }
}
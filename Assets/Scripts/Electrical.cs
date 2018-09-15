using UnityEngine;

namespace Structures
{
    public class Electrical : MonoBehaviour
    {
        [SerializeField] private bool powered;

        public bool Powered => powered;
    }
}
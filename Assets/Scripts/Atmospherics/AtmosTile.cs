using System;
using System.Linq;

namespace Atmospherics
{
    [Serializable]
    public struct AtmosTile
    {
        public float[] moles;
        public float[] temperature;

        public float TotalMoles => moles.Sum();
    }
}

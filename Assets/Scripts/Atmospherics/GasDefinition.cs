using System;

namespace Atmospherics
{
    [Serializable]
    public struct GasDefinition
    {
        public string name;
        public float mass;
        public float heatCapacity;
    }
}
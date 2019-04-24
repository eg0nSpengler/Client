using System;
using UnityEngine.Serialization;

namespace Atmospherics
{
    [Serializable]
    public struct GasDefinition
    {
        public string name;
        [FormerlySerializedAs("mass")] public float molarMass;
        public float heatCapacity;
        public float heatConductivity;
        public float viscosity;
    }
}
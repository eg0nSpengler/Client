using System;
using Unity.Entities;
using UnityEngine.Serialization;

namespace Atmospherics
{
    [Serializable]
    public struct GasDefinition
    {
        public NativeString64 name;
        [FormerlySerializedAs("mass")] public float molarMass;
        public float heatCapacity;
        public float heatConductivity;
        public float viscosity;
    }
}
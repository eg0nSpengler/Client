using System;
using Unity.Entities;
using UnityEngine.Serialization;

namespace Atmospherics
{
    [Serializable]
    public struct GasData
    {
        public NativeString64 name;
        public float molarMass;
        public float heatCapacity;
    }
}
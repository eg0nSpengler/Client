using System;
using Unity.Entities;

namespace Atmospherics.Components
{
    [Serializable]
    public struct GasData : IComponentData
    {
        public byte id;
        public float molarMass;
        public float heatCapacity;
        public float heatConductivity;
        public float viscosity;
    }
}
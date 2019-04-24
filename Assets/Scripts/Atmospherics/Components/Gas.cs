using Unity.Entities;
using Unity.Mathematics;

namespace Atmospherics.Components
{
    public struct Gas : IComponentData
    {
        public readonly byte id;
        
        public float moles;
        public float energy;
        public float4 flux;

        public Gas(byte id, float moles, float energy)
        {
            this.id = id;
            this.moles = moles;
            this.energy = energy;
            this.flux = float4.zero;
        }
    }
}
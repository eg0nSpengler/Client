using Unity.Entities;
using Unity.Mathematics;

namespace Atmospherics.Components
{
    public struct Gas : IComponentData
    {
        public byte id;
        
        public float moles;
        public float temperature;
        public float4 flux;

        public Gas(byte id, float moles)
        {
            this.id = id;
            this.moles = moles;
            this.temperature = 0;
            this.flux = float4.zero;
        }
    }
}
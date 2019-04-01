using Unity.Entities;

namespace Atmospherics.Components
{
    public struct Gas : IComponentData
    {
        public byte id;
        public float moles;

        public Gas(byte id, float moles)
        {
            this.id = id;
            this.moles = moles;
        }
    }
}
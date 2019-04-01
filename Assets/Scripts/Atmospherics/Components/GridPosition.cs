using Unity.Entities;
using Unity.Mathematics;

namespace Atmospherics.Components
{
    public struct GridPosition : IComponentData
    {
        public int3 value;

        public GridPosition(int3 value)
        {
            this.value = value;
        }

        public GridPosition(int x, int y, int z)
        {
            value = new int3(x, y, z);
        }
    }
}
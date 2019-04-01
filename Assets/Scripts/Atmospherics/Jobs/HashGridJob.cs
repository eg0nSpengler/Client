using Atmospherics.Components;
using Unity.Collections;
using Unity.Entities;

namespace Atmospherics.Jobs
{
    public struct HashGridJob<T> : IJobProcessComponentData<GridPosition, T> where T : struct, IComponentData
    {
        public NativeMultiHashMap<long, T>.Concurrent hashedGrid;
        
        public void Execute([ReadOnly] ref GridPosition position, [ReadOnly] ref T item)
        {
            hashedGrid.Add(AtmosphericsSystem.EncodePosition(position.value), item);
        }
    }
}
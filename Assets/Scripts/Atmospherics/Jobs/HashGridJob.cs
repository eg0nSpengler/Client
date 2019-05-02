using Atmospherics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Atmospherics.Jobs
{
    public struct HashGridJob<T> : IJobForEach<GridPosition, T> where T : struct, IComponentData
    {
        public NativeMultiHashMap<long, T>.Concurrent hashedGrid;
        
        [BurstCompile]
        public void Execute([ReadOnly] ref GridPosition position, [ReadOnly] ref T item)
        {
            hashedGrid.Add(AtmosphericsSystem.EncodePosition(position.value), item);
        }
    }
}
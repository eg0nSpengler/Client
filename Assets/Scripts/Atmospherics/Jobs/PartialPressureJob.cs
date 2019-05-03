using Atmospherics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Atmospherics.Jobs
{
    public struct PartialPressureJob : IJobForEach<GridPosition, Gas>
    {
        [ReadOnly] public NativeArray<GasData> gasData;
        [ReadOnly] public NativeMultiHashMap<long, Gas> gasses;

        [BurstCompile]
        public void Execute([ReadOnly] ref GridPosition position, ref Gas node)
        {
            node.partialPressure = GetPartialPressureAt(AtmosphericsSystem.EncodePosition(position.value), node.id);
        }

        private float GetPartialPressureAt(long pos, byte gasIndex)
        {
            var moles = 0f;
            var totalMoles = 0f;
            var totalEnergy = 0f;
            var totalCapacity = 0f;
            if (!gasses.TryGetFirstValue(pos, out var gas, out var it)) return 0;
            do
            {
                if (gas.id == gasIndex) moles = gas.moles;
                totalMoles += gas.moles;
                totalEnergy += gas.energy;
                totalCapacity += gasData[gas.id].heatCapacity * gas.moles;
            }
            while (gasses.TryGetNextValue(out gas, ref it));

            return totalCapacity > 0 && totalMoles > 0
                ? (moles / totalMoles) *
                  (totalMoles * AtmosphericsSystem.GasConstant * (totalEnergy / totalCapacity)
                   / AtmosphericsSystem.NodeVolume)
                : 0;
        }
    }
}
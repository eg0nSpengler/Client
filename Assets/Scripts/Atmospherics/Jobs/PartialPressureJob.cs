using Atmospherics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Atmospherics.Jobs
{
    public struct PartialPressureJob : IJobProcessComponentData<GridPosition, Gas>
    {
        [ReadOnly] public NativeArray<GasDefinition> gasses;
        [ReadOnly] public NativeMultiHashMap<long, Gas> gasMap;
        
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
            if (!gasMap.TryGetFirstValue(pos, out var gas, out var it)) return 0;
            do
            {
                if (gas.id == gasIndex) moles = gas.moles;
                totalMoles += gas.moles;
                totalEnergy += gas.energy;
                totalCapacity += gasses[gas.id].heatCapacity * gas.moles;
            }
            while (gasMap.TryGetNextValue(out gas, ref it));

            return moles/totalMoles * AtmosphericsSystem.Pressure(AtmosphericsSystem.NodeVolume, totalMoles, totalEnergy / totalCapacity);
        }
    }
}
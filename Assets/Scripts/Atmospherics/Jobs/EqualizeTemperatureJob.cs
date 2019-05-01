using Atmospherics.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Atmospherics.Jobs
{
    public struct EqualizeTemperatureJob : IJobProcessComponentData<GridPosition, Gas>
    {
        [ReadOnly] public NativeArray<GasData> gasses;
        [ReadOnly] public NativeMultiHashMap<long, Gas> gasMap;
        
        public void Execute([ReadOnly] ref GridPosition position, ref Gas node)
        {
            var pos = AtmosphericsSystem.EncodePosition(position.value);
            var totalEnergy = 0f;
            var totalCapacity = 0f;
            if (!gasMap.TryGetFirstValue(pos, out var gas, out var it)) return;
            do
            {
                totalCapacity += gasses[gas.id].heatCapacity * gas.moles;
                totalEnergy += gas.energy;
            }
            while (gasMap.TryGetNextValue(out gas, ref it));
           
            if(totalCapacity > 0)
                node.energy = totalEnergy * (gasses[node.id].heatCapacity * node.moles) / totalCapacity;
        }
    }
}
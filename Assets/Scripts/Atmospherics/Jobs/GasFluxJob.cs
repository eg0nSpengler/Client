using Atmospherics.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Atmospherics.Jobs
{
    public struct GasFluxJob : IJobProcessComponentData<GridPosition, Gas>
    {
        [ReadOnly] public NativeArray<GasDefinition> gasses;
        [ReadOnly] public NativeMultiHashMap<long, Gas> gasMap;
        [ReadOnly] public float deltaTime;
        
        [WriteOnly]public NativeMultiHashMap<long, MovedGas>.Concurrent movedGasses;

        public void Execute([ReadOnly] ref GridPosition position, ref Gas node)
        {
            if (node.moles == 0)
            {
                // TODO remove the gas entity when empty
                return;
            }
            
            var pressure = GetPartialPressureAt(AtmosphericsSystem.EncodePosition(position.value), node.id);
            var flux = node.flux;

            for (var i = 0; i < AtmosphericsSystem.Directions.Length; i++)
            {
                var pos = AtmosphericsSystem.EncodePosition(position.value + AtmosphericsSystem.Directions[i]);
                var p = GetPartialPressureAt(pos, node.id);
                var force = (pressure - p) / AtmosphericsSystem.ContactArea;
                var acceleration = force / node.moles * gasses[node.id].molarMass;

                flux[i] += 0.5f * acceleration * deltaTime;
                var amount = 2 * (flux[i] * deltaTime + 0.5f * acceleration * deltaTime * deltaTime);
                amount = math.min(amount, node.moles); // TODO fairly distribute in case there isn't enough
                
                if (amount > 0)
                {            
                    var energy = amount/node.moles;

                    if (Exists(pos, node.id))
                    {
                        //Debug.Log($"{position.value} -> {(position.value+AtmosphericsSystem.Directions[i])}: {pressure} {p} -> {force}");
                        
                        node.moles -= amount;
                        node.energy -= energy;
                    
                        movedGasses.Add(pos, new MovedGas(node.id, amount, energy));
                    }
                    // TODO create new gas node if none exists
                    
                }
                
                flux[i] *= 0.95f;
                if (flux[i] < 0) flux[i] = 0;
            }

            node.flux = flux;
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
        private bool Exists(long pos, byte gasIndex)
        {
            if (!gasMap.TryGetFirstValue(pos, out var gas, out var it)) return false;
            do if (gas.id == gasIndex) return true;
            while (gasMap.TryGetNextValue(out gas, ref it));
            return false;
        }
    }
}
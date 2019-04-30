using System;
using Atmospherics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Atmospherics.Jobs
{
    public struct GasFluxJob : IJobProcessComponentData<GridPosition, Gas>
    {
        [ReadOnly] public NativeArray<GasDefinition> gasses;
        [ReadOnly] public NativeMultiHashMap<long, Gas> gasMap;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public NativeArray<int3> directions;
        
        [WriteOnly]public NativeMultiHashMap<long, MovedGas>.Concurrent movedGasses;

        [BurstCompile]
        public void Execute([ReadOnly] ref GridPosition position, [ReadOnly] ref Gas node)
        {
            if (Math.Abs(node.moles) < 0.00001f)
            {
                // TODO remove the gas entity when empty
                return;
            }
            
            var pos = AtmosphericsSystem.EncodePosition(position.value);
            var pressure = node.partialPressure;
            var flux = node.flux;

            for (var i = 0; i < directions.Length; i++)
            {
                var neighborPos = AtmosphericsSystem.EncodePosition(position.value + directions[i]);
                var neighbor = GetNodeAt(neighborPos, node.id);
                
                var force = (pressure - neighbor.partialPressure) / AtmosphericsSystem.ContactArea;
                var acceleration = force / (node.moles * gasses[node.id].molarMass);

                flux[i] += 0.5f * acceleration * deltaTime;
                var molesMoved = AtmosphericsSystem.ContactArea * (flux[i] * deltaTime + 0.5f * acceleration * deltaTime * deltaTime);
                molesMoved = math.clamp(molesMoved, 0, node.moles); // TODO fairly distribute in case there isn't enough
                
                
                const float d = 2 * AtmosphericsSystem.ContactArea / AtmosphericsSystem.ContactCircumference;
                var h = gasses[node.id].heatConductivity 
                        * 0.023f 
                        * ((node.flux[i]+AtmosphericsSystem.BaseFlux)*d/gasses[node.id].viscosity) 
                        * math.sqrt(gasses[node.id].viscosity*gasses[node.id].heatCapacity/gasses[node.id].heatConductivity) 
                            / d;
        
                var energyMoved = h * 2 * ((neighbor.energy-node.energy) * gasses[node.id].heatCapacity) * deltaTime;
                
                if (molesMoved > 0)
                {            
                    var energy = node.energy * (molesMoved/node.moles);

                    if (neighbor.IsCreated)
                    {
                        movedGasses.Add(neighborPos, new MovedGas(node.id, molesMoved, energy));
                        movedGasses.Add(pos, new MovedGas(node.id, -molesMoved, -energy));
                    }
                    else
                    {
                        // TODO create new gas node if none exists
                    }
                }
                
                flux[i] *= 0.95f;
                if (flux[i] < 0) flux[i] = 0;
            }

            node.flux = flux;
        }

        private Gas GetNodeAt(long pos, byte gasIndex)
        {
            if (!gasMap.TryGetFirstValue(pos, out var gas, out var it)) return default;
            do if (gas.id == gasIndex) return gas;
            while (gasMap.TryGetNextValue(out gas, ref it));
            return default;
        }
    }
}
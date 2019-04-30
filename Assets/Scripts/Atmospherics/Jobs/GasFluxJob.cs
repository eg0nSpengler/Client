using System;
using Atmospherics.Components;
using Unity.Burst;
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
        [ReadOnly] public NativeArray<int3> directions;

        [WriteOnly] public NativeMultiHashMap<long, MovedGas>.Concurrent movedGasses;

        [BurstCompile]
        public void Execute([ReadOnly] ref GridPosition position, ref Gas node)
        {
            // ## Remove nodes that are empty

            if (math.abs(node.moles) < 0.00001f)
            {
                // TODO remove the gas entity when empty
                return;
            }


            // ## Get the current state

            var pos = AtmosphericsSystem.EncodePosition(position.value);
            var pressure = node.partialPressure;
            var flux = node.flux;


            // ## Allocate temporary arrays for data

            var neighbor = new NativeArray<Gas>(directions.Length,
                Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var molesMoved = new NativeArray<float>(directions.Length,
                Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var energyMoved = new NativeArray<float>(directions.Length,
                Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            var totalMolesMoved = 0f;
            var totalEnergyMoved = 0f;


            // ## Process nodes in each direction

            for (var i = 0; i < directions.Length; i++)
            {
                // ## Get the neighbor in this direction

                var neighborPos = AtmosphericsSystem.EncodePosition(position.value + directions[i]);
                neighbor[i] = GetNodeAt(neighborPos, node.id);


                // ## Calculate the forces from the pressure difference

                var force = (pressure - neighbor[i].partialPressure) / AtmosphericsSystem.ContactArea;
                var acceleration = force / (node.moles * gasses[node.id].molarMass);


                // ## Add to the flux

                flux[i] += 0.5f * acceleration * deltaTime;
                flux[i] *= AtmosphericsSystem.Drag;
                if (flux[i] < 0) flux[i] = 0;


                // ## Calculate how much we're actually trying to move for moles

                molesMoved[i] = AtmosphericsSystem.ContactArea *
                                (flux[i] * deltaTime + 0.5f * acceleration * deltaTime * deltaTime);
                if (molesMoved[i] < 0) molesMoved[i] = 0;


                // ## Calculate how much we're actually trying to move for energy

                const float d = 2 * AtmosphericsSystem.ContactArea / AtmosphericsSystem.ContactCircumference;
                var h = gasses[node.id].heatConductivity
                        * 0.023f
                        * ((node.flux[i] + AtmosphericsSystem.BaseFlux) * d / gasses[node.id].viscosity)
                        * math.sqrt(gasses[node.id].viscosity * gasses[node.id].heatCapacity /
                                    gasses[node.id].heatConductivity)
                        / d;

                energyMoved[i] = node.energy * (molesMoved[i] / node.moles);
                energyMoved[i] += deltaTime * h * 2 * 
                                  ((neighbor[i].energy - node.energy) 
                                   / (gasses[node.id].heatCapacity * (neighbor[i].moles + node.moles)));
                if (energyMoved[i] < 0) energyMoved[i] = 0;


                // ## Count the total we want to move

                if (!neighbor[i].IsCreated) continue;
                totalMolesMoved += molesMoved[i];
                totalEnergyMoved += energyMoved[i];
            }


            // ## Create movement for each of the valid directions

            for (var i = 0; i < directions.Length; i++)
            {
                var neighborPos = AtmosphericsSystem.EncodePosition(position.value + directions[i]);

                // ## If there's not enough to go around, balance the amount moved fairly

                if (totalMolesMoved > node.moles)
                    molesMoved[i] = node.moles * (molesMoved[i] / totalMolesMoved);
                if (totalEnergyMoved > node.energy)
                    energyMoved[i] = node.energy * (energyMoved[i] / totalEnergyMoved);


                // ## Don't move anything if there's nothing to move

                if (math.abs(energyMoved[i]) < 0.00001f && math.abs(molesMoved[i]) < 0.00001f) continue;


                // ## Actually move the stuff

                if (neighbor[i].IsCreated)
                {
                    movedGasses.Add(neighborPos, new MovedGas(node.id, molesMoved[i], energyMoved[i]));
                    movedGasses.Add(pos, new MovedGas(node.id, -molesMoved[i], -energyMoved[i]));
                }
                else
                {
                    // TODO create new gas node if none exists
                }
            }

            node.flux = flux;
        }

        private Gas GetNodeAt(long pos, byte gasIndex)
        {
            if (!gasMap.TryGetFirstValue(pos, out var gas, out var it)) return default;
            do
                if (gas.id == gasIndex)
                    return gas;
            while (gasMap.TryGetNextValue(out gas, ref it));
            return default;
        }
    }
}
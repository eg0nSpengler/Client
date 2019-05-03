using System;
using Atmospherics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Atmospherics.Jobs
{
    public struct GasFluxJob : IJobForEachWithEntity<GridPosition, Gas>
    {
        [ReadOnly] public NativeArray<GasData> gasData;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public NativeArray<int3> directions;
        
        [ReadOnly] public NativeMultiHashMap<long, Gas> gasses;
        [ReadOnly] public NativeMultiHashMap<long, GasBlocker> blockers;

        [WriteOnly] public NativeMultiHashMap<long, MovedGas>.Concurrent movedGasses;

        [BurstCompile]
        public void Execute(Entity entity, int index, [ReadOnly] ref GridPosition position, ref Gas node)
        {
            // ## Remove nodes that are empty

            if (math.abs(node.moles) < 0.00001f)
            {
                //entityCommand.DestroyEntity(index, entity);
                return;
            }


            // ## Get the current state

            var data = gasData[node.id];
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
            var blocked = new NativeArray<byte>(directions.Length,
                Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            var totalMolesMoved = 0f;
            var totalEnergyMoved = 0f;


            // ## Process nodes in each direction

            for (var i = 0; i < directions.Length; i++)
            {
                // ## Get the neighbor in this direction

                var neighborPos = AtmosphericsSystem.EncodePosition(position.value + directions[i]);
                blocked[i] = Blocked(neighborPos);
                if (blocked[i] != 0) continue;
                neighbor[i] = GetNodeAt(neighborPos, node.id);


                // ## Calculate the forces from the pressure difference

                var force = (pressure - neighbor[i].partialPressure) / AtmosphericsSystem.ContactArea;
                var acceleration = force / (node.moles * data.molarMass);


                // ## Add to the flux

                flux[i] += 0.5f * acceleration * deltaTime;
                flux[i] *= AtmosphericsSystem.Drag;
                if (flux[i] < 0) flux[i] = 0;


                // ## Calculate how much we're actually trying to move for moles

                const float d = 2 * AtmosphericsSystem.ContactArea / AtmosphericsSystem.ContactCircumference;

                molesMoved[i] = d * (flux[i] * deltaTime + 0.5f * acceleration * deltaTime * deltaTime) +
                    AtmosphericsSystem.BaseFlux * deltaTime;
                if (molesMoved[i] < 0) molesMoved[i] = 0;


                // ## Calculate how much we're actually trying to move for energy

                energyMoved[i] = node.energy * (molesMoved[i] / node.moles);
                if (energyMoved[i] < 0) energyMoved[i] = 0;


                // ## Count the total we want to move

                if (!neighbor[i].IsCreated) continue;
                totalMolesMoved += molesMoved[i];
                totalEnergyMoved += energyMoved[i];
            }


            // ## Create movement for each of the valid directions

            for (var i = 0; i < directions.Length; i++)
            {
                if (blocked[i] == 1) continue;
                
                var neighborPos = AtmosphericsSystem.EncodePosition(position.value + directions[i]);

                
                // ## If there's not enough to go around, balance the amount moved fairly

                if (totalMolesMoved > node.moles)
                    molesMoved[i] = node.moles * (molesMoved[i] / totalMolesMoved);
                if (totalEnergyMoved > node.energy)
                    energyMoved[i] = node.energy * (energyMoved[i] / totalEnergyMoved);


                // ## Don't move anything if there's nothing to move

                if (math.abs(energyMoved[i]) < 0.00001f && math.abs(molesMoved[i]) < 0.00001f) continue;


                // ## Actually move the stuff

                if(neighbor[i].IsCreated)
                {
                    movedGasses.Add(neighborPos, new MovedGas(node.id, molesMoved[i], energyMoved[i]));
                    movedGasses.Add(pos, new MovedGas(node.id, -molesMoved[i], -energyMoved[i]));
                }
            }

            node.flux = flux;
            
            neighbor.Dispose();
            molesMoved.Dispose();
            energyMoved.Dispose();
            blocked.Dispose();
        }

        private byte Blocked(long pos)
        {
            return (byte) (blockers.TryGetFirstValue(pos, out _, out _) ? 1 : 0);
        }

        private Gas GetNodeAt(long pos, byte gasIndex)
        {
            if (!gasses.TryGetFirstValue(pos, out var gas, out var it)) return default;
            do
            {
                if (gas.id == gasIndex)
                    return gas;
            }
            while (gasses.TryGetNextValue(out gas, ref it));
            return default;
        }
    }
}
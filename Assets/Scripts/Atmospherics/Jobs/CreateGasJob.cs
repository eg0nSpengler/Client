using Atmospherics.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Atmospherics.Jobs
{
    public struct CreateGasJob : IJob
    {
        [ReadOnly] public int maxGasses;
        [ReadOnly] public EntityArchetype gasArchetype;
        [ReadOnly] public NativeMultiHashMap<long, MovedGas> movedGasses;

        public NativeQueue<int3> addList;
        [WriteOnly] public EntityCommandBuffer commandBuffer;

        public void Execute()
        {
            var done = new NativeList<int3>(Allocator.Temp);
            var totalMoles = new NativeArray<float>(maxGasses, Allocator.Temp);
            var totalEnergy = new NativeArray<float>(maxGasses, Allocator.Temp);
           
            while (addList.TryDequeue(out var pos))
            {
                var alreadyCreated = false;
                for(var i = 0; i < done.Length; i++)
                    if (done[i].Equals(pos)) alreadyCreated = true;
                if(alreadyCreated) continue;
                done.Add(pos);
                
                var p = AtmosphericsSystem.EncodePosition(pos);
                if (!movedGasses.TryGetFirstValue(p, out var gas, out var it)) continue;
                do
                {
                    totalMoles[gas.gasIndex] += gas.amount;
                    totalEnergy[gas.gasIndex] += gas.energy;
                }
                while (movedGasses.TryGetNextValue(out gas, ref it));

                for (byte i = 0; i < maxGasses; i++)
                {
                    if (totalMoles[i] <= 0 && totalEnergy[i] <= 0) continue;

                    var entity = commandBuffer.CreateEntity(gasArchetype);
                    commandBuffer.SetComponent(entity, new GridPosition(pos));
                    commandBuffer.SetComponent(entity, new Gas(i, totalMoles[i], totalEnergy[i]));

                    totalMoles[i] = 0;
                    totalEnergy[i] = 0;
                }
            }

            done.Dispose();
            totalMoles.Dispose();
            totalEnergy.Dispose();
        }
    }
}
using System;
using Atmospherics.Components;
using Atmospherics.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Atmospherics
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AtmosphericsSystem : JobComponentSystem
    {
        public const float GasConstant = 8.31445984848f;
        public const float NodeVolume = 2;
        public const float NodeSurface = 10;
        public static readonly int3[] Directions = {
            new int3(0, 0, 1),new int3(1, 0, 0),new int3(0, 0, -1),new int3(-1, 0, 0),
        };
        
        
        private ComponentGroup nodeGroup;
        private NativeMultiHashMap<long, AtmosphericsNode> neighbors;
        private NativeMultiHashMap<long, Gas> gasses;
        
        protected override void OnCreateManager()
        {
            nodeGroup = GetComponentGroup(
                ComponentType.ReadOnly<GridPosition>(),
                ComponentType.ReadOnly<AtmosphericsNode>());
        }

        protected override void OnDestroyManager()
        {
            if(gasses.IsCreated) gasses.Dispose();
            if(neighbors.IsCreated) neighbors.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if(neighbors.IsCreated) neighbors.Dispose();
            if(gasses.IsCreated) gasses.Dispose();

            var length = nodeGroup.CalculateLength();
            neighbors = new NativeMultiHashMap<long, AtmosphericsNode>(length, Allocator.TempJob);
            gasses = new NativeMultiHashMap<long, Gas>(length * 4, Allocator.TempJob);

            return new AtmosphericsJob
            {
                gasMap = gasses,
                nodeMap = neighbors,
            }.Schedule(this, JobHandle.CombineDependencies(
                new HashGridJob<Gas>
                {
                    hashedGrid = gasses.ToConcurrent()
                }.Schedule(this, inputDeps),
                new HashGridJob<AtmosphericsNode>
                {
                    hashedGrid = neighbors.ToConcurrent()
                }.Schedule(this, inputDeps)));
        }

        internal static long EncodePosition(int3 pos)
        {
            var bytes = new byte[8];
            
            Array.Copy(BitConverter.GetBytes(pos.x), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(pos.z), 0, bytes, 4, 4);
            
            return BitConverter.ToInt64(bytes, 0);
        }
        
        internal static float Pressure(float volume, float moles, float temperature)
            => (moles * AtmosphericsSystem.GasConstant * temperature) / volume;
    }
}
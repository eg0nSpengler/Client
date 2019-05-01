using System;
using Atmospherics.Components;
using Atmospherics.Jobs;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Atmospherics
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AtmosphericsSystem : JobComponentSystem
    {
        public const float GasConstant = 8.31445984848f;
        public const float NodeVolume = 2;
        public const float NodeSurface = 10;
        public const float ContactArea = 2;
        public const float ContactCircumference = 6;
        public const float BaseFlux = 120f;
        public const float Drag = 0.996f;
        
        
        private ComponentGroup gasGroup;
        private NativeArray<int3> directions;
        private NativeMultiHashMap<long, Gas> gasses;
        private NativeMultiHashMap<long, MovedGas> movedGasses;
        private NativeMultiHashMap<long, Gas> postMovedGasses;

        private NativeArray<GasData> gasData;
        private int numGasses;
        
        protected override void OnCreateManager()
        {
            gasGroup = GetComponentGroup(
                ComponentType.ReadOnly<GridPosition>(),
                ComponentType.ReadOnly<Gas>());
            
            directions = new NativeArray<int3>(new []{
                new int3(0, 0, 1),new int3(1, 0, 0),new int3(0, 0, -1),new int3(-1, 0, 0),
            }, Allocator.Persistent);
        }

        protected override void OnDestroyManager()
        {
            if(gasData.IsCreated) gasData.Dispose();
            if(gasses.IsCreated) gasses.Dispose();
            if(movedGasses.IsCreated) movedGasses.Dispose();
            if(postMovedGasses.IsCreated) postMovedGasses.Dispose();
            if(directions.IsCreated) directions.Dispose();
        }

        public void RegisterGasses(GasData[] data)
        {
            gasData = new NativeArray<GasData>(data, Allocator.Persistent);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var currentGasses = gasGroup.CalculateLength() * directions.Length;

            if (currentGasses != numGasses)
            {
                if(gasses.IsCreated) gasses.Dispose();
                if(movedGasses.IsCreated) movedGasses.Dispose();
                if(postMovedGasses.IsCreated) postMovedGasses.Dispose();

                numGasses = currentGasses;
                gasses = new NativeMultiHashMap<long, Gas>(numGasses, Allocator.Persistent);
                movedGasses = new NativeMultiHashMap<long, MovedGas>(numGasses * 2, Allocator.Persistent);
                postMovedGasses = new NativeMultiHashMap<long, Gas>(numGasses, Allocator.Persistent);
            }
            else
            {
                gasses.Clear();
                movedGasses.Clear();
                postMovedGasses.Clear();
            }

            return new EqualizeTemperatureJob
            {
                gasses = gasData,
                gasMap = postMovedGasses,
            }.Schedule(this, new GasMoveJob
            {
                movedGasses = movedGasses,
                resultGasses = postMovedGasses.ToConcurrent(),
            }.Schedule(this, new GasFluxJob
            {
                directions = directions,
                gasMap = gasses,
                deltaTime = Time.deltaTime,
                gasses = gasData,
                movedGasses = movedGasses.ToConcurrent(),
            }.Schedule(this, new PartialPressureJob
            {
                gasses = gasData,
                gasMap = gasses,
            }.Schedule(this, new HashGridJob<Gas>
            {
                hashedGrid = gasses.ToConcurrent()
            }.Schedule(this, inputDeps)))));
        }

        internal static long EncodePosition(int3 pos)
        {
            return ((long)pos.x) | ((long)pos.z << 32);
        }
        
        internal static float Pressure(float volume, float moles, float temperature)
            => moles > 0 ? (moles * GasConstant * temperature) / volume : 0;
    }
}
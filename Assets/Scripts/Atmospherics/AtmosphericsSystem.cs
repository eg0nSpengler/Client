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
        public const float ContactArea = 2;
        public const float ContactCircumference = 6;
        public static readonly int3[] Directions = {
            new int3(0, 0, 1),new int3(1, 0, 0),new int3(0, 0, -1),new int3(-1, 0, 0),
        };
        
        
        private ComponentGroup gasGroup;
        private NativeArray<GasDefinition> gasConstants;
        private NativeMultiHashMap<long, Gas> gasses;
        
        protected override void OnCreateManager()
        {
            gasGroup = GetComponentGroup(
                ComponentType.ReadOnly<GridPosition>(),
                ComponentType.ReadOnly<Gas>());
            
            gasConstants = new NativeArray<GasDefinition>(new []
            {
                new GasDefinition
                {
                    name = new NativeString64("Nitrogen"),
                    molarMass = 0.028014f,
                    heatCapacity = 0.743f,
                    heatConductivity = 0.02583f,
                    viscosity = 1.78e-05f,
                }, 
                new GasDefinition
                {
                    name = new NativeString64("Oxygen"),
                    molarMass = 0.031998f,
                    heatCapacity = 0.659f,
                    heatConductivity = 0.02658f,
                    viscosity = 2.055e-05f,
                }, 
            }, Allocator.Persistent);
        }

        protected override void OnDestroyManager()
        {
            if(gasConstants.IsCreated) gasConstants.Dispose();
            if(gasses.IsCreated) gasses.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if(gasses.IsCreated) gasses.Dispose();

            var length = gasGroup.CalculateLength();
            gasses = new NativeMultiHashMap<long, Gas>(length * 4, Allocator.TempJob);

            return new AtmosphericsJob
            {
                gasMap = gasses,
                deltaTime = Time.deltaTime,
                gasses = gasConstants,
            }.Schedule(this, new HashGridJob<Gas>
            {
                hashedGrid = gasses.ToConcurrent()
            }.Schedule(this, inputDeps));
        }

        internal static long EncodePosition(int3 pos)
        {
            var bytes = new byte[8];
            
            Array.Copy(BitConverter.GetBytes(pos.x), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(pos.z), 0, bytes, 4, 4);
            
            return BitConverter.ToInt64(bytes, 0);
        }
        
        internal static float Pressure(float volume, float moles, float temperature)
            => moles > 0 ? (moles * GasConstant * temperature) / volume : 0;
    }
}
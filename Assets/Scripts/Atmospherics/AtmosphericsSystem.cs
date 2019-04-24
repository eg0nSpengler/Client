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
        
        
        private ComponentGroup gasGroup;
        private NativeArray<int3> directions;
        private NativeArray<GasDefinition> gasConstants;
        private NativeMultiHashMap<long, Gas> gasses;
        private NativeMultiHashMap<long, MovedGas> movedGasses;

        private int numGasses;
        
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
            
            directions = new NativeArray<int3>(new []{
                new int3(0, 0, 1),new int3(1, 0, 0),new int3(0, 0, -1),new int3(-1, 0, 0),
            }, Allocator.Persistent);
        }

        protected override void OnDestroyManager()
        {
            if(gasConstants.IsCreated) gasConstants.Dispose();
            if(gasses.IsCreated) gasses.Dispose();
            if(movedGasses.IsCreated) movedGasses.Dispose();
            if(directions.IsCreated) directions.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var currentGasses = gasGroup.CalculateLength() * directions.Length;

            if (currentGasses != numGasses)
            {
                if(gasses.IsCreated) gasses.Dispose();
                if(movedGasses.IsCreated) movedGasses.Dispose();

                numGasses = currentGasses;
                gasses = new NativeMultiHashMap<long, Gas>(numGasses, Allocator.Persistent);
                movedGasses = new NativeMultiHashMap<long, MovedGas>(numGasses, Allocator.Persistent);
            }
            else
            {
                gasses.Clear();
                movedGasses.Clear();
            }

            return new GasMoveJob
            {
                movedGasses = movedGasses,
            }.Schedule(this, new GasFluxJob
            {
                directions = directions,
                gasMap = gasses,
                deltaTime = Time.deltaTime,
                gasses = gasConstants,
                movedGasses = movedGasses.ToConcurrent(),
            }.Schedule(this, new PartialPressureJob
            {
                gasses = gasConstants,
                gasMap = gasses,
            }.Schedule(this, new HashGridJob<Gas>
            {
                hashedGrid = gasses.ToConcurrent()
            }.Schedule(this, inputDeps))));
        }

        internal static long EncodePosition(int3 pos)
        {
            return ((long)pos.x) | ((long)pos.z << 32);
        }
        
        internal static float Pressure(float volume, float moles, float temperature)
            => moles > 0 ? (moles * GasConstant * temperature) / volume : 0;
    }
}
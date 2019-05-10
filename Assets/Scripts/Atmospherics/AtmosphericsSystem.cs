using System;
using Atmospherics.Components;
using Atmospherics.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Atmospherics
{
    //[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class AtmosphericsSystem : JobComponentSystem
    {
        public const float GasConstant = 8.31445984848f;
        public const float NodeVolume = 2;
        public const float NodeSurface = 10;
        public const float ContactArea = 2;
        public const float ContactCircumference = 6;
        public const float BaseFlux = 120f;
        public const float Drag = 0.996f;


        private EntityArchetype gasArchetype;
        private EntityQuery gasGroup;
        private EntityQuery blockerGroup;

        private NativeArray<int3> directions;
        private NativeArray<GasData> gasData;

        private NativeMultiHashMap<long, Gas> gasses;
        private NativeMultiHashMap<long, GasBlocker> blockers;
        private NativeMultiHashMap<long, MovedGas> movedGasses;
        private NativeMultiHashMap<long, Gas> postMovedGasses;
        private NativeQueue<int3> createGasList;
        private EntityCommandBuffer commandBuffer;


        protected override void OnCreate()
        {
            gasArchetype = World.EntityManager.CreateArchetype(typeof(GridPosition), typeof(Gas));
            gasGroup = GetEntityQuery(
                ComponentType.ReadOnly<GridPosition>(),
                ComponentType.ReadOnly<Gas>());
            blockerGroup = GetEntityQuery(
                ComponentType.ReadOnly<GridPosition>(),
                ComponentType.ReadOnly<GasBlocker>());

            directions = new NativeArray<int3>(new[]
            {
                new int3(0, 0, 1), new int3(1, 0, 0), new int3(0, 0, -1), new int3(-1, 0, 0),
            }, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            if (directions.IsCreated) directions.Dispose();
            if (gasData.IsCreated) gasData.Dispose();

            if (gasses.IsCreated) gasses.Dispose();
            if (blockers.IsCreated) blockers.Dispose();
            if (movedGasses.IsCreated) movedGasses.Dispose();
            if (postMovedGasses.IsCreated) postMovedGasses.Dispose();

            if (createGasList.IsCreated) createGasList.Dispose();
            if (commandBuffer.IsCreated) commandBuffer.Dispose();
        }

        public void RegisterGasses(GasData[] data)
        {
            gasData = new NativeArray<GasData>(data, Allocator.Persistent);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Reallocate arrays if the sizes changed

            var currentGasses = gasGroup.CalculateLength() * directions.Length;
            var currentBlockers = blockerGroup.CalculateLength();

            if (!gasses.IsCreated || currentGasses != gasses.Capacity)
            {
                if (gasses.IsCreated) gasses.Dispose();
                if (movedGasses.IsCreated) movedGasses.Dispose();
                if (postMovedGasses.IsCreated) postMovedGasses.Dispose();

                gasses = new NativeMultiHashMap<long, Gas>(currentGasses, Allocator.Persistent);
                movedGasses = new NativeMultiHashMap<long, MovedGas>(currentGasses * 2, Allocator.Persistent);
                postMovedGasses = new NativeMultiHashMap<long, Gas>(currentGasses, Allocator.Persistent);
            }
            else
            {
                gasses.Clear();
                movedGasses.Clear();
                postMovedGasses.Clear();
            }

            if (!blockers.IsCreated || currentBlockers != blockers.Capacity)
            {
                if (blockers.IsCreated) blockers.Dispose();
                blockers = new NativeMultiHashMap<long, GasBlocker>(currentBlockers, Allocator.Persistent);
            }
            else
            {
                blockers.Clear();
            }


            // Schedule all the jobs

            if (createGasList.IsCreated) createGasList.Clear();
            else createGasList = new NativeQueue<int3>(Allocator.Persistent);

            if (commandBuffer.IsCreated)
            {
                commandBuffer.Playback(EntityManager);
                commandBuffer.Dispose();
            }
            commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            var hashBlockersHandle = new HashGridJob<GasBlocker>
            {
                hashedGrid = blockers.ToConcurrent()
            }.Schedule(this, inputDeps);
            var hashGassesHandle = new HashGridJob<Gas>
            {
                hashedGrid = gasses.ToConcurrent()
            }.Schedule(this, inputDeps);
            var partialPressureHandle = new PartialPressureJob
            {
                gasData = gasData,

                gasses = gasses,
            }.Schedule(this, hashGassesHandle);
            var gasFluxHandle = new GasFluxJob
            {
                deltaTime = Time.deltaTime,
                gasData = gasData,
                directions = directions,

                gasses = gasses,
                blockers = blockers,

                addList = createGasList.ToConcurrent(),
                movedGasses = movedGasses.ToConcurrent(),
                commandBuffer = commandBuffer.ToConcurrent(),
            }.Schedule(this, JobHandle.CombineDependencies(partialPressureHandle, hashBlockersHandle));
            var gasMoveHandle = new GasMoveJob
            {
                movedGasses = movedGasses,

                resultGasses = postMovedGasses.ToConcurrent(),
            }.Schedule(this, gasFluxHandle);
            var createGasHandle = new CreateGasJob
            {
                maxGasses = gasData.Length,
                gasArchetype = gasArchetype,

                addList = createGasList,
                movedGasses = movedGasses,

                commandBuffer = commandBuffer,
            }.Schedule(gasFluxHandle);
            var equalizeTemperatureHandle = new EqualizeTemperatureJob
            {
                gasses = gasData,
                gasMap = postMovedGasses,
            }.Schedule(this, gasMoveHandle);

            JobHandle.ScheduleBatchedJobs();
            return JobHandle.CombineDependencies(equalizeTemperatureHandle, createGasHandle);
        }


        internal static long EncodePosition(int3 pos)
        {
            return ((long) pos.x) | ((long) pos.z << 32);
        }
    }
}
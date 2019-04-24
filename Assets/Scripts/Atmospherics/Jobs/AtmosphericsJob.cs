using System.Collections.Generic;
using Atmospherics.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Atmospherics.Jobs
{
    public struct AtmosphericsJob : IJobProcessComponentData<GridPosition, Gas>
    {
        [ReadOnly] public NativeArray<GasDefinition> gasses;
        [ReadOnly] public NativeMultiHashMap<long, Gas> gasMap;
        [ReadOnly] public float deltaTime;

        public void Execute([ReadOnly] ref GridPosition position, ref Gas node)
        {
            if (node.moles == 0)
            {
                // TODO remove the gas entity when empty
                return;
            }
            
            var pressure = GetPartialPressureAt(AtmosphericsSystem.EncodePosition(position.value), node.moles);

            //Debug.Log($"{position.value} -> {pressure}");

            var flux = node.flux;

            for (var i = 0; i < AtmosphericsSystem.Directions.Length; i++)
            {
                var p = GetPartialPressureAt(AtmosphericsSystem.EncodePosition(position.value + AtmosphericsSystem.Directions[i]), node.id);
                var force = (pressure - p) / AtmosphericsSystem.ContactArea;
                var acceleration = force / node.moles * gasses[node.id].molarMass;
                Debug.Log($"{position.value} -> {(position.value+AtmosphericsSystem.Directions[i])}: {pressure} {p} -> {force}");

                flux[i] += 0.5f * acceleration * deltaTime;
                var amount = 2 * (flux[i] * deltaTime + 0.5f * acceleration * deltaTime * deltaTime);
                //Move(amount, gas, ref tiles[0], ref tiles[1]);
                
                flux[i] *= 0.95f;
                if (flux[i] < 0) flux[i] = 0;
            }

            node.flux = flux;
        }

        private float GetPressureAt(long pos)
        {
            var totalMoles = 0f;
            var totalEnergy = 0f;
            var totalCapacity = 0f;
            if (!gasMap.TryGetFirstValue(pos, out var gas, out var it)) return 0;
            do
            {
                totalMoles += gas.moles;
                totalEnergy += gas.energy;
                totalCapacity += gasses[gas.id].heatCapacity * gas.moles;
            }
            while (gasMap.TryGetNextValue(out gas, ref it));

            return AtmosphericsSystem.Pressure(AtmosphericsSystem.NodeVolume, totalMoles, totalEnergy / totalCapacity);
        }

        private float GetPartialPressureAt(long pos, float moles)
        {
            var totalMoles = 0f;
            var totalEnergy = 0f;
            var totalCapacity = 0f;
            if (!gasMap.TryGetFirstValue(pos, out var gas, out var it)) return 0;
            do
            {
                totalMoles += gas.moles;
                totalEnergy += gas.energy;
                totalCapacity += gasses[gas.id].heatCapacity * gas.moles;
            }
            while (gasMap.TryGetNextValue(out gas, ref it));

            return moles/totalMoles * AtmosphericsSystem.Pressure(AtmosphericsSystem.NodeVolume, totalMoles, totalEnergy / totalCapacity);
        }
    }
}
using System.Collections.Generic;
using Atmospherics.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Atmospherics.Jobs
{
    public struct AtmosphericsJob : IJobProcessComponentData<GridPosition, AtmosphericsNode>
    {
        [ReadOnly] public NativeMultiHashMap<long, AtmosphericsNode> nodeMap;
        [ReadOnly] public NativeMultiHashMap<long, Gas> gasMap;

        public void Execute([ReadOnly] ref GridPosition position, ref AtmosphericsNode node)
        {
            var pos = AtmosphericsSystem.EncodePosition(position.value);
            var pressure = GetPressureAt(pos);

            //Debug.Log($"{position.value} -> {pressure}");

            var flux = node.flux;

            for (var i = 0; i < AtmosphericsSystem.Directions.Length; i++)
            {
                var p = 
                    GetPressureAt(AtmosphericsSystem.EncodePosition(position.value + AtmosphericsSystem.Directions[i]));
                //flux[i] += (p - pressure) / AtmosphericsSystem.NodeSurface;
                
                //Debug.Log(position.value + " "+pressure +" "+p+" "+flux[i]);
            }

            node.flux = flux;
        }

        private float GetPressureAt(long pos)
        {
            if (!nodeMap.TryGetFirstValue(pos, out var node, out _)) return 0;

            var totalMoles = 0f;
            if (!gasMap.TryGetFirstValue(pos, out var gas, out var it)) return 0;
            do
            {
                totalMoles += gas.moles;
            }
            while (gasMap.TryGetNextValue(out gas, ref it));

            return AtmosphericsSystem.Pressure(AtmosphericsSystem.NodeVolume, totalMoles, node.temperature);
        }
    }
}
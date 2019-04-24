using Atmospherics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Atmospherics.Jobs
{
    public struct GasMoveJob : IJobProcessComponentData<GridPosition, Gas>
    {
        [ReadOnly] public NativeMultiHashMap<long, MovedGas> movedGasses;

        [BurstCompile]
        public void Execute([ReadOnly] ref GridPosition position, ref Gas node)
        {
            var pos = AtmosphericsSystem.EncodePosition(position.value);
            if (!movedGasses.TryGetFirstValue(pos, out var moved, out var it)) return;
            do
            {
                if(moved.gasIndex != node.id) continue;
                node.moles += moved.amount;
                node.energy += moved.energy;
            }
            while (movedGasses.TryGetNextValue(out moved, ref it));
        }
    }

    public struct MovedGas
    {
        public readonly byte gasIndex;
        public readonly float amount;
        public readonly float energy;

        public MovedGas(byte gasIndex, float amount, float energy)
        {
            this.gasIndex = gasIndex;
            this.amount = amount;
            this.energy = energy;
        }
    }
}
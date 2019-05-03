using Atmospherics.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace Atmospherics
{
    public class GridPositionProxy : ComponentDataProxy<GridPosition>
    {
        private void Awake()
        {
            var position = Value;
            position.value = new int3(transform.position);
            Value = position;
        }
    }
}
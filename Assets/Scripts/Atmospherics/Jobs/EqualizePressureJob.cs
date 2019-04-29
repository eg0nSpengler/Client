using Atmospherics.Components;
using Unity.Collections;
using Unity.Entities;

namespace Atmospherics.Jobs
{
    public class EqualizePressureJob : IJobProcessComponentData<GridPosition, Gas>
    {
        public void Execute([ReadOnly] ref GridPosition position, ref Gas node)
        {
            
        }
    }
}
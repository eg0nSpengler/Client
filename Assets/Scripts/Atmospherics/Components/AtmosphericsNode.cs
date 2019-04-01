using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Atmospherics.Components
{
    [Serializable]
    public struct AtmosphericsNode : IComponentData
    {
        public float temperature;
        public float4 flux;
        public float4 loss;

        public AtmosphericsNode(float temp)
        {
            temperature = temp;
            flux = float4.zero;
            loss = float4.zero;
        }
    }
}
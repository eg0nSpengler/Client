using System.Globalization;
using Unity.Entities;
using Unity.Mathematics;

namespace Atmospherics.Components
{
    public struct Gas : IComponentData
    {
        /// <summary>
        /// Shorthand to whether or not this node actually contains gas
        /// </summary>
        public bool IsCreated => created == 1;
        
        /// <summary>
        /// Id or index of the gas type of this node
        /// </summary>
        public readonly byte id;
        
        /// <summary>
        /// The total number of moles of this gas type at this position
        /// </summary>
        public float moles;
        
        /// <summary>
        /// The total number of joules in this gas type at this position
        /// </summary>
        public float energy;
        
        /// <summary>
        /// Flux is like velocity but in 4 directions at once
        /// </summary>
        public float4 flux;
        
        /// <summary>
        /// Temporary value for the partial pressure, not to be read by others
        /// </summary>
        internal float partialPressure;

        // TODO maybe remove once full lifecycle is done
        private readonly byte created;
        
        /// <summary>
        /// Constructor for a gas node
        /// </summary>
        /// <param name="id">The gas type</param>
        /// <param name="moles">The number of moles</param>
        /// <param name="energy">The total energy in joules</param>
        public Gas(byte id, float moles, float energy)
        {
            this.created = 1;
            this.id = id;
            this.moles = moles;
            this.energy = energy;
            this.flux = float4.zero;
            this.partialPressure = 0;
        }

        public override string ToString()
        {
            return
                $"[GAS {id.ToString()}, {energy.ToString(CultureInfo.InvariantCulture)} joules, {moles.ToString(CultureInfo.InvariantCulture)} moles, {partialPressure.ToString(CultureInfo.InvariantCulture)} pascal]";
        }
    }
}
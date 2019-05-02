using Unity.Entities;

namespace Atmospherics
{
    public struct GasBlocker : IComponentData { }
    public class GasBlockerProxy : ComponentDataProxy<GasBlocker> { }
}
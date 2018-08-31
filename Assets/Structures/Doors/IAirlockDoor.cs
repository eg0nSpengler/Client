namespace Structures.Doors
{
    public interface IAirlockDoor
    {
        AirlockState state { get;  }
        void SetOpen(bool open);
    }
}
namespace Enemies.Interfaces
{
    public interface ITrigger
    {
        bool IsTriggered { get; }
        void CheckTrigger();
    }
}

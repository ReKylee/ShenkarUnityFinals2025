using System;

namespace Collectables.Counter
{
    public interface ICounterModel
    {
        int Count { get; }
        event Action<int> OnCountChanged;
        void Increment();
        void Reset();
    }
}

using System;

namespace Collectables.Counter
{
    public class CounterModel : ICounterModel
    {
        public int Count { get; private set; }

        public event Action<int> OnCountChanged;

        public void Increment()
        {
            Count++;
            OnCountChanged?.Invoke(Count);
        }

        public void Reset()
        {
            Count = 0;
            OnCountChanged?.Invoke(Count);
        }
    }
}

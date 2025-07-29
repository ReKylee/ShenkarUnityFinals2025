using System;
using System.Collections.Generic;
using System.Linq;
using Enemies.Interfaces;

namespace Enemies.Core
{
    public enum TriggerCondition
    {
        Any,
        All
    }

    public class TriggerGroup
    {
        private readonly TriggerCondition _condition;
        private readonly Action _onEvaluate;
        private readonly List<ITrigger> _triggers;

        public TriggerGroup(IEnumerable<ITrigger> triggers, TriggerCondition condition, Action onEvaluate)
        {
            _triggers = triggers.ToList();
            _condition = condition;
            _onEvaluate = onEvaluate;
        }


        public void Update()
        {
            foreach (ITrigger trigger in _triggers)
            {
                trigger.CheckTrigger();
            }

            Evaluate();
        }

        private void Evaluate()
        {
            bool isActivated = _condition switch
            {
                TriggerCondition.Any => _triggers.Any(t => t.IsTriggered),
                TriggerCondition.All => _triggers.All(t => t.IsTriggered),
                _ => false
            };

            if (isActivated)
            {
                _onEvaluate?.Invoke();
            }
        }
    }
}

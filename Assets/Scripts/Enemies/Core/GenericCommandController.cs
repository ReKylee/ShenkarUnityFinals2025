using System.Collections.Generic;
using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Core
{
    // Generic controller for handling triggers and commands
    public class GenericCommandController<TCommand> : MonoBehaviour where TCommand : ICommand
    {
        [SerializeField] private TriggerCondition defaultTriggerCondition = TriggerCondition.Any;
        private List<TriggerGroup> _triggerGroups;
        private void Awake()
        {
            _triggerGroups = new List<TriggerGroup>();

            // Dynamically assign triggers and commands
            var triggers = GetComponents<ITrigger>();
            foreach (ITrigger trigger in triggers)
            {
                var commands = new List<TCommand>(GetComponents<TCommand>());
                TriggerGroup triggerGroup = new(new[] { trigger }, defaultTriggerCondition,
                    () => ExecuteCommands(commands));

                _triggerGroups.Add(triggerGroup);
            }
        }

        private void Update()
        {
            foreach (TriggerGroup triggerGroup in _triggerGroups)
            {
                triggerGroup.Update();
            }
        }

        private void ExecuteCommands(IEnumerable<TCommand> commands)
        {
            foreach (TCommand command in commands)
            {
                command.Execute();
            }
        }
    }
}

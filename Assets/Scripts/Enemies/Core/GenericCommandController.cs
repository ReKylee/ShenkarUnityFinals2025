using System.Collections.Generic;
using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Core
{
    // Generic controller for handling triggers and commands
    public class GenericCommandController<TCommand> : MonoBehaviour where TCommand : ICommand
    {
        [SerializeField] private TriggerCondition defaultTriggerCondition = TriggerCondition.Any;
        private TriggerGroup _triggerGroup;
        private List<TCommand> _commands;
        private bool _noTriggers;
        private bool _noCommands;
        private void Awake()
        {
            var triggers = GetComponents<ITrigger>();
            _commands = new List<TCommand>(GetComponents<TCommand>());

            if (triggers.Length == 0)
            {
                _noTriggers = true;
                return;
            }
            if (_commands.Count == 0)
            {
                _noCommands = true;
                return;
            }

            _triggerGroup = new TriggerGroup(triggers, defaultTriggerCondition,
                () => ExecuteCommands(_commands));
        }

        private void Update()
        {
            if (_noCommands)
            {
                Debug.LogWarning("No commands found in GenericCommandController. Ensure you have added command components.");
                return;
            }
            
            if (_noTriggers)
            {
                ExecuteCommands(_commands);
                return;
            }

            _triggerGroup.Update();
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

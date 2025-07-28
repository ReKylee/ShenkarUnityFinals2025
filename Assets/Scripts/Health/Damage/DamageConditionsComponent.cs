using System;
using System.Collections.Generic;
using System.Linq;
using Health.Interfaces;
using UnityEngine;

namespace Health.Damage
{
    [Serializable]
    public abstract class ConditionNode
    {
        public abstract bool Evaluate(GameObject damager);
    }

    [Serializable]
    public class ConditionLeaf : ConditionNode
    {
        [SerializeField] public MonoBehaviour conditionBehaviour;
        public override bool Evaluate(GameObject damager)
        {
            IDamageCondition cond = conditionBehaviour as IDamageCondition;
            return cond != null && cond.CanBeDamagedBy(damager);
        }
    }

    [Serializable]
    public class AndCondition : ConditionNode
    {
        [SerializeReference] public List<ConditionNode> children = new();
        public override bool Evaluate(GameObject damager)
            => children != null && children.All(c => c != null && c.Evaluate(damager));
    }

    [Serializable]
    public class OrCondition : ConditionNode
    {
        [SerializeReference] public List<ConditionNode> children = new();
        public override bool Evaluate(GameObject damager)
            => children != null && children.Any(c => c != null && c.Evaluate(damager));
    }

    [Serializable]
    public class NotCondition : ConditionNode
    {
        [SerializeReference] public ConditionNode child;
        public override bool Evaluate(GameObject damager)
            => child != null && !child.Evaluate(damager);
    }

    [DisallowMultipleComponent]
    public class DamageConditionsComponent : MonoBehaviour
    {
        [SerializeReference] public ConditionNode rootCondition;

        public void Reset()
        {
            rootCondition = new ConditionLeaf();
        }
        public bool CanBeDamagedBy(GameObject damager)
            => rootCondition != null && rootCondition.Evaluate(damager);
    }
}

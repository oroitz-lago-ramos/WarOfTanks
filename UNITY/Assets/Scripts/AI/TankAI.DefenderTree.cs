using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.AI.BehaviourTree;
using BehaviourTreeController = WarOfTanks.AI.BehaviourTree.BehaviourTree;

namespace WarOfTanks.AI
{
    public partial class TankAI
    {
        private BehaviourTreeController BuildDefenderTree()
        {
            Selector root = new Selector(new List<IBehaviourNode>
            {
                // Low HP -> retreat to spawn
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => _blackboard != null && _blackboard.hpRatio < 0.3f),
                    new ActionNode(MoveToSpawn)
                }),

                // Enemy visible near zone -> intercept and attack
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() =>
                        _blackboard != null &&
                        _blackboard.closestEnemy != null &&
                        _blackboard.closestEnemy.target != null &&
                        _zone != null &&
                        Vector2.Distance(_blackboard.closestEnemy.target.transform.position, _zone.transform.position) <= 5f),
                    new ActionNode(MoveToIntercept),
                    new ActionNode(AttackClosestEnemy)
                }),

                // Own team is capturing or controlling the zone -> guard perimeter
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() =>
                        _blackboard != null &&
                        _zone != null &&
                        _zone.controllingTeam == (int)_blackboard.teamId),
                    new ActionNode(MoveToZonePerimeter)
                }),

                // Default -> patrol between zone and spawn
                new ActionNode(PatrolZoneToSpawn)
            });

            return new BehaviourTreeController(root);
        }
    }
}

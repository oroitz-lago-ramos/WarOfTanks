using System;
using WarOfTanks.Enums;
using WarOfTanks.Interfaces;

namespace WarOfTanks.AI.BehaviourTree
{
    public class BehaviourTree
    {
        private readonly IBehaviourNode _root;

        public BehaviourTree(IBehaviourNode root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public NodeStatus Tick()
        {
            return _root.Tick();
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.AI.BehaviourTree;
using WarOfTanks.Enums;

namespace WarOfTanks.AI
{
    public partial class TankAI
    {
        private NodeStatus MoveToZone()
        {
            if (_zone == null || _grid == null) 
                return NodeStatus.Failure;

            if (IsInZone())
            {
                return NodeStatus.Success;
            }

            Vector2Int targetGrid = _grid.WorldToGridPosition(_zone.transform.position);

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;
            
            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        private NodeStatus MoveToSpawn()
        {
            if (_tank == null || _grid == null) 
                return NodeStatus.Failure;
            
            Vector2Int targetGrid = _grid.WorldToGridPosition(_tank.SpawnPosition);
            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);

            if (currentGrid == targetGrid)
            {
                HealthSystem healthSystem = _tank.GetComponent<HealthSystem>();
                healthSystem?.RestoreHealth();
                return NodeStatus.Success;
            }
            
            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        private NodeStatus MoveToFiringRange()
        {
            if (_blackboard == null || _blackboard.closestEnemy == null || _blackboard.closestEnemy.target == null || _grid == null || _tank == null)
                return NodeStatus.Failure;

            Tank enemy = _blackboard.closestEnemy.target;
            Vector2 enemyPosition = enemy.transform.position;
            Vector2 currentPosition = transform.position;

            if (Vector2.Distance(currentPosition, enemyPosition) <= _tank.FiringRange)
                return NodeStatus.Success;

            Vector2Int targetGrid = _grid.WorldToGridPosition(enemy.transform.position);

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        private NodeStatus MoveToZonePerimeter()
        {
            if (_zone == null || _grid == null || _blackboard == null)
                return NodeStatus.Failure;

            Vector3 offset = _blackboard.teamId == ETankTeam.PLAYER
                ? Vector3.left * 2f
                : Vector3.right * 2f;

            Vector3 perimeterPosition = _zone.transform.position + offset;
            Vector2Int targetGrid = _grid.WorldToGridPosition(perimeterPosition);
            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);

            if (currentGrid == targetGrid)
                return NodeStatus.Success;

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        private NodeStatus MoveToIntercept()
        {
            if (_blackboard == null || _blackboard.closestEnemy == null || _blackboard.closestEnemy.target == null || _grid == null || _tank == null )
                return NodeStatus.Failure;
            
            Tank enemy = _blackboard.closestEnemy.target;
            Vector3 interceptPosition = enemy.transform.position;
            Vector2Int targetGrid = _grid.WorldToGridPosition(interceptPosition);

            if (Vector2.Distance(transform.position, enemy.transform.position) <= _tank.FiringRange)
                return NodeStatus.Success;

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        private NodeStatus PatrolToEnemySpawn()
        {
            if (_blackboard == null || _grid == null || _tank == null || GameManager.Instance == null)
                return NodeStatus.Failure;

            List<Tank> allTanks = GameManager.Instance.GetAllTanks();

            Tank enemyTank = null;
            foreach (Tank tank in allTanks)
            {
                if (tank == null)
                    continue;

                if (tank.TeamId == _blackboard.enemyTeamId)
                {
                    enemyTank = tank;
                    break;
                }
            }
            if (enemyTank == null)
                return NodeStatus.Failure;

            Vector3 enemySpawnPosition = enemyTank.SpawnPosition;
            Vector2Int targetGrid = _grid.WorldToGridPosition(enemySpawnPosition);
            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);

            if (currentGrid == targetGrid)
                return NodeStatus.Success;

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        private NodeStatus PatrolZoneToSpawn()
        {
            if (_blackboard == null || _tank == null || _grid == null || _zone == null)
                return NodeStatus.Failure;

            Vector3 offset = _blackboard.teamId == ETankTeam.PLAYER
                ? Vector3.left * 2f
                : Vector3.right * 2f;

            Vector3 targetPosition = IsInZone()
                ? _tank.SpawnPosition
                : _zone.transform.position + offset;

            Vector2Int targetGrid = _grid.WorldToGridPosition(targetPosition);
            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);

            if (currentGrid == targetGrid)
                return NodeStatus.Success;

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        private NodeStatus AttackClosestEnemy()
        {
            if (_blackboard == null || _blackboard.closestEnemy == null || _blackboard.closestEnemy.target == null || _tank == null || _tank.Turret == null)
                return NodeStatus.Failure;

            Tank enemy = _blackboard.closestEnemy.target;
            Vector2 targetPosition = enemy.transform.position;

            _tank.Turret.RotateTo(targetPosition);

            if (_tank.Turret.CanFire && _tank.Turret.IsAimedAt(targetPosition, TankConstants.TURRET_TOLERANCE_ANGLE))
            {
                _tank.Turret.Fire();
            }

            return NodeStatus.Success;
        }

        private NodeStatus SignalEnemyVisible()
        {
            return NodeStatus.Running;
        }
    }
}

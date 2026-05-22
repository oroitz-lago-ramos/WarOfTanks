using UnityEngine;
using System.Collections.Generic;
using WarOfTanks.Enums;

namespace WarOfTanks.AI
{
    public class VisionSystem : MonoBehaviour 
    {
        [SerializeField] private float _detectionRadius;
        [SerializeField] private float _fieldOfViewAngle;

        public List<DetectionResult> Scan(List<Tank> allTanks, ETankTeam ownerTeamId)
        {
            // TODO: implement real detection in issue #19.
            return new List<DetectionResult>();
        }


        public DetectionResult GetClosestTarget(List<DetectionResult> results)
        {
            // TODO: implement real target selection in issue #19.
            return null;
        }
    }
}

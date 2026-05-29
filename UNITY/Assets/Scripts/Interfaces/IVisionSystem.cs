using System.Collections.Generic;
using WarOfTanks.AI;
using WarOfTanks.Enums;

public interface IVisionSystem
{
    List<DetectionResult> Scan(List<Tank> allTanks, ETankTeam ownerTeamId);
    DetectionResult GetClosestTarget(List<DetectionResult> results);
}
namespace WarOfTanks.AI
{
    /// <summary>Stores the result of detecting one target tank.</summary>
    public class DetectionResult
    {
        public Tank target;
        public float distance;
        public float angle;
        public bool isInLineOfSight;

        public DetectionResult(Tank target, float distance, float angle, bool isInLineOfSight)
        {
            this.target = target;
            this.distance = distance;
            this.angle = angle;
            this.isInLineOfSight = isInLineOfSight;
        }
    }
}

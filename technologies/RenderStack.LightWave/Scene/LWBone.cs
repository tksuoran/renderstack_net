using RenderStack.Math;

namespace RenderStack.LightWave
{
    public struct Limits
    {
        public double Min;
        public double Max;

        public Limits(double min, double max)
        {
            Min = min;
            Max = max;
        }
    }
    public class LWBone : LWItem
    {
        public int      IKAnchor;       
        public int      GoalObject;     //  For IK
        public Vector3  RestPosition;
        public Vector3  RestDirection;  //  Angles in degrees: h p b
        public double   RestLength;
        public double   Strength;
        public int      ScaleBoneStrength;
        public int      FalloffType;
        public int      HController;
        public int      PController;
        public int      BController;
        public Limits   HLimits;
        public Limits   PLimits;
        public Limits   BLimits;
        public int      LimitedRange;
        public Limits   Range;
        public int      Normalization;
        public string   WeightMapName;
        public int      WeightMapOnly;
    }
}

using System;

namespace Common
{
    public static class MathF
    {
        public static Func<double, float> Cos = angleR => (float) Math.Cos(angleR);
        public static Func<double, float> Sin = angleR => (float) Math.Sin(angleR);
        public static float PI => (float) Math.PI;
    }
}

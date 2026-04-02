using System;

namespace LH.Domain {
    public static class RandomExtension {
        /// <summary> float [-1f .. 1f] </summary>
        public static float NextFloat11(this Random random) => (float)(random.NextDouble() * 2f - 1f);

        /// <summary> float [0 .. max] </summary>
        public static float NextFloat(this Random random, float max) => (float)(random.NextDouble() * max);

        /// <summary> float [0 .. 1f] </summary>
        public static float NextFloat(this Random random) => (float)random.NextDouble();

        /// <summary> float [min .. max] </summary>
        public static float NextFloat(this Random random, float min, float max) => (float)(random.NextDouble() * (max - min) + min);
    }
}
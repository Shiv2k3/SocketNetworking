using System;

namespace Core.Util
{
    public class OL
    {
        public static float Time => UnityEngine.Time.realtimeSinceStartup;
        public static byte FloatToByte(float x) => (byte)((Math.Clamp(x, -1, 1) + 1) / 2f * byte.MaxValue);
        public static float ByteToFloat(byte x) => x / byte.MaxValue * 2 - 1;

    }
}

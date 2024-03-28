using System;

namespace Core.Util
{
    public class OL
    {
        public static float Time => DateTime.Now.Second + DateTime.Now.Millisecond / 1000f;
    }
}

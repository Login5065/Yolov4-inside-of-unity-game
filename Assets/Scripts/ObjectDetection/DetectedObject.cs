using System.Runtime.InteropServices;

namespace DobreKody.Basic
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DetectedObject 
    {
        public readonly float x, y, w, h;
        public readonly uint classIndex;
        public readonly float score;

        // sizeof(Detection)
        public static int Size = 6 * sizeof(int);   

        // String formatting
        public override string ToString()
            => $"({x},{y})-({w}x{h}):{classIndex}({score})";
    }
}

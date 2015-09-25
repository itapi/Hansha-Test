using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopDuplication
{
    public struct ScreenFrame
    {
        public ScreenFrameRectangle Boundaries;
        public ScreenFrameRectangle[] ModifiedRegions;
        public ScreenFrameRegion[] MovedRegions;
        public byte[] NewPixels;
        public byte[] PreviousPixels;
    }

    public struct ScreenFrameRectangle
    {
        public int Bottom;
        public int Left;
        public int Right;
        public int Top;
    }
    public struct ScreenFrameRegion
    {
        public ScreenFrameRectangle Destination;
        public int X;
        public int Y;
    }
}

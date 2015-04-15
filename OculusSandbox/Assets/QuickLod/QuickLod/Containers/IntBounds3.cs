using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickLod.Containers
{
    public struct IntBounds3
    {
        public int X;

        public int Y;

        public int Z;

        public int SizeX;

        public int SizeY;

        public int SizeZ;

        public bool IsInX(int value)
        {
            var valMinX = value - X;
            return valMinX >= 0 && valMinX <= SizeX;
        }

        public bool IsInY(int value)
        {
            var valMinY = value - Y;
            return valMinY >= 0 && valMinY <= SizeY;
        }

        public bool IsInZ(int value)
        {
            var valMinZ = value - Z;
            return valMinZ >= 0 && valMinZ <= SizeZ;
        }

        public bool IsInBoundary(int x, int y, int z)
        {
            return this.IsInX(x) && this.IsInY(y) && this.IsInZ(z);
        }
    }
}

using UnityEngine;

namespace App.Utility
{
    public static class VectorExt
    {
        public static int CompareLength(this Vector2Int a, Vector2Int b)
        {
            var aSqr = a.sqrMagnitude;
            var bSqr = b.sqrMagnitude;
            if (aSqr != bSqr)
                return aSqr.CompareTo(bSqr);
            if (a.x != b.x)
                return a.x.CompareTo(b.x);
            return a.y.CompareTo(b.y);
        }
        
        public static bool IsNextTo(this Vector2Int self, Vector2Int index)
        {
            var diff = index - self;
            return diff.x == 0 && (diff.y is <= 1 and >= -1) ||
                   (diff.x is <= 1 and >= -1) && diff.y == 0;
        }
    }
}
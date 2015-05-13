using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleScapesEngine
{
    public static class GeometryUtils
    {
        public static bool IsInSphere(Vector3Int center, int radius, Vector3Int testPosition)
        {
            double distance = Math.Pow(center.x - testPosition.x, 2) + Math.Pow(center.y - testPosition.y, 2) + Math.Pow(center.z - testPosition.z, 2);
            return distance <= Math.Pow(radius, 2);
        }
    }
}

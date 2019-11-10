using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetviewRipper
{
    public class StreetviewQualityDef
    {
        public void Set(int _z, int _x, int _y, int _s, int _a)
        {
            zoom = _z;
            x = _x;
            y = _y;
            size = _s;
            acc = _a;
        }
        public int zoom;
        public int y;
        public int x;
        public int size;
        public int acc;
    }
}

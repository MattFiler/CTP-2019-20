using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetviewRipper
{
    public class StreetviewQualityDef
    {
        public void Set(int _z, int _a)
        {
            zoom = _z;
            acc = _a;
        }
        public int zoom; //The image zoom level (similar to a map tile system)
        public int acc; //The accurary level for ground cuts
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothConnection;
using Alkoo.Resources;

namespace Alkoo
{
    class AlcoMeter
    {
        private const float base_reading = 55.0F; //sober
        private const float pcnt_reading = 828.0F;

        public int reading { get; set; }

        public float reading2BAC()
        {
            return Math.Max(((float)reading - base_reading) / pcnt_reading, 0.0F);
        }

    }
}

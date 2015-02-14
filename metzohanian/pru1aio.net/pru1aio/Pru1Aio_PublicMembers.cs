using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace Pru1Aio
{

    public partial class Pru1Aio
    {
        public List<int> DroppedBuffers
        {
            get
            {
                return _DroppedBuffers;
            }
        }

        public bool IsCapturing
        {
            get
            {
                return _IsCapturing;
            }
        }

    }

}
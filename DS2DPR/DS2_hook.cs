using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyHook;

namespace DS2DPR
{
    public class DS2_hook : PHook
    {
        //private PHPointer WorldChrBase;
        //private PHPointer ChrData1;
        private PHPointer ChrData2;

        public DS2_hook() : base(5000, 5000, p => p.ProcessName == "DarkSoulsII")
        {
            //WorldChrBase = CreateBasePointer((IntPtr)0x141D151B0, 0);
            //ChrData1 = CreateChildPointer(WorldChrBase, 0x68);
            ChrData2 = RegisterRelativeAOB("48 8B 05 ? ? ? ? 48 8B 58 38 48 85 DB 74 ? F6", 3, 7, 0, 0xD0, 0x490);
        }

        public int Death
        {
            get => ChrData2.ReadInt32(0x1A4);
        }

        // maybe for drp
        public int slLvl
        {
            get => ChrData2.ReadInt32(0xD0);
        }


    }
}

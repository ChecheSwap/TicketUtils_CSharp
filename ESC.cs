using System;
using System.Collections.Generic;
using System.Text;

namespace SetSoftware_Enterprise.UTILS.TICKETS
{
    public static class ESC
    {
        //// ESC/POS Commands
        public static readonly byte[] INIT = { 0x1B, 0x40 }; // Initialize printer
        public static readonly byte[] ALIGN_LEFT = { 0x1B, 0x61, 0x00 };
        public static readonly byte[] ALIGN_CENTER = { 0x1B, 0x61, 0x01 };
        public static readonly byte[] ALIGN_RIGHT = { 0x1B, 0x61, 0x02 };
        public static readonly byte[] BOLD_ON = { 0x1B, 0x45, 0x01 };
        public static readonly byte[] BOLD_OFF = { 0x1B, 0x45, 0x00 };
        public static readonly byte[] DOUBLE_HEIGHT_WIDTH_ON = { 0x1D, 0x21, 0x11 };
        public static readonly byte[] DOUBLE_WIDTH_ON = { 0x1D, 0x21, 0x10 };
        public static readonly byte[] NORMAL_SIZE = { 0x1D, 0x21, 0x00 };
        public static readonly byte[] CUT_PAPER = { 0x1D, 0x56, 0x00 };
        public static readonly byte[] FEED_LINE = { 0x0A };
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TaintAnalysis
{
    public class MergeBehavior
    {
        public int MinMergedCoins { get; set; } = 2;
        public int MaxMergedCoins { get; set; } = 5;
    }
}

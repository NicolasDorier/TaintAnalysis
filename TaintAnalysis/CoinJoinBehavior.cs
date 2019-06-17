using System;
using System.Collections.Generic;
using System.Text;

namespace TaintAnalysis
{
    public class CoinJoinBehavior
    {
        public int MinAnonymitySet { get; set; } = 3;
        public int MaxAnonymitySet { get; set; } = 6;
    }
}

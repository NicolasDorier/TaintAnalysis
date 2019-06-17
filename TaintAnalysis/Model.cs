using System;
using System.Collections.Generic;
using System.Text;

namespace TaintAnalysis
{
    public class Model
    {
        public CoinJoinBehavior CoinJoinBehavior { get; set; } = new CoinJoinBehavior();
        public MergeBehavior MergeBehavior { get; set; } = new MergeBehavior();
    }
}

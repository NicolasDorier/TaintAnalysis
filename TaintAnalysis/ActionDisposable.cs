using System;
using System.Collections.Generic;
using System.Text;

namespace TaintAnalysis
{
    public class ActionDisposable : IDisposable
    {
        private Action act;

        public ActionDisposable(Action act)
        {
            this.act = act;
        }

        public static ActionDisposable Create (Action act)
        {
            return new ActionDisposable(act);
        }

        public void Dispose()
        {
            act();
        }
    }
}

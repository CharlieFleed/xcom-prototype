using System.Collections.Generic;

namespace HSM
{
    public class UpdateResult
    {
        public List<ActionBase> actions = new List<ActionBase>();
        public Transition transition;
        public int transitionLevel;
        public bool stable = true;
    }
}
using System.Collections.Generic;

namespace HSM
{
    public class UpdateResult
    {
        public List<Action> actions = new List<Action>();
        public Transition transition;
        public int transitionLevel;
    }
}
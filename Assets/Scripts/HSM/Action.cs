namespace HSM
{
    public abstract class ActionBase
    {
        public abstract void Execute();
    }

    public delegate void ExecuteDelegate();

    public class Action : ActionBase
    {
        ExecuteDelegate _executeDel;

        public Action(ExecuteDelegate executeDel) : base()
        {
            _executeDel = executeDel;
        }

        public override void Execute()
        {
            _executeDel();
        }
    }
}
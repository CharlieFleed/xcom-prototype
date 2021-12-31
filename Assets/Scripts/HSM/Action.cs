namespace HSM
{
    public abstract class Action
    {
        public abstract void Execute();
    }

    public delegate void ExecuteDelegate();

    public class DelegateAction : Action
    {
        ExecuteDelegate _executeDel;

        public DelegateAction(ExecuteDelegate executeDel) : base()
        {
            _executeDel = executeDel;
        }

        public override void Execute()
        {
            _executeDel();
        }
    }
}
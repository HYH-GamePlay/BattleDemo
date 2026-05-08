using Battle.CombatInfo.Action;

namespace Battle.Behavior.Action.Executors
{
    public abstract class Executor
    {
        public void Enter(in ActionExecutionContext context, InstructData data)
        {
            OnEnter(context, data);
        }

        public void Execute(in ActionExecutionContext context, InstructData data)
        {
            OnExecute(context, data);
        }

        public void Exit(in ActionExecutionContext context, InstructData data)
        {
            OnExit(context, data);
        }

        protected virtual void OnEnter(in ActionExecutionContext context, InstructData data)
        {
        }

        protected virtual void OnExecute(in ActionExecutionContext context, InstructData data)
        {
        }

        protected virtual void OnExit(in ActionExecutionContext context, InstructData data)
        {
        }
    }

    public abstract class Executor<T> : Executor where T : InstructData
    {
        protected sealed override void OnEnter(in ActionExecutionContext context, InstructData data)
        {
            if (data is T typed)
            {
                OnEnter(context, typed);
            }
        }

        protected sealed override void OnExecute(in ActionExecutionContext context, InstructData data)
        {
            if (data is T typed)
            {
                OnExecute(context, typed);
            }
        }

        protected sealed override void OnExit(in ActionExecutionContext context, InstructData data)
        {
            if (data is T typed)
            {
                OnExit(context, typed);
            }
        }

        protected virtual void OnEnter(in ActionExecutionContext context, T data)
        {
        }

        protected virtual void OnExecute(in ActionExecutionContext context, T data)
        {
        }

        protected virtual void OnExit(in ActionExecutionContext context, T data)
        {
        }
    }
}

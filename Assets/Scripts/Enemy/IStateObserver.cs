namespace Enemy
{
    public interface IStateObserver<TContext>
    {
        void OnStateEnter(State<TContext> state);
        void OnStateExit(State<TContext> state);
        void OnStateUpdate(State<TContext> state);
    }
}
namespace RxStore
{
    public interface IDispatcher<TState, TAction>
    {
        void Dispatch(TAction action);
    }
}

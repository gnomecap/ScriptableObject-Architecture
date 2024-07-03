namespace ScriptableObjectArchitecture
{
    public interface IGameEventListener<T>
    {
        void EventRaised(T value);
    }
    public interface IGameEventListener
    {
        void EventRaised();
    } 
}
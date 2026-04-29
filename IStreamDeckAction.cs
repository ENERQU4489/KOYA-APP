namespace KOYA_APP
{
    public interface IStreamDeckAction
    {
        string Name { get; }
        string Description { get; }
        void Execute();
    }
}
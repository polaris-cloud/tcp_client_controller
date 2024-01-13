namespace Polaris.Connect.Tool;

public class ComMappingChangedEventArg:EventArgs
{
    public ComMappingChangedEventArg(MappingChangedState state, string contract)
    {
        State = state;
        Contract = contract;
    }

    public MappingChangedState State { get; }
    public string Contract { get; }
}
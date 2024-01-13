namespace Polaris.Connect.Tool.Base;

public class ComEventArg : EventArgs
{
    public ComState State { get;}
    public string ComDetails { get; }
    public ComEventArg(ComState state, string comDetails)
    {
        State = state;
        ComDetails = comDetails;
    }
}
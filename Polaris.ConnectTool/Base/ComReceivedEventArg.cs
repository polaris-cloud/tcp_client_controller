namespace Polaris.Connect.Tool.Base;

public class ComReceivedEventArg : EventArgs
{
    public ComReceivedEventArg(string contract,Memory<byte> data)
    {
        Data = data;
        Contract = contract;
    }

    public Memory<byte> Data { get; }
    public string Contract { get; }
}


public class ComErrorEventArg : EventArgs
{
    public ComErrorEventArg(string contract, Exception e)
    {
        Exception=e;
        Contract = contract;
    }

    public Exception Exception { get; }
    public string Contract { get; }
}
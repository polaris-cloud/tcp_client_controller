using System.Net;
using System.Text;

namespace Polaris.Connect.Tool.Base;

public abstract class SinglePointComBase :ICom
{



    private Stream _comStream;
    
    
    public SinglePointComBase()
    {
        MaxReceiveLength = 1024;
        MillisecondsTimeout = -1;
    }


    public abstract IEnumerable<string> ActiveClients { get; }

    public abstract Task WriteTo(string client, byte[] content, CancellationToken token);

    public abstract Task WriteTo(string client, string content, CancellationToken token);

    public event EventHandler<ComEventArg> WhenConnectChanged;
    public event EventHandler<ComReceivedEventArg> WhenDataReceived;
    public event EventHandler<ComErrorEventArg> WhenReceivedExceptionOccur;
    public int MaxReceiveLength { get; set; }
    public int MillisecondsTimeout { get; set; }
    
public  Task WriteAsync(byte[] bytes, CancellationToken token) =>

        _comStream.WriteAsync(bytes, 0, bytes.Length, token);
    
    public  Task WriteAsync(string content, CancellationToken token)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return WriteAsync(bytes, token);
    }

    protected void SetSingleStream(Stream stream)
    {
        _comStream = stream;
    }

    protected void RaiseDataReceived(ComReceivedEventArg arg)
    {
        WhenDataReceived?.Invoke(this, arg);
    }

    protected  virtual void RaiseReceivedExceptionOccur(ComErrorEventArg e)
    {
        WhenReceivedExceptionOccur?.Invoke(this, e);
    }

    protected void RaiseConnectChanged(ComEventArg newState)
    {
          WhenConnectChanged?.Invoke(this ,newState);
    }

     protected async void StartReceiveTask(string contract,CancellationToken token)
    {

        byte[] buffer = new byte[MaxReceiveLength];
        try
        {
            while (!token.IsCancellationRequested)
            {
                
                int bytesRead = await _comStream.ReadAsync(buffer, 0, MaxReceiveLength, token);
                RaiseDataReceived(new ComReceivedEventArg(contract,buffer.AsMemory(0, bytesRead)));
            }
        }
        catch (Exception e)
        {
            RaiseReceivedExceptionOccur(new ComErrorEventArg(contract,e));
        }
        
    }
    
     
    protected async void StartReceiveTaskAndTimeout(string contract,Stream stream, CancellationToken token)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);

        byte[] buffer = new byte[MaxReceiveLength];
        try
        {

            while (!token.IsCancellationRequested)
            {
                var readTask = stream.ReadAsync(buffer, 0, MaxReceiveLength, timeoutCts.Token);


                if (await Task.WhenAny(readTask, Task.Delay(MillisecondsTimeout, token)) == readTask)
                {
                    //_connectingCts.Cancel();
                    RaiseDataReceived(new ComReceivedEventArg(contract, buffer.AsMemory(0, readTask.Result)));

                }
                else
                {
                    timeoutCts.Cancel();
                }

            }
        }
        catch (Exception e)
        {
            await stream.FlushAsync(token);
            if (timeoutCts.IsCancellationRequested)
                RaiseReceivedExceptionOccur(new ComErrorEventArg(contract, new TimeoutException("ReadTimeOut")));
            RaiseReceivedExceptionOccur(new ComErrorEventArg(contract, e));
        }
    }

    protected abstract void Dispose(string contract);

    
}
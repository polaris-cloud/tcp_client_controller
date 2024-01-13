namespace Polaris.Connect.Tool.Base;

public interface ISinglePointComStream
{
    Task WriteAsync(byte[] bytes, CancellationToken token);
    Task WriteAsync(string content, CancellationToken token); 

}
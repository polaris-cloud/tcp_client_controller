namespace Polaris.Console.Wrapper;


    public delegate Task ComOperation(IConsoleWriter writer,string[] orderAndParas,CancellationToken token);

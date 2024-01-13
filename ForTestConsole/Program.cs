namespace ForTestConsole
{
    internal class Program
    {

        static async void delay()
        {
            await Task.Delay(2000).ContinueWith(_=>Console.WriteLine("wait"));
        }
        static async Task  awaitdelay()
        {
            await Task.Delay(2000).ContinueWith(_ => Console.WriteLine("wait"));
        }

        static async Task Main(string[] args)
        {
            await awaitdelay();
            Console.WriteLine("Hello, World!");
        }
    }
}

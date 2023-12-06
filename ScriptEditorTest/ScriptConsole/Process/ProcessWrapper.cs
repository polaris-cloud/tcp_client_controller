using System.Diagnostics;
using System.IO;
using System.Text;
using ScriptEditorTest.ScriptConsole.Stream;

namespace ScriptEditorTest.ScriptConsole.Process;

/// <summary>
/// A class the wraps a process, allowing programmatic input and output.
/// 使用CancellationTokenSource 和Task 代替BackgroundWorker
/// </summary>
public class ProcessWrapper:IConsoleStream
{
    public ProcessWrapper()
    {

    }

    /// <summary>
    /// The internal process.
    /// </summary>
    private System.Diagnostics.Process _process;

    /// <summary>
    /// The command starting the process
    /// </summary>
    private string _command;

    /// <summary>
    /// The command arguments.
    /// </summary>
    private string _commandArguments;

    /// <summary>
    /// The command working directory.
    /// </summary>
    private string _workingDirectory;

    /// <summary>
    /// Returns true when the process is running
    /// </summary>
    public bool IsProcessRunning
    {
        get
        {
            try
            {
                return !_process?.HasExited ?? false;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// The input writer.
    /// </summary>
    private StreamWriter _inputWriter;
    /// <summary>
    /// The output reader.
    /// </summary>
    private TextReader _outputReader;
    /// <summary>
    /// The error reader.
    /// </summary>
    private TextReader _errorReader;


    private readonly CancellationTokenSource _cts = new CancellationTokenSource();


    /// <summary>
    /// Internal class to hold the output chunk within the output worker
    /// </summary>
    protected class InternalOutputChunk
    {
        public MessageRank Rank { get; set; }
        public string Output { get; set; }
        public InternalOutputChunk(MessageRank rank, string output)
        {
            Rank = rank;
            Output = output;
        }

        public InternalOutputChunk():this(MessageRank.None,"")
        {
        }
    }


    /// <summary>
    /// Runs a process with given <paramref name="command"/>
    /// </summary>
    /// <param name="command">Name of the file to run</param>
    /// <param name="arguments">Optional command line arguments</param>
    /// <param name="workingDirectory">Optional working directory</param>
    public void StartProcess(string command, string arguments = null, string workingDirectory = null)
    {


        RaiseProcessOutputEvent(MessageRank.Diagnostic, Environment.NewLine + "Preparing to run " + command);
            if (!string.IsNullOrEmpty(arguments))
                RaiseProcessOutputEvent(MessageRank.Diagnostic," with arguments " + arguments + "." + Environment.NewLine);
            else
                RaiseProcessOutputEvent(MessageRank.Diagnostic,"." + Environment.NewLine);
        

        //Start the process.
        var result = StartProcessCore(command, arguments, workingDirectory);
            if (result)
            {
                RaiseProcessOutputEvent(MessageRank.Diagnostic, "Started " + command + Environment.NewLine);
            }
            else
            {
                RaiseProcessOutputEvent(MessageRank.Diagnostic, "Can't start " + command + " - another process is running" + Environment.NewLine);
            }

    }
    






    /// <summary>
    /// Runs a process with given <paramref name="command"/>
    /// </summary>
    /// <param name="command">Name of the file to run</param>
    /// <param name="arguments">Optional command line arguments</param>
    /// <param name="workingDirectory">Optional working directory</param>
    private bool StartProcessCore(string command, string arguments = null, string workingDirectory = null)
    {
        
        if (string.IsNullOrEmpty(command)) throw new ArgumentNullException(nameof(command));
        if (IsProcessRunning) return false;

        _command = command;
        _commandArguments = arguments;
        _workingDirectory = workingDirectory ?? Environment.CurrentDirectory;

        //Configure the process
        var processStartInfo = new ProcessStartInfo(command, arguments)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            WorkingDirectory = _workingDirectory
        };

        //Create the process
        _process = new System.Diagnostics.Process
        {
            StartInfo = processStartInfo,
            EnableRaisingEvents = true
        };
        _process.Exited += ProcessExitedHandler;
        //Start the process
        _process.Start();

        //Create readers and writers
        _inputWriter = _process.StandardInput;
        _outputReader = TextReader.Synchronized(_process.StandardOutput);
        _errorReader = TextReader.Synchronized(_process.StandardError);

        // Run the workers that read output and error
        ReadOutput(_outputReader, false, _cts);
        ReadOutput(_errorReader, true, _cts);



        return true;
    }

    /// <summary>
    /// Stops the process.
    /// </summary>
    public void StopProcess()
    {
        if (!IsProcessRunning) return;

        //Kill the process.

        _process.Kill();
        _cts.Cancel();
    }

    /// <summary>
    /// Writes the input into the process
    /// </summary>
    /// <param name="input">The process input</param>
    private void WriteInternal(string input)
    {
        if (!IsProcessRunning) return;

        _inputWriter.WriteLine(input);
        _inputWriter.Flush();
    }



    #region events

    ///// <summary>
    ///// Occurs when the process ends.
    ///// </summary>
    //public event ProcessEventHandler OnProcessExit;

    ///// <summary>
    ///// Raises OnProcessExit event
    ///// </summary>
    ///// <param name="code">The exit code</param>
    ///// <param name="command">Command name to be set to the args Content</param>
    //private void RaiseProcessExitEvent(int code, string command)
    //{
    //    OnProcessExit?.Invoke(this, new ProcessEventArgs(code, command));
    //}


    /// <summary>
    /// Occurs when process output (incl. error stream) is produced.
    /// </summary>
    public event EventHandler<ConsoleStreamEventArgs>? OnConsoleRead;

    /// <summary>
    /// Raises OnProcessOutput event
    /// </summary>
    /// <param name="output"></param>
    /// <param name="rank"></param>
    private void RaiseProcessOutputEvent(MessageRank rank,string output)
    {
        
        OnConsoleRead?.Invoke(this, new ConsoleStreamEventArgs(output, rank));
    }


    #endregion


    /// <summary>
    /// Handles the Exited event of the currentProcess control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ProcessExitedHandler(object? sender, EventArgs e)
    {
        var exitCode = 0;
        try
        {
            exitCode = _process.ExitCode;
        }
        catch (Exception)
        {
            //just ignore - process has been probably killed
        }
        //Cleanup
        var tmpCommand = _command;
        _cts.Cancel();
        //Raise process exited.
        RaiseProcessOutputEvent(MessageRank.Diagnostic, $"ExitCode: {exitCode} ,Command: {_command}");
    }

    /// <summary>
    /// Processes the output from standard output or error output stream <paramref name="reader"/>
    /// </summary>
    /// <param name="reader">Standard output or error output stream reader to process </param>
    /// <param name="isError">Flag whether the output is to be marked as error</param>
    /// <param name="cts">CancellationTokenSource</param>
    private void ReadOutput(TextReader reader, bool isError, CancellationTokenSource cts)
    {
        Task.Run(async () =>
        {
            var buffer = new char[1024];
            while (!cts.IsCancellationRequested)
            {
                int count;
                do
                {
                    var sb = new StringBuilder();
                    count = await reader.ReadAsync(buffer, cts.Token);
                    sb.Append(buffer, 0, count);
                    RaiseProcessOutputEvent( isError?MessageRank.Error:MessageRank.None,sb.ToString());
                } while (count > 0);
            }

        }, cts.Token);

    }

    
    
    public bool  Write(string content)
    {
        WriteInternal(content);
        return true;
    }
}
namespace ScriptEditorTest.ScriptConsole.Stream;

/// <summary>
/// A EventHandler is a delegate for Console Stream Event.
/// </summary>
/// <param name="sender">The event sender</param>
/// <param name="args">The <see cref="ConsoleStreamEventArgs"/> instance containing the event data</param>
public delegate void ConsoleStreamEventHandler(object sender, ConsoleStreamEventArgs args);
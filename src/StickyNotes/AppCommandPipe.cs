using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace StickyNotes;

// Only fixed command IDs cross this same-user, same-session channel; no paths or note content.
internal sealed class AppCommandPipe : IDisposable
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeServerProcessId(SafePipeHandle pipe, out uint processId);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllowSetForegroundWindow(uint processId);
    private readonly CancellationTokenSource stopping = new();
    private readonly NamedPipeServerStream server;
    internal Task Completion { get; }

    internal AppCommandPipe(string name, Action<AppCommand> received)
    {
        server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        Completion = Listen(received);
    }

    private async Task Listen(Action<AppCommand> received)
    {
        try
        {
            while (!stopping.IsCancellationRequested)
            {
                await server.WaitForConnectionAsync(stopping.Token).ConfigureAwait(false);
                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stopping.Token);
                    timeout.CancelAfter(TimeSpan.FromSeconds(2));
                    var buffer = new byte[1];
                    if (await server.ReadAsync(buffer, timeout.Token).ConfigureAwait(false) == 1 &&
                        AppCommands.All.FirstOrDefault(command => command.Code == buffer[0]) is { } command)
                    {
                        await server.WriteAsync(buffer, timeout.Token).ConfigureAwait(false);
                        received(command);
                    }
                }
                catch (IOException) { /* A client can exit before sending or receiving its acknowledgement. */ }
                catch (OperationCanceledException) when (!stopping.IsCancellationRequested) { }
                finally { if (!stopping.IsCancellationRequested) server.Disconnect(); }
            }
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
        catch (ObjectDisposedException) when (stopping.IsCancellationRequested) { }
        catch (IOException) when (stopping.IsCancellationRequested) { }
    }

    internal static async Task Send(string name, AppCommand command)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        using var client = new NamedPipeClientStream(".", name, PipeDirection.InOut,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        await client.ConnectAsync(timeout.Token).ConfigureAwait(false);
        // Pass the shell-launched client's foreground permission only to the connected app.
        if (GetNamedPipeServerProcessId(client.SafePipeHandle, out var processId)) AllowSetForegroundWindow(processId);
        var buffer = new[] { command.Code };
        await client.WriteAsync(buffer, timeout.Token).ConfigureAwait(false);
        if (await client.ReadAsync(buffer, timeout.Token).ConfigureAwait(false) != 1 || buffer[0] != command.Code)
            throw new IOException(L10n.Text("AppCommandPipe.Text01"));
    }

    public void Dispose()
    {
        stopping.Cancel();
        server.Dispose();
        // The loop uses ConfigureAwait(false), so disposal cannot wait on the UI dispatcher.
        try { Completion.GetAwaiter().GetResult(); }
        finally { stopping.Dispose(); }
    }
}

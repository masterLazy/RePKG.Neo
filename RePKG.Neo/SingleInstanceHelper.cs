/*
  Copyright 2025 masterLazy

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0
*/

using System.IO;
using System.IO.Pipes;

namespace RePKG.Neo;

public class SingleInstance {
    private readonly CancellationTokenSource _cts = new();

    public string PipeName { get; }

    public delegate void IncomingFilesHandler(string[] paths);

    public event IncomingFilesHandler? IncomingFiles;

    public SingleInstance(string pipeName) {
        PipeName = pipeName;
    }

    public void ConnectAndSend(string[] paths) {
        using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
        client.Connect(1000);
        using var writer = new StreamWriter(client);
        writer.AutoFlush = true;
        // Send path parameters
        writer.WriteLine(string.Join("|", paths));
    }

    public void StopServer() {
        _cts.Cancel();
    }

    public async void StartServer() {
        while (!_cts.Token.IsCancellationRequested) {
            try {
                await using var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances);

                await server.WaitForConnectionAsync();

                using var reader = new StreamReader(server);
                string message = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(message)) continue;

                string[] files = message.Trim().Split('|', StringSplitOptions.RemoveEmptyEntries);
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => IncomingFiles?.Invoke(files));
            } catch (Exception e) {
                if (e is OperationCanceledException or ObjectDisposedException) break;
                // Keep listening while exception occurs
                Log.Error($"Exception occured during listening on pipe {PipeName}: {Helper.ExceptionToString(e)}");
            }
        }
    }
}
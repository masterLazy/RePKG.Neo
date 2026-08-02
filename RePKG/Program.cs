using System;
using System.Threading;
using CommandLine;
using RePKG.Command;

namespace RePKG
{
    internal class Program
    {
        public static bool Closing;

        private static readonly CancellationTokenSource _cts = new();

        private static void Main(string[] args)
        {
            Console.CancelKeyPress += Cancel;

            if (args.Length > 0 && args[0] == "interactive")
            {
                InteractiveConsole();
                return;
            }

            Parser.Default.ParseArguments<ExtractOptions, InfoOptions>(args)
                .WithParsed<ExtractOptions>(o => RunExtract(o))
                .WithParsed<InfoOptions>(o => Info.Action(o));
        }

        private static void Cancel(object sender, ConsoleCancelEventArgs e)
        {
            Closing = true;
            e.Cancel = true;
            _cts.Cancel();
            Console.WriteLine("Cancelling...");
        }

        private static void RunExtract(ExtractOptions o)
        {
            try
            {
                Extract.Action(o, null, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Extraction cancelled.");
            }
        }

        private static void InteractiveConsole()
        {
            Console.WriteLine("RePKG started in interactive mode. You can now type commands");
            Console.WriteLine("Type \"help\" for commands");
            
            string line;

            while (!string.IsNullOrEmpty(line = Console.ReadLine()))
            {
                var interactiveArgs = line.SplitArguments();

                Parser.Default.ParseArguments<ExtractOptions, InfoOptions>(interactiveArgs)
                    .WithParsed<ExtractOptions>(o => RunExtract(o))
                    .WithParsed<InfoOptions>(o => Info.Action(o));
            }
        }
    }
}

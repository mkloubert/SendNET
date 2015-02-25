// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using MarcelJoachimKloubert.SendNET.Contracts;
using MarcelJoachimKloubert.SendNET.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MarcelJoachimKloubert.SendNET
{
    /// <summary>
    /// The program class.
    /// </summary>
    internal static class Program
    {
        #region Methods (1)

        /// <summary>
        /// The entry point.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        private static void Main(string[] args)
        {
            var normalizedArgs = args.Select(a => a.TrimStart())
                                     .Where(a => string.IsNullOrWhiteSpace(a) == false)
                                     .Distinct()
                                     .ToArray();

            PrintHeader();

            Action actionToInvoke = ShowShortHelp;
            AppSettings settings = null;

            try
            {
                Action markForShowHelp = () =>
                    {
                        actionToInvoke = ShowShortHelp;
                        settings = null;
                    };

                if (normalizedArgs.Length > 0)
                {
                    settings = new AppSettings();
                    settings.TargetDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory,
                                                                              "files"));

                    switch (normalizedArgs[0].ToLower().Trim())
                    {
                        case "/r":
                            settings.Type = SendOperationType.ReceiveFiles;
                            break;

                        case "/s":
                            settings.Type = SendOperationType.SendFiles;
                            break;

                        default:
                            markForShowHelp();
                            break;
                    }

                    if (settings != null)
                    {
                        switch (settings.Type)
                        {
                            case SendOperationType.ReceiveFiles:
                                break;

                            case SendOperationType.SendFiles:
                                foreach (var a in normalizedArgs.Skip(1))
                                {
                                    settings.FilesToSend
                                            .Add(new FileInfo(a));
                                }
                                break;
                        }
                    }
                }

                if (settings != null)
                {
                    actionToInvoke = () =>
                        {
                            var op = new SendOperation(settings);
                            SendDataService.Settings = settings;

                            op.Start();
                        };
                }

                if (actionToInvoke != null)
                {
                    actionToInvoke();
                }
            }
            catch (Exception ex)
            {
                var innerEx = ex.GetBaseException() ?? ex;

                ConsoleHelper.InvokeForColor(() => Console.WriteLine(innerEx),
                                             ConsoleColor.Yellow, ConsoleColor.Red);
            }

#if DEBUG
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("===== ENTER =====");
            Console.ReadLine();
#endif
        }

        private static void PrintHeader()
        {
            var title = string.Format("SendNET {0}",
                                      Assembly.GetExecutingAssembly().GetName().Version);

            Console.WriteLine(title);
            Console.WriteLine(string.Concat(Enumerable.Repeat("=",
                                                              title.Length + 5)));
            Console.WriteLine();
        }

        private static void ShowShortHelp()
        {
        }

        #endregion Methods (1)
    }
}
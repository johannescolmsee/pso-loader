using Common;
using LibPSO;
using LibPSO.PacketDefinitions;
using LibPSO.PsoPatcher;
using LibPSO.PsoServices;
using LibPSO.PsoServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PSOPatcherConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineOptions options = new CommandLineOptions();
            var l = CommandLine.Parser.Default.ParseArguments(args, options);
            if (!l)
            {
                return;
            }

            var filenames = options.InputFiles.Select(x => x.Replace(",", "").Replace(";", ""));
            IEnumerable<PsoPatchDefinition> patchDefs;
            try
            {
                patchDefs = filenames
                    .Select(x => PsoPatchDefinition.FromXml(x))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
@"At least one of the provided files could not be loaded:
{0}", ex.Message);
                return;
            }

            if (options.CheckPatches)
            {
                //check the patchDefs for errors and warnings
                var allErrorAndWarnings = patchDefs.
                    SelectMany(x => x.GetErrorsAndWarnings() ?? Enumerable.Empty<string>());
                if (allErrorAndWarnings.Any())
                {
                    Console.WriteLine(String.Join(Environment.NewLine, allErrorAndWarnings));
                }
            }
            var t = _DoPatchTask(patchDefs, options.Port, options.Verbose);
            try
            {
                t.Wait();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Exception: {0}", ex.GetBaseException().Message);
            }
        }

        private static async Task _DoPatchTask(IEnumerable<PsoPatchDefinition> patchDefinitions, int port, bool verbose)
        {
            var psoServer = new PsoServer();
            IPAddress address = IPAddress.Any;
            Console.WriteLine("Awaiting connection on port {0}, address {1}", port, address);
            var clientConnection = await psoServer.AwaitConnectionAsync(port, address, ClientType.Gamecube);
            Console.WriteLine("Client connected.");
            //send howdy
            await clientConnection.InitConnection(0x4f52554b, 0x49524f4d);
            Console.WriteLine("Sent howdy.");

            //read answer
            var message = await clientConnection.ReadMessageFromClient();
            Console.WriteLine("Received answer");
            if (verbose)
            {
                Console.WriteLine("Content of received message:");
                Console.WriteLine(HexStringFormater.GetHexText(message.MessageCrypted, 0, 16, 0));
            }

            foreach (var patchDef in patchDefinitions)
            {
                //prepare program
                var program = patchDef.GetPatchProgram();
                var programPacket = Packets.GetUpdateCodePacket(program, ClientType.Gamecube);

                //send program
                await clientConnection.SendMessageToClient(programPacket, true);
                Console.WriteLine("Sent patch.");
                if (verbose)
                {
                    Console.WriteLine("Patch message:");
                    Console.WriteLine(HexStringFormater.GetHexText(programPacket, 0, 16, 0));
                }
                //read answer
                var result = await clientConnection.ReadMessageFromClient();
                var resultPackage = UpdateCodePackageResultPackage.FromBytes(result.MessageCrypted, ClientType.Gamecube);

                Console.WriteLine(@"Number of patches applied: {0} (expected: {1})", resultPackage.ReturnValue, patchDef.Patches.Sum(x => x.GetPatchCount()));
                if (verbose)
                {
                    Console.WriteLine("Content of received message:");
                    Console.WriteLine(HexStringFormater.GetHexText(result.MessageCrypted, 0, 16, 0));
                }

            }

            var redirections = patchDefinitions
                .Where(x => x.Redirect != null && x.Redirect.IPAddress != null)
                .Select(x => x.Redirect)
                .ToArray();
            var redirect = redirections.FirstOrDefault();
            if (redirections.Length > 1)
            {
                Console.WriteLine("Warning, multiple redirections found. Using first.");
            }

            if (redirect != null)
            {
                //get redirect packet
                Console.WriteLine("Redirecting to: {0}:{1}", redirect.IPAddress, redirect.Port);
                var redirectPacket = redirect.GetRedirectPacket(ClientType.Gamecube);
                await clientConnection.SendMessageToClient(redirectPacket, true);
                Console.WriteLine("Sent redirect.");
                if (verbose)
                {
                    Console.WriteLine("Redirect Packet contents:");
                    Console.WriteLine(HexStringFormater.GetHexText(redirectPacket, 0, 16, 0));
                }
            }
            Console.WriteLine("DONE.");
        }
    }
}

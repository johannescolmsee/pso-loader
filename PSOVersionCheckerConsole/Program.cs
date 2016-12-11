using Common;
using LibPSO;
using LibPSO.PacketDefinitions;
using LibPSO.PsoServices;
using LibPSO.PsoServices.Interfaces;
using LibPSO.PsoVersionDetector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PSOVersionCheckerConsole
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

            PsoVersionDetectionDefinition versionDetector;
            try
            {
                versionDetector = PsoVersionDetectionDefinition.FromXml(options.InputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
@"The provided files could not be loaded:
{0}", ex.Message);
                return;
            }

            UInt32 result;
            var t = _DoVersionCheckTask(versionDetector, options.Port, options.Verbose);
            try
            {
                t.Wait();
                result = t.Result;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Exception: {0}", ex.GetBaseException().Message);
            }
        }

        private static async Task<UInt32> _DoVersionCheckTask(PsoVersionDetectionDefinition versionCheckDefinition, int port, bool verbose)
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

            var program = versionCheckDefinition.GetPsoVersionDetectionProgram();
            var programPacket = Packets.GetUpdateCodePacket(program, ClientType.Gamecube);

            //send program
            await clientConnection.SendMessageToClient(programPacket, true);
            Console.WriteLine("Sent program.");
            if (verbose)
            {
                Console.WriteLine("Program message:");
                Console.WriteLine(HexStringFormater.GetHexText(programPacket, 0, 16, 0));
            }
            //read answer
            var result = await clientConnection.ReadMessageFromClient();
            var resultPackage = UpdateCodePackageResultPackage.FromBytes(result.MessageCrypted, ClientType.Gamecube);

            Console.WriteLine(@"Return Value: {0} (0x{0:x8})", resultPackage.ReturnValue);
            if (verbose)
            {
                Console.WriteLine("Content of received message:");
                Console.WriteLine(HexStringFormater.GetHexText(result.MessageCrypted, 0, 16, 0));
            }


            Console.WriteLine("DONE.");
            return resultPackage.ReturnValue;
        }
    }
}

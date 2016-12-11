using Common;
using LibPSO;
using LibPSO.PacketDefinitions;
using LibPSO.PsoPatcher;
using LibPSO.PsoServices;
using LibPSO.PsoServices.Interfaces;
using PsoBackupServer.Model;
using PsoBackupServer.PsoReadMem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PsoBackupServer
{
    class Program
    {
        static void Main(string[] args)
        {



            var files = Directory.GetFiles(".", "*.psosave");
            var saveGames = files
                .Select(x => XmlSerializeHelper.FromXml<PsoSaveGame>(x))
                .ToArray();

            var playersClientSent = saveGames
                .Select(x => Player.FromBytes(x.DataSentByClient))
                .ToArray();

            var hexStringsClientSent = playersClientSent
                .Select(x => x.GetBytes().ToArray())
                .Select(x => HexStringFormater.GetHexText(x, 0, 16, 0))
                .ToArray();

            var hexStringsDownloaded = saveGames
                .Select(x => x.DataReadFromRam ?? new byte[0])
                .Select(x => HexStringFormater.GetHexText(x, 0, 16, 0))
                .ToArray();

            CommandLineOptions options = new CommandLineOptions();
            var l = CommandLine.Parser.Default.ParseArguments(args, options);
            if (!l)
            {
                return;
            }

            var t = _RunServerAsync(options.Port, options.Verbose);
            try
            {
                t.Wait();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Exception: {0}", ex.GetBaseException().Message);
            }
        }

        private static async Task _RunServerAsync(int port, bool verbose)
        {
            var psoServer = new PsoServer();
            IPAddress address = IPAddress.Any;
            while (true)
            {
                Console.WriteLine("Awaiting connection on port {0}, address {1}", port, address);
                var clientConnection = await psoServer.AwaitConnectionAsync(port, address, ClientType.Gamecube);
                Console.WriteLine("Client connected.");
                var clientTask = _RunClientAsync(clientConnection, verbose);
            }
        }

        private static async Task _RunClientAsync(IPsoServerClientConntection clientConnection, bool verbose)
        {
            using (clientConnection)
            {
                await clientConnection.InitConnection(0x4f52554b, 0x49524f4d);
                Console.WriteLine("Sent howdy.");

                //read answer -> gc will send HL information (SNAK, pw)
                var message = await clientConnection.ReadMessageFromClient();
                Console.WriteLine("Received answer");
                if (verbose)
                {
                    Console.WriteLine("Content of received message:");
                    Console.WriteLine(HexStringFormater.GetHexText(message.MessageCrypted, 0, 16, 0));
                }

                //send login 0x9A -> this will trigger sendback of chardata
                byte[] message0 = new byte[] { 0x9A, 0x00, 0x04, 0x00 };
                await clientConnection.SendMessageToClient(message0, true);
                Console.WriteLine("Sent message0 (0x9a login).");
                if (verbose)
                {
                    Console.WriteLine("message0:");
                    Console.WriteLine(HexStringFormater.GetHexText(message0, 0, 16, 0));
                }

                //read answer -> the answer will contain chardata
                message = await clientConnection.ReadMessageFromClient();
                Console.WriteLine("Received answer to message0 (chardata)");
                if (verbose)
                {
                    Console.WriteLine("Content of received message:");
                    Console.WriteLine(HexStringFormater.GetHexText(message.MessageCrypted, 0, 16, 0));
                }

                //security packet, includes GC info -> this triggers a save.
                byte[] message1 = new byte[] { 0x04, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x01, 0x00, 0xde, 0x3a, 0x00, 0x00 };
                await clientConnection.SendMessageToClient(message1, true);
                Console.WriteLine("Sent message1 (gc-data, security data - this will trigger a save).");

                if (verbose)
                {
                    Console.WriteLine("message1:");
                    Console.WriteLine(HexStringFormater.GetHexText(message1, 0, 16, 0));
                }

                //read answer
                message = await clientConnection.ReadMessageFromClient();
                Console.WriteLine("Received answer to message1");
                if (verbose)
                {
                    Console.WriteLine("Content of received message:");
                    Console.WriteLine(HexStringFormater.GetHexText(message.MessageCrypted, 0, 16, 0));
                }


                //"CHECKSUM_REPLY_TYPE"
                byte[] message2 = new byte[] { 0x97, 0x01, 0x04, 0x00 };
                await clientConnection.SendMessageToClient(message2, true);
                Console.WriteLine("Sent message2 (checksum reply) -> will trigger save.");
                if (verbose)
                {
                    Console.WriteLine("message2:");
                    Console.WriteLine(HexStringFormater.GetHexText(message2, 0, 16, 0));
                }


                //read answer
                message = await clientConnection.ReadMessageFromClient();
                Console.WriteLine("Received answer to message2");
                if (verbose)
                {
                    Console.WriteLine("Content of received message:");
                    Console.WriteLine(HexStringFormater.GetHexText(message.MessageCrypted, 0, 16, 0));
                }

                //CHAR_DATA_REQUEST_TYPE
                byte[] message3 = new byte[] { 0x95, 0x00, 0x04, 0x00 };
                await clientConnection.SendMessageToClient(message3, true);
                Console.WriteLine("Sent message3 (char data request)");
                if (verbose)
                {
                    Console.WriteLine("message3:");
                    Console.WriteLine(HexStringFormater.GetHexText(message3, 0, 16, 0));
                }

                //read answer
                message = await clientConnection.ReadMessageFromClient();
                Console.WriteLine("Received answer to message3");
                if (verbose)
                {
                    Console.WriteLine("Content of received message:");
                    Console.WriteLine(HexStringFormater.GetHexText(message.MessageCrypted, 0, 16, 0));
                }

                byte[] charDataBytesFromClient = message.MessageCrypted
                    .Skip(4)
                    .ToArray();


                /*
                 *  sprintf(pkt->timestamp, "%u:%02u:%02u: %02u:%02u:%02u.%03u",
                cooked.tm_year + 1900, cooked.tm_mon + 1, cooked.tm_mday,
                cooked.tm_hour, cooked.tm_min, cooked.tm_sec,
                (unsigned)(rawtime.tv_usec / 1000));*/

                //TIMESTAMP_TYPE
                byte[] message4 = Packets.GetTimeStampPacket(ClientType.Gamecube);// 
                //new byte[] { 0xB1, 0x00, 0x20, 0x00, 0x32, 0x30, 0x31, 0x36, 0x3A, 0x31, 0x31, 0x3A, 0x30, 0x33, 0x3A, 0x20, 0x32, 0x31, 0x3A, 0x30, 0x35, 0x3A, 0x31, 0x38, 0x2E, 0x31, 0x30, 0x32, 0x00, 0x00, 0x00, 0x00, };

                await clientConnection.SendMessageToClient(message4, true);
                Console.WriteLine("Sent message4 (time)");
                if (verbose)
                {
                    Console.WriteLine("message4:");
                    Console.WriteLine(HexStringFormater.GetHexText(message4, 0, 16, 0));
                }

                //read answer
                message = await clientConnection.ReadMessageFromClient();
                Console.WriteLine("Received answer to message4");
                if (verbose)
                {
                    Console.WriteLine("Content of received message:");
                    Console.WriteLine(HexStringFormater.GetHexText(message.MessageCrypted, 0, 16, 0));
                }

                await _SendMessageBoxAndWaitForAccept(clientConnection, @"Welcome to the PSO Backup Server", verbose);

                await _DoMenuLoop(clientConnection, charDataBytesFromClient, verbose);
            }
        }

        private static async Task _SendMessageBoxAndWaitForAccept(IPsoServerClientConntection clientConnection, string message, bool verbose)
        {
            //send hello message
            byte[] messagebytes = Packets.GetMessageBoxPacket(message, ClientType.Gamecube);
            //new byte[] 
            //{
            //    0xD5, 0x00, 0x50, 0x02, 0x09, 0x45, 0x57, 0x65, 0x6C, 0x63, 0x6F, 0x6D, 0x65, 0x20, 0x74, 0x6F, 0x20, 0x74, 0x68, 0x65, 0x20, 0x53, 0x79, 0x6C, 0x76, 0x65, 0x72, 0x61, 0x6E, 0x74, 0x20, 0x50, 0x53, 0x4F, 0x20, 0x53, 0x65, 0x72, 0x76, 0x65, 0x72, 0x21, 0x0A, 0x0A, 0x50, 0x6C, 0x65, 0x61, 0x73, 0x65, 0x20, 0x6E, 0x6F, 0x74, 0x65, 0x20, 0x74, 0x68, 0x61, 0x74, 0x20, 0x74, 0x68, 0x69, 0x73, 0x20, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20, 0x69, 0x73, 0x20, 0x73, 0x74, 0x69, 0x6C, 0x6C, 0x20, 0x76, 0x65, 0x72, 0x79, 0x20, 0x6D, 0x75, 0x63, 0x68, 0x0A, 0x69, 0x6E, 0x20, 0x74, 0x65, 0x73, 0x74, 0x69, 0x6E, 0x67, 0x2C, 0x20, 0x61, 0x6E, 0x64, 0x20, 0x74, 0x68, 0x65, 0x72, 0x65, 0x20, 0x61, 0x72, 0x65, 0x20, 0x62, 0x6F, 0x75, 0x6E, 0x64, 0x20, 0x74, 0x6F, 0x20, 0x62, 0x65, 0x20, 0x62, 0x75, 0x67, 0x73, 0x2E, 0x20, 0x49, 0x0A, 0x77, 0x69, 0x6C, 0x6C, 0x20, 0x6E, 0x6F, 0x74, 0x20, 0x62, 0x65, 0x20, 0x68, 0x65, 0x6C, 0x64, 0x20, 0x72, 0x65, 0x73, 0x70, 0x6F, 0x6E, 0x73, 0x69, 0x62, 0x6C, 0x65, 0x20, 0x66, 0x6F, 0x72, 0x20, 0x61, 0x6E, 0x79, 0x20, 0x70, 0x72, 0x6F, 0x62, 0x6C, 0x65, 0x6D, 0x73, 0x0A, 0x61, 0x73, 0x20, 0x61, 0x20, 0x72, 0x65, 0x73, 0x75, 0x6C, 0x74, 0x20, 0x6F, 0x66, 0x20, 0x62, 0x75, 0x67, 0x73, 0x2C, 0x20, 0x73, 0x6F, 0x20, 0x6B, 0x65, 0x65, 0x70, 0x20, 0x74, 0x68, 0x61, 0x74, 0x20, 0x69, 0x6E, 0x20, 0x6D, 0x69, 0x6E, 0x64, 0x2E, 0x0A, 0x0A, 0x54, 0x68, 0x69, 0x73, 0x20, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20, 0x69, 0x73, 0x20, 0x61, 0x6E, 0x20, 0x6F, 0x70, 0x65, 0x6E, 0x2D, 0x73, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x70, 0x72, 0x6F, 0x6A, 0x65, 0x63, 0x74, 0x20, 0x74, 0x68, 0x61, 0x74, 0x20, 0x68, 0x61, 0x73, 0x0A, 0x6F, 0x6E, 0x65, 0x20, 0x67, 0x6F, 0x61, 0x6C, 0x2C, 0x20, 0x61, 0x6E, 0x64, 0x20, 0x6F, 0x6E, 0x65, 0x20, 0x67, 0x6F, 0x61, 0x6C, 0x20, 0x6F, 0x6E, 0x6C, 0x79, 0x3A, 0x20, 0x74, 0x6F, 0x20, 0x6B, 0x65, 0x65, 0x70, 0x20, 0x74, 0x68, 0x65, 0x20, 0x50, 0x68, 0x61, 0x6E, 0x74, 0x61, 0x73, 0x79, 0x0A, 0x53, 0x74, 0x61, 0x72, 0x20, 0x4F, 0x6E, 0x6C, 0x69, 0x6E, 0x65, 0x20, 0x73, 0x65, 0x72, 0x69, 0x65, 0x73, 0x20, 0x6F, 0x66, 0x20, 0x67, 0x61, 0x6D, 0x65, 0x73, 0x20, 0x61, 0x6C, 0x69, 0x76, 0x65, 0x2E, 0x20, 0x54, 0x68, 0x69, 0x73, 0x20, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20, 0x69, 0x73, 0x0A, 0x6E, 0x6F, 0x74, 0x20, 0x6D, 0x65, 0x61, 0x6E, 0x74, 0x20, 0x74, 0x6F, 0x20, 0x63, 0x6F, 0x6D, 0x70, 0x65, 0x74, 0x65, 0x20, 0x77, 0x69, 0x74, 0x68, 0x20, 0x6F, 0x72, 0x20, 0x6F, 0x74, 0x68, 0x65, 0x72, 0x77, 0x69, 0x73, 0x65, 0x20, 0x64, 0x65, 0x74, 0x72, 0x61, 0x63, 0x74, 0x0A, 0x66, 0x72, 0x6F, 0x6D, 0x20, 0x53, 0x65, 0x67, 0x61, 0x27, 0x73, 0x20, 0x77, 0x6F, 0x72, 0x6B, 0x20, 0x6F, 0x6E, 0x20, 0x50, 0x53, 0x4F, 0x2E, 0x20, 0x54, 0x68, 0x61, 0x6E, 0x6B, 0x73, 0x2C, 0x20, 0x6F, 0x66, 0x20, 0x63, 0x6F, 0x75, 0x72, 0x73, 0x65, 0x2C, 0x20, 0x67, 0x6F, 0x0A, 0x6F, 0x75, 0x74, 0x20, 0x74, 0x6F, 0x20, 0x53, 0x65, 0x67, 0x61, 0x20, 0x66, 0x6F, 0x72, 0x20, 0x6D, 0x61, 0x6B, 0x69, 0x6E, 0x67, 0x20, 0x50, 0x53, 0x4F, 0x2E, 0x20, 0x57, 0x65, 0x27, 0x72, 0x65, 0x20, 0x61, 0x6C, 0x6C, 0x20, 0x62, 0x69, 0x67, 0x20, 0x66, 0x61, 0x6E, 0x73, 0x21, 0x0A, 0x0A, 0x48, 0x61, 0x76, 0x65, 0x20, 0x66, 0x75, 0x6E, 0x2C, 0x20, 0x61, 0x6E, 0x64, 0x20, 0x6B, 0x65, 0x65, 0x70, 0x20, 0x70, 0x6C, 0x61, 0x79, 0x69, 0x6E, 0x67, 0x20, 0x50, 0x53, 0x4F, 0x21, 0x0A, 0x2D, 0x42, 0x6C, 0x75, 0x65, 0x43, 0x72, 0x61, 0x62, 0x20, 0x28, 0x79, 0x6F, 0x75, 0x72, 0x20, 0x66, 0x72, 0x69, 0x65, 0x6E, 0x64, 0x6C, 0x79, 0x20, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20, 0x61, 0x64, 0x6D, 0x69, 0x6E, 0x29, 0x0A, 0x00, 0x00, 0x00
            //};
            await clientConnection.SendMessageToClient(messagebytes, true);
            Console.WriteLine("Sent message box");
            if (verbose)
            {
                Console.WriteLine("message:");
                Console.WriteLine(HexStringFormater.GetHexText(messagebytes, 0, 16, 0));
            }

            //read answer
            var answerMessage = await clientConnection.ReadMessageFromClient();
            Console.WriteLine("Received answer to message box");
            if (verbose)
            {
                Console.WriteLine("Content of received message:");
                Console.WriteLine(HexStringFormater.GetHexText(answerMessage.MessageCrypted, 0, 16, 0));
            }
        }

        enum MainMenuItemId : uint
        {
            Save,
            Load,
            Exit,
        };
        private const UInt32 BASE_ADDRESS_PAL_CHAR = 0x80F27820;
        private static async Task _DoMenuLoop(IPsoServerClientConntection clientConnection, byte[] playerDataFromClient, bool verbose)
        {
            bool exit = false;
            while (!exit)
            {
                //send menu
                var mmResult = await _ShowMenu(
                    clientConnection,
                    new[] 
                    {
                        new LibPSO.Packets.MenuItem() { Name = "Save Character", MenuId = 0, ItemId = (UInt32)MainMenuItemId.Save, Flags = 0x0004, },
                        new LibPSO.Packets.MenuItem() { Name = "Restore Character", MenuId = 0, ItemId = (UInt32)MainMenuItemId.Load, Flags = 0x0F04, },//0x0F04 probably means sub menu?! no idea
                        new LibPSO.Packets.MenuItem() { Name = "Exit Program", MenuId = 0, ItemId = (UInt32)MainMenuItemId.Exit, Flags = 0x0004, },
                    },
                    null,
                    verbose);

                MainMenuItemId mmId = (MainMenuItemId)mmResult;
                switch (mmId)
                {
                    case MainMenuItemId.Save:
                        Console.WriteLine("Loading complete player data.");
                        var dataFromRam = await _LoadCharDataFromClient(clientConnection, BASE_ADDRESS_PAL_CHAR, verbose);
                        _SaveCharDataToDisk(playerDataFromClient, dataFromRam);
                        await _SendMessageBoxAndWaitForAccept(clientConnection, @"Character data was backed up succesfully.", verbose);
                        break;
                    case MainMenuItemId.Load:
                        await _DoLoadMenuLoop(clientConnection, playerDataFromClient, verbose);
                        break;
                    case MainMenuItemId.Exit:
                        exit = true;
                        break;
                    default:
                        break;
                }
            }
        }

        enum LoadMenuItemId : uint
        {
            Exit = 0xFFFFFFFF,
        };

        private const int SIZE_BYTES_SAVEGAME_WITHOUT_CMODE = 1056;
        private const int SIZE_BYTES_SAVEGAME = 1656;
        private static async Task _DoLoadMenuLoop(IPsoServerClientConntection clientConnection, byte[] playerDataFromClient, bool verbose)
        {
            while (true)
            {
                var saveGames = Directory.GetFiles(".", "*.psosave")
                    .Select(x => XmlSerializeHelper.FromXml<PsoSaveGame>(x))
                    .ToArray();
                var characterGroups = saveGames
                    .GroupBy(x => x.CharName)
                    .ToArray();
                var loadMenuItems = characterGroups
                    .Select((x, idx) => new LibPSO.Packets.MenuItem()
                    {
                        Name = x.Key,
                        MenuId = 0,
                        ItemId = (UInt32)idx,
                        Flags = 0x0004,
                    })
                    .ToList();

                loadMenuItems.Insert(0, new LibPSO.Packets.MenuItem() { Name = "<- Main Menu", MenuId = 0, ItemId = (UInt32)LoadMenuItemId.Exit, Flags = 0x0004, });
                //send menu
                var charMenuResult = await _ShowMenu(
                    clientConnection,
                    loadMenuItems,
                    null,
                    verbose);

                if (charMenuResult == (UInt32)LoadMenuItemId.Exit)
                {
                    Console.WriteLine("exit load menu");
                    break;
                }
                //user chose a character
                if (charMenuResult < characterGroups.Length)
                {
                    var characterSaves = characterGroups[charMenuResult];

                    var saved = await _DoLoadCharacterLoadMenuLoop(clientConnection, playerDataFromClient, characterSaves, verbose);
                    if (saved)
                    {
                        Console.WriteLine("character saved - exit load menu");
                        break;
                    }
                }
            }
        }

        private static async Task<bool> _DoLoadCharacterLoadMenuLoop(IPsoServerClientConntection clientConnection, byte[] playerDataFromClient, IGrouping<string, PsoSaveGame> characterSaves, bool verbose)
        {
            while (true)
            {
                var loadMenuItems = characterSaves
                    .Select((x, idx) => new LibPSO.Packets.MenuItem()
                    {
                        Name = String.Format(@"{0:g}", x.SaveTime),
                        MenuId = 0,
                        ItemId = (UInt32)idx,
                        Flags = 0x0004,
                    })
                    .ToList();
                loadMenuItems.Insert(0, new LibPSO.Packets.MenuItem() { Name = String.Format("<- {0}", characterSaves.Key), MenuId = 0, ItemId = (UInt32)LoadMenuItemId.Exit, Flags = 0x0004, });
                //send menu
                var charSaveGameMenuResult = await _ShowMenu(
                    clientConnection,
                    loadMenuItems,
                    null,
                    verbose);

                if (charSaveGameMenuResult == (UInt32)LoadMenuItemId.Exit)
                {
                    Console.WriteLine("exit character load menu");
                    return false;
                }
                //user chose a savegame
                if (charSaveGameMenuResult < characterSaves.Count())
                {
                    var saveGame = characterSaves.Skip((int)charSaveGameMenuResult).FirstOrDefault();
                    
                    var restorePlayer = Player.FromBytes(saveGame.DataSentByClient);
                    var currentPlayer = Player.FromBytes(playerDataFromClient);

                    var restoreDataFull = restorePlayer.GetBytes().ToArray();

                    var characterNameRamBytes = await _GetCurrentCharacterName(clientConnection, BASE_ADDRESS_PAL_CHAR, verbose);
                    var charNameFromRam = _GetCharNameFromNameBytes(characterNameRamBytes);

                    var currentPlayerNameString = _GetCharNameFromNameBytes(currentPlayer.name);
                    var warningMessage = currentPlayerNameString == charNameFromRam ?
                        "" : @"
WARNING - name mismatch - this could indicate wrong address.
DO NOT RESTORE if you aren't sure.
";
                    var confirmMessage = String.Format(@"[CURRENT_NAME_RAM]:
'{0}'{1}
[CURRENT (as send at login)]:
Name: {2}
Level: {3}
[SAVED - {4:g}]:
Name: {5}
Level:{6}", 
charNameFromRam, 
warningMessage,
currentPlayerNameString,
currentPlayer.level,
saveGame.SaveTime,
_GetCharNameFromNameBytes(restorePlayer.name),
restorePlayer.level
);
                    bool okToOverwrite = await _ShowConfirmMenu(clientConnection, confirmMessage, verbose);
                    if (!okToOverwrite)
                    {
                        return false;
                    }
                    var charData = restoreDataFull
                        .Take(SIZE_BYTES_SAVEGAME_WITHOUT_CMODE)//0x3c0)//0x3c0 -> without config
                        .ToArray();

                    var patch = new PsoPatchDefinition();
                    patch.Patches.Add(new XmlPatchDefinition() { Address = 2163374112, ByteValues = charData });

                    var program = patch.GetPatchProgram();
                    var programPacket = Packets.GetUpdateCodePacket(program, ClientType.Gamecube);

                    await clientConnection.SendMessageToClient(programPacket, true);
                    Console.WriteLine("Sent update char data patch");
                    if (verbose)
                    {
                        Console.WriteLine("update char data patch:");
                        Console.WriteLine(HexStringFormater.GetHexText(programPacket, 0, 16, 0));
                    }
                    //read answer
                    var message = await clientConnection.ReadMessageFromClient();
                    Console.WriteLine("Received answer to char data patch");
                    if (verbose)
                    {
                        Console.WriteLine("Content of received message:");
                        Console.WriteLine(HexStringFormater.GetHexText(message.MessageCrypted, 0, 16, 0));
                    }
                    await _SendMessageBoxAndWaitForAccept(clientConnection, @"Character data was restored successfully..", verbose);
                    return true;
                }
            }
        }

        private enum ConfirmMenuItemId : uint
        {
            No,
            Yes,
        };
        private static async Task<bool> _ShowConfirmMenu(IPsoServerClientConntection clientConnection, string message, bool verbose)
        {
            var result = await _ShowMenu(
                clientConnection, new[] 
                {
                    new LibPSO.Packets.MenuItem() { Name = "No", MenuId = 0, ItemId = (UInt32)ConfirmMenuItemId.No, Flags = 0x0004, },
                    new LibPSO.Packets.MenuItem() { Name = "Yes", MenuId = 0, ItemId = (UInt32)ConfirmMenuItemId.Yes, Flags = 0x0004, },
                },
                message, 
                verbose);


            ConfirmMenuItemId id = (ConfirmMenuItemId)result;
            switch (id)
            {
                case ConfirmMenuItemId.Yes:
                    return true;
                case ConfirmMenuItemId.No:
                    return false;
                default:
                    throw new Exception("Unexpected Menu Id");
            }
        }

        private static async Task<UInt32> _ShowMenu(IPsoServerClientConntection clientConnection, IEnumerable<Packets.MenuItem> menueItems, string messageBeforeMenu, bool verbose)
        {
            if (!String.IsNullOrWhiteSpace(messageBeforeMenu))
            {
                await _SendMessageBoxAndWaitForAccept(clientConnection, messageBeforeMenu, verbose);
            }
            //send menu
            var menuMessage = Packets.GetBlockListMenuPacket(menueItems, ClientType.Gamecube);

            await clientConnection.SendMessageToClient(menuMessage, true);
            Console.WriteLine("Sent menu");
            if (verbose)
            {
                Console.WriteLine("menu:");
                Console.WriteLine(HexStringFormater.GetHexText(menuMessage, 0, 16, 0));
            }

            //read answer
            var answer = await clientConnection.ReadMessageFromClient();
            Console.WriteLine("Received answer to menu");
            if (verbose)
            {
                Console.WriteLine("Content of received message:");
                Console.WriteLine(HexStringFormater.GetHexText(answer.MessageCrypted, 0, 16, 0));
            }

            var res = MenuResultPackage.FromBytes(answer.MessageCrypted, ClientType.Gamecube);

            return res.ItemId;
        }

        private const UInt32 OFFSET_NAME = 880;
        private const UInt32 SIZE_NAME = 16;
        private static async Task<byte[]> _GetCurrentCharacterName(IPsoServerClientConntection clientConnection, UInt32 baseAddress, bool verbose)
        {
            Console.WriteLine("Reading RAM from client (charactername).");
            List<byte> data = new List<byte>();
            for (UInt32 i = 0; i < SIZE_NAME; i += 4)
            {
                var result = await _ReadRamFromClient(clientConnection, baseAddress + OFFSET_NAME + i);
                data.AddRange(Helper.GetBytes(result));
                Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine("Done reading RAM from client (charactername).");
            var rVal = data.ToArray();
            if (verbose)
            {
                Console.WriteLine("Content of received RAM:");
                Console.WriteLine(HexStringFormater.GetHexText(rVal, 0, 16, 0));
            }
            return rVal;
        }

        private static async Task<byte[]> _LoadCharDataFromClient(IPsoServerClientConntection clientConnection, UInt32 baseAddress, bool verbose)
        {
            Console.WriteLine("Reading RAM from client.");
            List<byte> data = new List<byte>();
            for (UInt32 i = 0; i < SIZE_BYTES_SAVEGAME; i += 4)
            {
                var value = await _ReadRamFromClient(clientConnection, baseAddress + i);
                data.AddRange(Helper.GetBytes(value));
                Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine("Done reading RAM from client.");
            var rVal = data.ToArray();
            if (verbose)
            {
                Console.WriteLine("Content of received RAM:");
                Console.WriteLine(HexStringFormater.GetHexText(rVal, 0, 16, 0));
            }
            return rVal;
        }

        private static async Task<UInt32> _ReadRamFromClient(IPsoServerClientConntection clientConnection, UInt32 address)
        {
            var program = PsoReadMemProgram.GetPsoReadMemProgram(address);
            var packet = Packets.GetUpdateCodePacket(program, ClientType.Gamecube, 0);
            await clientConnection.SendMessageToClient(packet, true);
            var resultPacket = await clientConnection.ReadMessageFromClient();

            var result = UpdateCodePackageResultPackage.FromBytes(resultPacket.MessageCrypted, ClientType.Gamecube);
            var resultRam = result.ReturnValue;
            return resultRam;
        }

        private static void _SaveCharDataToDisk(byte[] dataSentByClient, byte[] playerDataFromRam)
        {
            var nameBytes = dataSentByClient.Skip(880).Take(16);
            var charName = _GetCharNameFromNameBytes(nameBytes);
            PsoSaveGame sg = new PsoSaveGame()
            {
                CharName = charName,
                SaveTime = DateTime.Now,
                DataReadFromRam = playerDataFromRam,
                DataSentByClient = dataSentByClient,
            };
            var baseFilename = String.Format(@"{0:yyyyMMdd HHmmss}", DateTime.Now);
            var filename = String.Format(@"{0}.psosave", baseFilename);
            XmlSerializeHelper.ToXml(filename, sg);
        }

        private static string _GetCharNameFromNameBytes(IEnumerable<byte> nameBytes)
        {
            var charName = Encoding.ASCII.GetString(nameBytes.Where(x => x >= ' ').ToArray());
            return charName;
        }
    }
}

This project started after after AleronIves told me about his work-in-progress patches for the Gamecube version of PSO.

Apparently all version of "Phantasy Star Online" had a builtin patching system, which allowed to run code sent by the servers.
This feature however was never officially used to my knowledge, the only prior use was to use it to boot homebrew code on the Gamecube.

Two programs (to my knowledge) were released, which allowed you to upload homebrew programs to the gamecube:
- PSOLoader
- PSUL

Other information wasn't available on the internet.

So i installed a bunch of programs (Gamecube emulator with debugging, network traffic sniffer etc.) and started analyzing the network traffic and the assembly code running....
After a couple of days of staring on HEX and ASSEMBLY I finally figured out enough to reproduce the patching protocoll.

If you are only interested in how you could create the patching messages, have a look at:
[SOURCE_ROOT]/PaketDescribed.rtf 
	-> mixed english and german, sorry about that - skip to the HEX as english speaker, and probably miss most info
[SOURCE_ROOT]/LibPSO/Packets.cs
	-> look at method: public static byte[] GetUpdateCodePacket(byte[] code, ClientType clientType, byte flags = 0)
[SOURCE_ROOT]/LibPSO/PacketDefinitions/UpdateCodePackage.cs
	-> GetBytes() creates the package.
	-> this is where .Net sucks really....lots of code just to map a struct to a byte array...hmpf...
	-> I know this could have been done cleaner in C# as well....but you know how it is.... ;)
[SOURCE_ROOT]/LibPSO/PacketDefinitions/UpdateCodePackageResultPackage.cs
	-> The code which is run on gamecube can even return an 32bit value.



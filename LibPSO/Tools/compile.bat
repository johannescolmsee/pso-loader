C:\devkitPro\devkitPPC\bin\powerpc-eabi-as.exe -o obj.o %1%
C:\devkitPro\devkitPPC\bin\powerpc-eabi-ld.exe -Ttext 0x80000000 obj.o
C:\devkitPro\devkitPPC\bin\powerpc-eabi-objcopy.exe -O binary obj.o %2% 
del obj.o
del a.out




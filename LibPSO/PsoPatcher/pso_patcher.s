.globl _main
_main:

#get the address of the data portion
		mflr 5                      #copy linkregister to r5
		bl _end						#"call" to "endf"
		mflr 4						#register 4 contains address of datastart now (one opcode after "blrl")
		mtlr 5						#copy back old linkregister value


		lwz 5, 0(4)					#load count of patches to register 5
		cmplwi 5, 0					#compare to zero
		ble _ret                    #if 0 patches branch to _ret (return early)
		mtctr 5                     #move number of patches to ctr

#loop patches
_blp:	lwzu 7, 0x4(4)				#load the address and type to be patched (type is bit 31 and 30 - upper two bits)
		srwi  6, 7, 30        		#extract type into r6 (shift right)
		rlwinm 7, 7, 0, 2, 31		#mask out top two bits
		oris 7, 7, 0x8000   		#fix address (cached range)
		cmplwi 6, 3					#compare type with 3 (3 = range)
		beq _rword					#patch type: range of words
		lwzu 8, 0x4(4)              #load the value
		cmplwi 6, 1					#compare type with 1
		blt _byte					# less than ( = 0) -> byte
		beq _hword					# equal ( = 1) -> halfword
		stw  8, 0x0(7)              #patch it (word)
		b _elp   					#go to end of loop
_hword: sth  8, 0x0(7)				#store byte
		b _elp  					#go to end of loop
_byte:  stb  8, 0x0(7)				#store byte
		b _elp  					#go to end of loop
_rword: lwzu 9, 0x4(4)				#load the count of the range
		li   10, 0					#set r10 = 0 (use as counter)
		cmplwi 9, 0					#compare with 0
		beq _elp					#0 values go to end of loop
		mulli 9, 9, 4				#4 bytes stride
#range patch loop
_brlp:	lwzu 8, 0x4(4)				#load the value
		stwx 8, 10, 7				#store at address r10(7) (indexed)
		addi 10, 10, 4				#increment the counter
		cmplw 10, 9					#compare counter with length
		bne _brlp					#branch unless equal
_elp:	bdnz _blp                   #decrement count branch if not zero



_ret:   mr 3, 5                     #copy # of patches applied to return value
		sync						#write back the cache
		blr							#return

#this line is only to determine the end of the code section. after this opcode the data will begin.
_end:	blrl						#return to linkregister, set link register


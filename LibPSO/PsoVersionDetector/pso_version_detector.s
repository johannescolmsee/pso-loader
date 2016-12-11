.globl _main
_main:

#get the address of the data portion
			mflr 5                      #copy linkregister to r5
			bl _end						#"call" to "endf"
			mflr 4						#register 4 contains address of datastart now (one opcode after "blrl")
			mtlr 5						#copy back old linkregister value

			lwz 3, 0(4)					#load the default return value to r3
			lwzu 5, 4(4)				#load count of checks to register 5
			cmplwi 5, 0					#compare to zero
			ble _ret                    #if 0 checks branch to _ret (return early)
			mtctr 5                     #move number of checks to ctr

#loop checks
_blp:		lwzu 7, 0x4(4)				#load the address
			lwzu 8, 0x4(4)              #load the compare value
			lwzu 9, 0(7)				#load the value at address
			cmplw 8, 9					#compare
			beq _match					#match!
			addi 4, 4, 0x4				#skip the return value, since we did not find a match

_endloop:	bdnz _blp                   #decrement count branch if not zero
			b _ret						#return (r3 has the default return value already)
_match:
			lwzu 3, 0x4(4)				# load the return value

_ret:		blr							#return

#this line is only to determine the end of the code section. after this opcode the data will begin.
_end:		blrl						#return to linkregister, set link register


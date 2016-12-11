.globl _main
_main:

#get the address of the data portion
		mflr 5                      #copy linkregister to r5
		bl _end						#"call" to "endf"
		mflr 4						#register 4 contains address of datastart now (one opcode after "blrl")
		mtlr 5						#copy back old linkregister value


		lwz 5, 0(4)					#load the address
		lwz 3, 0(5)					#load the value (to the return resgister)
		blr							#return

#this line is only to determine the end of the code section. after this opcode the data will begin.
_end:	blrl						#return to linkregister, set link register


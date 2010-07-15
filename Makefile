# Default, build with debugging symbols
lib:
	csc @dev.rsp UseCanonicalNameModule.cs
	
# production version: No debugging symbols, optimized
libprod:
	csc @prod.rsp UseCanonicalNameModule.cs
	
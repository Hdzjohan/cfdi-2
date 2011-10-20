using System;

namespace omarkhd.Cfdi
{
	public class CFDException : SystemException
	{
		public CFDException() : base() {}
		public CFDException(string desc) : base(desc) {}
	}
	
	public class MalformedCfdException : CFDException {}
}
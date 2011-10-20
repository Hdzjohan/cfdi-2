using System;

namespace omarkhd.Cfdi
{
	public class Receptor : Contribuyente
	{
		public Receptor() {}
		public Receptor(string nombre, string rfc) : base(nombre, rfc)
		{
			//this.TagName = "Receptor";
		}
	}
}

using System;
namespace omarkhd.Cfdi
{
	public class Emisor : Contribuyente
	{
		public Emisor() {}
		public Emisor(string nombre, string rfc) : base(nombre, rfc)
		{
			//this.TagName = "Emisor";
		}
	}
}
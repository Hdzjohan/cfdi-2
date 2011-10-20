using System;
using System.Xml;

namespace omarkhd.Cfdi
{
	public abstract class Contribuyente // : XmlExportable
	{
		public string Nombre { get; set; }
		public string RFC { get; set; }
		public DomicilioFiscal Domicilio { get; set; }
		//protected string TagName;
		
		public Contribuyente()
		{
			this.Domicilio = new DomicilioFiscal();
		}
		
		public Contribuyente(string nombre, string rfc) : base()
		{
			this.Nombre = nombre;
			this.RFC = rfc;
		}
		
		/*public void WriteToXml(XmlTextWriter writer)
		{
			if(!this.Validar().Status)
				throw new MalformedCfdException();
				
			writer.WriteStartElement(this.TagName);
			writer.WriteAttributeString("rfc", Utilidades.PurgeString(this.RFC));
			writer.WriteAttributeString("nombre", Utilidades.PurgeString(this.Nombre));
			writer.WriteEndElement();
		}*/
	}
}
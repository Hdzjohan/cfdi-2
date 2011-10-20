using System;
using System.Xml;

namespace omarkhd.Cfdi
{
	public class Concepto // : XmlExportable
	{
		public int Cantidad;			//req
		public string Unidad;					//opt
		public string NoIdentificacion;		//opt
		public string Descripcion;			//req
		public double ValorUnitario;			//req
		public double Importe;
		
		public void AutoCalcular()
		{
			this.Importe = (double) this.Cantidad * this.ValorUnitario;
		}
		
		/*public void WriteToXml(XmlTextWriter writer)
		{
		}*/
	}
}
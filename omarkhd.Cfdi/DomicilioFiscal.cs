using System;
namespace omarkhd.Cfdi
{
	public class DomicilioFiscal
	{
		private string _MunicipioDelegacion;
		
		public string Nombre { get; set; }
		public int NoExterior { get; set; }
		public string Colonia { get; set; }
		public string Ciudad { get; set; }
		public string Estado { get; set; }
		public int CodigoPostal { get; set; }
		public string Pais { get; set; }
		public string Calle;
		
		public string Municipio
		{
			get { return this._MunicipioDelegacion; }
			set { this._MunicipioDelegacion = value; }
		}
		
		public string Delegacion
		{
			get { return this._MunicipioDelegacion; }
			set { this._MunicipioDelegacion = value; }
		}
		
		public DomicilioFiscal()
		{
			this.Pais = "MÉXICO";
		}
	}
}
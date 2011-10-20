using System;
using System.Xml;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using omarkhd.DataStructures;

namespace omarkhd.Cfdi
{
	public class Cfdi
	{
		public static string Namespace;
		public static X509Certificate2 CertificadoPfxDefault;
		public static Timbrado TimbradoDefault;
		
		static Cfdi()
		{
			Namespace = "http://www.sat.gob.mx/cfd/3";
			CertificadoPfxDefault = null;
			TimbradoDefault = null;
		}
		
		public string Version { get; private set; }
		public string TipoDeComprobante;
		public string Serie;
		public string FormaDePago;		
		public string NoCertificado;
		public string Certificado;
		public string CondicionesDePago;
		public string MotivoDescuento;
		public string MetodoDePago;
		
		public DateTime Fecha;
		
		public ulong Folio;	
			
		public int NoAprobacion;
		public int AñoAprobacion;
		
		public double Total;
		public double SubTotal;		
		
		public Emisor Emisor;
		public Receptor Receptor;
		
		public ConceptosHandler Conceptos;
		public ImpuestosHandler Impuestos;
		
		//stamping information
		public string CadenaOriginal;
		public byte[] Firma { get; private set; }
		public string Sello;
		
		//timbrado
		public Timbre Timbre;
		
		public Cfdi()
		{
			this.Init();
		}
		
		private void Init()
		{
			this.Version = "3.0";
			this.Emisor = new Emisor();
			this.Receptor = new Receptor();
			this.Conceptos = new ConceptosHandler();
			this.Impuestos = new ImpuestosHandler();
			this.Timbre = null;
		}
		
		public void Sellar(X509Certificate2 cert)
		{
			string cadena_original = Util.Sellado.GenerarCadenaOriginal(this);
			byte[] firma = Util.Sellado.GenerarFirma(cert, cadena_original);
			string sello = Util.Sellado.GenerarSello(firma);
						
			this.CadenaOriginal = cadena_original;
			this.Sello = sello;
			this.NoCertificado = Util.Sellado.GenerarNoSerie(cert);
			this.Certificado = Util.Sellado.GenerarCertificadoPuro64(cert);
		}
		
		public void Sellar()
		{
			this.Sellar(Cfdi.CertificadoPfxDefault);
		}
		
		public static Cfdi DesdeXml(string xml)
		{
			return Util.Xml.GenerarCfdi(xml);	
		}
		
		public string ConstruirXml()
		{
			return Util.Xml.GenerarXml(this);
		}
		
		public string GuardarXml(string ruta, bool escribir_bom)
		{
			string xml = this.ConstruirXml();
			Util.WriteToFile(xml, ruta, escribir_bom);
			return xml;
		}
		
		public string GuardarXml(string ruta)
		{
			return this.GuardarXml(ruta, true);
		}
		
		public bool Timbrar(Timbrado timbrado)
		{
			Timbre t = timbrado.Timbrar(this);
			if(t == null)
				return false;
				
			this.Timbre = t;
			return true;
		}
		
		public bool Timbrar()
		{
			return this.Timbrar(Cfdi.TimbradoDefault);
		}
	}
}


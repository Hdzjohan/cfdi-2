using System;
using System.Security.Cryptography.X509Certificates;
using System.Web.Services;
using System.IO;
using System.Text;

namespace omarkhd.Cfdi
{
	public class TimbradoDiverza : Timbrado
	{
		public string Url;
		public string RfcEmisor;
		public string RfcReceptor;
		public string Version;
		
		public TimbradoDiverza(X509Certificate2 cert) : base(cert)
		{
			this.Url = "https://demotf.buzonfiscal.com/timbrado?wsdl";
			this.Version = "3.0";
		}
		
		public override Timbre Timbrar(Cfdi cfdi)
		{
			this.RfcEmisor = cfdi.Emisor.RFC;
			this.RfcReceptor = cfdi.Receptor.RFC;
			return this.Timbrar(cfdi.ConstruirXml());
		}
		
		public override Timbre Timbrar(string xml)
		{
			Encoding encoding = new UTF8Encoding(false);
			MemoryStream stream = new MemoryStream();
			TextWriter writer = new StreamWriter(stream, encoding);
			writer.Write(xml);
			writer.Flush();
			byte[] xml_bin = stream.ToArray();
			writer.Close();
			return this.TimbrarBin(xml_bin);
		}
		
		public Timbre TimbrarArchivo(string path)
		{
			if(!File.Exists(path))
				return null;
				
			FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
			byte[] bin = new byte[fs.Length];
			Console.Out.WriteLine(fs.Read(bin, 0, bin.Length));
			return TimbrarBin(bin);
		}
		
		private Timbre TimbrarBin(byte[] cfdi)
		{
			this.Error = string.Empty;
			RequestTimbradoCFDType request = new RequestTimbradoCFDType();
			request.InfoBasica = new InfoBasicaType();
			request.InfoBasica.RfcEmisor = this.RfcEmisor;
			request.InfoBasica.RfcReceptor = this.RfcReceptor;
			request.Documento = new DocumentoType();
			request.Documento.Tipo = DocumentoTypeTipo.XML;
			request.Documento.Version = this.Version;
			request.Documento.Archivo = cfdi;
			
			TimbradoCFDI proxy = new TimbradoCFDI();
			proxy.Url = this.Url;
			proxy.ClientCertificates.Add(this.Certificado);
			TimbreFiscalDigital tfd = null;
			
			try
			{
				System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, c, ch, e) => true;
				tfd = proxy.timbradoCFD(request);
			}
			
			catch(System.Web.Services.Protocols.SoapException e)
			{
				this.Error = e.Message;
				return null;
			}
			
			catch(Exception e)
			{
				this.Error = e.Message;
				return null;
			}
			
			Timbre timbre = new Timbre();
			timbre.FechaTimbrado = tfd.FechaTimbrado;
			timbre.NoCertificadoSat = tfd.noCertificadoSAT;
			timbre.SelloCfd = tfd.selloCFD;
			timbre.SelloSat = tfd.selloSAT;
			timbre.UUID = tfd.UUID;
			timbre.Version = tfd.version;
			
			return timbre;
		}
	}
}


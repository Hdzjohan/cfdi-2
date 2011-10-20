using System;
using System.Security.Cryptography.X509Certificates;

namespace omarkhd.Cfdi
{
	public abstract class Timbrado
	{
		protected X509Certificate2 Certificado;
		public string Error { get; protected set; }
		
		private Timbrado() {}
		public Timbrado(X509Certificate2 cert)
		{
			this.Certificado = cert;
		}
		
		public abstract Timbre Timbrar(Cfdi cfdi);
		public abstract Timbre Timbrar(string xml);
	}
}


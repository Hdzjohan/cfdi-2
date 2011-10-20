using System;

namespace omarkhd.Cfdi
{
	public class Timbre
	{
		public static string Namespace;
		
		static Timbre()
		{
			Namespace = "http://www.sat.gob.mx/TimbreFiscalDigital";
		}
		
		public string SelloSat;
		public string NoCertificadoSat;
		public string SelloCfd;
		public DateTime FechaTimbrado;
		public string UUID;
		public string Version;
		
		public Timbre() {}
	}
}


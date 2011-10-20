using System;
namespace omarkhd.Cfdi
{
	public abstract class AImpuesto
	{
		public AImpuesto() {}
		public AImpuesto(string impuesto, double importe)
		{
			this.Impuesto = impuesto;
			this.Importe = importe;
		}
		
		public string Impuesto;
		public double Importe;
	}
}


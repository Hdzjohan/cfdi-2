using System;
namespace omarkhd.Cfdi
{
	public class Traslado : AImpuesto
	{
		public double Tasa;
		
		public Traslado() {}
		public Traslado(string impuesto, double importe, double tasa) : base(impuesto, importe)
		{
			this.Tasa = tasa;
		}
	}
}


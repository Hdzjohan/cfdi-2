using System;
namespace omarkhd.Cfdi
{
	public class ImpuestosHandler
	{
		public ListWrapper<Retencion> Retenciones { get; private set; }
		public ListWrapper<Traslado> Traslados { get; private set; }
		public double TotalImpuestosRetenidos;
		public double TotalImpuestosTrasladados;
		
		public ImpuestosHandler ()
		{
			this.Retenciones = new ListWrapper<Retencion>();
			this.Traslados = new ListWrapper<Traslado>();
		}
		
		public void AutoCalcular()
		{
			double  total_retenciones = 0.0;
			for(int i = 0; i < this.Retenciones.Contar(); i++)
				total_retenciones += this.Retenciones[i].Importe;
			double total_traslados = 0.0;
			for(int i = 0; i < this.Traslados.Contar(); i++)
				total_traslados += this.Traslados[i].Importe;
				
			this.TotalImpuestosRetenidos = total_retenciones;
			this.TotalImpuestosTrasladados = total_traslados;
		}
	}
}


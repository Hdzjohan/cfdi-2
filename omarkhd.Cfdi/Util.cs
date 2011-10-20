using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;

namespace omarkhd.Cfdi
{
	public static class Util 
	{
		public static string Purge(string str)
		{
			if(string.IsNullOrEmpty(str))
				return string.Empty;
				
			string purged_str = string.Empty;
			
			if(str.Contains("|"))
			{
				for(int i = 0; i < str.Length; i++)
					if(str[i] != '|')
						purged_str += str[i];
			}
			
			else
				purged_str = str;
				
			string[] words = purged_str.Split(' ', '\n', '\t');
			purged_str = string.Empty;
			
			foreach(string word in words)
				if(word != string.Empty)
					purged_str += word + " ";
			purged_str = purged_str.Trim();
			return purged_str;
		}
		
		//what, where and how
		public static void WriteToFile(string xml, string path, bool bom)
		{
			Encoding encoding = new UTF8Encoding(bom);
			FileStream stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
			TextWriter writer = new StreamWriter(stream, encoding);	
			writer.Write(xml);
			writer.Flush();
			writer.Close();
			
		}
		
		public static void WriteToFile(string xml, string path)
		{
			WriteToFile(xml, path, true);
		}
		
		public static class Validacion
		{
			private static bool Success;
			public static string UltimoMensaje;
			
			public static bool Validar(Cfdi cfdi)
			{
				return Validar(cfdi.ConstruirXml());
			}
		
			public static bool Validar(string xml)
			{
				Success = true;
				UltimoMensaje = string.Empty;
				
	            XmlDocument doc = new XmlDocument();
            	doc.LoadXml(xml);
				string esquema_cfdi = "http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv3.xsd";
				//System.Xml.Schema
	            ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationCallback);
    	        doc.Schemas.Add("http://www.sat.gob.mx/cfd/3", esquema_cfdi);
        	    doc.Validate(eventHandler);
    	        return Success;
	        }
        
	       	static void ValidationCallback(Object sender, ValidationEventArgs e)
    	    {
        	    switch (e.Severity)
            	{
                	case XmlSeverityType.Error:
                		UltimoMensaje = e.Message;
	                    Success = false;
    	                break;
    	                
        	        case XmlSeverityType.Warning:
        	        	UltimoMensaje = e.Message;
                	    break;
       		    }
			} 
		}

		//inner class for signing cfdi
		public static class Sellado
		{
			public static string GenerarSello(byte[] firma)
			{
				string base64 = Convert.ToBase64String(firma);
				return base64;
			}
			
			public static byte[] GenerarFirma(X509Certificate2 cert, string cadena_original)
			{
				Encoding encode = new UTF8Encoding();
				byte[] bin = encode.GetBytes(cadena_original);
				
				SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
				RSACryptoServiceProvider rsa = (RSACryptoServiceProvider) cert.PrivateKey;
				byte[] signed_bin = rsa.SignData(bin, sha);
				
				return signed_bin;
			}
			
			public static string GenerarNoSerie(X509Certificate2 cert)
			{
				string serial_number = "";
				char[] chars = cert.SerialNumber.ToCharArray();
				for(int i = 1; i < chars.Length; i += 2)
					serial_number += chars[i];
				return serial_number;
			}
			
			public static string GenerarCertificadoPuro64(X509Certificate2 cert)
			{
				byte[] raw_cert = cert.GetRawCertData();
				return Convert.ToBase64String(raw_cert);
			}
			
			public static string GenerarCadenaOriginal(Cfdi cfdi)
			{
				/*
					por ahora sólo se tomarán en cuenta los atributos
					marcados como requeridos, por practicidad y falta
					de tiempo, aun así la factura debe poder ser
					timbrada sin ningún problema
				*/
				
				string cad = "||";
				
				//información del comprobante
				cad += cfdi.Version + "|";
				cad += GetPipeOf(cfdi.Fecha);
				cad += GetPipeOf(cfdi.TipoDeComprobante);
				cad += GetPipeOf(cfdi.FormaDePago);
				cad += GetPipeOf(cfdi.SubTotal);
				cad += GetPipeOf(cfdi.Total);
				
				//información del emisor
				cad += GetPipeOf(cfdi.Emisor.RFC);
				cad += GetPipeOf(cfdi.Emisor.Nombre);
			
				cad += GetPipeOf(cfdi.Emisor.Domicilio.Calle);
				cad += GetPipeOf(cfdi.Emisor.Domicilio.Municipio);
				cad += GetPipeOf(cfdi.Emisor.Domicilio.Estado);
				cad += GetPipeOf(cfdi.Emisor.Domicilio.Pais);
				cad += GetPipeOf(cfdi.Emisor.Domicilio.CodigoPostal);
				
				//información del receptor
				cad += GetPipeOf(cfdi.Receptor.RFC);
				cad += GetPipeOf(cfdi.Receptor.Nombre); //no es obligatorio, pero sin esto (según) el pac devuelve "error de comunicación con backend" :S
				
				//información de conceptos
				for(int i = 0; i < cfdi.Conceptos.Contar(); i++)
				{
					Concepto c = cfdi.Conceptos[i];
					cad += GetPipeOf(c.Cantidad);
					cad += GetPipeOf(c.Descripcion);
					cad += GetPipeOf(c.ValorUnitario);
					cad += GetPipeOf(c.Importe);
				}
				
				//información de impuestos
				int retentions = cfdi.Impuestos.Retenciones.Contar();
				int traslations = cfdi.Impuestos.Traslados.Contar();
				
				for(int i = 0; i < retentions; i++)
				{
					Retencion r = cfdi.Impuestos.Retenciones[i];
					cad += GetPipeOf(r.Impuesto);
					cad += GetPipeOf(r.Importe);
				}
				cad += retentions > 0 ? GetPipeOf(cfdi.Impuestos.TotalImpuestosRetenidos) : "";
				
				for(int i = 0; i < traslations; i++)
				{
					Traslado t = cfdi.Impuestos.Traslados[i];
					cad += GetPipeOf(t.Impuesto);
					cad += GetPipeOf(t.Tasa);
					cad += GetPipeOf(t.Importe);
				}
				cad += traslations > 0 ? GetPipeOf(cfdi.Impuestos.TotalImpuestosTrasladados) : "";
				
				cad += "|";
				return cad;	
			}
			
			/*
				todos los métodos siguientes son usados para la conversión
				de datos del cfdi a su versión string con sus respectivos
				valores por default para los datos obligatorios
			*/
			
			private static string GetPipeOf(string str)
			{
				return Purge(str) + "|";
			}
			
			private static string GetPipeOf(double dbl)
			{
				return string.Format("{0:0.00}", dbl) + "|";
			}
			
			private static string GetPipeOf(int i)
			{
				return i + "|";
			}
			
			private static string GetPipeOf(DateTime datetime)
			{
				return datetime.ToString("yyyy-MM-ddThh:mm:ss") + "|";
			}
		}
		
		public static class Xml
		{
			public static string GenerarXml(Cfdi cfdi)
			{
				MemoryStream stream = new MemoryStream();
				System.Text.Encoding encoding = new System.Text.UTF8Encoding();
				XmlTextWriter writer = new XmlTextWriter(stream, encoding);
				writer.WriteStartDocument();
				
				//settings
				//writer.Namespaces = true;
				string pf = "cfdi";
				string ns = Cfdi.Namespace;
				
				/*
					inicio de la generación del xml cfdi v3
				*/
				
				writer.WriteStartElement(pf, "Comprobante", ns);
				writer.WriteAttributeString("version", cfdi.Version);
				writer.WriteAttributeString("fecha", StringOf(cfdi.Fecha));
				writer.WriteAttributeString("tipoDeComprobante", StringOf(cfdi.TipoDeComprobante));
				writer.WriteAttributeString("formaDePago", StringOf(cfdi.FormaDePago));
				writer.WriteAttributeString("subTotal", StringOf(cfdi.SubTotal));
				writer.WriteAttributeString("total", StringOf(cfdi.Total));
				writer.WriteAttributeString("sello", StringOf(cfdi.Sello));
				writer.WriteAttributeString("noCertificado", StringOf(cfdi.NoCertificado));
				writer.WriteAttributeString("certificado", StringOf(cfdi.Certificado));
				
				//Emisor
				writer.WriteStartElement(pf, "Emisor", ns);
				writer.WriteAttributeString("rfc", StringOf(cfdi.Emisor.RFC));
				writer.WriteAttributeString("nombre", StringOf(cfdi.Emisor.Nombre));
				
				//Emisor -> Domicilio Fiscal
				writer.WriteStartElement(pf, "DomicilioFiscal", ns);
				writer.WriteAttributeString("calle", StringOf(cfdi.Emisor.Domicilio.Calle));
				writer.WriteAttributeString("municipio", StringOf(cfdi.Emisor.Domicilio.Municipio));
				writer.WriteAttributeString("estado", StringOf(cfdi.Emisor.Domicilio.Estado));
				writer.WriteAttributeString("pais", StringOf(cfdi.Emisor.Domicilio.Pais));
				writer.WriteAttributeString("codigoPostal", StringOf(cfdi.Emisor.Domicilio.CodigoPostal));
				writer.WriteEndElement(); //end of Emisor -> Domicilio Fiscal
				writer.WriteEndElement(); //end of Emisor
				
				//Receptor
				writer.WriteStartElement(pf, "Receptor", ns);
				writer.WriteAttributeString("rfc", StringOf(cfdi.Receptor.RFC));
				writer.WriteAttributeString("nombre", StringOf(cfdi.Receptor.Nombre)); //no es obligatorio, misma razón que en cadena original
				writer.WriteEndElement(); //end of Receptor
				
				writer.WriteStartElement(pf, "Conceptos", ns);
				for(int i = 0; i < cfdi.Conceptos.Contar(); i++)
				{
					Concepto c = cfdi.Conceptos[i];
					writer.WriteStartElement(pf, "Concepto", ns);
					writer.WriteAttributeString("cantidad", StringOf(c.Cantidad));
					writer.WriteAttributeString("descripcion", StringOf(c.Descripcion));
					writer.WriteAttributeString("valorUnitario", StringOf(c.ValorUnitario));
					writer.WriteAttributeString("importe", StringOf(c.Importe));
					writer.WriteEndElement(); //end of every Concepto c
				}				
				writer.WriteEndElement(); //end of Conceptos
				
				int retenciones = cfdi.Impuestos.Retenciones.Contar();
				int traslados = cfdi.Impuestos.Traslados.Contar();
				
				if(retenciones + traslados > 0)
				{
					writer.WriteStartElement(pf, "Impuestos", ns);
					if(retenciones > 0)
						writer.WriteAttributeString("totalImpuestosRetenidos", StringOf(cfdi.Impuestos.TotalImpuestosRetenidos));
					if(traslados > 0)
						writer.WriteAttributeString("totalImpuestosTrasladados", StringOf(cfdi.Impuestos.TotalImpuestosTrasladados));
						
					if(retenciones > 0)
					{
						writer.WriteStartElement(pf, "Retenciones", ns);
						for(int i = 0; i < retenciones; i++)
						{
							Retencion r = cfdi.Impuestos.Retenciones[i];
							writer.WriteStartElement(pf, "Retencion", ns);
							writer.WriteAttributeString("impuesto", StringOf(r.Impuesto));
							writer.WriteAttributeString("importe", StringOf(r.Importe));
							writer.WriteEndElement(); //end of every Retencion r
						}
						writer.WriteEndElement(); //end of retenciones
					}
					
					if(traslados > 0)
					{
						writer.WriteStartElement(pf, "Traslados", ns);
						for(int i = 0; i < traslados; i++)
						{
							Traslado t = cfdi.Impuestos.Traslados[i];
							writer.WriteStartElement(pf, "Traslado", ns);
							writer.WriteAttributeString("impuesto", StringOf(t.Impuesto));
							writer.WriteAttributeString("tasa", StringOf(t.Tasa));
							writer.WriteAttributeString("importe", StringOf(t.Importe));
							writer.WriteEndElement(); //end of every Traslado t
						}
						writer.WriteEndElement(); //end of traslados
					}
					writer.WriteEndElement(); //end of Impuestos
				}
				
				//checking if it has a 'timbrado'
				if(cfdi.Timbre != null)
				{
					string t_pf = "tfd";
					string t_ns = Timbre.Namespace;
									
					writer.WriteStartElement(pf, "Complemento", ns);
					writer.WriteStartElement(t_pf, "TimbreFiscalDigital", t_ns);
					writer.WriteAttributeString("selloSAT", StringOf(cfdi.Timbre.SelloSat));
					writer.WriteAttributeString("noCertificadoSAT", StringOf(cfdi.Timbre.NoCertificadoSat));
					writer.WriteAttributeString("selloCFD", StringOf(cfdi.Timbre.SelloCfd));
					writer.WriteAttributeString("FechaTimbrado", StringOf(cfdi.Timbre.FechaTimbrado));
					writer.WriteAttributeString("UUID", StringOf(cfdi.Timbre.UUID));
					writer.WriteAttributeString("version", StringOf(cfdi.Timbre.Version));
					writer.WriteEndElement(); //end of TimbreFiscalDigital
					writer.WriteEndElement(); //end of Complemento
				}
				
				writer.WriteEndElement(); //end of Comprobante
				writer.Flush();
				
				/*
					fin de las operaciones de generación del xml
				*/
				
				StreamReader reader = new StreamReader(stream);
				stream.Position = 0;
				string xml = reader.ReadToEnd();
				
				writer.Close();
				return xml;
			}
			
			public static Cfdi GenerarCfdi(string xml)
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);
				Cfdi cfdi = new Cfdi();
				string prefix = doc.DocumentElement.GetPrefixOfNamespace(Cfdi.Namespace);
				prefix = string.IsNullOrEmpty(prefix) ? "" : prefix + ":";
			
				//invoice details
				XmlNode invoice = doc.DocumentElement;
				cfdi.TipoDeComprobante = GetAttr(invoice.Attributes["tipoDeComprobante"]);
				cfdi.Total = GetAttrDouble(invoice.Attributes["total"]);
				cfdi.SubTotal = GetAttrDouble(invoice.Attributes["subTotal"]);
				cfdi.Certificado = GetAttr(invoice.Attributes["certificado"]);
				cfdi.NoCertificado = GetAttr(invoice.Attributes["noCertificado"]);
				cfdi.FormaDePago = GetAttr(invoice.Attributes["formaDePago"]);
				cfdi.CondicionesDePago = GetAttr(invoice.Attributes["condicionesDePago"]);
				cfdi.Sello = GetAttr(invoice.Attributes["sello"]);
				cfdi.Fecha = GetAttrDate(invoice.Attributes["fecha"]);
				cfdi.Folio = GetAttrULong(invoice.Attributes["folio"]);
			
				Contribuyente[] parts = new Contribuyente[2];
				string[] parts_tags = new string[] { "Emisor", "Receptor" };
				parts[0] = cfdi.Emisor;
				parts[1] = cfdi.Receptor;
			
			
				//part_node and receiver details
			
				for(int i = 0; i < parts.Length; i++)
				{
					XmlNode part_node = GetFirstChild(invoice, prefix + parts_tags[i]);
					if(part_node != null)
					{
						parts[i].Nombre = GetAttr(part_node.Attributes["nombre"]);
						parts[i].RFC = GetAttr(part_node.Attributes["rfc"]);
						XmlNode part_addr_node = GetFirstChild(part_node, prefix + "DomicilioFiscal");
						if(part_addr_node != null)
						{
							DomicilioFiscal domicilio = parts[i].Domicilio;
							domicilio.CodigoPostal = GetAttrInt(part_addr_node.Attributes["codigoPostal"]);
							domicilio.Pais = GetAttr(part_addr_node.Attributes["pais"]);
							domicilio.Estado = GetAttr(part_addr_node.Attributes["estado"]);
							domicilio.Municipio = GetAttr(part_addr_node.Attributes["municipio"]);
							domicilio.Colonia = GetAttr(part_addr_node.Attributes["colonia"]);
							domicilio.NoExterior = GetAttrInt(part_addr_node.Attributes["noExterior"]);
							domicilio.Calle = GetAttr(part_addr_node.Attributes["calle"]);
						}
					}
				}
			
				XmlNode items_node = GetFirstChild(invoice, prefix + "Conceptos");
				if(items_node != null)
				{
					XmlNodeList items_list = items_node.ChildNodes;
					for(int i = 0; i < items_list.Count; i++)
					{
						XmlNode item_node = items_list[i];
						if(item_node.Name != prefix + "Concepto")
							continue;
							
						Concepto item = new Concepto();
						item.Cantidad = GetAttrInt(item_node.Attributes["cantidad"]);
						item.Descripcion = GetAttr(item_node.Attributes["descripcion"]);
						item.Importe = GetAttrDouble(item_node.Attributes["importe"]);
						item.Unidad = GetAttr(item_node.Attributes["unidad"]);
						item.ValorUnitario = GetAttrDouble(item_node.Attributes["valorUnitario"]);
						item.NoIdentificacion = GetAttr(item_node.Attributes["noIdentificacion"]);
						cfdi.Conceptos.Agregar(item);
					}
				}
			
				XmlNode taxes_node = GetFirstChild(invoice, prefix + "Impuestos");
				XmlNode retentions_node = null;
				XmlNode traslations_node = null;
				if(taxes_node != null)
				{
					retentions_node = GetFirstChild(taxes_node, prefix + "Retenciones");
					traslations_node = GetFirstChild(taxes_node, prefix + "Traslados");
					cfdi.Impuestos.TotalImpuestosRetenidos = GetAttrDouble(taxes_node.Attributes["totalImpuestosRetenidos"]);	
					cfdi.Impuestos.TotalImpuestosTrasladados = GetAttrDouble(taxes_node.Attributes["totalImpuestosTrasladados"]);	
				}
			
				if(retentions_node != null)
				{
					XmlNodeList child_nodes = retentions_node.ChildNodes;
					for(int i = 0; i < child_nodes.Count; i++)
					{
						XmlNode t_node = child_nodes[i];
						if(t_node.Name != prefix + "Retencion")
							continue;
						
						Retencion retencion = new Retencion();
						retencion.Impuesto = GetAttr(t_node.Attributes["impuesto"]);
						retencion.Importe = GetAttrDouble(t_node.Attributes["importe"]);
						cfdi.Impuestos.Retenciones.Agregar(retencion);
					}
				}
				
				if(traslations_node != null)
				{
					XmlNodeList child_nodes = traslations_node.ChildNodes;
					for(int i = 0; i < child_nodes.Count; i++)
					{
						XmlNode t_node = child_nodes[i];
						if(t_node.Name != prefix + "Traslado")
							continue;
						
						Traslado traslado = new Traslado();
						traslado.Impuesto = GetAttr(t_node.Attributes["impuesto"]);
						traslado.Importe = GetAttrDouble(t_node.Attributes["importe"]);
						traslado.Tasa = GetAttrDouble(t_node.Attributes["tasa"]);
						cfdi.Impuestos.Traslados.Agregar(traslado);
					}
				}
				
				return cfdi;
			}
			
			private static string StringOf(string str)
			{
				return Purge(str);
			}
			
			private static string StringOf(double dbl)
			{
				return string.Format("{0:0.00}", dbl);
			}
			
			private static string StringOf(int i)
			{
				return i + "";
			}
			
			private static string StringOf(DateTime datetime)
			{
				return datetime.ToString("yyyy-MM-ddThh:mm:ss");
			}
			
			//methods for dealing with the invoice xml
		
			public static XmlNode GetFirstChild(XmlNode node, string tag_name)
			{
				XmlNodeList list = node.ChildNodes;
				for(int i = 0; i < list.Count; i++)
					if(list[i].Name == tag_name)
						return list[i];
					
				return null;
			}
		
			public static string GetAttr(XmlAttribute attr)
			{
				return attr == null ? "" : attr.Value;
			}
		
			public static double GetAttrDouble(XmlAttribute attr)
			{
				string str = GetAttr(attr);
				double val = 0.0;
				double.TryParse(str, out val);
				return val;
			}
		
			public static DateTime GetAttrDate(XmlAttribute attr)
			{
				string str = GetAttr(attr);
				DateTime val = DateTime.Now;
				DateTime.TryParse(str, out val);
				return val;
			}
		
			public static ulong GetAttrULong(XmlAttribute attr)
			{
				string str = GetAttr(attr);
				ulong val = 0;
				ulong.TryParse(str, out val);
				return val;
			}
		
			public static int GetAttrInt(XmlAttribute attr)
			{
				string str = GetAttr(attr);
				int val = 0;
				int.TryParse(str, out val);
				return val;
			}
		}
	}
}
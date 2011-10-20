using System;
using omarkhd.DataStructures;

namespace omarkhd.Cfdi
{
	public class ListWrapper<T>
	{
		private IList<T> Items;
		
		public ListWrapper()
		{
			this.Items = new LinkedList<T>();
		}
		
		public void Agregar(T item)
		{
			this.Items.Add(item);
		}
		
		public T this[int index]
		{
			get { return this.Items[index]; }
			set { this.Items[index] = value; }
		}
		
		public T Obtener(int index)
		{
			return this.Items[index];
		}
		
		public void Eliminar(int index)
		{
			this.Items.RemoveAt(index);
		}
		
		public void EliminarTodos()
		{
			this.Items.Clear();
		}
		
		public int Contar()
		{
			return this.Items.Length;
		}
	}
}


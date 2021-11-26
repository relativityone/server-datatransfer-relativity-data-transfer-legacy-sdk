using System.Collections;
using System.Collections.Generic;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal class IDListDictionary<T> : IEnumerable<KeyValuePair<T, List<long>>>
	{
		private readonly Dictionary<T, List<long>> _dict;

		public IDListDictionary()
		{
			_dict = new Dictionary<T, List<long>>();
		}

		public IEnumerable<T> Keys => _dict.Keys;

		public void Add(T key, IEnumerable<long> value)
		{
			if (!_dict.ContainsKey(key))
			{
				_dict.Add(key, new List<long>());
			}

			_dict[key].AddRange(value);
		}

		public void Add(T key, long value)
		{
			if (!_dict.ContainsKey(key))
			{
				_dict.Add(key, new List<long>());
			}

			_dict[key].Add(value);
		}

		public void Add(KeyValuePair<T, List<long>> keyValuePair)
		{
			Add(keyValuePair.Key, keyValuePair.Value);
		}

		public void Add(IDListDictionary<T> listDictionary)
		{
			foreach (KeyValuePair<T, List<long>> keyValuePair in listDictionary._dict)
				Add(keyValuePair);
		}

		public List<long> Item(T key)
		{
			return _dict[key];
		}

		public IEnumerator<KeyValuePair<T, List<long>>> GetEnumerator()
		{
			return _dict.GetEnumerator();
		}

		public IEnumerator<KeyValuePair<T, List<long>>> IEnumerable_GetEnumerator() => GetEnumerator();	

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dict.GetEnumerator();
		}
	}
}
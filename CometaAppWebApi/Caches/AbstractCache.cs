using System;
using CometaAppWebApi.Caches.CachedModels;

namespace CometaAppWebApi.Caches
{
	public class AbstractCache<T>
	{
		//Inizializziamo tutti i dizionari.
		//Questo dizionario conterrà tutti gli ID degli oggetti di C#
		private Dictionary<String, Dictionary<Object, long>> _cache = new Dictionary<String, Dictionary<Object, long>>();
		//Questo dizionario ci permetterà di risalire all'oggetto partendo dall'ID di C#
		private Dictionary<long, CachedModel<T>> _references = new Dictionary<long, CachedModel<T>>();
		//Questo dizionario ci permetterà di risalire a tutti i nomi dei field di un oggetto salvati nella cache partendo dall'ID di C#
		private Dictionary<long, List<Object[]>> _stores = new Dictionary<long, List<Object[]>>();

		public AbstractCache()
		{

		}

		//Questo metodo serve per inizializzare correttamente il dizionario di un determinato field.
		private void createIfNotExists(String fieldName, Object key)
        {
            if (!_cache.ContainsKey(fieldName))
            {
                _cache[fieldName] = new Dictionary<Object, long>();
            }
        }

		//Questo metodo serve per inizializzare correttamente lo store dei field di un oggetto memorizzato
		//nella cache tramite l'ID degli oggetti di C#.
		private void createStoreIfNotExists(long refId)
		{
			if(!_stores.ContainsKey(refId))
			{
				_stores[refId] = new List<object[]>();
			}
		}

		//Questo metodo serve per inserire un oggetto nella cache.
		public void put(String fieldName, Object key, T obj)
		{
			if (obj == null) return;
			createIfNotExists(fieldName, key);
			createStoreIfNotExists(obj.GetHashCode());
			_references[obj.GetHashCode()] = new CachedModel<T>(obj);
			_stores[obj.GetHashCode()].Add(new Object[] { fieldName, key });
			_cache[fieldName][key] = obj.GetHashCode();
		}

		//Questo è il metodo interno per eliminare un oggetto dalla cache (utilizzare delete() e non _delete())
		private void _delete(String fieldName, Object key)
		{
			_cache[fieldName].Remove(key);
		}

        //Questo metodo serve per eliminare un oggetto della cache (compresi anche tutti i riferimenti fatti allo stesso oggetto).
        public void delete(String fieldName, Object key)
        {
            createIfNotExists(fieldName, key);
			long refId = _cache[fieldName].ContainsKey(key) ? _cache[fieldName][key] : 0;
			createStoreIfNotExists(refId);
			if(refId == 0)
			{
				return;
			}
			foreach (Object[] store in _stores[refId])
			{
				_delete((String) store[0], store[1]);
			}
			_stores.Remove(refId);
			_references.Remove(refId);
		}

		//Con questo metodo potremo prelevare un oggetto dalla cache (a patto che non sia scaduto, default: 2 minuti)
		public T? get(String fieldName, Object key)
        {
            createIfNotExists(fieldName, key);
            long refId = _cache[fieldName].ContainsKey(key) ? _cache[fieldName][key] : 0;
			if(_references.ContainsKey(refId))
			{
				if (!_references[refId].isExpired())
				{
					return _references[refId].getValue();
				}
			}
			return default(T);
        }
	}
}


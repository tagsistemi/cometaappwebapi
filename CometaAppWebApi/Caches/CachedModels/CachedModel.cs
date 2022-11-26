using System;
namespace CometaAppWebApi.Caches.CachedModels
{
	//Questa classe servirà per associare una scadenza ad ogni oggetto inserito nella cache.
	public class CachedModel<T>
	{
		private DateTime _cacheExpiration { get; }
		private T _obj { get; }

		public CachedModel(T obj)
		{
			_cacheExpiration = DateTime.UtcNow.AddMinutes(2);
			_obj = obj;
		}

		//Con questo metodo possiamo verificare se un oggetto è scaduto.
		public Boolean isExpired()
		{
			return new DateTimeOffset(_cacheExpiration).ToUnixTimeMilliseconds() < new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
		}

		//Con questo metodo possiamo ottenere l'oggetto originale.
		public T getValue()
		{
			return _obj;
		}
	}
}


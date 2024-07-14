using HealthSync.Core.Interfaces;
using HealthSync.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HealthSync.Core.Services
{
	public class PluginFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly List<Type> _pluginTypes;

		public PluginFactory(IServiceProvider serviceProvider, [FromKeyedServices("Plugins")] IEnumerable<Assembly> assemblies)
		{
			_serviceProvider = serviceProvider;
			_pluginTypes = [];

            foreach (var item in assemblies)
            {
				_pluginTypes.AddRange(item.GetTypes().Where(t => typeof(IHealthPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract));
			}
        }

		public T CreatePlugin<T>(string pluginId, SyncContext context) where T : IHealthPlugin
		{
			var pluginType = _pluginTypes
				.FirstOrDefault(p => typeof(T).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract && p.GetProperty("UniqueId")?.GetValue(null) as string == pluginId);

			return pluginType == null
				? throw new ArgumentException($"Plugin with id '{pluginId}' not found.")
				: (T)ActivatorUtilities.CreateInstance(_serviceProvider, pluginType, context);
		}
	}
}

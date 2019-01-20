///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://griffin.plus)
//
// Copyright 2018-2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Configuration
{
	/// <summary>
	/// Base class for persistence strategies enabling the <see cref="CascadedConfiguration"/> to make its data persistent.
	/// </summary>
	public abstract class CascadedConfigurationPersistenceStrategy : ICascadedConfigurationPersistenceStrategy
	{
		private readonly Dictionary<Type,IConverter> mValueConverters = new Dictionary<Type,IConverter>();
		
		/// <summary>
		/// Synchronization object used for synchronizing access to the persistence strategy.
		/// </summary>
		protected readonly object mSync = new object();

		/// <summary>
		/// Registers a converter that tells the configuration how to convert an object in the configuration to its
		/// string representation and vice versa.
		/// </summary>
		/// <param name="converter">Converter to register.</param>
		public void RegisterValueConverter(IConverter converter)
		{
			lock (mSync)
			{
				mValueConverters.Add(converter.Type, converter);
			}
		}

		/// <summary>
		/// Gets a converter for the specified type.
		/// </summary>
		/// <param name="type">Type to get a converter for.</param>
		/// <returns>
		/// The requested converter;
		/// null, if there is no converter registered for the specified type.
		/// </returns>
		public IConverter GetValueConverter(Type type)
		{
			lock (mSync)
			{
				IConverter converter;
				mValueConverters.TryGetValue(type, out converter);
				return converter;
			}
		}

		/// <summary>
		/// Checks whether the specified name is a valid configuration name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>
		/// true, if the specified configuration name is valid for use with the strategy;
		/// otherwise false.
		/// </returns>
		public abstract bool IsValidConfigurationName(string name);

		/// <summary>
		/// Checks whether the specified name is a valid item name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>
		/// true, if the specified item name is valid for use with the strategy;
		/// otherwise false.
		/// </returns>
		public abstract bool IsValidItemName(string name);

		/// <summary>
		/// Checks whether the persistence strategy supports the specified type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>
		/// true, if the persistence strategy supports the specified type;
		/// otherwise false.
		/// </returns>
		public abstract bool SupportsType(Type type);

		/// <summary>
		/// Gets a value indicating whether the persistence strategy supports comments.
		/// </summary>
		public abstract bool SupportsComments {
			get;
		}

		/// <summary>
		/// Checks whether a configuration item of the specified type may be set to the specified value.
		/// </summary>
		/// <param name="type">Type of the value of the configuration item to check.</param>
		/// <param name="value">Value to check.</param>
		/// <returns>
		/// true, if the specified value may be assigned to a configuration item of the specified type;
		/// otherwise false.
		/// </returns>
		/// <remarks>
		/// This method always returns <c>true</c>, if the type of the value matches the specified type. Otherwise
		/// <c>false</c> is returned. This is useful, if the persistence strategy does not save any type information,
		/// so the type of the configuration item is the only chance to determine the type to construct when loading
		/// a configuration item.
		/// </remarks>
		public virtual bool IsAssignable(Type type, object value)
		{
			if (value != null) {
				return value.GetType() == type;
			} else {
				return !type.IsValueType;
			}
		}

		/// <summary>
		/// Loads configuration data from the backend storage into the specified configuration.
		/// </summary>
		/// <param name="configuration">Configuration to update.</param>
		public abstract void Load(CascadedConfiguration configuration);

		/// <summary>
		/// Loads the value of the specified configuration item from the persistent storage.
		/// </summary>
		/// <param name="item">Item to load.</param>
		public abstract void LoadItem(ICascadedConfigurationItem item);

		/// <summary>
		/// Saves configuration data from the specified configuration into the backend storage.
		/// </summary>
		/// <param name="configuration">Configuration to save.</param>
		/// <param name="flags">Flags controlling the saving behavior.</param>
		public abstract void Save(CascadedConfiguration configuration, CascadedConfigurationSaveFlags flags);

	}
}

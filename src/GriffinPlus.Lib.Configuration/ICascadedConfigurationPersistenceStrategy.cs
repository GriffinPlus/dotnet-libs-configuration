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

namespace GriffinPlus.Lib.Configuration
{
	/// <summary>
	/// Interface for classes implementing a persistence strategy to enable the <see cref="CascadedConfiguration"/> to load/save its data.
	/// </summary>
	public interface ICascadedConfigurationPersistenceStrategy
	{
		/// <summary>
		/// Checks whether the specified name is a valid configuration name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>true, if the specified configuration name is valid for use with the strategy; otherwise false.</returns>
		bool IsValidConfigurationName(string name);

		/// <summary>
		/// Checks whether the specified name is a valid item name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>true, if the specified item name is valid for use with the strategy; otherwise false.</returns>
		bool IsValidItemName(string name);

		/// <summary>
		/// Checks whether the persistence strategy supports the specified type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>true, if the persistence strategy supports the specified type; otherwise false.</returns>
		bool SupportsType(Type type);

		/// <summary>
		/// Gets a value indicating whether the persistence strategy supports comments.
		/// </summary>
		bool SupportsComments {
			get;
		}

		/// <summary>
		/// Checks whether a configuration item of the specified type may be set to the specified value.
		/// </summary>
		/// <param name="type">Type of the value of the configuration item to check.</param>
		/// <param name="value">Value to check.</param>
		/// <returns>true, if the specified value may be assigned to a configuration item with the specified type; otherwise false.</returns>
		bool IsAssignable(Type type, object value);

		/// <summary>
		/// Loads configuration data from the backend storage into the specified configuration.
		/// </summary>
		/// <param name="configuration">Configuration to update.</param>
		/// <exception cref="ConfigurationException">Loading the configuration failed (reason depends on the persistence strategy).</exception>
		void Load(CascadedConfiguration configuration);

		/// <summary>
		/// Loads the value of the specified configuration item from the persistent storage.
		/// </summary>
		/// <param name="item">Item to load.</param>
		void LoadItem(ICascadedConfigurationItem item);

		/// <summary>
		/// Saves configuration data from the specified configuration into the backend storage.
		/// </summary>
		/// <param name="configuration">Configuration to save.</param>
		/// <param name="flags">Flags controlling the saving behavior.</param>
		/// <exception cref="ConfigurationException">Saving the configuration failed (reason depends on the persistence strategy).</exception>
		void Save(CascadedConfiguration configuration, CascadedConfigurationSaveFlags flags);
	}
}

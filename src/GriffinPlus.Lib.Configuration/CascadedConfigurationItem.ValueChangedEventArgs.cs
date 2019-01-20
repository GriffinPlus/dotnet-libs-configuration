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
	partial class CascadedConfigurationItem<T>
	{
		/// <summary>
		/// Event arguments carrying some information about a changed configuration item in a cascaded configuration.
		/// </summary>
		public class ValueChangedEventArgs : EventArgs
		{

			/// <summary>
			/// Initializes a new instance of the <see cref="ValueChangedEventArgs"/> class.
			/// </summary>
			/// <param name="issuer">The configuration item that has changed.</param>
			/// <param name="newValue">New value of the configuration item.</param>
			public ValueChangedEventArgs(CascadedConfigurationItem<T> issuer, T newValue)
			{
				Issuer = issuer;
				NewValue = newValue;
			}

			/// <summary>
			/// Gets the issuer of the event (can be the same as the sender of the event or a configuration item with the same name
			/// provided by an inherited configuration in the configuration cascade).
			/// </summary>
			public CascadedConfigurationItem<T> Issuer { get; }

			/// <summary>
			/// Gets the new value of the configuration item.
			/// </summary>
			public T NewValue { get; }

		}
	}
}

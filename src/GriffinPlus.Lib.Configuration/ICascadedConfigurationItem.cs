///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://griffin.plus)
//
// Copyright 2018 Sascha Falk <sascha@falk-online.eu>
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
	/// Untyped interface to configuration items.
	/// </summary>
	public interface ICascadedConfigurationItem
	{
		/// <summary>
		/// Gets the name of the configuration item.
		/// </summary>
		string Name {
			get;
		}

		/// <summary>
		/// Gets the path of the configuration item in the configuration hierarchy.
		/// </summary>
		string Path {
			get;
		}

		/// <summary>
		/// Gets the type of the value in the configuration item.
		/// </summary>
		Type Type {
			get;
		}

		/// <summary>
		/// Gets a value indicating whether the configuration item contains a valid value.
		/// </summary>
		bool HasValue {
			get;
		}

		/// <summary>
		/// Gets or sets the value of the configuration item.
		/// </summary>
		object Value {
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether the configuration supports comments.
		/// </summary>
		bool SupportsComments {
			get;
		}

		/// <summary>
		/// Gets a value indicating whether the configuration item contains a comment.
		/// </summary>
		bool HasComment {
			get;
		}

		/// <summary>
		/// Gets or sets the comment describing the configuration item.
		/// </summary>
		string Comment {
			get;
			set;
		}

		/// <summary>
		/// Gets the configuration the current item is in.
		/// </summary>
		CascadedConfiguration Configuration {
			get;
		}

		/// <summary>
		/// Resets the value of the configuration item, so an inherited configuration value is returned
		/// by the <see cref="Value"/> property.
		/// </summary>
		void ResetValue();
	}
}

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
	/// Controls the behavior when it somes to saving a configuration.
	/// </summary>
	[Flags]
	public enum CascadedConfigurationSaveFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Save inherited settings, if a configuration item does not have an own value.
		/// If this flag is omitted only configuration items that have a value are saved.
		/// </summary>
		SaveInheritedSettings = 0x1
	}
}

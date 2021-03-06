﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

namespace GriffinPlus.Lib.Configuration
{
	/// <summary>
	/// Untyped interface to configuration items (for internal use only).
	/// </summary>
	internal interface ICascadedConfigurationItemInternal : ICascadedConfigurationItem
	{
		/// <summary>
		/// Sets the configuration the current item is in.
		/// </summary>
		/// <param name="configuration">Configuration to set.</param>
		void SetConfiguration(CascadedConfiguration configuration);
	}
}

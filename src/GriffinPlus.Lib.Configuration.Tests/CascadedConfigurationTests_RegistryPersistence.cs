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

using GriffinPlus.Lib.Configuration;
using System;
using Xunit;

namespace UnitTests
{
	public class CascadedConfigurationTests_RegistryPersistence : CascadedConfigurationTests_NoPersistence
	{
		private const string RegistryKeyPath = @"HKEY_CURRENT_USER\Software\Griffin+ Configuration Test";
		private const string RootConfigurationName = "Cascaded Registry Configuration Test";

		public CascadedConfigurationTests_RegistryPersistence()
		{
			ICascadedConfigurationPersistenceStrategy persistence = new RegistryPersistenceStrategy(RegistryKeyPath);
			mConfiguration = new CascadedConfiguration(RootConfigurationName, persistence);
		}


		[Theory]
		[InlineData("Ä")]
		[InlineData("A/Ä")]
		[InlineData("A/Ä/A")]
		public void GetChildConfiguration_InvalidPath(string path)
		{
			Assert.Throws<ConfigurationException>(() => {
				mConfiguration.GetChildConfiguration(path, false);
			});
		}

	}

}

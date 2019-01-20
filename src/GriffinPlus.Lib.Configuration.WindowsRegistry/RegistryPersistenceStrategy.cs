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

using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Text.RegularExpressions;

namespace GriffinPlus.Lib.Configuration
{
	/// <summary>
	/// A persistance strategy that enables a <see cref="CascadedConfiguration"/> to persist its data in the registry.
	/// </summary>
	/// <remarks>
	/// The configuration supports the following types that may be used in configuration items:
	/// - System.Boolean (maps to a registry DWORD)
	/// - System.SByte (maps to a registry string)
	/// - System.Byte (maps to a registry DWORD)
	/// - System.Int16 (maps to a registry string)
	/// - System.UInt16 (maps to a registry DWORD)
	/// - System.Int32 (maps to a registry string)
	/// - System.UInt32 (maps to a registry DWORD)
	/// - System.Int64 (maps to a registry string)
	/// - System.UInt64 (maps to a registry QWORD)
	/// - System.Single (maps to a registry string)
	/// - System.Double (maps to a registry string)
	/// - System.String (maps to a registry string)
	/// - enumeration types (map to a registry string)
	/// - all types that have a converter (see <see cref="Converters"/>, map to a registry string)
	/// - arrays of the types above (map to register multi-string)
	/// </remarks>
	public class RegistryPersistenceStrategy : CascadedConfigurationPersistenceStrategy
	{
		private const string allowedCharacters = @"a-zA-Z0-9!§\$%&\/\(\)\[\]\?\+\-\*'""=_<>@,;\.:\#";
		private static string sRegexString = string.Format(@"^[{0}][{0} ]*[{0}]|[{0}]$", allowedCharacters);
		private static Regex sValidConfigurationNameRegex = new Regex(sRegexString, RegexOptions.Compiled);
		private static Regex sValidItemNameRegex = new Regex(sRegexString, RegexOptions.Compiled);

		private readonly string mKeyBasePath;


		/// <summary>
		/// Initializes a new instance of the <see cref="RegistryPersistenceStrategy"/> class.
		/// </summary>
		/// <param name="path">Base path of the registry key where the configuration is stored.</param>
		public RegistryPersistenceStrategy(string path)
		{
			mKeyBasePath = path;
		}


		/// <summary>
		/// Checks whether the specified name is a valid configuration name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>true, if the specified configuration name is valid for use with the strategy; otherwise false.</returns>
		public override bool IsValidConfigurationName(string name)
		{
			return sValidConfigurationNameRegex.IsMatch(name);
		}


		/// <summary>
		/// Checks whether the specified name is a valid item name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>true, if the specified item name is valid for use with the strategy; otherwise false.</returns>
		public override bool IsValidItemName(string name)
		{
			return sValidItemNameRegex.IsMatch(name);
		}


		/// <summary>
		/// Gets a value indicating whether the persistence strategy supports comments.
		/// </summary>
		public override bool SupportsComments
		{
			get { return false; }
		}


		/// <summary>
		/// Checks whether the persistence strategy supports the specified type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>true, if the persistence strategy supports the specified type; otherwise false.</returns>
		public override bool SupportsType(Type type)
		{
			// check fixed types
			if (type.IsArray && type.GetArrayRank() == 1 && SupportsType(type.GetElementType())) return true;
			if (type.IsEnum) return true;

			IConverter converter = GetValueConverter(type);
			if (converter == null)
			{
				converter = Converters.GetGlobalConverter(type);
			}

			return converter != null;
		}


		/// <summary>
		/// Updates the specified configuration loading settings from the registry.
		/// </summary>
		/// <exception cref="SecurityException">
		/// The user does not have the permissions required to read from the registry key.
		/// </exception>
		/// <exception cref="ConfigurationException">
		/// Loading the configuration failed due to a serialization error.
		/// </exception>
		public override void Load(CascadedConfiguration configuration)
		{
			lock (configuration.Sync)
			{
				string keyPath = mKeyBasePath + "\\" + configuration.Path.Trim('/').Replace('/', '\\');

				foreach (ICascadedConfigurationItem item in configuration.Items)
				{
					object registryValue = Registry.GetValue(keyPath, item.Name, null);

					if (registryValue != null)
					{
						if (item.Type.IsArray)
						{
							// array types are always mapped to a registry multi-string
							if (registryValue.GetType() == typeof(string[]))
							{
								IConverter converter = GetValueConverter(item.Type);
								if (converter == null)
								{
									converter = Converters.GetGlobalConverter(item.Type);
								}

								if (converter != null)
								{
									try
									{
										string[] regMultiStringValue = (string[])registryValue;
										Array conversionResult = Array.CreateInstance(item.Type.GetElementType(), regMultiStringValue.Length);
										for (int i = 0; i < regMultiStringValue.Length; i++)
										{
											object obj = converter.ConvertStringToObject(regMultiStringValue[i], CultureInfo.InvariantCulture);
											conversionResult.SetValue(obj, i);
										}
										item.Value = conversionResult;
										continue;
									}
									catch (Exception)
									{
										throw new ConfigurationException(
											@"Loading configuration item (path: {0}, type: {1}) failed, because parsing the registry value (path: {2}\{3}, value: {4}) failed.",
											item.Path, item.Type.FullName, keyPath, item.Name, registryValue);
									}
								}
							}
							else
							{
								throw new ConfigurationException(
									@"Loading configuration item (path: {0}, type: {1}) failed, because the registry value (path: {2}\{3}) is expected to be a '{4}', but it isn't.",
									item.Path, item.Type.FullName, keyPath, item.Name, RegistryValueKind.MultiString);
							}
						}
						else
						{
							if (item.Type == typeof(bool) || item.Type == typeof(byte) || item.Type == typeof(ushort) || item.Type == typeof(uint))
							{
								// booleans and 8/16/32 bit unsigned integers are mapped to a registry DWORD
								if (registryValue.GetType() == typeof(int))
								{
									if (item.Type == typeof(bool)) item.Value = (bool)((int)registryValue != 0);
									else if (item.Type == typeof(byte)) item.Value = (byte)(int)registryValue;
									else if (item.Type == typeof(ushort)) item.Value = (ushort)(int)registryValue;
									else if (item.Type == typeof(uint)) item.Value = (uint)(int)registryValue;
									continue;
								}
								else
								{
									throw new ConfigurationException(
										@"Loading configuration item (path: {0}, type: {1}) failed, because the registry value (path: {2}\{3}) is expected to be a '{4}', but it isn't.",
										item.Path, item.Type.FullName, mKeyBasePath, item.Name, RegistryValueKind.DWord);
								}
							}
							else if (item.Type == typeof(ulong))
							{
								// 64 bit unsigned integers are mapped to a registry QWORD
								if (registryValue.GetType() == typeof(long))
								{
									item.Value = (ulong)(long)registryValue;
									continue;
								}
								else
								{
									throw new ConfigurationException(
										@"Loading configuration item (path: {0}, type: {1}) failed, because the registry value (path: {2}\{3}) is expected to be a '{4}', but it isn't.",
										item.Path, item.Type.FullName, keyPath, item.Name, RegistryValueKind.QWord);
								}
							}
							else if (item.Type == typeof(string[]))
							{
								// string arrays are mapped to a registry multi-string
								if (registryValue.GetType() == typeof(string[]))
								{
									item.Value = registryValue;
									continue;
								}
								else
								{
									throw new ConfigurationException(
										@"Loading configuration item (path: {0}, type: {1}) failed, because the registry value (path: {2}\{3}) is expected to be a '{4}', but it isn't.",
										item.Path, item.Type.FullName, keyPath, item.Name, RegistryValueKind.MultiString);
								}
							}
							else if (item.Type.IsEnum)
							{
								// enumerations are mapped to a registry string
								if (registryValue.GetType() == typeof(string))
								{
									try
									{
										item.Value = Enum.Parse(item.Type, (string)registryValue);
										continue;
									}
									catch (Exception)
									{
										throw new ConfigurationException(
											@"Loading configuration item (path: {0}, type: {1}) failed, because parsing the registry value (path: {2}\{3}, value: {4}) failed.",
											item.Path, item.Type.FullName, keyPath, item.Name, registryValue);
									}
								}
								else
								{
									throw new ConfigurationException(
										@"Loading configuration item (path: {0}, type: {1}) failed, because the registry value (path: {2}\{3}) is expected to be a '{4}', but it isn't.",
										item.Path, item.Type.FullName, keyPath, item.Name, RegistryValueKind.String);
								}
							}
							else
							{
								// any other type is converted to a string using a converter
								if (registryValue.GetType() == typeof(string))
								{
									IConverter converter = GetValueConverter(item.Type);
									if (converter == null)
									{
										converter = Converters.GetGlobalConverter(item.Type);
									}

									try
									{
										if (converter != null)
										{
											object obj = converter.ConvertStringToObject((string)registryValue, CultureInfo.InvariantCulture);
											item.Value = obj;
											continue;
										}
									}
									catch (Exception)
									{
										throw new ConfigurationException(
											@"Loading configuration item (path: {0}, type: {1}) failed, because parsing the registry value (path: {2}\{3}, value: {4}) failed.",
											item.Path, item.Type.FullName, keyPath, item.Name, registryValue);
									}
								}
								else
								{
									throw new ConfigurationException(
										@"Loading configuration item (path: {0}, type: {1}) failed, because the registry value (path: {2}\{3}) is expected to be a '{4}', but it isn't.",
										item.Path, item.Type.FullName, keyPath, item.Name, RegistryValueKind.String);
								}
							}

							// there is no way to make the configuration item persistent
							// (should never happen, since types that are not supported should never get into the configuration)
							throw new ConfigurationException(
								"The configuration contains an item (path: {0}, type: {1}) that cannot be loaded/saved.",
								item.Path, item.Type.FullName);
						}
					}
				}

				// load child configurations
				foreach (CascadedConfiguration child in configuration.Children)
				{
					child.Load();
				}
			}
		}


		/// <summary>
		/// Loads the value of the specified configuration item from the persistent storage.
		/// </summary>
		/// <param name="item">Item to load.</param>
		public override void LoadItem(ICascadedConfigurationItem item)
		{
			// not supported, yet.
			// (all configurations items must be added before the settings are loaded)
		}


		/// <summary>
		/// Saves the specified configuration in the registry.
		/// </summary>
		/// <param name="configuration">Configuration to save.</param>
		/// <param name="flags">Flags controlling the save behavior.</param>
		/// <exception cref="SecurityException">The user does not have the permissions required to read from the registry key.</exception>
		/// <exception cref="ConfigurationException">Saving the configuration failed due to a serialization error.</exception>
		public override void Save(CascadedConfiguration configuration, CascadedConfigurationSaveFlags flags)
		{
			lock (configuration.Sync)
			{
				string keyPath = mKeyBasePath + "\\" + configuration.Path.Trim('/').Replace('/', '\\');

				foreach (ICascadedConfigurationItem item in configuration.Items)
				{
					if (item.HasValue || flags.HasFlag(CascadedConfigurationSaveFlags.SaveInheritedSettings))
					{
						if (item.Type.IsArray)
						{
							// array type => always mapped to a multi-string
							// => use converter to perform the conversion (all basically supported types also have a converter)
							Type elementType = item.Type.GetElementType();
							IConverter converter = GetValueConverter(elementType);
							if (converter == null)
							{
								converter = Converters.GetGlobalConverter(elementType);
							}

							List<string> value = new List<string>();
							foreach (object obj in (IEnumerable)item.Value)
							{
								value.Add(converter.ConvertObjectToString(obj, CultureInfo.InvariantCulture));
							}

							Registry.SetValue(keyPath, item.Name, value.ToArray(), RegistryValueKind.MultiString);
						}
						else
						{
							if (item.Type == typeof(bool) || item.Type == typeof(byte) || item.Type == typeof(ushort) || item.Type == typeof(uint))
							{
								// booleans and unsigned integers (8/16/32-bit) best map to a registry DWORD (32-bit)
								object value = item.Value;
								if (value is bool) value = (bool)value ? 1 : 0;
								Registry.SetValue(keyPath, item.Name, value, RegistryValueKind.DWord);
							}
							else if (item.Type == typeof(ulong))
							{
								// unsigned integer (64-bit) best maps to a registry DWORD (64-bit)
								Registry.SetValue(keyPath, item.Name, item.Value, RegistryValueKind.QWord);
							}
							else if (item.Type == typeof(string[]))
							{
								// a string hash becomes a registry multi-string
								Registry.SetValue(keyPath, item.Name, item.Value, RegistryValueKind.MultiString);
							}
							else if (item.Type.IsEnum)
							{
								// an enumeration value should be stored as a string
								string value = item.Value.ToString();
								Registry.SetValue(keyPath, item.Name, value, RegistryValueKind.String);
							}
							else
							{
								// the configuration item contains a value that is not covered by the standard types
								// => try to find some other way to make it persistent...
								if (SaveUsingConverter(item, keyPath)) continue;

								// there is no way to make the configuration item persistent
								// (should never happen, since types that are not supported should never get into the configuration)
								throw new ConfigurationException(
									"The configuration contains an item (path: {0}, type: {1}) that cannot be loaded/saved.",
									item.Path, item.Type.FullName);
							}
						}
					}
					else
					{
						// the configuration item does not have a value
						// => remove corresponding registry value
						DeleteRegistryValue(keyPath, item.Name);
					}
				}
			}

			// save child configurations
			foreach (CascadedConfiguration child in configuration.Children)
			{
				child.Save(flags);
			}
		}


		/// <summary>
		/// Tries to save the specified configuration item using a converter.
		/// </summary>
		/// <param name="item">Configuration item to save.</param>
		/// <param name="keyPath">Path of the registry key to save the configuration to.</param>
		/// <returns>true, if saving the configuration item succeeded; otherwise false.</returns>
		private bool SaveUsingConverter(ICascadedConfigurationItem item, string keyPath)
		{
			// try to get a converter that has been registered with the configuration
			IConverter converter = GetValueConverter(item.Type);

			if (converter == null)
			{
				// the configuration does not have a registered converter for the type of the configuration item
				// => try to get a converter
				converter = Converters.GetGlobalConverter(item.Type);
				if (converter == null)
				{
					return false;
				}
			}

			// convert the object to a string and save it in the registry
			string value = converter.ConvertObjectToString(item.Value, CultureInfo.InvariantCulture);
			Registry.SetValue(keyPath, item.Name, value, RegistryValueKind.String);
			return true;
		}


		/// <summary>
		/// Parses the specified registry path and returns the registry hive the addressed registry key/value is in.
		/// </summary>
		/// <param name="path">Path to parse.</param>
		private static RegistryHive GetHive(string path)
		{
			int index = path.IndexOf('\\');
			string hive = path.Substring(0, index).ToUpper();

			switch (hive)
			{
				case "HKEY_CLASSES_ROOT": return RegistryHive.ClassesRoot;
				case "HKEY_CURRENT_USER": return RegistryHive.CurrentUser;
				case "HKEY_LOCAL_MACHINE": return RegistryHive.LocalMachine;
				case "HKEY_USERS": return RegistryHive.Users;
				case "HKEY_PERFORMANCE_DATA": return RegistryHive.PerformanceData;
				case "HKEY_CURRENT_CONFIG": return RegistryHive.CurrentConfig;
				case "HKEY_DYN_DATA": return RegistryHive.DynData;
				default: throw new ConfigurationException("Invalid registry hive '{0}'.", hive);
			}
		}


		/// <summary>
		/// Removes the value with the specified name from the registry key with the specified path.
		/// </summary>
		/// <param name="keyPath">Path of the registry containing the value to remove.</param>
		/// <param name="name">Name of the value to remove.</param>
		private static void DeleteRegistryValue(string keyPath, string name)
		{
			RegistryHive hive = GetHive(keyPath);
			RegistryKey key = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
			string[] tokens = keyPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
			DeleteRegistryValue(key, tokens, 1, name);
		}


		/// <summary>
		/// Removes the value with the specified name from the registry key with the specified path (called recursively).
		/// </summary>
		/// <param name="key">Current registry key.</param>
		/// <param name="path">Split path elements.</param>
		/// <param name="pathIndex">Index of the path element to process.</param>
		/// <param name="name">Name of the value to remove.</param>
		private static void DeleteRegistryValue(RegistryKey key, string[] path, int pathIndex, string name)
		{
			if (pathIndex == path.Length)
			{
				key.DeleteValue(name, false);
				return;
			}

			key = key.OpenSubKey(path[pathIndex], pathIndex == path.Length - 1);
			DeleteRegistryValue(key, path, pathIndex + 1, name);
		}

	}
}

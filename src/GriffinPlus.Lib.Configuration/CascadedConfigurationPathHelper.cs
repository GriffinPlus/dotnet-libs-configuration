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
using System.Text.RegularExpressions;

namespace GriffinPlus.Lib.Configuration
{
	/// <summary>
	/// Some helper functions for handling paths in a <see cref="CascadedConfiguration"/>.
	/// </summary>
	public static class CascadedConfigurationPathHelper
	{
		private static Regex sPathSplitterRegex = new Regex(@"(?:(?<![\\])[/])|(?:(?<![\\])[\\](?![\\]))", RegexOptions.Compiled);

		/// <summary>
		/// Splits up the specified path into a list of path segments using '/' and '\' as delimiters.
		/// </summary>
		/// <param name="strategy">The persistence strategy that is used.</param>
		/// <param name="path">
		/// Path to split (path delimiter are '/' and '\', escape these characters, if a path segment contains one of them,
		/// otherwise the segment will be split up).
		/// </param>
		/// <param name="isItemPath">
		/// true, if the path is an item path;
		/// false, if the specified path is a configuration node path.
		/// </param>
		/// <param name="checkValidity">
		/// true to check the validity of path segment names using the specified persistence strategy;
		/// false to skip checking the validity of path segment names.
		/// </param>
		/// <returns>The resulting list of path segments.</returns>
		public static string[] SplitPath(
			ICascadedConfigurationPersistenceStrategy strategy,
			string path,
			bool isItemPath,
			bool checkValidity)
		{
			List<string> segments = new List<string>();
			foreach (string segment in sPathSplitterRegex.Split(path))
			{
				string s = segment.Trim();
				if (s.Length > 0)
				{
					segments.Add(segment);
				}
			}

			if (checkValidity)
			{
				if (strategy != null)
				{
					for (int i = 0; i < segments.Count; i++)
					{
						string name = segments[i];
						if (i + 1 < segments.Count)
						{
							// intermediate segment (can be a configuration only)
							if (!strategy.IsValidConfigurationName(name))
							{
								throw new ConfigurationException("The specified configuration name ({0}) is not supported by the persistence strategy.", name);
							}
						}
						else
						{
							// last segment (can be a configuration or an item)
							if (isItemPath)
							{
								if (!strategy.IsValidItemName(name))
								{
									throw new ConfigurationException("The specified configuration name ({0}) is not supported by the persistence strategy.", name);
								}
							}
							else
							{
								if (!strategy.IsValidConfigurationName(name))
								{
									throw new ConfigurationException("The specified configuration name ({0}) is not supported by the persistence strategy.", name);
								}
							}
						}
					}
				}
			}

			if (segments.Count == 0)
			{
				throw new ArgumentException("The path is invalid, since it does not contain any location information.");
			}

			return segments.ToArray();
		}

		/// <summary>
		/// Checks whether the persistence strategy can handle the specified type and throws an exception, if it can not.
		/// </summary>
		/// <param name="strategy">The persistence strategy that is used.</param>
		/// <param name="type">Type to check.</param>
		public static void EnsureThatPersistenceStrategyCanHandleValueType(
			ICascadedConfigurationPersistenceStrategy strategy,
			Type type)
		{
			if (strategy != null)
			{
				if (!strategy.SupportsType(type))
				{
					throw new ConfigurationException(
						"The specified type ({0}) is not supported by the persistence strategy.",
						type.FullName);
				}
			}
		}

		/// <summary>
		/// Checks whether the persistence strategy supports assigning the specified value to an item of the specified type;
		/// throws an exception, if it can not.
		/// </summary>
		/// <param name="strategy">The persistence strategy that is used.</param>
		/// <param name="itemType">Item type to check.</param>
		/// <param name="value">Value to check.</param>
		public static void EnsureThatPersistenceStrategyCanAssignValue(
			ICascadedConfigurationPersistenceStrategy strategy,
			Type itemType,
			object value)
		{
			if (strategy != null)
			{
				if (!strategy.IsAssignable(itemType, value))
				{
					throw new ConfigurationException(
						"The specified value is not supported for a configuration item of type '{0}'.",
						itemType.FullName);
				}
			}
		}

		/// <summary>
		/// Checks whether the specified string contains a (non-escaped) path separator.
		/// </summary>
		/// <param name="s">String to check.</param>
		/// <returns>true, if the specified strings contains a path separator; otherwise false.</returns>
		public static bool ContainsPathSeparator(string s)
		{
			return sPathSplitterRegex.IsMatch(s);
		}

		private static Regex sEscapeRegex = new Regex(@"(?<sep>[\\/])", RegexOptions.Compiled);

		/// <summary>
		/// Escapes the specified name for use in the configuration (avoid splitting up path segments unintentionally).
		/// </summary>
		/// <param name="s">String to escape.</param>
		/// <returns>The escaped string.</returns>
		public static string EscapeName(string s)
		{
			return sEscapeRegex.Replace(s, "\\${sep}");
		}

		private static Regex sUnescapeRegex = new Regex(@"[\\](?<sep>[\\/])", RegexOptions.Compiled);

		/// <summary>
		/// Removes path delimiter escaping from the specified string.
		/// </summary>
		/// <param name="s">String to remove path delimiter escaping from.</param>
		/// <returns>The resulting string.</returns>
		public static string UnescapeName(string s)
		{
			return sUnescapeRegex.Replace(s, "${sep}");
		}

	}
}

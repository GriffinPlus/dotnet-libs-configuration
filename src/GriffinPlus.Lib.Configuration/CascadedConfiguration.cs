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
using System.Collections.Generic;
using System.Reflection;

namespace GriffinPlus.Lib.Configuration
{
	/// <summary>
	/// A cascadable configuration allowing to build hierarchical configurations with multiple levels of inheritance and different kinds of persistence.
	/// </summary>
	/// <remarks>
	/// This class provides everything that is needed to build up a configuration system with multiple levels of inheritance by chaining configurations
	/// together. The primary configuration (i.e. a configuration that does not inherit from any other configuration) must always provide a value for each
	/// and every configuration item. Therefore it is recommended to populate the primary configuration with default settings. Configurations deriving from
	/// the primary configuration may hide default settings by overwriting configuration items. A query will always return the value of the most specific
	/// configuration item that provides a value.
	/// 
	/// Any configuration can have multiple child configurations that allow you to create hierarchical configurations.
	/// </remarks>
	public partial class CascadedConfiguration
	{
		private List<CascadedConfiguration> mChildren = new List<CascadedConfiguration>();
		private List<ICascadedConfigurationItemInternal> mItems = new List<ICascadedConfigurationItemInternal>();
		private List<CascadedConfiguration> mDerivedConfigurations = new List<CascadedConfiguration>();
		private CascadedConfiguration mInheritedConfiguration;
		private CascadedConfiguration mParent;
		private ICascadedConfigurationPersistenceStrategy mPersistenceStrategy;
		private string mName;
		private string mPath;
		private bool mIsModified;
		private readonly object mSync;

		/// <summary>
		/// Initializes a new instance of the <see cref="CascadedConfiguration"/> class
		/// (for root configurations that do not derive from other configurations).
		/// </summary>
		/// <param name="name">Name of the configuration.</param>
		/// <param name="persistence">Some object that is responsible for persisting configuration items (null, if you don't need any persistence).</param>
		public CascadedConfiguration(string name, ICascadedConfigurationPersistenceStrategy persistence)
		{
			mName = name;
			mPath = "/";
			mSync = new object();
			mPersistenceStrategy = persistence;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CascadedConfiguration"/> class
		/// (for root configurations that derive from other configurations).
		/// </summary>
		/// <param name="configurationToInheritFrom">
		/// Configuration to inherit from, i.e. this configuration is queried, if the current configuration does not provide a value for a configuration item.
		/// </param>
		/// <param name="persistence">Some object that is responsible for persisting configuration items (null, if you don't need any persistence).</param>
		public CascadedConfiguration(CascadedConfiguration configurationToInheritFrom, ICascadedConfigurationPersistenceStrategy persistence)
		{
			mInheritedConfiguration = configurationToInheritFrom;
			mDerivedConfigurations = new List<CascadedConfiguration>();
			mName = mInheritedConfiguration.mName;
			mSync = mInheritedConfiguration.mSync;
			mPath = mInheritedConfiguration.mPath;
			mPersistenceStrategy = persistence;
			lock (mSync)
			{
				mInheritedConfiguration.mDerivedConfigurations.Add(this);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CascadedConfiguration"/> class
		/// (for child configurations, i.e. configurations that have a parent configuration regardless whether the configuration derives from another configuration).
		/// </summary>
		/// <param name="name">Name of the configuration.</param>
		/// <param name="parent">Parent configuration.</param>
		protected CascadedConfiguration(string name, CascadedConfiguration parent)
		{
			lock (parent.mSync)
			{
				mName = name;
				mPath = parent.mPath.TrimEnd('/') + "/" + CascadedConfigurationPathHelper.EscapeName(name);
				mSync = parent.mSync;
				mParent = parent;
				mPersistenceStrategy = parent.mPersistenceStrategy;
				parent.mChildren.Add(this);
				parent.mIsModified = true;
				if (parent.mInheritedConfiguration != null)
				{
					mInheritedConfiguration = parent.mInheritedConfiguration.GetChildConfiguration(CascadedConfigurationPathHelper.EscapeName(name), true);
					mInheritedConfiguration.mDerivedConfigurations.Add(this);
					parent.mInheritedConfiguration.mIsModified = true;
				}
			}
		}

		/// <summary>
		/// Gets the name of the configuration.
		/// </summary>
		public string Name
		{
			get { return mName; }
		}

		/// <summary>
		/// Gets the path of the configuration in the configuration hierarchy.
		/// </summary>
		public string Path
		{
			get { return mPath; }
		}

		/// <summary>
		/// Gets the items in the configuration.
		/// </summary>
		public IEnumerable<ICascadedConfigurationItem> Items
		{
			get
			{
				lock (mSync)
				{
					IEnumerator<ICascadedConfigurationItem> enumerator = new MonitorSynchronizedEnumerator<ICascadedConfigurationItem>(mItems.GetEnumerator(), mSync);
					try
					{
						while (enumerator.MoveNext())
						{
							yield return enumerator.Current;
						}
					}
					finally
					{
						enumerator.Dispose();
					}
				}
			}
		}

		/// <summary>
		/// Gets child configurations of the configuration.
		/// </summary>
		public IEnumerable<CascadedConfiguration> Children
		{
			get
			{
				lock (mSync)
				{
					IEnumerator<CascadedConfiguration> enumerator = new MonitorSynchronizedEnumerator<CascadedConfiguration>(mChildren.GetEnumerator(), mSync);
					try
					{
						while (enumerator.MoveNext())
						{
							yield return enumerator.Current;
						}
					}
					finally
					{
						enumerator.Dispose();
					}
				}
			}
		}

		/// <summary>
		/// Gets the inherited configuration in the configuration cascade
		/// (null, if the current configuration does not derive from any other configuration).
		/// </summary>
		public CascadedConfiguration InheritedConfiguration
		{
			get
			{
				return mInheritedConfiguration;
			}
		}

		/// <summary>
		/// Gets the parent of the configuration
		/// (null, if the current configuration is a root configuration).
		/// </summary>
		public CascadedConfiguration Parent
		{
			get { return mParent; }
		}

		/// <summary>
		/// Gets a value indicating whether the configuration has been modified.
		/// </summary>
		public bool IsModified
		{
			get
			{
				lock (mSync)
				{
					foreach (CascadedConfiguration child in mChildren)
					{
						if (child.IsModified) return true;
					}

					return mIsModified;
				}
			}

			internal set
			{
				lock (mSync)
				{
					if (value)
					{
						mIsModified = true;
					}
					else
					{
						foreach (CascadedConfiguration child in mChildren)
						{
							child.IsModified = false;
						}
						mIsModified = false;
					}
				}
			}
		}

		/// <summary>
		/// Gets the root configuration.
		/// </summary>
		public CascadedConfiguration RootConfiguration
		{
			get
			{
				if (mParent == null) return this;
				else return mParent.RootConfiguration;
			}
		}

		/// <summary>
		/// Gets the persistence strategy to use when loading/saving the configuration.
		/// </summary>
		public ICascadedConfigurationPersistenceStrategy PersistenceStrategy
		{
			get { return mPersistenceStrategy; }
		}

		/// <summary>
		/// Gets the object used to synchronize access to the configuration and it's items
		/// (used in conjunction with the <see cref="System.Threading.Monitor"/> class or a lock() statement).
		/// </summary>
		public object Sync
		{
			get
			{
				return mSync;
			}
		}

		/// <summary>
		/// Adds a configuration item with the specified type at the specified location, if it does not exist, yet.
		/// </summary>
		/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
		/// <param name="path">
		/// Relative path of the configuration item to add (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		public CascadedConfigurationItem<T> SetItem<T>(string path)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, true);
					return configuration.SetItem<T>(pathSegments[pathSegments.Length - 1]);
				}
				else
				{
					CascadedConfigurationItem<T> item = null;
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem ci = mItems[i];
						if (ci.Name == itemName)
						{
							if (ci.Type != typeof(T)) throw new ConfigurationException("The configuration already contains an item with the specified name, but with a different type.");
							item = ci as CascadedConfigurationItem<T>;
							return item;
						}
					}

					// check whether the persistence strategy can handle the type
					CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanHandleValueType(mPersistenceStrategy, typeof(T));

					item = new CascadedConfigurationItem<T>(itemName, mPath.TrimEnd('/') + "/" + path);
					mItems.Add(item);
					item.Configuration = this;
					if (mPersistenceStrategy != null)
					{
						bool wasModified = mIsModified;
						mPersistenceStrategy.LoadItem(item);
						mIsModified = wasModified;
					}

					return item;
				}
			}
		}

		/// <summary>
		/// Adds a configuration item with the specified type at the specified location in the configuration.
		/// </summary>
		/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
		/// <param name="path">
		/// Relative path of the configuration item to add (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		/// <param name="value">Initial value of the configuration item.</param>
		public CascadedConfigurationItem<T> SetValue<T>(string path, T value)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, true);
					return configuration.SetValue<T>(pathSegments[pathSegments.Length - 1], value);
				}
				else
				{
					CascadedConfigurationItem<T> item = null;
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem ci = mItems[i];
						if (ci.Name == itemName)
						{
							// ensure the specified type is the same as the type of the existing configuration item
							if (ci.Type != typeof(T))
							{
								throw new ConfigurationException("The configuration already contains an item with the specified name, but with a different type.");
							}

							// check whether the persistence strategy accepts the specified value for that configuration item
							CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanAssignValue(mPersistenceStrategy, ci.Type, value);

							item = ci as CascadedConfigurationItem<T>;
							item.Value = value;
							return item;
						}
					}

					// check whether the persistence strategy can handle the type
					CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanHandleValueType(mPersistenceStrategy, typeof(T));

					item = new CascadedConfigurationItem<T>(itemName, mPath.TrimEnd('/') + "/" + path);
					item.Configuration = this;
					mItems.Add(item);

					// check whether the persistence strategy accepts the specified value for that configuration item
					try
					{
						CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanAssignValue(mPersistenceStrategy, item.Type, value);
					}
					catch
					{
						mItems.Remove(item);
						throw;
					}

					item.Value = value; // sets the 'modified' flag
					return item;
				}
			}
		}

		/// <summary>
		/// Adds a configuration item with the specified type at the specified location in the configuration, if it does not exists, yet.
		/// </summary>
		/// <param name="type">Type of the value in the configuration item.</param>
		/// <param name="path">
		/// Relative path of the configuration item to add (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		public ICascadedConfigurationItem SetItem(string path, Type type)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, true);
					return configuration.SetItem(pathSegments[pathSegments.Length - 1], type);
				}
				else
				{
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem ci = mItems[i];
						if (ci.Name == itemName)
						{
							// ensure the specified type is the same as the type of the existing configuration item
							if (ci.Type != type)
							{
								throw new ConfigurationException("The configuration already contains an item with the specified name, but with a different type.");
							}

							return ci;
						}
					}

					// check whether the persistence strategy can handle the type
					CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanHandleValueType(mPersistenceStrategy, type);

					// add configuration item
					ICascadedConfigurationItemInternal item = CreateItem(itemName, mPath.TrimEnd('/') + "/" + path, type);
					item.SetConfiguration(this);
					mItems.Add(item);
					if (mPersistenceStrategy != null)
					{
						bool wasModified = mIsModified;
						mPersistenceStrategy.LoadItem(item);
						mIsModified = wasModified;
					}

					return item;
				}
			}
		}

		/// <summary>
		/// Adds a configuration item with the specified type at the specified location in the configuration.
		/// </summary>
		/// <param name="path">
		/// Relative path of the configuration item to add (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		/// <param name="type">Type of the value in the configuration item.</param>
		/// <param name="value">Initial value of the configuration item.</param>
		public ICascadedConfigurationItem SetValue(string path, Type type, object value)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, true);
					return configuration.SetValue(pathSegments[pathSegments.Length - 1], type, value);
				}
				else
				{
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem ci = mItems[i];
						if (ci.Name == itemName)
						{
							// ensure the specified type is the same as the type of the existing configuration item
							if (ci.Type != type)
							{
								throw new ConfigurationException("The configuration already contains an item with the specified name, but with a different type.");
							}

							// check whether the persistence strategy accepts the specified value for that configuration item
							CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanAssignValue(mPersistenceStrategy, ci.Type, value);

							ci.Value = value; // sets the 'modified' flag
							return ci;
						}
					}

					// check whether the persistence strategy can handle the type
					CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanHandleValueType(mPersistenceStrategy, type);

					// add configuration item
					ICascadedConfigurationItemInternal item = CreateItem(itemName, mPath.TrimEnd('/') + "/" + path, type);
					item.SetConfiguration(this);
					mItems.Add(item);

					// check whether the persistence strategy accepts the specified value for that configuration item
					try
					{
						CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanAssignValue(mPersistenceStrategy, item.Type, value);
					}
					catch
					{
						mItems.Remove(item);
						throw;
					}

					// everything is ok, set the configuration item
					item.Value = value; // sets the 'modified' flag
					return item;
				}
			}
		}

		/// <summary>
		/// Removes the configuration item at the specified location.
		/// </summary>
		/// <param name="path">
		/// Relative path of the configuration item to remove (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		public bool RemoveItem(string path)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
					if (configuration == null) return false;
					return configuration.RemoveItem(pathSegments[pathSegments.Length - 1]);
				}
				else
				{
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItemInternal ci = mItems[i];
						if (ci.Name == itemName)
						{
							ci.SetConfiguration(null);
							mItems.RemoveAt(i);
							mIsModified = true;
							return true;
						}
					}

					return false;
				}
			}
		}

		/// <summary>
		/// Clears the entire configuration.
		/// </summary>
		public void Clear()
		{
			lock (mSync)
			{
				// remove all children
				// -------------------------------------------------------------------------
				for (int i = 0; i < mChildren.Count; i++)
				{
					CascadedConfiguration configuration = mChildren[i];

					// let children clear their collections
					configuration.Clear();

					// remove current configuration
					configuration.mName = "<<< Deleted >>>";
					configuration.mPath = "/";
					configuration.mParent = null;
					configuration.mPersistenceStrategy = null;
					if (configuration.mInheritedConfiguration != null)
					{
						configuration.mInheritedConfiguration.mDerivedConfigurations.Remove(this);
						configuration.mInheritedConfiguration = null;
					}
				}

				if (mChildren.Count > 0)
				{
					mChildren.Clear();
					mIsModified = true;
				}

				// remove all items
				// -------------------------------------------------------------------------
				for (int i = 0; i < mItems.Count; i++)
				{
					ICascadedConfigurationItemInternal item = mItems[i];
					item.SetConfiguration(null);
				}

				if (mItems.Count > 0)
				{
					mItems.Clear();
					mIsModified = true;
				}
			}
		}

		/// <summary>
		/// Resets all items of the current configuration (optionally all items of child configurations as well).
		/// </summary>
		/// <param name="recursively">
		/// true to reset items of child configurations as well;
		/// false to reset items of the current configuration.
		/// </param>
		public void ResetItems(bool recursively = false)
		{
			lock (mSync)
			{
				if (recursively)
				{
					for (int i = 0; i < mChildren.Count; i++)
					{
						CascadedConfiguration configuration = mChildren[i];
						configuration.ResetItems(true);
					}
				}

				for (int i = 0; i < mItems.Count; i++)
				{
					ICascadedConfigurationItem item = mItems[i];
					item.ResetValue();
				}
			}
		}

		/// <summary>
		/// Gets the value of the configuration item at the specified location.
		/// </summary>
		/// <typeparam name="T">Type of the value to get.</typeparam>
		/// <param name="path">
		/// Relative path of the configuration item (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		/// <param name="inherit">
		/// true to try to retrieve the value from an item in the current configuration first, then check inherited configurations;
		/// false to try to retrieve the value from the current configuration only.
		/// </param>
		/// <returns>Value of the configuration value.</returns>
		/// <exception cref="ConfigurationException">
		/// The configuration does not contain an item at the specified location -or-\n
		/// The configuration contains an item at the specified location, but the item has a different type.
		/// </exception>
		public T GetValue<T>(string path, bool inherit = true)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
					if (configuration == null)
					{
						if (inherit && mInheritedConfiguration != null)
						{
							configuration = mInheritedConfiguration.GetChildConfiguration(childConfigurationPath, false);
						}
					}

					if (configuration == null)
					{
						throw new ConfigurationException("The configuration does not contain an item at the specified path ({0}) or the item does not contain a valid value.", path);
					}

					return configuration.GetValue<T>(pathSegments[pathSegments.Length - 1], inherit);
				}
				else
				{
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem item = mItems[i];
						if (item.Name == itemName)
						{
							// ensure that the type of the configuration item matches the specified one
							if (item.Type != typeof(T))
							{
								throw new ConfigurationException(
									"The configuration contains an item at the specified path, but the item has a different type (configuration item: {0}, specified: {1}).",
									item.Type.FullName, typeof(T).FullName);
							}

							// return value, if the configuration item provides one
							if (item.HasValue)
							{
								return (T)item.Value;
							}
						}
					}

					// the current configuration does not contain a configuration item with the specified name or the configuration item does not contain a valid value
					// => query the next configuration in the configuration cascade, if allowed...
					if (inherit && mInheritedConfiguration != null)
					{
						return mInheritedConfiguration.GetValue<T>(path);
					}

					// there is no configuration item with the specified name and a valid value in the configuration cascade
					throw new ConfigurationException(
						"The configuration does not contain an item with the specified name ({0}) or the item does not contain a valid value.",
						path);
				}
			}
		}

		/// <summary>
		/// Gets the comment of the configuration item at the specified location.
		/// </summary>
		/// <param name="path">
		/// Relative path of the configuration item (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		/// <param name="inherit">
		/// true to try to retrieve the comment from an item in the current configuration first, then check inherited configurations;
		/// false to try to retrieve the comment from the current configuration only.
		/// </param>
		/// <returns>Comment of the configuration item (null, if the item does not exist or does not have a comment).</returns>
		public string GetComment(string path, bool inherit = true)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
					if (configuration == null)
					{
						if (inherit && mInheritedConfiguration != null)
						{
							configuration = mInheritedConfiguration.GetChildConfiguration(childConfigurationPath, false);
						}
					}

					if (configuration == null) return null;
					return configuration.GetComment(pathSegments[pathSegments.Length - 1], inherit);
				}
				else
				{
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					// query the current configuration first
					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem item = mItems[i];
						if (item.Name == itemName)
						{
							if (item.HasComment)
							{
								return item.Comment;
							}
						}
					}

					// abort, if inherited configurations are not to be checked
					if (!inherit)
					{
						return null;
					}

					// the current configuration does not contain a configuration item with the specified name or the configuration item does not contain a comment
					// => query the configuration inheriting from
					if (mInheritedConfiguration != null)
					{
						return mInheritedConfiguration.GetComment(path);
					}

					// there is no configuration item with the specified name and a comment in the configuration cascade
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the configuration item at the specified location.
		/// </summary>
		/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
		/// <param name="path">
		/// Relative path of the configuration item to get (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		/// <returns>
		/// The configuration item at the specified path;
		/// null, if the configuration does not contain a configuration item with the specified name.
		/// </returns>
		/// <exception cref="ConfigurationException">The configuration contains an item with the specified name, but the item has a different type.</exception>
		public CascadedConfigurationItem<T> GetItem<T>(string path)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
					if (configuration == null) return null;
					return configuration.GetItem<T>(pathSegments[pathSegments.Length - 1]);
				}
				else
				{
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem item = mItems[i];
						if (item.Name == itemName)
						{
							if (item.Type != typeof(T))
							{
								throw new ConfigurationException(
									"The configuration contains an item at the specified path, but with a different type (configuration item: {0}, specified: {1}).",
									item.Type.FullName, typeof(T).FullName);
							}

							return (CascadedConfigurationItem<T>)item;
						}
					}

					return null;
				}
			}
		}

		/// <summary>
		/// Gets the configuration item at the specified location.
		/// </summary>
		/// <param name="path">
		/// Relative path of the configuration item to get (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		/// <returns>
		/// The configuration item at the specified location;
		/// null, if the configuration does not contain a configuration item at the specified location.
		/// </returns>
		public ICascadedConfigurationItem GetItem(string path)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
					if (configuration == null) return null;
					return configuration.GetItem(pathSegments[pathSegments.Length - 1]);
				}
				else
				{
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem item = mItems[i];
						if (item.Name == itemName)
						{
							return item;
						}
					}

					return null;
				}
			}
		}

		/// <summary>
		/// Gets the configuration item at the specified location which has a valid value.
		/// </summary>
		/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
		/// <param name="path">
		/// Path of the configuration item to get (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		/// <param name="inherit">
		/// true to check inherited configurations as well;
		/// false to check the current configuration only.
		/// </param>
		/// <returns>
		/// The configuration item at the specified location which has a valid value;
		/// null, if the configuration does not contain a configuration item at the specified location which has a valid value.
		/// </returns>
		/// <exception cref="ConfigurationException">The configuration contains an item at the specified location, but the item has a different type.</exception>
		public CascadedConfigurationItem<T> GetItemThatHasValue<T>(string path, bool inherit = true)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
					if (configuration == null)
					{
						if (inherit && mInheritedConfiguration != null)
						{
							configuration = mInheritedConfiguration.GetChildConfiguration(childConfigurationPath, false);
						}
					}

					if (configuration == null) return null;
					return configuration.GetItemThatHasValue<T>(pathSegments[pathSegments.Length - 1], inherit);
				}
				else
				{
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem item = mItems[i];
						if (item.Name == itemName)
						{
							if (item.Type != typeof(T))
							{
								string error = string.Format("The configuration contains an item at the specified location, but with a different type (configuration item: {0}, specified: {1}).", item.Type.FullName, typeof(T).FullName);
								throw new ConfigurationException(error);
							}

							if (item.HasValue)
							{
								return (CascadedConfigurationItem<T>)item;
							}
						}
					}

					// abort, if the configuration does not have an inherited configuration
					if (mInheritedConfiguration == null)
					{
						return null;
					}

					// query inherited configuration
					return mInheritedConfiguration.GetItemThatHasValue<T>(path, true);
				}
			}
		}

		/// <summary>
		/// Gets the configuration item at the specified location which has a comment.
		/// </summary>
		/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
		/// <param name="path">
		/// Relative path of the configuration item to get (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		/// <param name="inherit">
		/// true to check the current configuration and inherited configurations as well;
		/// false to check the current configuration only.
		/// </param>
		/// <returns>
		/// The configuration item at the specified location which has a comment;
		/// null, if the configuration does not contain a configuration item at the specified location which has a comment.
		/// </returns>
		/// <exception cref="ConfigurationException">The configuration contains an item at the specified location, but the item has a different type.</exception>
		public CascadedConfigurationItem<T> GetItemThatHasComment<T>(string path, bool inherit = true)
		{
			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, true, true);

				if (pathSegments.Length > 1)
				{
					// the path contains child configurations
					// => dive into the appropriate configuration
					string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
					CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
					if (configuration == null)
					{
						if (inherit && mInheritedConfiguration != null)
						{
							configuration = mInheritedConfiguration.GetChildConfiguration(childConfigurationPath, false);
						}
					}

					if (configuration == null) return null;
					return configuration.GetItemThatHasComment<T>(pathSegments[pathSegments.Length - 1], inherit);
				}
				else
				{
					string itemName = CascadedConfigurationPathHelper.UnescapeName(path);

					// check current configuration first
					for (int i = 0; i < mItems.Count; i++)
					{
						ICascadedConfigurationItem item = mItems[i];
						if (item.Name == itemName)
						{
							if (item.HasComment)
							{
								return (CascadedConfigurationItem<T>)item;
							}
						}
					}

					// abort, if the configuration does not have an inherited configuration or querying that configuration is not allowed
					if (!inherit || mInheritedConfiguration == null)
					{
						return null;
					}

					// query inherited configuration
					return mInheritedConfiguration.GetItemThatHasComment<T>(path, true);
				}
			}
		}

		/// <summary>
		/// Gets all configuration items of the configuration and optionally all items of its child configurations
		/// (does not dive down into the configuration inheriting from, if any).
		/// </summary>
		/// <param name="recursively">
		/// true to get the items of the child configuration as well;
		/// false to get the items of the current configuration only.
		/// </param>
		/// <returns>The requested configuration items.</returns>
		public ICascadedConfigurationItem[] GetAllItems(bool recursively)
		{
			List<ICascadedConfigurationItem> items = new List<ICascadedConfigurationItem>();

			lock (mSync)
			{
				for (int i = 0; i < mItems.Count; i++)
				{
					ICascadedConfigurationItem item = mItems[i];
					items.Add(item);
				}

				if (recursively)
				{
					for (int i = 0; i < mChildren.Count; i++)
					{
						CascadedConfiguration child = mChildren[i];
						items.AddRange(child.GetAllItems(true));
					}
				}
			}

			return items.ToArray();
		}

		/// <summary>
		/// Gets the child configuration at the specified location (optionally creates a new configurations on the path).
		/// </summary>
		/// <param name="path">
		/// Relative path of the configuration to get/create (path delimiter are '/' and '\',
		/// escape these characters, if a path segment contains one of them, otherwise the segment will be split up).
		/// </param>
		/// <param name="create">
		/// true to create the child configuration, if it does not exist;
		/// false to return <c>null</c>, if the configuration does not exist.
		/// </param>
		/// <returns>
		/// The requested child configuration;
		/// null, if the child configuration at the specified path does not exist and <paramref name="create"/> is <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">The specified path is null.</exception>
		/// <exception cref="ArgumentException">The specified path is empty.</exception>
		public CascadedConfiguration GetChildConfiguration(string path, bool create)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (path.Length == 0) throw new ArgumentException("The path of the child configuration to get/create must not be empty.");

			lock (mSync)
			{
				string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(mPersistenceStrategy, path, false, true);
				string childConfigurationPath = string.Join("/", pathSegments, 1, pathSegments.Length - 1);
				string configurationName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

				for (int i = 0; i < mChildren.Count; i++)
				{
					CascadedConfiguration configuration = mChildren[i];
					if (configuration.mName == configurationName)
					{
						if (pathSegments.Length == 1) return configuration;
						return configuration.GetChildConfiguration(childConfigurationPath, create);
					}
				}

				// configuration does not exist
				// => create and add a new child configuration with the specified name, if requested
				if (create)
				{
					CascadedConfiguration configuration = AddChildConfiguration(configurationName);
					if (pathSegments.Length == 1) return configuration;
					return configuration.GetChildConfiguration(childConfigurationPath, true);
				}

				// configuration does not exist
				return null;
			}
		}

		/// <summary>
		/// Creates new instance of the <see cref="CascadedConfiguration"/> class for use as a child configuration of the current configuration
		/// (the caller will do the integration of the object into the configuration).
		/// </summary>
		/// <param name="name">Name of the configuration to create.</param>
		/// <returns>The created child configuration.</returns>
		protected virtual CascadedConfiguration AddChildConfiguration(string name)
		{
			return new CascadedConfiguration(name, this); // links itself to the current configuration
		}

		/// <summary>
		/// Loads the current settings from the storage backend (<see cref="CascadedConfiguration"/> does not have a backend storage,
		/// but derived classes may override this method to implement a storage backend).
		/// </summary>
		/// <exception cref="NotSupportedException">The configuration does not support persistence.</exception>
		public virtual void Load()
		{
			if (mPersistenceStrategy == null)
			{
				throw new NotSupportedException("The configuration does not support persistence.");
			}

			mPersistenceStrategy.Load(this);
			IsModified = false; // works recursively
		}

		/// <summary>
		/// Saves the current settings to the storage backend (<see cref="CascadedConfiguration"/> does not have a backend storage,
		/// but derived classes may override this method to implement a storage backend).
		/// </summary>
		/// <param name="flags">Flags controlling the save behavior.</param>
		/// <exception cref="NotSupportedException">The configuration does not support persistence.</exception>
		public virtual void Save(CascadedConfigurationSaveFlags flags)
		{
			if (mPersistenceStrategy == null)
			{
				throw new NotSupportedException("The configuration does not support persistence.");
			}

			mPersistenceStrategy.Save(this, flags);
			IsModified = false; // works recursively
		}

		/// <summary>
		/// Notifies configurations deriving from the current one of a change to the value of a configuration item.
		/// </summary>
		/// <param name="item">Configuration item that has changed.</param>
		/// <param name="newValue">The new value.</param>
		internal void NotifyItemValueChanged<T>(CascadedConfigurationItem<T> item, T newValue)
		{
			string escapedItemName = CascadedConfigurationPathHelper.EscapeName(item.Name);

			for (int i = 0; i < mDerivedConfigurations.Count; i++)
			{
				CascadedConfiguration configuration = mDerivedConfigurations[i];
				CascadedConfigurationItem<T> derivedItem = configuration.GetItem<T>(escapedItemName);
				if (derivedItem == null || derivedItem.HasValue) continue;
				derivedItem.OnValueChanged(item, newValue);
				configuration.NotifyItemValueChanged(item, newValue);
			}

			mIsModified = true;
		}

		/// <summary>
		/// Notifies configurations deriving from the current one of a change to the comment of a configuration item.
		/// </summary>
		/// <param name="item">Configuration item that has changed.</param>
		/// <param name="newComment">The new comment.</param>
		internal void NotifyItemCommentChanged<T>(CascadedConfigurationItem<T> item, string newComment)
		{
			string escapedItemName = CascadedConfigurationPathHelper.EscapeName(item.Name);

			for (int i = 0; i < mDerivedConfigurations.Count; i++)
			{
				CascadedConfiguration configuration = mDerivedConfigurations[i];
				CascadedConfigurationItem<T> derivedItem = configuration.GetItem<T>(escapedItemName);
				if (derivedItem == null || derivedItem.HasValue) continue;
				derivedItem.OnCommentChanged(item, newComment);
				configuration.NotifyItemCommentChanged(item, newComment);
			}

			mIsModified = true;
		}

		/// <summary>
		/// Gets the string representation of the current object.
		/// </summary>
		/// <returns>String representation of the current object.</returns>
		public override string ToString()
		{
			return string.Format("Configuration | Path: {0}", mPath);
		}

		/// <summary>
		/// Creates a new configuration item with the specified name and type.
		/// </summary>
		/// <param name="name">Name of the configuration item to create.</param>
		/// <param name="path">Path of the configuration item in the configuration hierarchy.</param>
		/// <param name="type">Type of the configuration item value.</param>
		/// <returns>The created configuration item.</returns>
		private ICascadedConfigurationItemInternal CreateItem(string name, string path, Type type)
		{
			Type itemType = typeof(CascadedConfigurationItem<>).MakeGenericType(type);
			ConstructorInfo constructor = itemType.GetConstructor(
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new Type[] { typeof(string), typeof(string) },
				null);

			return (ICascadedConfigurationItemInternal)constructor.Invoke(new object[] { name, path });
		}

	}
}

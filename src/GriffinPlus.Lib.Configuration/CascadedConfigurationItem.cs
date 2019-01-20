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
	/// An item in the <see cref="CascadedConfiguration"/>.
	/// </summary>
	public partial class CascadedConfigurationItem<T> : ICascadedConfigurationItemInternal
	{
		private readonly string mName;
		private readonly string mPath;
		private T mValue;
		private string mComment;
		private bool mHasValue;
		private bool mHasComment;

		/// <summary>
		/// Occurs when the value of the configuration item changes (directly or indirectly).
		/// </summary>
		public event EventHandler<ValueChangedEventArgs> ValueChanged;

		/// <summary>
		/// Occurs when the comment of the configuration item changes (directly or indirectly).
		/// </summary>
		public event EventHandler<CommentChangedEventArgs> CommentChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="CascadedConfigurationItem{T}"/> class.
		/// </summary>
		/// <param name="name">Name of the configuration item.</param>
		/// <param name="path">Path of the configuration item in the configuration hierarchy.</param>
		internal CascadedConfigurationItem(string name, string path)
		{
			mName = name;
			mValue = default(T);
			mHasValue = false;
			mHasComment = false;
			mPath = path;
		}

		/// <summary>
		/// Gets the configuration the current item is in.
		/// </summary>
		public CascadedConfiguration Configuration { get; internal set; }

		/// <summary>
		/// Gets the name of the configuration item.
		/// </summary>
		public string Name
		{
			get
			{
				lock (Configuration.Sync)
				{
					return mName;
				}
			}
		}

		/// <summary>
		/// Gets the path of the configuration item in the configuration hierarchy.
		/// </summary>
		public string Path
		{
			get
			{
				lock (Configuration.Sync)
				{
					return mPath;
				}
			}
		}

		/// <summary>
		/// Gets the type of the value in the configuration item.
		/// </summary>
		public Type Type
		{
			get
			{
				return typeof(T); // immutable part => no synchronization necessary
			}
		}

		/// <summary>
		/// Gets a value indicating whether the configuration item contains a valid value.
		/// </summary>
		public bool HasValue
		{
			get
			{
				lock (Configuration.Sync)
				{
					return mHasValue;
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of the configuration item.
		/// </summary>
		/// <exception cref="ConfigurationException">The configuration item does not have a value.</exception>
		/// <remarks>
		/// This property gets the value of the current configuration item, if the current configuration item provides
		/// a value for it. If it doesn't inherited configurations in the configuration cascade are queried.
		/// Setting the property effects the current configuration item only.
		/// </remarks>
		public T Value
		{
			get
			{
				lock (Configuration.Sync)
				{
					if (mHasValue)
					{
						return mValue;
					}
					else
					{
						return Configuration.GetValue<T>(mName);
					}
				}
			}

			set
			{
				lock (Configuration.Sync)
				{
					if (Configuration.PersistenceStrategy != null)
					{
						if (!Configuration.PersistenceStrategy.IsAssignable(Type, value))
						{
							throw new ConfigurationException(
								"The specified value is not supported for a configuration item of type '{0}'.",
								typeof(T).FullName);
						}
					}

					if (!mHasValue || !object.Equals(mValue, value))
					{
						mValue = value;
						mHasValue = true;
						OnValueChanged(this, value);
						Configuration.NotifyItemValueChanged<T>(this, mValue);
					}
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the configuration item contains a comment.
		/// </summary>
		public bool HasComment
		{
			get
			{
				lock (Configuration.Sync)
				{
					return mHasComment;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the configuration supports comments.
		/// </summary>
		public bool SupportsComments
		{
			get
			{
				return Configuration.PersistenceStrategy == null || Configuration.PersistenceStrategy.SupportsComments;
			}
		}

		/// <summary>
		/// Gets or sets the comment describing the configuration item.
		/// </summary>
		/// <remarks>
		/// This property gets the comment of the current configuration item, if the current configuration item provides a comment.
		/// If it doesn't inherited configurations in the configuration cascade are queried. Setting the property effects the current
		/// configuration item only.
		/// </remarks>
		public string Comment
		{
			get
			{
				lock (Configuration.Sync)
				{
					if (mHasComment)
					{
						return mComment;
					}
					else
					{
						return Configuration.GetComment(mName);
					}
				}
			}

			set
			{
				lock (Configuration.Sync)
				{
					if (Configuration.PersistenceStrategy != null && !Configuration.PersistenceStrategy.SupportsComments)
					{
						throw new NotSupportedException("The persistence strategy does not support comments.");
					}

					if (!mHasComment || !object.Equals(mComment, value))
					{
						mComment = value;
						mHasComment = true;
						OnCommentChanged(this, value);
						Configuration.NotifyItemCommentChanged(this, mComment);
					}
				}
			}
		}

		/// <summary>
		/// Resets the value of the configuration item, so an inherited configuration value is returned by the <see cref="Value"/> property.
		/// </summary>
		public void ResetValue()
		{
			lock (Configuration.Sync)
			{
				if (mHasValue)
				{
					mHasValue = false;
					mValue = default(T);

					if (Configuration.InheritedConfiguration != null)
					{
						CascadedConfigurationItem<T> item = Configuration.InheritedConfiguration.GetItemThatHasValue<T>(mName, true);
						if (item != null)
						{
							OnValueChanged(this, item.Value);
							Configuration.NotifyItemValueChanged<T>(item, item.Value);
						}
						else
						{
							throw new ConfigurationException(
								"Configuration item does not inherit a value from some other configuration (configuration: {0}, item: {1}, type: {2}).",
								Configuration.Name, mName, typeof(T).FullName);
						}
					}
					else
					{
						throw new ConfigurationException(
							"Configuration item does not inherit a value from from other configuration (configuration: {0}, item: {1}, type: {2}).",
							Configuration.Name, mName, typeof(T).FullName);
					}
				}
			}
		}

		/// <summary>
		/// Resets the comment of the configuration item, so an inherited configuration value is returned by the <see cref="Comment"/> property.
		/// </summary>
		public void ResetComment()
		{
			lock (Configuration.Sync)
			{
				if (mHasComment)
				{
					mHasComment = false;
					mComment = null;

					if (Configuration.InheritedConfiguration != null)
					{
						CascadedConfigurationItem<T> item = Configuration.InheritedConfiguration.GetItemThatHasComment<T>(mName, true);
						if (item != null)
						{
							OnCommentChanged(this, item.Comment);
							Configuration.NotifyItemCommentChanged<T>(item, item.Comment);
						}
						else
						{
							OnCommentChanged(this, null);
							Configuration.NotifyItemCommentChanged<T>(this, null);
						}
					}
					else
					{
						OnCommentChanged(this, null);
						Configuration.NotifyItemCommentChanged<T>(this, null);
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of the configuration item.
		/// </summary>
		object ICascadedConfigurationItem.Value
		{
			get { return Value; }
			set { Value = (T)value; }
		}

		/// <summary>
		/// Sets the configuration the current item is in.
		/// </summary>
		/// <param name="configuration">Configuration to set.</param>
		void ICascadedConfigurationItemInternal.SetConfiguration(CascadedConfiguration configuration)
		{
			Configuration = configuration;
		}

		/// <summary>
		/// Raises the <see cref="ValueChanged"/> event.
		/// </summary>
		/// <param name="issuer">Issuer of the event notification.</param>
		/// <param name="newValue">The new value.</param>
		protected internal virtual void OnValueChanged(CascadedConfigurationItem<T> issuer, T newValue)
		{
			EventHandler<ValueChangedEventArgs> handler = ValueChanged;
			if (handler != null)
			{
				handler(this, new ValueChangedEventArgs(issuer, newValue));
			}
		}

		/// <summary>
		/// Raises the <see cref="CommentChanged"/> event.
		/// </summary>
		/// <param name="issuer">Issuer of the event notification.</param>
		/// <param name="newComment">The new value.</param>
		protected internal virtual void OnCommentChanged(CascadedConfigurationItem<T> issuer, string newComment)
		{
			EventHandler<CommentChangedEventArgs> handler = CommentChanged;
			if (handler != null)
			{
				handler(this, new CommentChangedEventArgs(issuer, newComment));
			}
		}

		/// <summary>
		/// Gets the string representation of the current object.
		/// </summary>
		/// <returns>String representation of the current object.</returns>
		public override string ToString()
		{
			if (mHasValue)
			{
				return string.Format("Item | Path: {0} | Value: {1}", mPath, mValue);
			}
			else
			{
				try
				{
					T value = Value;
					return string.Format("Item | Path: {0} | Value: <no value> (inherited: {1})", mPath, value);
				}
				catch (ConfigurationException)
				{
					return string.Format("Item | Path: {0} | Value: <no value> (no inherited value)", mPath);
				}
			}

		}

	}
}

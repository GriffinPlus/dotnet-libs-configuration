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
		/// Event arguments carrying some information about a comment that has changed in a cascaded configuration.
		/// </summary>
		public class CommentChangedEventArgs : EventArgs
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="CommentChangedEventArgs"/> class.
			/// </summary>
			/// <param name="issuer">The configuration item whose comment has changed.</param>
			/// <param name="newComment">New comment of the configuration item.</param>
			public CommentChangedEventArgs(CascadedConfigurationItem<T> issuer, string newComment)
			{
				Issuer = issuer;
				NewComment = newComment;
			}

			/// <summary>
			/// Gets the issuer of the event (can be the same as the sender of the event or a configuration item with the same name
			/// provided by an inherited configuration in the configuration cascade).
			/// </summary>
			public CascadedConfigurationItem<T> Issuer { get; }

			/// <summary>
			/// Gets the new comment of the configuration item.
			/// </summary>
			public string NewComment { get; }

		}
	}
}

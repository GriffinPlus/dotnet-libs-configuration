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
	/// Base class for converters implementing common parts of a converter.
	/// </summary>
	/// <typeparam name="T">The type of the value the converter works with.</typeparam>
	public class Converter<T> : IConverter
	{
		/// <summary>
		/// Delegate for functions converting an object to a string.
		/// </summary>
		/// <param name="obj">Object to convert to a string.</param>
		/// <param name="provider">Format provider to use.</param>
		/// <returns>The object as a string.</returns>
		public delegate string ObjectToStringConversionDelegate(T obj, IFormatProvider provider = null);

		/// <summary>
		/// Delegate for functions converting a string to the corresponding object.
		/// </summary>
		/// <param name="s">String to parse.</param>
		/// <param name="provider">Format provider to use.</param>
		/// <returns>The created object built from the specified string.</returns>
		public delegate T StringToObjectConversionDelegate(string s, IFormatProvider provider = null);

		/// <summary>
		/// Initializes the <see cref="Converter{T}"/> class.
		/// </summary>
		static Converter()
		{
			DefaultObjectToStringConversion = new ObjectToStringConversionDelegate((obj, provider) =>
			{
				if (obj == null) throw new ArgumentNullException(nameof(obj));
				if (obj.GetType() != typeof(T)) {
					string error = string.Format("Expecting an object of type {0}, got {1}.", typeof(T).FullName, obj.GetType().FullName);
					throw new ArgumentException(error);
				}

				if (provider != null) {
					return string.Format(provider, "{0}", obj);
				} else {
					return string.Format("{0}", obj);
				}
			});
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Converter{T}"/> class.
		/// </summary>
		/// <param name="string2obj">Function that parses a string and creates an object of the corresponding type.</param>
		/// <param name="obj2string">
		/// Function that converts an object of the corresponding type to its string representation
		/// (null to use a primitive conversion using <see cref="string.Format(IFormatProvider, string, object)"/>, which
		/// suits the needs in most cases).
		/// </param>
		public Converter(StringToObjectConversionDelegate string2obj, ObjectToStringConversionDelegate obj2string = null)
		{
			ObjectToStringConversion = obj2string != null ? obj2string : DefaultObjectToStringConversion;
			StringToObjectConversion = string2obj;
		}

		/// <summary>
		/// Gets the type of the value the current converter is working with.
		/// </summary>
		public Type Type
		{
			get { return typeof(T); }
		}

		/// <summary>
		/// Gets the default conversion from an object of the corresponding type to its string representation using
		/// <see cref="string.Format(IFormatProvider, string, object)"/>, which suits the needs in most cases.
		/// </summary>
		public static ObjectToStringConversionDelegate DefaultObjectToStringConversion
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the function that converts an object of the corresponding type to its string representation.
		/// </summary>
		public ObjectToStringConversionDelegate ObjectToStringConversion
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the function that parses the string representation of an object of the corresponding type
		/// to the actual object.
		/// </summary>
		public StringToObjectConversionDelegate StringToObjectConversion
		{
			get;
			private set;
		}

		/// <summary>
		/// Converts an object to its string representation.
		/// </summary>
		/// <param name="obj">Object to convert.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (null to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The string represention of the object.</returns>
		public virtual string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			if (obj.GetType() != typeof(T)) {
				string error = string.Format("Expecting an object of type {0}, got {1}.", typeof(T).FullName, obj.GetType().FullName);
				throw new ArgumentException(error, nameof(obj));
			}

			return ObjectToStringConversion((T)obj, provider);
		}

		/// <summary>
		/// Parses the specified string creating the corresponding object.
		/// </summary>
		/// <param name="s">String to parse.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (null to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The created object.</returns>
		public object ConvertStringToObject(string s, IFormatProvider provider = null)
		{
			return StringToObjectConversion(s, provider);
		}
	}
}

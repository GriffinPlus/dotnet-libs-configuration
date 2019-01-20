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
using System.Net;

namespace GriffinPlus.Lib.Configuration
{
	/// <summary>
	/// Converters the configuration subsystem uses to format and parse setting values.
	/// </summary>
	public class Converters
	{
		/// <summary>
		/// A converter for translating a <see cref="System.Boolean"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Boolean> Boolean = new Converter<Boolean>((s, provider) => System.Boolean.Parse(s));

		/// <summary>
		/// A converter for translating a <see cref="System.SByte"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<SByte> SByte = new Converter<SByte>((s, provider) => System.SByte.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Byte"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Byte> Byte = new Converter<Byte>((s, provider) => System.Byte.Parse(s, provider));

		/// <summary>
		/// A converter for translating an array of <see cref="System.Byte"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Byte[]> ByteArray = new Converter<Byte[]>(
			(s, provider) => Convert.FromBase64String(s),
			(obj, provider) => Convert.ToBase64String((Byte[])obj)
		);

		/// <summary>
		/// A converter for translating a <see cref="System.Int16"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Int16> Int16 = new Converter<Int16>((s, provider) => System.Int16.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.UInt16"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<UInt16> UInt16 = new Converter<UInt16>((s, provider) => System.UInt16.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Int32"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Int32> Int32 = new Converter<Int32>((s, provider) => System.Int32.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.UInt32"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<UInt32> UInt32 = new Converter<UInt32>((s, provider) => System.UInt32.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Int64"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Int64> Int64 = new Converter<Int64>((s, provider) => System.Int64.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.UInt64"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<UInt64> UInt64 = new Converter<UInt64>((s, provider) => System.UInt64.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Decimal"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Decimal> Decimal = new Converter<Decimal>((s, provider) => System.Decimal.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Single"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Single> Single = new Converter<Single>((s, provider) => System.Single.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Double"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Double> Double = new Converter<Double>((s, provider) => System.Double.Parse(s, provider));

		/// <summary>
		/// The string identity conversion (the string remains the same).
		/// </summary>
		public readonly static Converter<String> String = new Converter<String>((s, provider) => s, (obj, provider) => obj);

		/// <summary>
		/// A converter for translating a <see cref="System.Guid"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<Guid> Guid = new Converter<Guid>(
			(s, provider) => System.Guid.Parse(s),
			(obj, provider) => obj.ToString("D")
		);

		/// <summary>
		/// A converter for translating a <see cref="System.DateTime"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<DateTime> DateTime = new Converter<DateTime>(
			(s, provider) => System.DateTime.Parse(s, provider),
			(obj, provider) => obj.ToString("o", provider)
		);

		/// <summary>
		/// A converter for translating a <see cref="System.TimeSpan"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<TimeSpan> TimeSpan = new Converter<TimeSpan>(
			(s, provider) => System.TimeSpan.Parse(s, provider),
			(obj, provider) => obj.ToString("c", provider)
		);

		/// <summary>
		/// A converter for translating a <see cref="System.Net.IPAddress"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter<IPAddress> IPAddress = new Converter<IPAddress>((s, provider) => System.Net.IPAddress.Parse(s));

		/// <summary>
		/// Gets all converters that are provided by the <see cref="Converters"/> class out-of-the-box.
		/// </summary>
		public readonly static IConverter[] Predefined = new IConverter[]
		{
			Boolean,
			SByte,
			Byte,
			ByteArray,
			Int16,
			UInt16,
			Int32,
			UInt32,
			Int64,
			UInt64,
			Decimal,
			Single,
			Double,
			String,
			Guid,
			DateTime,
			TimeSpan,
			IPAddress
		};

		private static object sSync = new object();
		private static volatile Dictionary<Type, IConverter> sConverters;

		/// <summary>
		/// Initializes the <see cref="Converters"/> class.
		/// </summary>
		static Converters()
		{
			// add predefined converters to the list of converters
			sConverters = new Dictionary<Type, IConverter>();
			foreach (IConverter converter in Predefined)
			{
				sConverters.Add(converter.Type, converter);
			}
		}

		/// <summary>
		/// Not available, since this is a utility class.
		/// </summary>
		private Converters()
		{

		}

		/// <summary>
		/// Registers a converter for global use, i.e. it can be queried using the <see cref="GetGlobalConverter(Type)"/> method.
		/// </summary>
		/// <param name="converter">Converter to register</param>
		public static void RegisterGlobalConverter(IConverter converter)
		{
			lock (sSync)
			{
				// copy the current global converter dictionary
				Dictionary<Type, IConverter> copy = new Dictionary<Type, IConverter>();
				foreach (KeyValuePair<Type, IConverter> kvp in sConverters)
				{
					copy.Add(kvp.Key, kvp.Value);
				}

				// add the new converter to the copy
				copy.Add(converter.Type, converter);

				// replace the old converter dictionary
				sConverters = copy;
			}
		}

		/// <summary>
		/// Gets a converter for the specified type.
		/// </summary>
		/// <param name="type">Type of the value to get a converter for.</param>
		/// <returns>
		/// A converter for the specified type;
		/// null, if there is no converter for the specified type.
		/// </returns>
		public static IConverter GetGlobalConverter(Type type)
		{
			if (type.IsEnum)
			{
				return new Converter_Enum(type);
			}

			IConverter converter = null;
			sConverters.TryGetValue(type, out converter);
			return converter;
		}

		/// <summary>
		/// Gets converters that are predefined or have been registered using the <see cref="RegisterGlobalConverter"/> method.
		/// </summary>
		public static IEnumerable<IConverter> GlobalConverters
		{
			get
			{
				return sConverters.Values;
			}
		}
	}
}

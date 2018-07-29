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
using Xunit;
using System.Reflection;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace GriffinPlus.Lib.Configuration.Tests
{
	public class CascadedConfigurationTests_NoPersistence
	{
		private static DateTime sEinsteinsBirthday = new DateTime(1879, 3, 14);
		protected enum TestEnum { A, B, C };

		public static IEnumerable<object[]> ItemValueData
		{
			get
			{
				//                          Value Type        Value
				yield return new object[] { typeof(SByte), SByte.MinValue };
				yield return new object[] { typeof(SByte), (SByte)0 };
				yield return new object[] { typeof(SByte), SByte.MaxValue };

				yield return new object[] { typeof(SByte[]), new SByte[] { SByte.MinValue, 0, SByte.MaxValue } };

				yield return new object[] { typeof(Byte), Byte.MinValue };
				yield return new object[] { typeof(Byte), Byte.MaxValue };

				yield return new object[] { typeof(Byte[]), new Byte[] { Byte.MinValue, Byte.MinValue + 1, 0x02, Byte.MaxValue - 1, Byte.MaxValue } };

				yield return new object[] { typeof(Int16), Int16.MinValue };
				yield return new object[] { typeof(Int16), (Int16)0 };
				yield return new object[] { typeof(Int16), Int16.MaxValue };

				yield return new object[] { typeof(Int16[]), new Int16[] { Int16.MinValue, 0, Int16.MaxValue } };

				yield return new object[] { typeof(UInt16), UInt16.MinValue };
				yield return new object[] { typeof(UInt16), UInt16.MaxValue };

				yield return new object[] { typeof(UInt16[]), new UInt16[] { UInt16.MinValue, UInt16.MinValue + 1, UInt16.MaxValue - 1, UInt16.MaxValue } };

				yield return new object[] { typeof(Int32), Int32.MinValue };
				yield return new object[] { typeof(Int32), (Int32)0 };
				yield return new object[] { typeof(Int32), Int32.MaxValue };

				yield return new object[] { typeof(Int32[]), new Int32[] { Int32.MinValue, 0, Int32.MaxValue } };

				yield return new object[] { typeof(UInt32), UInt32.MinValue };
				yield return new object[] { typeof(UInt32), UInt32.MaxValue };

				yield return new object[] { typeof(UInt32[]), new UInt32[] { UInt32.MinValue, UInt32.MinValue + 1, UInt32.MaxValue - 1, UInt32.MaxValue } };

				yield return new object[] { typeof(Int64), Int64.MinValue };
				yield return new object[] { typeof(Int64), (Int64)0 };
				yield return new object[] { typeof(Int64), Int64.MaxValue };

				yield return new object[] { typeof(Int64[]), new Int64[] { Int64.MinValue, 0, Int64.MaxValue } };

				yield return new object[] { typeof(UInt64), UInt64.MinValue };
				yield return new object[] { typeof(UInt64), UInt64.MaxValue };

				yield return new object[] { typeof(UInt64[]), new UInt64[] { UInt64.MinValue, UInt64.MinValue + 1, UInt64.MaxValue - 1, UInt64.MaxValue } };

				yield return new object[] { typeof(Single), Single.MinValue };
				yield return new object[] { typeof(Single), 0.0f };
				yield return new object[] { typeof(Single), Single.MaxValue };
				yield return new object[] { typeof(Single), Single.NegativeInfinity };
				yield return new object[] { typeof(Single), Single.PositiveInfinity };

				yield return new object[] { typeof(Single[]), new Single[] {
					Single.NegativeInfinity,
					Single.MinValue,
					Single.MinValue + 1,
					0.0f,
					Single.MaxValue - 1,
					Single.MaxValue,
					Single.PositiveInfinity
				}};

				yield return new object[] { typeof(Double), Double.MinValue };
				yield return new object[] { typeof(Double), 0.0 };
				yield return new object[] { typeof(Double), Double.MaxValue };
				yield return new object[] { typeof(Double), Double.NegativeInfinity };
				yield return new object[] { typeof(Double), Double.PositiveInfinity };

				yield return new object[] { typeof(Double[]), new Double[] {
					Double.NegativeInfinity,
					Double.MinValue,
					Double.MinValue + 1,
					0.0,
					Double.MaxValue - 1,
					Double.MaxValue,
					Double.PositiveInfinity
				}};

				yield return new object[] { typeof(Decimal), Decimal.MinValue };
				yield return new object[] { typeof(Decimal), Decimal.Zero };
				yield return new object[] { typeof(Decimal), Decimal.MaxValue };

				yield return new object[] { typeof(Decimal[]), new Decimal[] {
					Decimal.MinValue,
					Decimal.MinValue + 1,
					Decimal.Zero,
					Decimal.MaxValue - 1,
					Decimal.MaxValue
				} };

				yield return new object[] { typeof(String), "The quick brown fox jumps over the lazy dog" };

				yield return new object[] { typeof(String[]), new string[] {
					"The",
					"quick",
					"brown",
					"fox",
					"jumps",
					"over",
					"the",
					"lazy",
					"dog"
				}};

				yield return new object[] { typeof(TestEnum), TestEnum.A }; // 0
				yield return new object[] { typeof(TestEnum), TestEnum.B }; // 1
				yield return new object[] { typeof(TestEnum), TestEnum.C }; // 2

				yield return new object[] { typeof(Guid), Guid.Parse("{52F3FBBB-F755-468B-904E-D1B1EDD81368}") };

				yield return new object[] { typeof(Guid[]), new Guid[]
				{
					Guid.Parse("{52F3FBBB-F755-468B-904E-D1B1EDD81368}"),
					Guid.Parse("{0359E56C-81B0-4874-BF04-1B362A652465}"),
					Guid.Parse("{196BB94E-1BE3-4295-94C4-B1ED2D17DAE9}"),
				} };

				yield return new object[] { typeof(DateTime), DateTime.MinValue };
				yield return new object[] { typeof(DateTime), sEinsteinsBirthday };
				yield return new object[] { typeof(DateTime), DateTime.MaxValue };

				yield return new object[] { typeof(DateTime[]), new DateTime[] {
					DateTime.MinValue,
					sEinsteinsBirthday,
					DateTime.MaxValue
				} };

				yield return new object[] { typeof(TimeSpan), TimeSpan.MinValue };
				yield return new object[] { typeof(TimeSpan), TimeSpan.Zero };
				yield return new object[] { typeof(TimeSpan), TimeSpan.MaxValue };

				yield return new object[] { typeof(TimeSpan[]), new TimeSpan[] {
					TimeSpan.MinValue,
					TimeSpan.Zero,
					TimeSpan.MaxValue
				} };

				yield return new object[] { typeof(IPAddress), IPAddress.Parse("0.0.0.0") };               // IPv4 Any
				yield return new object[] { typeof(IPAddress), IPAddress.Parse("255.255.255.255") };       // IPv4 Broadcast
				yield return new object[] { typeof(IPAddress), IPAddress.Parse("127.0.0.1") };             // IPv4 Loopback
				yield return new object[] { typeof(IPAddress), IPAddress.Parse("192.168.10.20") };         // IPv4 Address (Private Network Range)
				yield return new object[] { typeof(IPAddress), IPAddress.Parse("::") };                    // IPv6 Any
				yield return new object[] { typeof(IPAddress), IPAddress.Parse("::0") };                   // IPv6 None
				yield return new object[] { typeof(IPAddress), IPAddress.Parse("::1") };                   // IPv6 Loopback
				yield return new object[] { typeof(IPAddress), IPAddress.Parse("fd01:dead:beef::affe") };  // IPv6 Address (ULA range)

				yield return new object[] { typeof(IPAddress[]), new IPAddress[] {
					IPAddress.Parse("0.0.0.0"),               // IPv4 Any
					IPAddress.Parse("255.255.255.255"),       // IPv4 Broadcast
					IPAddress.Parse("127.0.0.1"),             // IPv4 Loopback
					IPAddress.Parse("192.168.10.20"),         // IPv4 Address (Private Network Range)
					IPAddress.Parse("::"),                    // IPv6 Any
					IPAddress.Parse("::0"),                   // IPv6 None
					IPAddress.Parse("::1"),                   // IPv6 Loopback
					IPAddress.Parse("fd01:dead:beef::affe"),  // IPv6 Address (ULA range)
				} };

			}
		}


		protected CascadedConfiguration mConfiguration;


		public CascadedConfigurationTests_NoPersistence()
		{
			mConfiguration = new CascadedConfiguration("Test Configuration", null);
		}


		[Theory]
		[MemberData(nameof(ItemValueData))]
		public void SetValue(Type type, object value)
		{
			MethodInfo method = mConfiguration.GetType()
				.GetMethods()
				.Where(x => x.Name == nameof(CascadedConfiguration.SetValue) && x.GetParameters().Count() == 2)
				.Where(x => x.IsGenericMethodDefinition && x.GetGenericArguments().Count() == 1)
				.Single()
				.MakeGenericMethod(type);

			ICascadedConfigurationItem item = (ICascadedConfigurationItem)method.Invoke(mConfiguration, new[] { "Value", value });
			Assert.True(item.HasValue);
			Assert.Equal(type, item.Type);
			Assert.Equal(value, item.Value);
		}


#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
		[Theory]
		[MemberData(nameof(ItemValueData))]
		public void SetItem(Type type, object value)
		{
			MethodInfo method = mConfiguration.GetType()
				.GetMethods()
				.Where(x => x.Name == nameof(CascadedConfiguration.SetItem) && x.GetParameters().Count() == 1)
				.Where(x => x.IsGenericMethodDefinition && x.GetGenericArguments().Count() == 1)
				.Where(x => x.GetParameters().Count() == 1)
				.Single()
				.MakeGenericMethod(type);

			ICascadedConfigurationItem item = (ICascadedConfigurationItem)method.Invoke(mConfiguration, new[] { "Value" });
			Assert.False(item.HasValue);
			Assert.Equal(type, item.Type);
			Assert.Throws<ConfigurationException>(() => item.Value);
		}
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters


		[Theory]
		[MemberData(nameof(ItemValueData))]
		public void SetItemValue_ExistingEmptyItem(Type type, object value)
		{
			// create an item without a value
			// --------------------------------------------------------------------------------------------------------
			MethodInfo method = mConfiguration.GetType()
				.GetMethods()
				.Where(x => x.Name == nameof(CascadedConfiguration.SetItem) && x.GetParameters().Count() == 1)
				.Where(x => x.IsGenericMethodDefinition && x.GetGenericArguments().Count() == 1)
				.Where(x => x.GetParameters().Count() == 1)
				.Single()
				.MakeGenericMethod(type);

			ICascadedConfigurationItem item = (ICascadedConfigurationItem)method.Invoke(mConfiguration, new[] { "Value" });
			Assert.False(item.HasValue);
			Assert.Equal(type, item.Type);
			Assert.Throws<ConfigurationException>(() => item.Value);

			// set the value
			// --------------------------------------------------------------------------------------------------------
			item.Value = value;
			Assert.True(item.HasValue);
			Assert.Equal(type, item.Type);
			Assert.Equal(value, item.Value);
		}


		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void GetChildConfiguration_PathNull(bool create)
		{
			Assert.Throws<ArgumentNullException>(() => {
				mConfiguration.GetChildConfiguration(null, create);
			});
		}


		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void GetChildConfiguration_EmptyPath(bool create)
		{
			Assert.Throws<ArgumentException>(() => {
				mConfiguration.GetChildConfiguration("", create);
			});
		}


		[Theory]
		[InlineData("Child #1")]
		[InlineData("Child #1/Child #2")]
		[InlineData("Child #1/Child #2/Child #3")]
		public void GetChildConfiguration_AddNewAndGet(string path)
		{
			// at the beginning, the configuration should not exist
			CascadedConfiguration child = mConfiguration.GetChildConfiguration(path, false);
			Assert.Null(child);

			// create configuration
			child = mConfiguration.GetChildConfiguration(path, true);
			Assert.NotNull(child);

			// get existing configuration
			CascadedConfiguration existingChild = mConfiguration.GetChildConfiguration(path, false);
			Assert.NotNull(existingChild);
			Assert.Same(child, existingChild);
		}

	}
}



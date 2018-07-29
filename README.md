# Griffin+ Configuration

[![Build (master)](https://img.shields.io/appveyor/ci/ravenpride/dotnet-libs-configuration/master.svg?logo=appveyor)](https://ci.appveyor.com/project/ravenpride/dotnet-libs-configuration/branch/master)
[![Tests (master)](https://img.shields.io/appveyor/tests/ravenpride/dotnet-libs-configuration/master.svg?logo=appveyor)](https://ci.appveyor.com/project/ravenpride/dotnet-libs-configuration/branch/master)
[![NuGet Version](https://img.shields.io/nuget/v/GriffinPlus.Lib.Configuration.svg)](https://www.nuget.org/packages/GriffinPlus.Lib.Configuration)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GriffinPlus.Lib.Configuration.svg)](https://www.nuget.org/packages/GriffinPlus.Lib.Configuration)

## Overview

The configuration subsystem is part of the Griffin+ Common Library Suite for .NET.

The main feature are hierarchical configurations that are cascadable, i.e. multiple configurations can be stacked on another. This allows to create a composite configuration that automatically merges settings from different sources into a single configuration. Configurations at a higher level override settings derived from configurations at a lower level. The configuration subsystem comes with built-in support for persisting settings to files (XML, YAML) and the Windows Registry. Custom persistence strategies can be added as well.

The configuration subsystem is designed with usability in mind. The setup is quite easy and done with only a few lines of code. As settings in configurations are organized hierarchically, access to settings can be conveniently done using filesystem like paths. Traversing along nodes in the configuration tree is possible, but not necessary.

## Supported Platforms

The *Griffin+ Configuration* subsystem itself and the XML persistence strategy is written in .NET Standard 2.0.

Therefore these libraries should work on the following platforms (or higher):
- .NET Framework 4.6.1
- .NET Core 2.0
- Mono 5.4
- Xamarin iOS 10.14
- Xamarin Mac 3.8
- Xamarin Android 8.0
- Universal Windows Platform (UWP) 10.0.16299

The *Windows Registry* persistence strategy is platform dependent and needs the .NET framework 4.6.1 or higher to work.

## Using

TBD: easy example

### More Complex Example

To illustrate the use of the configuration subsystem in a more complex scenario, let's assume that we want to have machine specific settings in the registry and user specific settings in a XML file. Furthermore default user specific settings should be defined at machine level. Default user specific settings apply to all users unless a setting is overridden at user level.

The requirements translate into the following configuration stack:
- Configuration without persistence that contains default settings
- Configuration backed by the Windows Registry for machine specific settings and default user specific settings
- Configuration for user specific settings stored in a XML file

The following code will create the configurations, wire them up and add a machine specific setting and a user specific setting. Both settings are set in the default configuration. The machine specific configuration and the user specific configuration inherit their setting only as these configurations have a configuration item for their setting, but no own value. Furthermore the user specific setting is overridden at machine level. Saving the configurations at the end persists inherited settings in the machine configuration and the user configuration. After that a user can see the settings and modify the defaults which is less error prone than telling a user to insert a certain value with a certain name at a specific place in a configuration. Please note, that the user has a fully initialized configuration after saving inherited settings, i.e. all previously inherited user specific settings (at machine level) are overridden at  user level. Changes to default user settings after that have no effect. If you do not want this, please save using `CascadedConfigurationSaveFlags.None` instead of `CascadedConfigurationSaveFlags.SaveInheritedSettings`. This will only persist settings that are really set in the user level configuration.

```csharp
var defaultConfiguration = new CascadedConfiguration("Root Configuration", null);
var machineConfiguration = new CascadedConfiguration(defaultConfiguration, new RegistryPersistenceStrategy(@"HKEY_LOCAL_MACHINE\Software\My Company\My App"));
var userConfiguration    = new CascadedConfiguration(machineConfiguration, new XmlFilePersistenceStrategy(@"%LOCALAPPDATA%\My Company\My App"));

// add machine specific setting
string path = "/Settings/in/the/deep/Machine Name";
defaultConfiguration.SetValue<string>(path, "Fancy Machine");
machineConfiguration.SetItem<string>(path); // no value, inherits value from default configuration

// add user specific setting
path = "/Settings/somewhere/else/User Name";
defaultConfiguration.SetValue<string>(path, "Default User");
machineConfiguration.SetValue<string>(path, "Machine User");
userConfiguration.SetItem<string>(path); // no value, inherits value from default configuration

// save inherited settings to machine/user configurations
machineConfiguration.Save(CascadedConfigurationSaveFlags.SaveInheritedSettings);
userConfiguration.Save(CascadedConfigurationSaveFlags.SaveInheritedSettings);
```


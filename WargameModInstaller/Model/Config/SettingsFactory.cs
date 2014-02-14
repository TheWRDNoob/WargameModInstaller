﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Properties;

namespace WargameModInstaller.Model.Config
{
    public class SettingsFactory : ISettingsFactory
    {
        private readonly string backgroundResPath = "WargameModInstaller.Resources.BackImage.jpg";

        private Dictionary<SettingEntryType, Func<object>> factoryFuncs;

        protected Dictionary<SettingEntryType, Func<object>> FactoryFuncs
        {
            get
            {
                if (factoryFuncs == null)
                {
                    factoryFuncs = CreateFactoryFuncs();
                }

                return factoryFuncs;
            }
        }

        public virtual object CreateSettings(SettingEntryType settingType)
        {
            if (!FactoryFuncs.ContainsKey(settingType))
            {
                throw new InvalidOperationException("Cannot create setting of the given type");
            }

            return FactoryFuncs[settingType]();
        }

        public virtual T CreateSettings<T>(SettingEntryType settingType) where T : class
        {
            if (!FactoryFuncs.ContainsKey(settingType))
            {
                throw new InvalidOperationException("Cannot create setting of the given type");
            }

            var stronglyTypedSetting = FactoryFuncs[settingType]() as T;
            if (stronglyTypedSetting == null)
            {
                throw new InvalidOperationException("Cannot create setting of the given type");
            }

            return stronglyTypedSetting;
        }

        protected virtual Dictionary<SettingEntryType, Func<object>> CreateFactoryFuncs()
        {
            var funcs = new Dictionary<SettingEntryType, Func<object>>();

            funcs.Add(GeneralSettingEntryType.InstallationBackup, () => CreateGeneralSetting(GeneralSettingEntryType.InstallationBackup, Boolean.TrueString));

            funcs.Add(GeneralSettingEntryType.ModName, () => CreateGeneralSetting(GeneralSettingEntryType.ModName, Resources.DefaultModname));

            funcs.Add(GeneralSettingEntryType.CriticalCommands, () => CreateGeneralSetting(GeneralSettingEntryType.CriticalCommands, Boolean.FalseString));

            funcs.Add(ScreenSettingsEntryType.WelcomeScreen, () =>
                CreateScreenSettings(
                    ScreenSettingsEntryType.WelcomeScreen,
                    Resources.WelcomeScreenHeader,
                    Resources.WelcomeScreenDetail,
                    new WMIPath(backgroundResPath, WMIPathType.EmbeddedResource)));

            funcs.Add(ScreenSettingsEntryType.LocationScreen, () =>
                CreateScreenSettings(
                    ScreenSettingsEntryType.LocationScreen,
                    Resources.LocationScreenHeader,
                    Resources.LocationScreenDetail,
                    new WMIPath(backgroundResPath, WMIPathType.EmbeddedResource)));

            funcs.Add(ScreenSettingsEntryType.ProgressScreen, () =>
                CreateScreenSettings(
                    ScreenSettingsEntryType.ProgressScreen,
                    Resources.ProgressScreenInstallingHeader,
                    Resources.ProgressScreenInstallingDetail,
                    new WMIPath(backgroundResPath, WMIPathType.EmbeddedResource)));

            funcs.Add(ScreenSettingsEntryType.InstallCompletedScreen, () =>
                CreateScreenSettings(
                    ScreenSettingsEntryType.InstallCompletedScreen,
                    Resources.EndScreenCompletedHeader,
                    Resources.EndScreenCompletedDetail,
                    new WMIPath(backgroundResPath, WMIPathType.EmbeddedResource)));

            funcs.Add(ScreenSettingsEntryType.InstallCanceledScreen, () =>
                CreateScreenSettings(
                    ScreenSettingsEntryType.InstallCanceledScreen,
                    Resources.EndScreenCanceledHeader,
                    Resources.EndScreenCanceledDetail,
                    new WMIPath(backgroundResPath, WMIPathType.EmbeddedResource)));

            funcs.Add(ScreenSettingsEntryType.InstallFailedScreen, () =>
                CreateScreenSettings(
                    ScreenSettingsEntryType.InstallFailedScreen,
                    Resources.EndScreenFailedHeader,
                    Resources.EndScreenFailedDetail,
                    new WMIPath(backgroundResPath, WMIPathType.EmbeddedResource)));

            return funcs;
        }

        private GeneralSetting CreateGeneralSetting(GeneralSettingEntryType associatedEntryType, String value)
        {
            return new GeneralSetting(associatedEntryType, value);
        }

        private ScreenSettings CreateScreenSettings(
            ScreenSettingsEntryType associatedEntryType,
            String header,
            String description,
            WMIPath backgroundPath)
        {
            return new ScreenSettings(associatedEntryType)
            {
                Header = header,
                Description = description,
                Background = backgroundPath,
            };
        }

    }

}

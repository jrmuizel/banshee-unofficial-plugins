
using System;

using Banshee.Base;
using Banshee.Configuration;

namespace Banshee.AlarmClock
{
    public static class ConfigurationSchema
    {
        private const string conf_namespace = "plugins.alarm";
        
        public static readonly SchemaEntry<bool> IsEnabled 
            = new SchemaEntry<bool> (
                conf_namespace, "is_enabled", 
                false, "Enable the Alarm plugin",
                ""
            );

        public static readonly SchemaEntry<int> AlarmHour 
            = new SchemaEntry<int> (
                conf_namespace, "alarm_hour", 
                0, "The hour at which the alarm goes off",
                ""
            );

        public static readonly SchemaEntry<int> AlarmMinute 
            = new SchemaEntry<int> (
                conf_namespace, "alarm_minute", 
                0, "The minute at which the alarm goes off",
                ""
            );

        public static readonly SchemaEntry<string> AlarmCommand 
            = new SchemaEntry<string> (
                conf_namespace, "alarm_command", 
                "", "The command executed when the alarm goes off",
                ""
            );

        public static readonly SchemaEntry<int> FadeStartVolume 
            = new SchemaEntry<int> (
                conf_namespace, "fade_start_volume", 
                0, "The volume level at which the alarm starts",
                ""
            );

        public static readonly SchemaEntry<int> FadeEndVolume 
            = new SchemaEntry<int> (
                conf_namespace, "fade_end_volume", 
                100, "The volume level at which the alarm ends",
                ""
            );

        public static readonly SchemaEntry<int> FadeDuration 
            = new SchemaEntry<int> (
                conf_namespace, "fade_duration", 
                60, "Duration of the volume fade",
                ""
            );
    }
}
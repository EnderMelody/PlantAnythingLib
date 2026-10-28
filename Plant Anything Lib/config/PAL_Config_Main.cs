using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Cairo;
using Newtonsoft.Json.Linq;

namespace PlantAnythingLib.config
{
    public class PAL_Config_Main
    {
        public class Config_System
        {
            public System_Debug System_Debug_Acc = new();
            public class System_Debug
            {
                public bool? DebugLogging { get; set; } = Default_DebugLogging; public string Comment_DebugLogging => $"Default: {Default_DebugLogging}; Toggle; Enables debug logging output";
                public static bool? Default_DebugLogging = null;
                public bool? DebugLogging_Verbose { get; set; } = Default_DebugLogging_Verbose; public string Comment_DebugLogging_Verbose => $"Default: {Default_DebugLogging_Verbose}; Toggle; Enables verbose debug logging output";
                public static bool? Default_DebugLogging_Verbose = null;
            }
        }
    }

    public class Config_Reference
    {
        public static Config_Reference Config_Loaded { get; set; } = new Config_Reference();
        public const string Config_FilePath = $"{VarRef.ModName}_Config.json";
        [JsonExtensionData] public Dictionary<string, JToken>? Config_UnknownValues { get; set; } = [];

        public PAL_Config_Main.Config_System Config_System { get; set; } = new();
    }
}

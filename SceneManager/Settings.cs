﻿using Rage;
using System.Collections.Generic;
using System.Windows.Forms;
using SceneManager.Utils;
using System.IO;

namespace SceneManager
{
    internal static class Settings
    {
        internal static readonly InitializationFile ini = new InitializationFile("Plugins/SceneManager.ini");

        // Keybindings
        internal static Keys ToggleKey = Keys.T;
        internal static Keys ModifierKey = Keys.LShiftKey;
        internal static ControllerButtons ToggleButton = ControllerButtons.Y;
        internal static ControllerButtons ModifierButton = ControllerButtons.A;

        // Plugin Settings
        internal static bool Enable3DWaypoints = true;
        internal static bool EnableMapBlips = true;
        internal static bool EnableHints = true;
        internal static SpeedUnits SpeedUnit = SpeedUnits.MPH;
        internal static float BarrierPlacementDistance = 30f;
        internal static bool EnableAdvancedBarricadeOptions = false;
        internal static bool EnableBarrierLightsDefaultOn = false;

        // Default Waypoint Settings
        internal static int CollectorRadius = 1;
        internal static int SpeedZoneRadius = 5;
        internal static bool StopWaypoint = false;
        internal static bool DirectDrivingBehavior = false;
        internal static int WaypointSpeed = 5;

        // Barriers
        internal static Dictionary<string, Model> barriers = new Dictionary<string, Model>();
        //internal static List<string> barrierKeys = new List<string>();
        //internal static List<Model> barrierValues = new List<Model>();

        internal static void LoadSettings()
        {
            Game.LogTrivial("Loading SceneManager.ini settings");
            ini.Create();

            // Keybindings
            ToggleKey = ini.ReadEnum("Keybindings", "ToggleKey", Keys.T);
            ModifierKey = ini.ReadEnum("Keybindings", "ModifierKey", Keys.LShiftKey);
            ToggleButton = ini.ReadEnum("Keybindings", "ToggleButton", ControllerButtons.A);
            ModifierButton = ini.ReadEnum("Keybindings", "ModifierButton", ControllerButtons.DPadDown);
            
            // Plugin Settings
            Enable3DWaypoints = ini.ReadBoolean("Plugin Settings", "Enable3DWaypoints", true);
            EnableMapBlips = ini.ReadBoolean("Plugin Settings", "EnableMapBlips", true);
            EnableHints = ini.ReadBoolean("Plugin Settings", "EnableHints", true);
            SpeedUnit = ini.ReadEnum("Plugin Settings", "SpeedUnits", SpeedUnits.MPH);
            BarrierPlacementDistance = ini.ReadInt32("Plugin Settings", "BarrierPlacementDistance", 30);
            EnableAdvancedBarricadeOptions = ini.ReadBoolean("Plugin Settings", "EnableAdvancedBarricadeOptions", false);
            EnableBarrierLightsDefaultOn = ini.ReadBoolean("Plugin Settings", "EnableBarrierLightsDefaultOn", false);

            // Default Waypoint Settings
            CollectorRadius = ini.ReadInt32("Default Waypoint Settings", "CollectorRadius", 1);
            SpeedZoneRadius = ini.ReadInt32("Default Waypoint Settings", "SpeedZoneRadius", 5);
            StopWaypoint = ini.ReadBoolean("Default Waypoint Settings", "StopWaypoint", false);
            DirectDrivingBehavior = ini.ReadBoolean("Default Waypoint Settings", "DirectDrivingBehavior", false);
            WaypointSpeed = ini.ReadInt32("Default Waypoint Settings", "WaypointSpeed", 5);
            CheckForValidWaypointSettings();
            
            // Barriers
            foreach(string displayName in ini.GetKeyNames("Barriers"))
            {
                var model = new Model(ini.ReadString("Barriers", displayName.Trim()));
                if (model.IsValid)
                {
                    barriers.Add(displayName, model);
                    //barrierKeys.Add(key.Trim());
                    //barrierValues.Add(model);
                }
                else
                {
                    Game.LogTrivial($"{model.Name} is not valid.");
                }
            }

            //ImportPaths();

            void CheckForValidWaypointSettings()
            {
                if(CollectorRadius > 50 || CollectorRadius < 1)
                {
                    CollectorRadius = 1;
                    Game.LogTrivial($"Invalid value for CollectorRadius in user settings, resetting to default.");
                }
                if(SpeedZoneRadius > 200 || SpeedZoneRadius < 5)
                {
                    SpeedZoneRadius = 5;
                    Game.LogTrivial($"Invalid value for SpeedZoneRadius in user settings, resetting to default.");
                }
                if (CollectorRadius > SpeedZoneRadius)
                {
                    CollectorRadius = 1;
                    SpeedZoneRadius = 5;
                    Game.LogTrivial($"CollectorRadius is greater than SpeedZoneRadius in user settings, resetting to defaults.");
                }
                if (WaypointSpeed > 100 || WaypointSpeed < 5)
                {
                    WaypointSpeed = 5;
                    Game.LogTrivial($"Invalid value for WaypointSpeed in user settings, resetting to default.");
                }
            }

            void ImportPaths()
            {
                //read each file name in Saved Paths
                var GAME_DIRECTORY = Directory.GetCurrentDirectory();
                var SAVED_PATHS_DIRECTORY = GAME_DIRECTORY + "/plugins/SceneManager/Saved Paths/";
                if (!Directory.Exists(SAVED_PATHS_DIRECTORY))
                {
                    Game.LogTrivial($"Directory '/plugins/SceneManager/Saved Paths' does not exist.  No paths available to import.");
                    return;
                }

                //add file name to PathMainMenu.importedPaths
                var paths = Directory.GetFiles(SAVED_PATHS_DIRECTORY);
                if (paths.Length == 0)
                {
                    Game.LogTrivial($"No saved paths found.");
                    return;
                }

                foreach (string path in paths)
                {
                    Game.LogTrivial($"Path to import: {Path.GetFileName(path)}");

                    // Check if XML is valid before actually importing
                }
            }
        }

        internal static void UpdateSettings(bool threeDWaypointsEnabled, bool mapBlipsEnabled, bool hintsEnabled, SpeedUnits unit)
        {
            ini.Write("Other Settings", "Enable3DWaypoints", threeDWaypointsEnabled);
            ini.Write("Other Settings", "EnableMapBlips", mapBlipsEnabled);
            ini.Write("Other Settings", "EnableHints", hintsEnabled);
            ini.Write("Other Settings", "SpeedUnits", unit);
        }
    }
}

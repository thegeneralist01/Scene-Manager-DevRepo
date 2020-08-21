﻿using System.Drawing;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace SceneManager
{
    class EditPathMenu
    {
        public static UIMenu editPathMenu { get; private set; }
        private static UIMenuItem editPathWaypoints, deletePath;
        public static UIMenuCheckboxItem togglePath;

        internal static void InstantiateMenu()
        {
            editPathMenu = new UIMenu("Scene Manager", "~o~Edit Path");
            editPathMenu.ParentMenu = PathMainMenu.pathMainMenu;
            MenuManager.menuPool.Add(editPathMenu);
        }

        public static void BuildEditPathMenu()
        {
            editPathMenu.AddItem(togglePath = new UIMenuCheckboxItem("Disable Path", false));
            editPathMenu.AddItem(editPathWaypoints = new UIMenuItem("Edit Waypoints"));
            editPathMenu.AddItem(deletePath = new UIMenuItem("Delete Path"));

            editPathMenu.RefreshIndex();
            editPathMenu.OnItemSelect += EditPath_OnItemSelected;
            editPathMenu.OnCheckboxChange += EditPath_OnCheckboxChange;
        }

        private static void EditPath_OnItemSelected(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            var currentPath = PathMainMenu.GetPaths()[PathMainMenu.editPath.Index];

            if (selectedItem == editPathWaypoints)
            {
                EditWaypointMenu.BuildEditWaypointMenu();
            }

            if (selectedItem == deletePath)
            {
                PathMainMenu.DeletePath(currentPath, currentPath.PathNum - 1, PathMainMenu.Delete.Single);
            }
        }

        private static void EditPath_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool @checked)
        {
            if (checkboxItem == togglePath)
            {
                var currentPath = PathMainMenu.GetPaths()[PathMainMenu.editPath.Index];
                if (togglePath.Checked)
                {
                    currentPath.DisablePath();
                    Game.LogTrivial($"Path {currentPath.PathNum} disabled.");
                }
                else
                {
                    currentPath.EnablePath();
                    Game.LogTrivial($"Path {currentPath.PathNum} enabled.");
                }
            }
        }
    }
}

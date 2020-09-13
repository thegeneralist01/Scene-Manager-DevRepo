﻿using Rage;
using System.Drawing;
using System.Linq;

namespace SceneManager
{
    public class Waypoint
    {
        private Path _path { get; set; }
        private int _number { get; set; }
        private Vector3 _position { get; set; }
        private float _speed { get; set; }
        private VehicleDrivingFlags _drivingFlag { get; set; }
        private Blip _blip { get; set; }
        private bool _isCollector { get; set; }
        private float _collectorRadius { get; set; }
        private Blip _collectorRadiusBlip { get; set; }
        private float _speedZoneRadius { get; set; }
        private uint _speedZone { get; set; }
        private bool _enableWaypointMarker { get; set; } = true;
        private bool _enableEditMarker { get; set; } = false;

        public Path Path { get { return _path; } set { _path = value; } }
        public int Number { get { return _number; } set { _number = value; } }
        public Vector3 Position { get { return _position; } }
        public float Speed { get { return _speed; } }
        public VehicleDrivingFlags DrivingFlag { get { return _drivingFlag; } }
        public Blip Blip { get { return _blip; } }
        public bool IsCollector { get { return _isCollector; } }
        public float CollectorRadius { get { return _collectorRadius; } }
        public Blip CollectorRadiusBlip { get { return _collectorRadiusBlip; } }
        public float SpeedZoneRadius { get { return _speedZoneRadius; } }
        public uint SpeedZone { get { return _speedZone; } set { _speedZone = value; } }
        public bool EnableWaypointMarker { get { return _enableWaypointMarker; } set { _enableWaypointMarker = value; } }
        public bool EnableEditMarker { get { return _enableEditMarker; } set { _enableEditMarker = value; } }

        public Waypoint(Path path, int waypointNum, Vector3 waypointPos, float speed, VehicleDrivingFlags drivingFlag, Blip waypointBlip, bool collector = false, float collectorRadius = 1, float speedZoneRadius = 0)
        {
            _path = path;
            _number = waypointNum;
            _position = waypointPos;
            _speed = speed;
            _drivingFlag = drivingFlag;
            _blip = waypointBlip;
            _isCollector = collector;
            _collectorRadius = collectorRadius;
            _speedZoneRadius = speedZoneRadius;
            AddSpeedZone();
            _collectorRadiusBlip = new Blip(waypointBlip.Position, collectorRadius)
            {
                Color = waypointBlip.Color,
            };
            if (SettingsMenu.mapBlips.Checked)
            {
                _collectorRadiusBlip.Alpha = 0.5f;
            }
            else
            {
                _collectorRadiusBlip.Alpha = 0f;
            }
            DrawWaypointMarker();
        }

        public void UpdateWaypoint(Waypoint currentWaypoint, VehicleDrivingFlags drivingFlag, float speed, bool collectorWaypointChecked, float collectorRadius, float speedZoneRadius, bool updateWaypointPositionChecked)
        {
            UpdateDrivingFlag(drivingFlag);
            UpdateWaypointSpeed(speed);
            UpdateCollectorOptions();
            if (updateWaypointPositionChecked)
            {
                UpdateWaypointPosition(Game.LocalPlayer.Character.Position);
            }

            void UpdateDrivingFlag(VehicleDrivingFlags newDrivingFlag)
            {
                if(_drivingFlag == VehicleDrivingFlags.StopAtDestination && newDrivingFlag != VehicleDrivingFlags.StopAtDestination)
                {
                    foreach(CollectedVehicle cv in VehicleCollector.collectedVehicles.Where(cv => cv.Path == _path && cv.CurrentWaypoint == this && cv.StoppedAtWaypoint))
                    {
                        Game.LogTrivial($"Setting StoppedAtWaypoint to false for {cv.Vehicle.Model.Name}");
                        cv.StoppedAtWaypoint = false;
                    }
                }
                _drivingFlag = newDrivingFlag;
                if (newDrivingFlag == VehicleDrivingFlags.StopAtDestination)
                {
                    _blip.Color = Color.Red;
                }
                else
                {
                    _blip.Color = Color.Green;
                }
            }

            void UpdateWaypointSpeed(float newWaypointSpeed)
            {
                _speed = newWaypointSpeed;
            }

            void UpdateCollectorOptions()
            {
                if (collectorWaypointChecked)
                {
                    _isCollector = true;
                    RemoveSpeedZone();
                    _speedZone = World.AddSpeedZone(Game.LocalPlayer.Character.Position, SpeedZoneRadius, speed);
                    _blip.Color = Color.Blue;
                    if (_collectorRadiusBlip)
                    {
                        _collectorRadiusBlip.Position = Game.LocalPlayer.Character.Position;
                        _collectorRadiusBlip.Alpha = 0.5f;
                        _collectorRadiusBlip.Scale = collectorRadius * 0.5f;
                    }
                    else
                    {
                        _collectorRadiusBlip = new Blip(Blip.Position)
                        {
                            Color = Blip.Color,
                            Alpha = 0.5f
                        };
                    }
                    _collectorRadius = collectorRadius;
                    _speedZoneRadius = speedZoneRadius;
                }
                else
                {
                    _isCollector = false;
                    RemoveSpeedZone();
                    if (_collectorRadiusBlip)
                    {
                        _collectorRadiusBlip.Delete();
                    }
                }
            }

            void UpdateWaypointPosition(Vector3 newWaypointPosition)
            {
                _position = newWaypointPosition;
                RemoveSpeedZone();
                AddSpeedZone();
                UpdateWaypointBlipPosition();
            }

            void UpdateWaypointBlipPosition()
            {
                _blip.Position = Game.LocalPlayer.Character.Position;
            }
        }

        public void AddSpeedZone()
        {
            _speedZone = World.AddSpeedZone(_position, _speedZoneRadius, _speed);
        }

        public void RemoveSpeedZone()
        {
            World.RemoveSpeedZone(_speedZone);
        }

        public void DrawWaypointMarker()
        {
            // This is called once when the waypoint is created
            GameFiber.StartNew(() =>
            {
                while (SettingsMenu.threeDWaypoints.Checked && _enableWaypointMarker && _path.Waypoints.Contains(this))
                {
                    if (EditWaypointMenu.editWaypointMenu.Visible && EditWaypointMenu.editWaypoint.Value == Number)
                    {
                        if (EditWaypointMenu.collectorWaypoint.Checked)
                        {
                            if (EditWaypointMenu.updateWaypointPosition.Checked)
                            {
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Game.LocalPlayer.Character.Position.X, Game.LocalPlayer.Character.Position.Y, Game.LocalPlayer.Character.Position.Z - 1, 0, 0, 0, 0, 0, 0, (float)EditWaypointMenu.changeCollectorRadius.Value * 2, (float)EditWaypointMenu.changeCollectorRadius.Value * 2, 1f, 80, 130, 255, 80, false, false, 2, false, 0, 0, false);
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Game.LocalPlayer.Character.Position.X, Game.LocalPlayer.Character.Position.Y, Game.LocalPlayer.Character.Position.Z - 1, 0, 0, 0, 0, 0, 0, (float)EditWaypointMenu.changeSpeedZoneRadius.Value * 2, (float)EditWaypointMenu.changeSpeedZoneRadius.Value * 2, 1f, 255, 185, 80, 80, false, false, 2, false, 0, 0, false);
                            }
                            else
                            {
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, (float)EditWaypointMenu.changeCollectorRadius.Value * 2, (float)EditWaypointMenu.changeCollectorRadius.Value * 2, 1f, 80, 130, 255, 100, false, false, 2, false, 0, 0, false);
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, (float)EditWaypointMenu.changeSpeedZoneRadius.Value * 2, (float)EditWaypointMenu.changeSpeedZoneRadius.Value * 2, 1f, 255, 185, 80, 100, false, false, 2, false, 0, 0, false);
                            }
                        }
                        else if (EditWaypointMenu.changeWaypointType.SelectedItem == "Stop")
                        {
                            if (EditWaypointMenu.updateWaypointPosition.Checked)
                            {
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, 1f, 1f, 1f, 255, 65, 65, 100, false, false, 2, false, 0, 0, false);
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Game.LocalPlayer.Character.Position.X, Game.LocalPlayer.Character.Position.Y, Game.LocalPlayer.Character.Position.Z - 1, 0, 0, 0, 0, 0, 0, 1f, 1f, 1f, 255, 65, 65, 100, false, false, 2, false, 0, 0, false);
                            }
                            else
                            {
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, 1f, 1f, 1f, 255, 65, 65, 100, false, false, 2, false, 0, 0, false);
                            }

                        }
                        else
                        {
                            if (EditWaypointMenu.updateWaypointPosition.Checked)
                            {
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, 1f, 1f, 1f, 65, 255, 65, 100, false, false, 2, false, 0, 0, false);
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Game.LocalPlayer.Character.Position.X, Game.LocalPlayer.Character.Position.Y, Game.LocalPlayer.Character.Position.Z - 1, 0, 0, 0, 0, 0, 0, 1f, 1f, 1f, 65, 255, 65, 100, false, false, 2, false, 0, 0, false);
                            }
                            else
                            {
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, 1f, 1f, 1f, 65, 255, 65, 100, false, false, 2, false, 0, 0, false);
                            }
                        }
                    }
                    else if ((_path.State == State.Finished && MenuManager.menuPool.IsAnyMenuOpen()) || (_path.State == State.Creating && PathCreationMenu.pathCreationMenu.Visible))
                    {
                        if (IsCollector && CollectorRadius > 0)
                        {
                            Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, CollectorRadius * 2, CollectorRadius * 2, 1f, 80, 130, 255, 80, false, false, 2, false, 0, 0, false);
                            if (SpeedZoneRadius > 0)
                            {
                                Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, SpeedZoneRadius * 2, SpeedZoneRadius * 2, 1f, 255, 185, 80, 80, false, false, 2, false, 0, 0, false);
                            }
                        }
                        else if (DrivingFlag == VehicleDrivingFlags.StopAtDestination)
                        {
                            Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, 1f, 1f, 1f, 255, 65, 65, 80, false, false, 2, false, 0, 0, false);
                        }
                        else
                        {
                            Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, Position.X, Position.Y, Position.Z - 1, 0, 0, 0, 0, 0, 0, 1f, 1f, 1f, 65, 255, 65, 80, false, false, 2, false, 0, 0, false);
                        }
                    }

                    GameFiber.Yield();
                }
            });
        }

        public void EnableBlip()
        {
            if(!PathMainMenu.GetPaths().Where(p => p == _path).First().IsEnabled)
            {
                _blip.Alpha = 0.5f;
                _collectorRadiusBlip.Alpha = 0.25f;
            }
            else
            {
                _blip.Alpha = 1.0f;
                _collectorRadiusBlip.Alpha = 0.5f;
            }

        }

        public void DisableBlip()
        {
            _blip.Alpha = 0;
            _collectorRadiusBlip.Alpha = 0;
        }

        public void RemoveWaypoint()
        {
            _path = null;
            _number = 0;
            _position = new Vector3(0,0,0);
            _speed = 0;
            _drivingFlag = 0;
            _blip.Delete();
            _isCollector = false;
            _collectorRadius = 0;
            _speedZoneRadius = 0;
            RemoveSpeedZone();
            _collectorRadiusBlip.Delete();
            _enableWaypointMarker = false;
        }
    }
}

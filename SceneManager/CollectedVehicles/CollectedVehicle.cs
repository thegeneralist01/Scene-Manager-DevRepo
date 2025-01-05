using Rage;
using System.Collections.Generic;
using System.Linq;
using SceneManager.Utils;
using SceneManager.Waypoints;
using SceneManager.Paths;

namespace SceneManager.CollectedPeds
{
    internal class CollectedVehicle : Vehicle
    {
        internal CollectedPed BoundPed;

        internal CollectedVehicle(Vehicle baseVehicle, CollectedPed ped) : base(baseVehicle.Handle)
        {
            Handle = baseVehicle.Handle;
            BoundPed = ped;
            //GameFiber.StartNew(() => AssignWaypointTasks(), "Task Assignment Fiber");
        }

        public bool OptionalCleanUp()
        {
            if (!this) return true;
            if (!BoundPed
                || (BoundPed && (BoundPed.IsDead || !BoundPed.CurrentVehicle || (BoundPed.CurrentVehicle && BoundPed.CurrentVehicle.Handle != Handle))))
            {
                if (this.IsOnFire || this.IsDead)
                {
                    this.Repair();
                }

                this.Delete();
                return true;
            }
            return false;
        }
    }
}
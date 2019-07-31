using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using DJI.WindowsSDK;
using System.Threading.Tasks;

namespace _24S
{
    class DJIMissionManager
    {
        public static DJIMissionManager Instance { get; } = new DJIMissionManager(); // Singleton

        private static Waypoint InitWaypoint(double latitude, double longitude, double altitude, double gimbalPitch, double speed, int stayTimeMinutes, int rotation)
        {
            Waypoint waypoint = new Waypoint()
            {
                location = new LocationCoordinate2D() { latitude = latitude, longitude = longitude },
                altitude = altitude,
                gimbalPitch = 0,
                turnMode = WaypointTurnMode.CLOCKWISE,
                heading = 0,
                actionRepeatTimes = 1,
                actionTimeoutInSeconds = 60,
                cornerRadiusInMeters = 0.2,
                speed = 5,
                shootPhotoTimeInterval = -1,
                shootPhotoDistanceInterval = -1,
                waypointActions = new List<WaypointAction>()
                {
                    //new WaypointAction(){actionType = WaypointActionType.STAY, actionParam = 0},
                    //new WaypointAction(){actionType = WaypointActionType.ROTATE_AIRCRAFT, actionParam = rotation},
                }
            };
            return waypoint;
        }


        public SDKError LoadMission(String json_mision)
        {

            JArray points = JArray.Parse(json_mision);

            int waypointCount = points.Count;
            double maxFlightSpeed = 15; // meters per second TODO
            double autoFlightSpeed = 10; // meters per second TODO
            int missionID = 0; //TODO

            List<Waypoint> waypoints = new List<Waypoint>(waypointCount);

            foreach (JObject point in points)
            {
                JObject coord = (JObject)point.SelectToken("coord");
                JArray coordinates = (JArray)coord.SelectToken("coordinates");
                double latitude = (double)coordinates.First[0];
                double longitude = (double)coordinates.First[1];
                double gimbalPitch = (double)point.SelectToken("angulo");
                double speed = (double)point.SelectToken("velocidad"); // meters per second
                double altitude = (double)point.SelectToken("altura");
                int stayTime = (int)point.SelectToken("tiempo");
                int rotation = (int)point.SelectToken("rotacion");
                waypoints.Add(InitWaypoint(latitude, longitude, altitude, gimbalPitch, speed, stayTime, rotation));
                System.Diagnostics.Debug.WriteLine("lat {0}, lng {1})", latitude, longitude);
            }


            WaypointMission mission = new WaypointMission()
            {
                waypointCount = points.Count,
                maxFlightSpeed = maxFlightSpeed,
                autoFlightSpeed = autoFlightSpeed,
                finishedAction = WaypointMissionFinishedAction.GO_HOME,
                headingMode = WaypointMissionHeadingMode.AUTO,
                flightPathMode = WaypointMissionFlightPathMode.NORMAL,
                gotoFirstWaypointMode = WaypointMissionGotoFirstWaypointMode.SAFELY,
                exitMissionOnRCSignalLostEnabled = false,
                gimbalPitchRotationEnabled = true,
                repeatTimes = 1,
                missionID = missionID,
                waypoints = waypoints
            };

            //LoadMission
            SDKError loadError = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).LoadMission(mission);
            System.Diagnostics.Debug.WriteLine("LOAD MISSION: " + loadError.ToString());

            return loadError;

        }

        public async Task<SDKError> UploadMission()
        {
            //UploadMission
            SDKError uploadError = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadMission();
            System.Diagnostics.Debug.WriteLine("UPLOAD MISSION: " + uploadError.ToString());

            return uploadError;
        }

        public async Task<SDKError> StartMission()
        {
            //6. StartMission
            SDKError errStartMission = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StartMission();
            System.Diagnostics.Debug.WriteLine("START MISSION: " + errStartMission.ToString());

            return errStartMission;
        }

        public string WaypointMissionCurrentState()
        {
            return DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState().ToString();
        }

    }
}

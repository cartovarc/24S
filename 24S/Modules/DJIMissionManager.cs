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

        private static Waypoint InitWaypoint(double latitude, double longitude, double altitude, double gimbalPitch, double speed, int stayTimeSeconds, int rotation, int orientation)
        {
            Waypoint waypoint = new Waypoint()
            {
                location = new LocationCoordinate2D() { latitude = latitude, longitude = longitude },
                altitude = altitude,
                gimbalPitch = gimbalPitch,
                turnMode = WaypointTurnMode.CLOCKWISE,
                //heading = orientation,
                actionRepeatTimes = 1,
                actionTimeoutInSeconds = 60,
                cornerRadiusInMeters = 0.2,
                speed = 2.0,
                shootPhotoTimeInterval = -1,
                shootPhotoDistanceInterval = -1,
                waypointActions = new List<WaypointAction>()
                {
                    //new WaypointAction(){actionType = WaypointActionType.STAY, actionParam = stayTimeSeconds*1000 },
                    //new WaypointAction(){actionType = WaypointActionType.ROTATE_AIRCRAFT, actionParam = rotation},
                }
            };
            return waypoint;
        }


        public SDKError LoadMission(String json_mission)
        {
            JObject missionData = JObject.Parse(json_mission);
            string landingType = (string) missionData.SelectToken("tipo_aterrizaje");
            JArray points = (JArray) missionData.SelectToken("puntos_mision");

            JObject firstPoint = (JObject) missionData.SelectToken("punto_inicial");
            double firstPointLatitude = (double) firstPoint.SelectToken("latitud");
            double firstPointLongitude = (double) firstPoint.SelectToken("longitud");
            double firstPointAltitude = (double)firstPoint.SelectToken("altitud");

            int waypointCount = points.Count;
            double maxFlightSpeed = 15; // meters per second TODO
            double autoFlightSpeed = 10; // meters per second TODO
            int missionID = 0; //TODO

            List<Waypoint> waypoints = new List<Waypoint>(waypointCount);

            // TODO use class to retrieve constants
            if (landingType.Equals("USE_PL_ARAS"))
            {
                double goFirstPointSpeed = 5;
                waypoints.Add(InitWaypoint(firstPointLatitude, firstPointLongitude, firstPointAltitude, 0, goFirstPointSpeed, 0, 0, 0));
            }

            foreach (JObject point in points)
            {
                double latitude = (double)point.SelectToken("latitud");
                double longitude = (double)point.SelectToken("longitud");
                double gimbalPitch = (double)point.SelectToken("angulo");
                double speed = (double)point.SelectToken("velocidad"); // meters per second
                double altitude = (double)point.SelectToken("altitud");
                int stayTime = (int)point.SelectToken("tiempo");
                int rotation = (int)point.SelectToken("rotacion");
                int orientation = (int)point.SelectToken("orientacion");
                waypoints.Add(InitWaypoint(latitude, longitude, altitude, gimbalPitch, speed, stayTime, rotation, orientation));
            }

            double nowLat = DJIComponentManager.Instance.AircraftLocation.latitude;
            double nowLng = DJIComponentManager.Instance.AircraftLocation.longitude;
            double goOverFirstPointSpeed = 15;
            double overFirstPointAltitude = 48;
            double landingSpeed = 2; // m per s
            double returnToHomeGimbalAngle = -90;

            // TODO use class to retrieve constants
            if (landingType.Equals("USE_PL_ARAS"))
            {
                //last waypoints
                waypoints.Add(InitWaypoint(firstPointLatitude, firstPointLongitude, overFirstPointAltitude, returnToHomeGimbalAngle, goOverFirstPointSpeed, 0, 0, 0));
                waypoints.Add(InitWaypoint(firstPointLatitude, firstPointLongitude, firstPointAltitude, returnToHomeGimbalAngle, landingSpeed, 0, 0, 0));
                waypoints.Add(InitWaypoint(nowLat, nowLng, firstPointAltitude, returnToHomeGimbalAngle, landingSpeed, 0, 0, 0));
                waypointCount += 4; //additional waypoints
            }

            WaypointMission mission = new WaypointMission()
            {
                waypointCount = waypointCount,
                maxFlightSpeed = maxFlightSpeed,
                autoFlightSpeed = autoFlightSpeed,
                finishedAction = landingType.Equals("USE_PL_ARAS") ? WaypointMissionFinishedAction.AUTO_LAND : WaypointMissionFinishedAction.GO_HOME,
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
            LoggingServices.Instance.WriteLine<DJIComponentManager>("LOAD MISSION: " + loadError.ToString(), MetroLog.LogLevel.Trace);

            return loadError;

        }

        public async Task<SDKError> UploadMission()
        {
            //UploadMission
            SDKError uploadError = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadMission();
            LoggingServices.Instance.WriteLine<DJIComponentManager>("UPLOAD MISSION: " + uploadError.ToString(), MetroLog.LogLevel.Trace);

            return uploadError;
        }

        public async Task<SDKError> StartMission()
        {
            //6. StartMission
            SDKError errStartMission = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StartMission();
            LoggingServices.Instance.WriteLine<DJIComponentManager>("START MISSION: " + errStartMission.ToString(), MetroLog.LogLevel.Trace);

            return errStartMission;
        }

        public async Task<SDKError> PauseMission()
        {
            SDKError errPauseMission = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).PauseMission();
            LoggingServices.Instance.WriteLine<DJIComponentManager>("PAUSE MISSION: " + errPauseMission.ToString(), MetroLog.LogLevel.Trace);

            return errPauseMission;
        }

        public async Task<SDKError> ResumeMission()
        {
            SDKError errResumeMission = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).ResumeMission();
            LoggingServices.Instance.WriteLine<DJIComponentManager>("RESUME MISSION: " + errResumeMission.ToString(), MetroLog.LogLevel.Trace);

            return errResumeMission;
        }

        public async Task<SDKError> StopMission()
        {
            SDKError errStopMission = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StopMission();
            LoggingServices.Instance.WriteLine<DJIComponentManager>("STOP MISSION: " + errStopMission.ToString(), MetroLog.LogLevel.Trace);

            return errStopMission;
        }
        public string WaypointMissionCurrentState()
        {
            return DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState().ToString();
        }

    }
}

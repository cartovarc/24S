using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DJI.WindowsSDK;

namespace _24S
{
    class MessageManager
    {

        public static MessageManager Instance { get; } = new MessageManager(); // Singleton

        private void sendStringMessageToClient(Stream clientStream, String message)
        {
            try
            {
                byte[] msgToSend = System.Text.Encoding.ASCII.GetBytes(message);
                clientStream.Write(msgToSend, 0, msgToSend.Length);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("SOMETHING WRONG SENDING MESSAGE TO STREAM CLIENT: sendStringMessageToClient print: {0}", e.ToString());
            }

        }

        private string buildResponse(bool success, string resultCode, string data)
        {
            dynamic jsonObject = new JObject();
            jsonObject.success = success;
            jsonObject.resultCode = resultCode == null ? "NULL" : resultCode; ;
            jsonObject.data = data == null ? "NULL" : data; ;
            String jsonString = JsonConvert.SerializeObject(jsonObject);

            return jsonString;
        }

        public async void OnSocketDataReceivedAsync(Stream clientStream, String message)
        {
            System.Diagnostics.Debug.WriteLine("MESSAGE RECEIVED: OnSocketDataReceivedAsync print: {0}", message);

            // {"COMMAND": 'GET_LOCATION',
            //  "COMMAND_TYPE": 'TELEMETRY',
            //  "COMMAND_INFO": "NONE",
            // }

            JObject messageObject = null;

            try
            {
                messageObject = JObject.Parse(message);
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                sendStringMessageToClient(clientStream, buildResponse(false, "JSON_PARSE_ERROR", null));
                System.Diagnostics.Debug.WriteLine("JSON PARSE ERROR: OnSocketDataReceivedAsync print {0}", e.ToString());
                return;
            }

            
            string command = (string)messageObject.SelectToken("COMMAND");
            string commandTpye = (string)messageObject.SelectToken("COMMAND_TYPE");

            string resultCode = null, dataToClient = null;

            if (commandTpye.Equals("MISSION"))
            {
                if (command.Equals("SET_PRECISION_LANDING"))
                {
                    SDKError err = await DJIComponentManager.Instance.SetPrecisionLanding(true); //TODO use command_info to set false too
                    resultCode = err.ToString();
                }
                else if (command.Equals("SET_GSME"))
                {
                    SDKError err = await DJIComponentManager.Instance.SetGroundStationModeEnabled(true); //TODO use command_info to set false too
                    resultCode = err.ToString();
                }
                else if (command.Equals("LOAD_MISSION"))
                {
                    JArray infoCommand = (JArray)messageObject.SelectToken("COMMAND_INFO");
                    SDKError err = DJIMissionManager.Instance.LoadMission(infoCommand.ToString());
                    resultCode = err.ToString();

                }
                if (command.Equals("UPLOAD_MISSION"))
                {
                    SDKError err = await DJIMissionManager.Instance.UploadMission();
                    resultCode = err.ToString();
                }
                else if (command.Equals("START_MISSION"))
                {
                    SDKError err = await DJIMissionManager.Instance.StartMission();
                    resultCode = err.ToString();
                }
                else if (command.Equals("GET_MISSION_STATE"))
                {
                    resultCode = SDKError.NO_ERROR.ToString();
                    dataToClient = DJIMissionManager.Instance.WaypointMissionCurrentState();
                }
            }
            else if (commandTpye.Equals("TELEMETRY"))
            {
                if (command.Equals("GET_LOCATION"))
                {
                    if (DJIComponentManager.Instance.AircraftConnection)
                    {
                        dynamic jsonObject = new JObject();
                        jsonObject.latitude = DJIComponentManager.Instance.AircraftLocation.latitude;
                        jsonObject.longitude = DJIComponentManager.Instance.AircraftLocation.longitude;
                        jsonObject.altitude = DJIComponentManager.Instance.AircraftAltitude;
                        String jsonString = JsonConvert.SerializeObject(jsonObject);

                        resultCode = SDKError.NO_ERROR.ToString();
                        dataToClient = jsonString;
                    }
                    else
                    {
                        resultCode = SDKError.DISCONNECTED.ToString();
                    }
                }
            }
            else if (commandTpye.Equals("VIDEO_STREAMING"))
            {
                if (command.Equals("START_VIDEO_STREAMING"))
                {
                    DJIVideoManager.Instance.InitializeVideoFeedModule();
                    resultCode = SDKError.NO_ERROR.ToString(); //TODO
                }
                else if (command.Equals("STOP_VIDEO_STREAMING"))
                {
                    DJIVideoManager.Instance.UninitializeVideoFeedModule();
                    resultCode = SDKError.NO_ERROR.ToString();//TODO
                }
                else if (command.Equals("RESTART_VIDEO_STREAMING"))
                {
                    DJIVideoManager.Instance.UninitializeVideoFeedModule();
                    DJIVideoManager.Instance.InitializeVideoFeedModule();
                    resultCode = SDKError.NO_ERROR.ToString();//TODO
                }
                else if (command.Equals("ESTABLISH_CONNECTION"))
                {
                    DJIVideoManager.Instance.videoClient = clientStream; //set a video stream client
                    resultCode = SDKError.NO_ERROR.ToString();//TODO
                }
                    
            }else if (commandTpye.Equals("GIMBAL"))
            {
                if (command.Equals("ROTATE_GIMBAL"))
                {
                    double factor = 10.0;
                    JObject infoCommand = (JObject)messageObject.SelectToken("COMMAND_INFO");
                    double pitch = (double)infoCommand.SelectToken("y");
                    double roll = 0.0; //(double)infoCommand.SelectToken("roll");
                    double yaw = (double)infoCommand.SelectToken("x");
                    double duration = 1; //(double)infoCommand.SelectToken("duration");
                    System.Diagnostics.Debug.WriteLine("mensaje");
                    SDKError err = await DJIComponentManager.Instance.RotateGimbalByAngle(pitch * factor, roll * factor, yaw * factor, duration);
                    resultCode = err.ToString();
                }
            }

            sendStringMessageToClient(clientStream, buildResponse(resultCode == SDKError.NO_ERROR.ToString(), resultCode, dataToClient));

        }
    }
}

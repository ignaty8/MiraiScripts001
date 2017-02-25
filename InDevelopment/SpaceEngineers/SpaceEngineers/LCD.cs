#region pre-script
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...
        public Program()
        {
            LcdSetupForDebugging("VarPanel");
        }

        public void Save()
        {

        }

        public void Main(string argument)
        {
            string n = LcdClear();
            int k = 1 + Convert.ToInt32(n);
            LcdPrint(k.ToString());
        }
        //to this comment.
        #region post-script
        #endregion

        
        /// <summary>
        /// Checks if the first string contains the second string at the front.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public bool StringBeginsWith(string text, string header)
        {
            if(text.Length < header.Length)
            {
                return false;
            }
            return text.Substring(0, header.Length).Equals(header);
        }

        /// <summary>
        /// Returns block of given name and Type T. Enabling anyBlock returns any block of type T if no name match is found.
        /// </summary>
        /// <typeparam name="T">Block Tpye</typeparam>
        /// <param name="blockName">Block Name</param>
        /// <param name="anyBlock"></param>
        /// <returns></returns>
        public T GetBlock<T>(string blockName, bool anyBlock = false) where T : class, IMyTerminalBlock
        {
            T block = GridTerminalSystem.GetBlockWithName(blockName) as T;
            if (block != null && !anyBlock)
            {
                return block;
            }
            List<T> blocks = new List<T>();
            GridTerminalSystem.GetBlocksOfType<T>(blocks);
            if (blocks.Count > 0)
            {
                return blocks[0];
            }
            else
            {
                return null;
            }
        }

        #region Angle-And-Rotation-Basic

        /// <summary>
        /// Represents angle units. Frequency has 1 = pi radians.
        /// </summary>
        public enum AngleUnit {
            RADIAN, DEGREE, FREQUENCY
        }

        /// <summary>
        /// Maps each AngleUnit to a corresponding coefficient that represents the vaule at pi radians.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static float AngleUnitConvertionCoefficient(AngleUnit unit) {
            switch (unit) {
                case AngleUnit.RADIAN:
                    return (float)(Math.PI);

                case AngleUnit.DEGREE:
                    return 180f;

                case AngleUnit.FREQUENCY:
                    return 1f;

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Stores roll pitch and yaw as an object as well as the units used. Allows converting from one unit to another.
        /// Values are stored as floats rather than doubles because gyroscopes accept (or used to) floats only.
        /// </summary>
        public struct RollPitchYaw {
            public float roll, pitch, yaw;
            public AngleUnit unit;

            public RollPitchYaw(float r, float p, float y, AngleUnit unit = AngleUnit.RADIAN) {
                roll = r;
                pitch = p;
                yaw = y;
                this.unit = unit;
            }

            public void ConvertToUnit(AngleUnit newUnit) {
                roll = roll / AngleUnitConvertionCoefficient(this.unit) * AngleUnitConvertionCoefficient(newUnit);
            }

            public static float ConverToUnit(float angle, AngleUnit newUnit, AngleUnit oldUnit) {
                return angle * AngleUnitConvertionCoefficient(newUnit) / AngleUnitConvertionCoefficient(oldUnit);
            }
        }

        #endregion

        #region Quaternions
        #region Quaternions-Basic-Operations

        #region Special-Constants
        /*     Yaw-    Yaw+
         *      <------->
         *         /|\ Pitch-
         *          |
         * Roll-    |     Roll+
         *  <--------------->
         *          |
         *          |
         *         \|/ Pitch+
         *      (Top View)
        */
        public Quaternion YawPlus = Quaternion.CreateFromTwoVectors(Vector3.Forward, Vector3.Right);
        public Quaternion YawMinus = Quaternion.CreateFromTwoVectors(Vector3.Forward, Vector3.Left);
        public Quaternion PitchPlus = Quaternion.CreateFromTwoVectors(Vector3.Forward, Vector3.Up);
        public Quaternion PitchMinus = Quaternion.CreateFromTwoVectors(Vector3.Forward, Vector3.Down);
        public Quaternion RollPlus = Quaternion.CreateFromTwoVectors(Vector3.Up, Vector3.Right);
        public Quaternion RollMinus = Quaternion.CreateFromTwoVectors(Vector3.Up, Vector3.Left);
        #endregion

        #region Vector-Rotation

        /// <summary>
        /// Rotates provided vector by quaternion, effectively switching coordinate system. 
        /// If you set "reverse", the vector will be rotated in the opposite direction (useful for switching from local to world coordinates).
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="quaternion"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public Vector3 RotateVectorByQuaternion(Vector3 vector, Quaternion quaternion, bool reverse = false) {
            //Calculates [0, v'] = q * [0, v] * q^{-1}
            Quaternion vectorQuat = VectorToQuaternionXYZ(vector);//Quaternion.CreateFromAxisAngle(vector, 0f); // = [0, v]
            Quaternion conjugate = Quaternion.Conjugate(quaternion);
            //Note quaternion multiplication is associative, but NOT commutative (i.e. do not change the order here).
            Quaternion resultQuat;
            //LcdPrintln(vectorQuat.ToString());
            //LcdPrintln(quaternion.ToString());
            //LcdPrintln(conjugate.ToString());
            if (!reverse) {
                resultQuat = Quaternion.Multiply(quaternion, Quaternion.Multiply(vectorQuat, conjugate));
            } else {
                resultQuat = Quaternion.Multiply(conjugate, Quaternion.Multiply(vectorQuat, quaternion));
            }
            Vector3 result = new Vector3();
            float angle = 0f;
            //angle should stay 0!
            resultQuat.GetAxisAngle(out result, out angle);
            return QuaternionXYZToVector(resultQuat);
        }

        /// <summary>
        /// Returns quaternion with X,Y,Z equal to those of the vector and sets W to 0.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public Quaternion VectorToQuaternionXYZ(Vector3 vector) {
            return new Quaternion(vector.X, vector.Y, vector.Z, 0);
        }

        /// <summary>
        /// Returns vector with the X,Y,Z equal to thos of the quaternion.
        /// </summary>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public Vector3 QuaternionXYZToVector(Quaternion quaternion) {
            return new Vector3(quaternion.X, quaternion.Y, quaternion.Z);
        }

        #endregion

        #region Roll-Pitch-Yaw-Conversions

        //TODO: Change all floats to doubles?
        /// <summary>
        /// Converts quaternion to RollPitchYaw object.
        /// </summary>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public RollPitchYaw QuaternionToRollPitchYaw(Quaternion quaternion) {
            RollPitchYaw rpy = new RollPitchYaw();
            QuaternionToRollPitchYaw(quaternion, out rpy.roll, out rpy.pitch, out rpy.yaw);
            return rpy;
        }

        /// <summary>
        /// Converts quaternion to roll, pitch, yaw values.
        /// </summary>
        /// <param name="quaternion"></param>
        /// <param name="roll"></param>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        public void QuaternionToRollPitchYaw(Quaternion quaternion, out float roll, out float pitch, out float yaw) {
            double w = quaternion.W;
            double x = quaternion.X;
            double y = quaternion.Y;
            double z = quaternion.Z;

            // Yaw/roll seems to be switched...
            yaw = (float)-Math.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z);
            pitch = (float)Math.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z);
            roll = (float)-Math.Asin(2 * x * y + 2 * z * w);
            //roll  = Math.Atan2(2*y*w + 2*x*z, 1 - 2*y*y - 2*z*z);
            //pitch = Math.Atan2(2 * x * w + 2 * y * z, 1 - 2 * x * x - 2 * z * z);
            //yaw = Math.Asin(2 * x * y + 2 * z * w);
        }

        #endregion

        #endregion

        #region Quaternion-Printing-And-Reading

        /// <summary>
        /// Debug prints a quaternion in Roll/Pitch/Yaw form. Option to chose specific lcd name.
        /// </summary>
        /// <param name="quaternion"></param>
        /// <param name="lcdName"></param>
        public void QuaternionToRollPitchYawDebugPrint(Quaternion quaternion, string quaternionName = "", string lcdName = "") {
            float r, p, y;
            QuaternionToRollPitchYaw(quaternion, out r, out p, out y);
            string output = "RPY:" + quaternionName + ":" + r.ToString() + ":" + p.ToString() + ":" + y.ToString();
            if (lcdName.Equals("")) {
                LcdPrintln(output);
            } else {
                LcdPrintln(output, lcdName);
            }

        }

        /// <summary>
        /// Converts quaternion to raw string of format QAT:Name:W:X:Y:Z
        /// </summary>
        /// <param name="quat"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string QuaternionToRawString(Quaternion quat, string name = "") {
            return "QAT:" + name + ":" + quat.W + ":" + quat.X + ":" + quat.Y + ":" + quat.Z;
        }

        /// <summary>
        /// Decodes raw quaternion string in format QAT:Name:W:X:Y:Z
        /// </summary>
        /// <param name="rawQuat"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Quaternion QuaternionFromRawString(string rawQuat, out string name) {
            string[] parametres = rawQuat.Split(':');

            if (!parametres[0].Equals("QAT")) {
                name = null;
                return Quaternion.Zero;
            }
            Quaternion quat = new Quaternion(Convert.ToSingle(parametres[2]), Convert.ToSingle(parametres[3]), Convert.ToSingle(parametres[4]), Convert.ToSingle(parametres[5]));
            name = parametres[1];
            return quat;
        }

        /// <summary>
        /// Decodes raw quaternion string in format QAT:Name:W:X:Y:Z
        /// </summary>
        /// <param name="rawQuat"></param>
        /// <returns></returns>
        public Quaternion QuaternionFromRawString(string rawQuat) {
            string name;
            return QuaternionFromRawString(rawQuat, out name);
        }

        #endregion

        #region Quaternions-Grid-Specific-Actions

        /// <summary>
        /// Returns ship rotation in relation to world coordinate system.
        /// </summary>
        /// <returns></returns>
        public Quaternion GetGridRotation() {
            IMyRemoteControl remote = getRemoteControl();
            Vector3 origin = remote.CubeGrid.GridIntegerToWorld(Vector3I.Zero);
            Vector3 forward = remote.CubeGrid.GridIntegerToWorld(Vector3I.Forward);
            forward = forward - origin;
            //LcdPrintln(VectorToGPS(forward,"ForwardVector"));
            //Should get ship's rotation in relation to origin... Should.
            Quaternion shipRotation = Quaternion.CreateFromTwoVectors(Vector3.Forward, forward);
            return shipRotation;
        }

        #endregion
        #endregion

        #region GPS-to-Vector3

        //TODO: Add checks for the 'GPS:' token at head of string.

        /// <summary>
        /// Converts GPS-formatted string to Vector3 Object. If string is not GPS-formatted, returns Vector3.MaxValue
        /// </summary>
        /// <param name="gps"></param>
        /// <returns></returns>
        public Vector3 GPStoVector(string gps)
        {

            string[] coords;
            // The following are BRP comments (handle with _care_):
            //Getting Target Position//

            coords = gps.Split(':');

            //Pending, need to change indices

            
            if (!coords[0].Equals("GPS") && !coords[0].Equals("ABS") && !coords[0].Equals("REL")) {
                return Vector3.MaxValue;
            }

            Vector3 gpsvector = new Vector3(Convert.ToSingle(coords[2]),
            Convert.ToSingle(coords[3]),
            Convert.ToSingle(coords[4]));

            return gpsvector;
        }

        public string GPStoName(string gps)
        {
            string[] coords = gps.Split(':');
            return coords[1];
        }

        public Dictionary<string, Vector3> coords;


        /// <summary>
        /// Encodes GPS dictionary in human-readable way.
        /// </summary>
        /// <param name="coords">Dictionary to encode</param>
        /// <returns></returns>
        public string EncodeGPS(Dictionary<string,Vector3> coords, char separator = '\n')
        {
            string output = "";
            foreach(KeyValuePair<string,Vector3> coord in coords){
                output += VectorToGPS(coord.Value, coord.Key) + separator;
            }
            return output;
        }

        /// <summary>
        /// Decodes group of GPS coordinates separated by a specific character. Skips sections if headers don't match "GPS".
        /// </summary>
        /// <param name="list"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public Dictionary<string, Vector3> DecodeGPS(string list, char separator)
        {
            Dictionary<string, Vector3> coords = new Dictionary<string, Vector3>();
            string[] coordsList = list.Split(separator);
            
            foreach(string coordString in coordsList)
            {
                if(!StringBeginsWith(coordString, "GPS")) { continue; }
                coords.Add(GPStoName(coordString) ,GPStoVector(coordString));
            }

            return coords;
        }

        public Dictionary<string, Vector3> DecodeGPS(string list)
        {
            return DecodeGPS(list, '\n');
        }

        public string EncodeGPSSingle(KeyValuePair<string, Vector3> pair)
        {
            return VectorToGPS(pair.Value, pair.Key);
        }

        public KeyValuePair<string, Vector3> DecodeGPSSingle(string gps)
        {
            if (!StringBeginsWith(gps, "GPS")) { /*TODO: trouble*/}
            return new KeyValuePair<string, Vector3>(GPStoName(gps), GPStoVector(gps));
        }

        public string VectorToGPS(Vector3 vec)
        {
            return VectorToGPS(vec, "location");
        }

        public string VectorToGPS(Vector3 vec, string name)
        {
            string output;

            output = "GPS:" + name + ":"
            + Convert.ToString(vec.X) + ":"
            + Convert.ToString(vec.Y) + ":"
            + Convert.ToString(vec.Z) + ":";

            return output;
        }

        #endregion

        #region GPS-Generalised
        public Dictionary<string, Vector3> DecodeGPSGeneral(string list, char separator, Vector3 referencePos, Quaternion referenceRot)
        {
            Dictionary<string, Vector3> coords = new Dictionary<string, Vector3>();
            string[] coordsList = list.Split(separator);

            foreach (string coordString in coordsList)
            {
                if (StringBeginsWith(coordString, "GPS"))
                {
                    coords.Add(GPStoName(coordString), GPStoVector(coordString));
                }
                else if(StringBeginsWith(coordString, "ABS"))
                {
                    LcdPrintln(coordString);
                    Vector3 gps = GPStoVector(coordString);
                    //TODO: Weird bugs roam here

                    LcdPrintln(VectorToGPS(gps, "ABS-PreTest"));
                    LcdPrintln(VectorToGPS(referencePos, "ABS-Reference"));
                    gps.Add(referencePos);
                    LcdPrintln(VectorToGPS(gps, "ABS-Test"));
                    //coords.Add(GPStoName(coordString), gps);
                }
                else if(StringBeginsWith(coordString, "REL"))
                {
                    //TODO: Untested, highly experimental dark maths...
                    Vector3 relativeDirection = GPStoVector(coordString);
                    Quaternion moveDirection = Quaternion.CreateFromTwoVectors(Vector3.Forward, relativeDirection);
                    moveDirection = Quaternion.Concatenate(moveDirection, referenceRot);
                    Vector3 direction = new Vector3();
                    Quaternion.GetForward(ref moveDirection, out direction);
                    coords.Add(GPStoName(coordString), direction);
                }
            }
            return coords;
        }
        #endregion

        #region LCD-Print-Basic

        public void LcdPrintln(string msg)
        {
            LcdPrint(msg + '\n');
        }

        public void LcdPrint(string msg)
        {
            LcdPrint(msg, "VarPanel");
        }

        public void LcdPrintln(string msg, string lcdName)
        {
            LcdPrint(msg + '\n', lcdName);
        }

        public void LcdPrint(string msg, string lcdName)
        {
            IMyTextPanel lcd =
              GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
            lcd.WritePublicText(lcd.GetPublicText() + msg);
        }

        /// <summary>
        /// Clears selected LCD's public text and returns it.
        /// </summary>
        /// <param name="lcdName">Block Name of LCD (not the Title!)</param>
        /// <returns></returns>
        public string LcdClear(string lcdName)
        {
            IMyTextPanel lcd =
              GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
            string text = lcd.GetPublicText();
            lcd.WritePublicText("");
            return text;

        }

        /// <summary>
        /// Clears the default "VarPanel" LCD.
        /// </summary>
        /// <returns></returns>
        public string LcdClear()
        {
            return LcdClear("VarPanel");
        }

        /// <summary>
        /// Finds an LCD with the specified string as its block name (not its Title!)
        /// </summary>
        /// <param name="lcdName"></param>
        /// <returns></returns>
        public IMyTextPanel getLcd(string lcdName)
        {
            return GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
        }

        public IMyTextPanel getLcd()
        {
            return getLcd("VarPanel");
        }

        #endregion

        #region LCD-Advanced

        /// <summary>
        /// Sets up selected LCD for printing debug info.
        /// </summary>
        /// <param name="lcdName"></param>
        public void LcdSetupForDebugging(string lcdName)
        {
            IMyTextPanel lcd = getLcd(lcdName);

            // Seems to be unaccessible in-game :(
            //lcd.SetShowOnScreen(VRage.Game.GUI.TextPanel.ShowTextOnScreenFlag.PUBLIC);
        }

        #endregion

        #region Remote-Control-Basic

        /// <summary>
        /// Gets reference to remote control of that name.
        /// Setting "anyRemote" to true causes method to return a random remote if there is none that match the name.
        /// </summary>
        /// <param name="remoteName"></param><param name="anyRemote"></param>
        /// <returns></returns>
        public IMyRemoteControl getRemoteControl(string remoteName, bool anyRemote = false)
        {
            IMyRemoteControl remote = GridTerminalSystem.GetBlockWithName(remoteName) as IMyRemoteControl;
            if (remote != null && !anyRemote)
            {
                return remote;
            }
            List<IMyRemoteControl> remotes = new List<IMyRemoteControl>();
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remotes);
            if (remotes.Count > 0)
            {
                return remotes[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get default remote control, "DefaultRemote".
        /// </summary>
        /// <returns></returns>
        public IMyRemoteControl getRemoteControl()
        {
            return getRemoteControl("DefaultRemote", true);
        }

        #endregion

        #region Remote-Control-Waypoints

        /// <summary>
        /// Gets the waypoint coordinates from selected remote control.
        /// </summary>
        /// <param name="remoteName">Remote's block name.</param>
        /// <returns></returns>
        public Dictionary<string,Vector3> RemoteControlGetWaypointsGPS(string remoteName)
        {

            IMyRemoteControl remote = getRemoteControl(remoteName);

            List<MyWaypointInfo> waypoints = new List<MyWaypointInfo>();
            remote.GetWaypointInfo(waypoints);

            Dictionary<string, Vector3> coords = new Dictionary<string, Vector3>();

            foreach (MyWaypointInfo waypoint in waypoints)
            {
                coords.Add(waypoint.Name, waypoint.Coords);
            }

            //LcdPrint(EncodeGPS(coords));
            return coords;
        }

        /// <summary>
        /// Gets waypoints of default remote control, "DefaultRemote".
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Vector3> RemoteControlGetWaypointsGPS()
        {
            return RemoteControlGetWaypointsGPS("DefaultRemote");
        }

        /// <summary>
        /// Replaces waypoints of given remote control.
        /// </summary>
        /// <param name="coords"></param>
        public void RemoteControlSetWaypointsGPS(Dictionary<string,Vector3> coords, string remoteName)
        {
            IMyRemoteControl remote = getRemoteControl(remoteName);

            remote.ClearWaypoints();
            RemoteControlAddWaypointGPS(coords, remoteName);
        }
        
        /// <summary>
        /// Replaces waypoints of default remote control.
        /// </summary>
        /// <param name="coords"></param>
        public void RemoteControlSetWaypointsGPS(Dictionary<string, Vector3> coords)
        {
            RemoteControlSetWaypointsGPS(coords, "DefaultRemote");
        }

        /// <summary>
        /// Appends waypoints to given remote control.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="remoteName"></param>
        public void RemoteControlAddWaypointGPS(Dictionary<string,Vector3> coords, IMyRemoteControl remote)
        {
            foreach (KeyValuePair<string, Vector3> coord in coords)
            {
                remote.AddWaypoint(coord.Value, coord.Key);
            }
        }

        /// <summary>
        /// Appends waypoints to given remote control.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="remoteName"></param>
        public void RemoteControlAddWaypointGPS(Dictionary<string, Vector3> coords, string remoteName)
        {
            IMyRemoteControl remote = getRemoteControl(remoteName);
            RemoteControlAddWaypointGPS(coords, remote);
        }

        #endregion

        #region Gyro-Control

        #region Simple-Gyro-Operations

        /// <summary>
        /// Returns gyro-specific rotation based on orientation.
        /// </summary>
        /// <param name="desiredLocalRotation"></param>
        /// <param name="gyroName"></param>
        /// <returns></returns>
        public Quaternion GyroMovementFromLocalRotation(Quaternion desiredLocalRotation, string gyroName) {
            return GyroMovementFromLocalRotation(desiredLocalRotation, GetBlock<IMyGyro>(gyroName));
        }

        /// <summary>
        /// Returns gyro-specific rotation based on orientation.
        /// </summary>
        /// <param name="desiredLocalRotation"></param>
        /// <param name="gyro"></param>
        /// <returns></returns>
        public Quaternion GyroMovementFromLocalRotation(Quaternion desiredLocalRotation, IMyGyro gyro) {
            Quaternion gyroOrientation;//= new Quaternion();
            gyro.Orientation.GetQuaternion(out gyroOrientation);
            return GyroMovementFromLocalRotation(desiredLocalRotation, gyroOrientation);
        }

        /// <summary>
        /// Returns gyro-specific rotation based on orientation.
        /// </summary>
        /// <param name="desiredLocalRotation"></param>
        /// <param name="gyroOrientation"></param>
        /// <returns></returns>
        public Quaternion GyroMovementFromLocalRotation(Quaternion desiredLocalRotation, Quaternion gyroOrientation) {
            return Quaternion.Multiply(Quaternion.Conjugate(gyroOrientation), desiredLocalRotation);
        }

        #endregion

        #region Gyro-Status-Printing

        /// <summary>
        /// Prints rotation override parametres of given gyro in RPY:rpyName:Roll:Pitch:Yaw format.
        /// </summary>
        /// <param name="gyro"></param>
        /// <param name="rpyName"></param>
        /// <param name="lcdName"></param>
        public void GyroRollPitchYawDebugPrint(IMyGyro gyro, string rpyName = "", string lcdName = "") {
            if (rpyName.Equals("")) {
                rpyName = gyro.CustomName;
            }
            string text = "RPY:" + rpyName + ":" + gyro.Roll.ToString() + ":" + gyro.Pitch.ToString() + ":" + gyro.Yaw.ToString();
            if (lcdName.Equals("")) {
                LcdPrintln(text);
            } else {
                LcdPrintln(text, lcdName);
            }
        }

        #endregion

        #endregion

        #region MSG-And-Commands

        #region MSG-Decoder
        // TODO: Returns error code if message decoding fails.
        public void MSGReceiver(string msg)
        {
            bool debug = true;
            string[] components = msg.Split(';');

            string[] header = components[0].Split(':');

            if (!MSGHeaderIsValid(header))
            {
                //return -1;
                LcdPrintln("Invalid MSG Header!");
                return;
            }
            if (debug) { LcdPrintln("MSGHeader Verified"); }


            if (!MSGIsReceiver(components[1]))
            {
                LcdPrintln("Not Message Receiver!");
                return;
            }
            if (debug) { LcdPrintln("Receiver Validated"); }

            //Bit inefficient but works for now.
            //components.RemoveIndices<string>(new List<int>(0));

            bool cmd = false, psw = false;

            switch (header[1])
            {
                //TODO: Implement passwords
                case "CMD":
                    cmd = true;
                    break;
            }

            // The remaining bits of the header are reserved for 
            // custom functionality, such as passwords.
            for (int k = 2; k < header.Length; k++)
            {
                switch (header[k])
                {
                    case "PSW":
                        psw = true;
                        break;
                }
            }

            if (debug) { LcdPrintln("MSG Header Decoded"); }

            // Analyses remaining data blocks
            for (int k = 4; k < components.Length; k++)
            {
                string[] splitData = components[k].Split(':');
                if (psw)
                {
                    if (splitData[0].Equals("Pass"))
                    {
                        if (!MSGPasswordAnalyser(splitData[1]))
                        {
                            // TODO: Improve authentication system!
                            LcdPrintln("MSG Authentication Failure!");
                            return;
                        }
                        if (debug) { LcdPrintln("PSW Authentication Success"); }
                    }
                }
            }

            if (debug) { LcdPrintln("Custom Data Blocks Analysed"); }

            //Interprets commands
            if (cmd)
            {
                CommandWrapperInterpreter(components);

                if (debug) { LcdPrintln("Command Executed"); }
                //return;
            }

            if (debug) { LcdPrintln("MSG Execution Complete"); }
        }

        public bool MSGPasswordAnalyser(string pass)
        {
            string storedPass = getLcd("PasswordStore").GetPublicText();
            return storedPass.Equals(pass);
        }

        // MSG:CMD:PSW;Rec:1.2.3.4.UID;RCT'MV'RemoteName'GPS:x:y:z;Pass:XYZilovecookies;

        public bool MSGHeaderIsValid(string[] header)
        {
            return header[0].Equals("MSG");
        }

        /// <summary>
        /// Checks that current grid has the adress of MSG Rec section.
        /// </summary>
        /// <param name="receiverSection">Rec(eiver) section of MSG</param>
        /// <returns></returns>
        public bool MSGIsReceiver(string receiverSection)
        {
            string[] splitSection = receiverSection.Split(':');

            if (!splitSection[0].Equals("Rec"))
            {
                //TODO: React to badly formed message
                return false;
            }

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(splitSection[1], blocks);
            {
                return blocks.Count > 0;
            }
        }

        #endregion

        #region CMD-Wrapper-And-Decoders

        public void CommandWrapperInterpreter(string[] cwrap)
        {
            //TODO: Fix offset in case command contained in message...
            string[] cmd = cwrap[3].Split('\'');

            switch (cmd[0])
            {
                case "RCT":
                    RemoteControlCommandInterpreter(cmd);
                    return;
            }
        }

        public void RemoteControlCommandInterpreter(string[] cmd)
        {
            //RCT'cmd'RemoteName'args
            IMyRemoteControl remote = getRemoteControl(cmd[2]);
            switch (cmd[1])
            {
                //RCT'MV'RemoteName'GPS:Name:x:y:z|GPS:x:y:z
                case "MV":
                    //TODO: Implement single waypoint addition!
                    //TODO: Optimise by passing object refernce rather than object name...
                    //Dictionary<string, Vector3> waypoint = DecodeGPS(cmd[3],'|');
                    Vector3 rcPos = remote.GetPosition();;
                    Quaternion rcRot = Quaternion.Identity;//remote.CubeGrid; TODO: How do you get the rotation?!

                    Dictionary<string, Vector3> waypoint = DecodeGPSGeneral(cmd[3],'|', rcPos, rcRot);

                    RemoteControlSetWaypointsGPS(waypoint, cmd[2]);
                    remote.SetAutoPilotEnabled(true);
                    return;
            }
        }

        #endregion

        #endregion

        public IMyRadioAntenna GetAntenna(string antennaName, bool anyAntenna = false)
        {
            return GetBlock<IMyRadioAntenna>(antennaName, anyAntenna);
        }

        public IMyRadioAntenna GetAntenna()
        {
            return GetAntenna("DefaultAntenna", true);
        }

        public void DebugPrintBlockActions(IMyTerminalBlock block)
        {
            List<ITerminalAction> actions = new List<ITerminalAction>();
            block.GetActions(actions);
            foreach (ITerminalAction action in actions)
            {
                LcdPrintln(action.Name.ToString());
            }
        }

        //IMyTextPanel tact = GridTerminalSystem.GetBlockWithName(TACTICAL_INFO_NAME) as IMyTextPanel;

        public enum AssemblerComponent
        {
            Missile = 0, GatlingAmmo = 1, GunAmmo = 2, Glass = 3, Computer = 4, Box = 5, Detector = 6,
            Display = 7, Explosive = 8, Girder = 9, Gravity = 10, InteriorPlate = 11, LargeTube = 12,
            Medical = 13, Grid = 14, Motor = 15, Battery = 16, Radio = 17, Reactor = 18, SmallTube = 19,
            Solar = 20, Steel = 21, Superconductor = 22, Thruster = 23
        }
        //TODO: Add custom location name support

    }
}
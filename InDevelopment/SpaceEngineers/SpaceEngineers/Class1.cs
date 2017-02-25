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
            //LcdSetupForDebugging("VarPanel");
            string msg = getLcd("TestMSG").GetPublicText();
            string[] split = msg.Split('\n');
            msg = String.Join("", split);
            split = msg.Split(':');
            int goal = Int32.MaxValue;
            for(int k = 0; k < split.Length; k++)
            {
                if (split[k].Equals("Loc1"))
                {
                    goal = k + 2;
                }
                if (k == goal)
                {
                    split[k] = (Convert.ToInt32(split[k]) + Convert.ToInt32(argument)).ToString();
                }
            }
            msg = String.Join(":", split);
            MSGSend(msg);
            LcdClear("TestMSG");
            LcdPrint(msg, "TestMSG");
        }

        public void Save()
        {

        }

        public void Main(string argument)
        {
            LcdClear();
            MSGReceiver(argument);
            LcdPrintln(argument);
        }

        public void patrolList()
        {
            LcdClear();
            Dictionary<string, Vector3> coords = RemoteControlGetWaypointsGPS();
            Vector3 v = coords["New"];
            v.X++;
            coords["New"] = v;
            RemoteControlSetWaypointsGPS(coords);
            LcdPrint(EncodeGPS(coords));
        }

        public void MSGSend(string msg)
        {
            GetAntenna("DefaultAntenna").TransmitMessage(msg);
        }

        public void antennaStuff(string shipIP)
        {
            IMyRadioAntenna antenna = GridTerminalSystem.GetBlockWithName("DefaultAntenna") as IMyRadioAntenna;
            antenna.TransmitMessage("MSG:CMD;Rec:1.2.3.4.UID;Snd:" + shipIP + ";RCT'MV'RemoteControl'GPS:Loc1:x:y:z;");
        }
        #region 1
        #region 2
        #endregion
        #endregion

        public void programmableStuff()
        {
            IMyProgrammableBlock prog = GridTerminalSystem.GetBlockWithName("MSGRecevier") as IMyProgrammableBlock;
            DebugPrintBlockActions(prog);
        }

        public void quaternionTester()
        {

        }

        

        

       

        public void gyroStuff() 
        {
            //TODO: Extensive Gyro Testing: Find orientation to yaw/roll/pitch relation
            IMyGyro gyro = GetBlock<IMyGyro>("DefaultGyro", true);
            MyBlockOrientation orientation = gyro.Orientation;
            Quaternion quat = new Quaternion();
            orientation.GetQuaternion(out quat);
            Vector3 test = Vector3.Up;
            test = RotateVectorByQuaternion(test, quat);
            LcdPrintln(VectorToGPS(test, "GyroUp"));
            test = RotateVectorByQuaternion(Vector3.Forward, quat);
            LcdPrintln(VectorToGPS(test, "GyroForward"));
            GyroRollPitchYawDebugPrint(gyro);
            //GyroSetOverrideToRollPitchYaw(gyro)

            // roll  = Mathf.Atan2(2*y*w - 2*x*z, 1 - 2*y*y - 2*z*z);
            // pitch = Mathf.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z);
            // yaw = Mathf.Asin(2 * x * y + 2 * z * w);
            QuaternionToRollPitchYawDebugPrint(quat);
            QuaternionToRollPitchYawDebugPrint(YawPlus, "Yaw+");
            QuaternionToRollPitchYawDebugPrint(YawMinus, "Yaw-");
            QuaternionToRollPitchYawDebugPrint(PitchPlus, "Pitch+");
            QuaternionToRollPitchYawDebugPrint(PitchMinus, "Pitch-");
            QuaternionToRollPitchYawDebugPrint(RollPlus, "Roll+");
            QuaternionToRollPitchYawDebugPrint(RollMinus, "Roll-");

            for(float n = 0; n < 10; n+=0.1f)
            {
                quat = Quaternion.CreateFromYawPitchRoll(n, n, n);
                string ns = n.ToString();
                //QuaternionToRollPitchYawDebugPrint(quat, n+","+n+","+n);
                float cos = (float) Math.Cos((double) n);

                quat = Quaternion.CreateFromYawPitchRoll(cos, cos, cos);
                QuaternionToRollPitchYawDebugPrint(quat, n + "," + cos + "," + cos);
            }
        }

        //TODO: Clean this up and debug the value issues.

        /// <summary>
        /// Sets override to given roll pitch yaw object.
        /// </summary>
        /// <param name="gyro"></param>
        /// <param name="rpy"></param>
        public void GyroSetOverrideToRollPitchYaw(IMyGyro gyro, RollPitchYaw rpy, bool enableGyro = true)
        {
            RollPitchYaw tmpRpy = rpy;
            tmpRpy.ConvertToUnit(AngleUnit.FREQUENCY);

            if (enableGyro)
            {
                gyro.GyroOverride = true;
            }
            gyro.Roll = tmpRpy.roll;
            gyro.Pitch = tmpRpy.pitch;
            gyro.Yaw = tmpRpy.yaw;
        }

        /// <summary>
        /// Sets override to given rolla pitch and yaw values. Units are radians by defaults.
        /// </summary>
        /// <param name="gyro"></param>
        /// <param name="roll"></param>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        public void GyroSetOverrideToRollPitchYaw(IMyGyro gyro, float roll, float pitch, float yaw, bool enableGyro = true,  AngleUnit angleUnits = AngleUnit.RADIAN)
        {
            //delegate float lambdaFloatConverter(float x);
            //lambdaFloatConverter radiansToFrequency = x => x / (float) AngleUnitConvertionCoefficient(angleUnits);

            //roll = RollPitchYaw.ConverToUnit(roll, AngleUnit.FREQUENCY, AngleUnit.RADIAN);
            //pitch = RollPitchYaw.ConverToUnit(pitch, AngleUnit.FREQUENCY, AngleUnit.RADIAN);
            //yaw = RollPitchYaw.ConverToUnit(yaw, AngleUnit.FREQUENCY, AngleUnit.RADIAN);

            GyroSetOverrideToRollPitchYaw(gyro, new RollPitchYaw(roll, pitch, yaw, AngleUnit.RADIAN), enableGyro);
            return;

            if (enableGyro) {
                gyro.GyroOverride = true;
            }

            gyro.Roll = roll;   gyro.Pitch = pitch; gyro.Yaw = yaw;
        }

        public void GyroStopOverride(IMyGyro gyro)
        {
            gyro.GyroOverride = false;
        }

        public void GyroSetOverrideFromGridReferenceFrame(Quaternion targetRotation, IMyGyro specificGyro = null, bool allGyros = false)
        {
            List<IMyGyro> gyros;
            if (allGyros)
            {
                gyros = GetBlocks<IMyGyro>();
            } else if(specificGyro == null)
            {
                //TODO Error message location
                return;
            } else
            {
                gyros = new List<IMyGyro>();
                gyros.Add(specificGyro);
            }

            //TODO: Optimise quaternion operations, possibly by sorting gyros using their orientaions
            foreach(IMyGyro gyro in gyros)
            {
                Quaternion rotationOperation = GyroMovementFromLocalRotation(targetRotation, gyro);
                RollPitchYaw rpy = QuaternionToRollPitchYaw(rotationOperation);
                GyroSetOverrideToRollPitchYaw(gyro, rpy);
            }


        }

        public void GyroSetOverrideFromWorldReferenceFrame(Quaternion targetRotation, IMyGyro specificGyro = null, bool allGyros = false)
        {
            Quaternion gridSpecifictargetRotation = GetGridRotation();
            //gridRot^-1 . targetRotation = gridMovement
            Quaternion localTargetRotation = Quaternion.Concatenate(Quaternion.Conjugate(gridSpecifictargetRotation), targetRotation);

            GyroSetOverrideFromGridReferenceFrame(localTargetRotation, specificGyro, allGyros);
        }

        public List<T> GetBlocks<T>(bool all = true, string name = "", string group = "") where T : class, IMyTerminalBlock
        {
            List<T> blocks = new List<T>();
            GridTerminalSystem.GetBlocksOfType(blocks);
            return blocks;
        }

        public void rotationTester()
        {
            Quaternion rotate90 = Quaternion.CreateFromTwoVectors(Vector3.Forward, Vector3.Up);
            GyroSetOverrideFromGridReferenceFrame(rotate90);
            Quaternion target = Quaternion.Concatenate(GetGridRotation(), rotate90);
            //GyroSetOverrideFromWorldReferenceFrame(target);
        }

       // public Quaternion GetGridRotationFromWorldGrid()
       // {

       // }
        

        

        

        

        

        
        // Attempts to measure gyro torque
        public void gyroForce()
        {
            string state = getLcd("DebugState").GetPublicText();
            LcdClear("DebugState");
            //TODO: Determine parametres of gyro acceleration and top speed
            IMyGyro gyro = GetBlock<IMyGyro>("DefaultGyro", true);
            int t;
            Quaternion worldOrientation;
            if (state.Equals("") || state.Equals("reset"))
            {
                LcdClear();
                // Must activate override before assigning values!!!
                gyro.GyroOverride = true;
                gyro.Yaw = 1;
                //GyroSetOverrideToRollPitchYaw(gyro, 0, 0, 1, true, AngleUnit.FREQUENCY);
                t = 0;
                worldOrientation = GetGridRotation();
            } else if (state.Equals("stop"))
            {
                gyro.Yaw = 0;
                gyro.GyroOverride = false;
                LcdPrint("reset", "DebugState");

                IMyTimerBlock timer = GetBlock<IMyTimerBlock>("DebugTimer");
                timer.StopCountdown();

                int shipMass = getRemoteControl("", true).CalculateShipMass().PhysicalMass;
                LcdPrintln("Mass:" + shipMass);
                return;
            } else
            {
                string[] data = state.Split('\n');
                worldOrientation = QuaternionFromRawString(data[1]);
                t = Convert.ToInt32(data[0]);
            }
            MyBlockOrientation orientation = gyro.Orientation;
            Quaternion quat = GetGridRotation();
            quat = Quaternion.Multiply(worldOrientation, quat);
            //orientation.GetQuaternion(out quat);
            QuaternionToRollPitchYawDebugPrint(quat, "t:" + t); // Already prefixes "RPY" to front
            t++;
            LcdPrintln(t.ToString(), "DebugState");
            LcdPrint(QuaternionToRawString(worldOrientation), "DebugState");
        }

        
        // Quaternion operations tester
        public void rotationTester()
        {
            Vector3 v = Vector3.Forward;
            LcdPrintln(VectorToGPS(v, "Up"));
            LcdPrintln(VectorToGPS(Vector3.Forward, "Forwards"));
            LcdPrintln(VectorToGPS(Vector3.Right, "Right"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v, YawPlus),"YawPlus"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v, YawPlus, true), "YawPlusInverted"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v, YawMinus), "YawMinus"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(RotateVectorByQuaternion(v, YawPlus),YawPlus,true), "YawPlusRev"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(RotateVectorByQuaternion(v, YawPlus), YawMinus), "YawPlusMinus"));
            Vector3 v180 = RotateVectorByQuaternion(RotateVectorByQuaternion(v, YawPlus), YawPlus);
            LcdPrintln(VectorToGPS(v180, "Yaw180"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v180, YawPlus), "Yaw270"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(RotateVectorByQuaternion(v180, YawPlus), YawPlus), "Yaw360"));

            v = Vector3.Forward;
            LcdPrintln(VectorToGPS(v, "Up"));
            LcdPrintln(VectorToGPS(Vector3.Forward, "Forwards"));
            LcdPrintln(VectorToGPS(Vector3.Right, "Right"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v, PitchPlus), "PitchPlus"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v, PitchPlus, true), "PitchPlusInverted"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v, PitchMinus), "PitchMinus"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(RotateVectorByQuaternion(v, PitchPlus), PitchPlus, true), "PitchPlusRev"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(RotateVectorByQuaternion(v, PitchPlus), PitchMinus), "PitchPlusMinus"));
            v180 = RotateVectorByQuaternion(RotateVectorByQuaternion(v, PitchPlus), PitchPlus);
            LcdPrintln(VectorToGPS(v180, "Pitch180"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v180, PitchPlus), "Pitch270"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(RotateVectorByQuaternion(v180, PitchPlus), PitchPlus), "Pitch360"));

            v = Vector3.Up;
            LcdPrintln(VectorToGPS(v, "Up"));
            LcdPrintln(VectorToGPS(Vector3.Forward, "Forwards"));
            LcdPrintln(VectorToGPS(Vector3.Right, "Right"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v, RollPlus), "RollPlus"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v, RollPlus, true), "RollPlusInverted"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v, RollMinus), "RollMinus"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(RotateVectorByQuaternion(v, RollPlus), RollPlus, true), "RollPlusRev"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(RotateVectorByQuaternion(v, RollPlus), RollMinus), "RollPlusMinus"));
            v180 = RotateVectorByQuaternion(RotateVectorByQuaternion(v, RollPlus), RollPlus);
            LcdPrintln(VectorToGPS(v180, "Roll180"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(v180, RollPlus), "Roll270"));
            LcdPrintln(VectorToGPS(RotateVectorByQuaternion(RotateVectorByQuaternion(v180, RollPlus), RollPlus), "Roll360"));
        }


        // MSG:CMD;Rec:1.2.3.4.UID;RCT'MV'RemoteName'GPS:x:y:z;
    }
}
//TODO: Default get methods get first RC/Antenna if they can't find the default name...
        //to this comment.
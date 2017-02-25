// Works with standard MSG library
public void antennaStuff()
{
  // Sends sample antenna message
  IMyRadioAntenna antenna = GridTerminalSystem.GetBlockWithName("DefaultAntenna") as IMyRadioAntenna;
  antenna.TransmitMessage("MSG:CMD;Rec:1.2.3.4.UID;RCT'MV'RemoteControl'GPS:Loc1:x:y:z;");
}

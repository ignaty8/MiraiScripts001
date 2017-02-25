public void LcdPrintln(string msg) {
  LcdPrint(msg + '\n');
}

public void LcdPrint(string msg) {
  LcdPrint(nsg, "VarPanel");
}

public void LcdPrintln(string msg, string lcdName) {
  LcdPrint(msg + '\n', lcdName);
}

public void LcdPrint(string msg, string lcdName) {
  IMyTextPanel lcd =
    GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
  lcd.WritePublicText(lcd.GetPublicText() + msg);
}

//IMyTextPanel tact = GridTerminalSystem.GetBlockWithName(TACTICAL_INFO_NAME) as IMyTextPanel;

public Vector3 GPStoVector(string gps){

  string[] coords;

  //Getting Target Position//

  coords = gps.Split(':');

  //Pending, need to change indices

  Vector3 gpsvector = new Vector3(Convert.ToSingle(coords[2]),
  Convert.ToSingle(coords[3]),
  Convert.ToSingle(coords[4]));

  return gpsvector;
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

//TODO: Add custom location name support

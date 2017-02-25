public void GoTo(IMyRemoteControl cntrl, Vector3 Pos, bool Relative) {
  //orders ship to move to Vector location
  Vector3 obj;

  if (Relative == true) {
    obj = Pos + cntrl.GetPosition();
  } else {
    obj = Pos;
  }

  cntrl.ClearWaypoints();
  cntrl.AddWaypoint(obj,"obj");
  cntrl.SetAutoPilotEnabled(true);
}

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

public string VectorToGPS(Vector3 vec){
  string output;

  output = "GPS:location:"
  + Convert.ToString(vec.X) +    ":"
  + Convert.ToString(vec.Y) + ":"
  + Convert.ToString(vec.Z) + ":";

  return output;
}

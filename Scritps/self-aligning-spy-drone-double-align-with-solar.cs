const float EPSILON = 0.1f;
Vector3 zeroVector = new Vector3 (0, 0, 0);

const string REMOTE_NAME = "RCMain";
const string OUTPUT_PANEL_NAME = "OutTerminal";
const string DOWN_BLOCK_NAME = 	"DownIndicator";

// Remembers wether the ship is travelling somewhere or not.
bool isCruising = false;

// Tells if ship is aligned to planet gravity field.
bool isAligned = false;

// Curent position of ship.
float posX, posY, posZ;

// Previous position of ship.
float lastX = 0, lastY = 0, lastZ = 0;

// Counts number of waypoints. :)
int waypoints = 0;

// Used to trigger second step of realignment process.
bool isAligning = false;

Vector3 velocity;
Vector3 downVec = new Vector3(0, 0, 0);
Vector3 gravityVec = new Vector3(0, 0, 0);

void Main(string argument){

	LoadVariables();

	


	if(!isCruising){
		isAligned = IsAligned();

		if(!isAligned && !isAligning){
			ReverseAlign();
		} else if (isAligning) {
			AlignToGravity();
		} else {
			RemoveWaypoints();
		}
	}

	//isAligned = IsAligned();

	UpdateVelocity();

	OrientSolarPanels();

	StoreVariables();
}

void UpdateVelocity(){
	IMyRemoteControl rc = GridTerminalSystem.GetBlockWithName(REMOTE_NAME) as IMyRemoteControl;
	Vector3 pos = rc.GetPosition();
	posX = pos.X;
	posY = pos.Y;
	posZ = pos.Z;
	velocity = new Vector3(posX - lastX, posY - lastY, posZ - lastZ);

	isCruising = !Vector3Equal(velocity, zeroVector);
}

bool IsAligned(){

	IMyRemoteControl rc = GridTerminalSystem.GetBlockWithName(REMOTE_NAME) as IMyRemoteControl;
	IMyThrust down = GridTerminalSystem.GetBlockWithName(DOWN_BLOCK_NAME) as IMyThrust;

	Vector3 downDir = down.GetPosition() - rc.GetPosition();
	downDir = Vector3.Normalize(downDir);
	downVec = downDir;

	Vector3 gravity = rc.GetNaturalGravity();
	gravity = Vector3.Normalize(gravity);
	gravityVec = gravity;

	return Vector3Equal(downDir, gravity);
}

void AlignToGravity(){

	IMyRemoteControl rc = GridTerminalSystem.GetBlockWithName(REMOTE_NAME) as IMyRemoteControl;

	Vector3 gravity = rc.GetNaturalGravity();

	if(waypoints > 1){
		RemoveWaypoints();	
	}

	TravelInDirection(gravity, rc);

	isAligned = true;
	isCruising = true;
	isAligning = false;
}

void ReverseAlign() {
	IMyRemoteControl rc = GridTerminalSystem.GetBlockWithName(REMOTE_NAME) as IMyRemoteControl;

	Vector3 gravity = rc.GetNaturalGravity();

	TravelInDirection(-gravity, rc);

	isAligned = true;
	isCruising = true;
	isAligning = true;
}

void TravelInDirection(Vector3 dir, IMyRemoteControl rc, float distance = 40f){

	// This turns the vector into a coordinate that the ship should travel to.
	dir = Vector3.Normalize(dir);
	dir *= distance;

	dir += rc.GetPosition();
	if (waypoints == 0){
		rc.AddWaypoint (dir, "Direction");
		waypoints++;
	}
	rc.SetAutoPilotEnabled (true);
	isCruising = true;
}

void RemoveWaypoints(){
	IMyRemoteControl rc = GridTerminalSystem.GetBlockWithName(REMOTE_NAME) as IMyRemoteControl;

	rc.ClearWaypoints();
	rc.SetAutoPilotEnabled(false);
	waypoints = 0;
}

void StoreVariables(){
	ClearPanel();
	PrintToPanel(isCruising.ToString());
	PrintToPanel(isAligned.ToString());
	PrintToPanel(posX.ToString());
	PrintToPanel(posY.ToString());
	PrintToPanel(posZ.ToString());
	PrintToPanel(waypoints.ToString());
	PrintToPanel(isAligning.ToString());

	PrintToPanel(lastPowerValue.ToString());
	PrintToPanel(yawLeft.ToString());

	PrintToPanel(velocity.X.ToString());
	PrintToPanel(velocity.Y.ToString());
	PrintToPanel(velocity.Z.ToString());
	PrintToPanel(MathHelper.Distance(velocity.X, zeroVector.X).ToString());
	PrintToPanel(MathHelper.Distance(velocity.Y, zeroVector.Y).ToString());
	PrintToPanel(MathHelper.Distance(velocity.Z, zeroVector.Z).ToString());
	PrintToPanel(EPSILON.ToString());
	PrintToPanel(Vector3Equal(velocity, zeroVector).ToString());
	PrintToPanel(IsAligned().ToString());
	PrintToPanel(downVec.X.ToString());
	PrintToPanel(downVec.Y.ToString());
	PrintToPanel(downVec.Z.ToString());
	PrintToPanel(gravityVec.X.ToString());
	PrintToPanel(gravityVec.Y.ToString());
	PrintToPanel(gravityVec.Z.ToString());
}

void LoadVariables(){
	// Loads text stored on panel and splits into array.
	IMyTextPanel panel = GridTerminalSystem.GetBlockWithName(OUTPUT_PANEL_NAME) as IMyTextPanel;
	string panelText = panel.GetPublicText();

	if(panelText == ""){
		return;
	}

	string[] lines = panelText.Split('\n');

	// Each line get assigned to variable.
	isCruising = Boolean.Parse(lines[0]);
	isAligned  = Boolean.Parse(lines[1]);
	lastX = float.Parse(lines[2]);
	lastY = float.Parse(lines[3]);
	lastZ = float.Parse(lines[4]);
	waypoints = int.Parse(lines[5]);
	isAligning = Boolean.Parse(lines[6]);

	lastPowerValue = float.Parse(lines[7]);
	yawLeft = Boolean.Parse(lines[8]);
}

// Panel Code
void PrintToPanel(string text){

	IMyTextPanel p = GridTerminalSystem.GetBlockWithName(OUTPUT_PANEL_NAME) as IMyTextPanel;


	// Appends text to panel
	p.WritePublicText(text, true);
	p.WritePublicText("\n", true);
}

void ClearPanel(){

	IMyTextPanel p = GridTerminalSystem.GetBlockWithName(OUTPUT_PANEL_NAME) as IMyTextPanel;

	p.WritePublicText("");
}

void ClearPanelTitle(){

	IMyTextPanel p = GridTerminalSystem.GetBlockWithName(OUTPUT_PANEL_NAME) as IMyTextPanel;

	p.WritePublicTitle("");
}




// Utility Code:

bool Vector3Equal(Vector3 v1, Vector3 v2){
	return ((MathHelper.Distance(v1.X, v2.X) < EPSILON) 
		&& (MathHelper.Distance(v1.Y, v2.Y) < EPSILON) 
		&& (MathHelper.Distance(v1.Z, v2.Z) < EPSILON));
}


// Solar Alignment Code

const string SOLAR_PANEL_1_NAME = "ReferencePanlel1";
const string GYRO_NAME = "OrientGyro";

float lastPowerValue = 0;
bool yawLeft = true;

int yawSpeed = 1;

void OrientSolarPanels(){
	float power = GetCurrentPower();

	if (power < lastPowerValue) {
		ReverseYaw();
	}
}

float GetCurrentPower(){
	IMySolarPanel panel = GridTerminalSystem.GetBlockWithName(SOLAR_PANEL_1_NAME) as IMySolarPanel;

	return panel.CurrentOutput;
}

void ReverseYaw () {
	yawLeft = !yawLeft;
	Yaw (yawLeft, yawSpeed * 2);
}

void Yaw(bool left, int speed){
	IMyGyro gyro = GridTerminalSystem.GetBlockWithName(GYRO_NAME) as IMyGyro;


	//gyro.Pitch = 0;
	//gyro.Roll = 0;

	//gyro.Yaw = 0;
	if (gyro.Yaw == 0){
		Yaw(left, speed / 2);
	}

	for (int k = 0; k < speed; k++) {
		//if(gyro.Yaw == 0){
			if(left){
				gyro.IncreaseYaw();
			} else {
				gyro.DecreaseYaw();
			}
		/*} else if ((gyro.Yaw > 0 && left) || (gyro.Yaw < 0 && !left)) {
			gyro.Yaw = 0;
		}*/
	}

	gyro.Override = true;
}
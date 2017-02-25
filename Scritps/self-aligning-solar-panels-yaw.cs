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
	Yaw (yawLeft, yawSpeed);
}

void Yaw(bool left, int speed){
	IMyGyro gyro = GridTerminalSystem.GetBlockWithName(GYRO_NAME) as IMyGyro;


	gyro.Pitch = 0;
	gyro.Roll = 0;

	gyro.Yaw = 0;

	for (int k = 0; k < speed; k++) {
		//if(gyro.Yaw == 0){
			if(left){
				gyro.IncreaseYawOverride();
			} else {
				gyro.DecreaseYawOverride();
			}
		/*} else if ((gyro.Yaw > 0 && left) || (gyro.Yaw < 0 && !left)) {
			gyro.Yaw = 0;
		}*/
	}

	gyro.Override = true;
}
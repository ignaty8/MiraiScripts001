IMyTextPanel tact = GridTerminalSystem.GetBlockWithName(TACTICAL_INFO_NAME) as IMyTextPanel;

//ALL SPACE ENGINEERS METHODS//

//MULTI-PURPOSE//

public string GetRaw(IMyTextPanel txt){

	//Gets RAW output from text panel (the text)

	string output;

	output=txt.GetPublicText();

	return output;
}





public string[] GetRef(IMyTextPanel txt,char sep){
	//Divides RAW into an array for later use

	string raw;

	raw=GetRaw(txt);

	string[] output =raw.Split(sep);

	return output;

}

public void ExecuteSimple(string TIMER_NAME){
	//Starts a Timer Block

	IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;


	if(timer != null){


		ITerminalAction start;

		start=timer.GetActionWithName("Start");

		start.Apply(timer);
	}
}




public void Execute(string TIMER_NAME, string ACTION_NAME){
	//Executes Timer Action
	IMyTimerBlock timer;

	IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;

	if(timer != null){

		ITerminalAction start;

		start=timer.GetActionWithName(ACTION_NAME);

		start.apply(timer);
	}
}





public IMyTextPanel[] MassGetTxt(string[] TXT_NAMES){

	IMyTextPanel[] output;

	IMyTextPanel txttemp

	for(int it = 0 ; it < TXT_NAMES.Length ; it++){

		IMyTextPanel txttemp = GridTerminalSystem.GetBlockWithName(TXT_NAMES[it]) as IMyTextPanel;

		output[it] = txttemp;
	}

	return output;
}



public string[] GetDemands(string txt2_ref){
	//txt2_ref Gets the Cost Which contains the cost in EVERY component (including 0's)
	//this method takes those then saves all the non-zeroes and keeps their Component ID

	string[] output;

	int it0 = 0;

	for(int it = 0 ; it < txt2_ref.Length-1 ; it++){
		//here it is the Component ID by design

		//Checking if we actually have a cost
		if(txt2_ref[it] != "0"){

			//Saving
			output[it0] = Convert.ToString(it) + ";" + txt2_ref[it];
		}

	}

	return output;

}



public string Vector3ToString(Vector3 vector){

	string[] output_array;

	string output;

	output_array[0] = Convert.ToString(vector.X);

	output_array[1] = Convert.ToString(vector.Y);

	output_array[2] = Convert.ToString(vector.Z);

	output = output_array[0] + " " + output_array[1] + " " + output_array[2];
}



public Vector3 GPStoVector(string gps){

	string[] coords;

	//Getting Target Position//

	coords = gps.Split(':');

	//Pending, need to change indices

	Vector3 gpsvector = new Vector3(Convert.ToSingle(coords[1]),
	Convert.ToSingle(coords[2]),
	Convert.ToSingle(coords[3]));

	return gpsvector;
}

public string VectorToGPS(Vector3 vec){
	string output;

	output = "GPS:location:"
	+ Convert.ToString(vec.X) +	":"
	+ Convert.ToString(vec.Y) + ":"
	+ Convert.ToString(vec.Z) + ":";

	return output;
}



public IMyTextPanel GetNewCommand(string TXT0_NAME, string TXT1_NAME){
	//To a Drone Associates a new Commander
	//TXT0 is Info & TXT1 is orders

	//Initialising Variables//

	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel;

	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;



	//Extracting info//

	string[] txt0_ref;

	txt0_ref = GetRef(txt0,'$');




	//Extracting Orders//

	string[] txt1_ref;

	txt1_ref = GetRef(txt1,';');



	//Getting updated info//

	string info;

	info = txt0_ref[0] + "$" + txt0_ref[1] + "$" + txt1_ref[3] + "$" +txt1_ref[4];



	//Saving Updated info//

	txt0.WritePublicText(info);

	//Returning txt1//
	//Because why not ?
	//Will probably need it later
	return txt1;
}


public string[] RemoveZeroeCost(string[] input){
	string[] output;

	output = new string[input.Length];

	//Takes the input array
	//-removes ALL elements with 0 cost
	//-saves them along with their original position in this format : pos,cost
	int it0 = 0;

	for(int it = 0; it <= input.Length-1; it++){
		if(input[it] != "0"){
			output[it0] = Convert.ToString(it) + "," + input[it];

			it0++;
		}
	}

	return output;
}

public float[] MassStringToFloat(string[] input){
	//Self-Explanatory//

	//okay lists would probably have been better but we are already using arrays EVERYWHERE

	float[] output;

	output = new float[input.Length];

	for(int it = 0; it <= input.Length-1; it++){
		output[it] = float.Parse(input[it]);
	}

	return output;
}


public float[] MassMultiply(float[] factors,float factor){
	//Multiplies each elemnt in an array by a single factor

	//okay lists would probably have been better but we are already using arrays EVERYWHERE
	float[] output;

	output = new float[factors.Length];

	for(int it = 0; it <= factors.Length-1; it++){
		output[it] = factors[it] * factor;
	}

	return output;
}


public string[] MassFloatToString(float[] input){

	string[] output;

	output = new string[input.Length];

	for(int it = 0; it <= input.Length-1; it++){
		output[it] = Convert.ToString(input[it]);
	}

	return output;
}



public var AccessInventory(string CARGO_NAME,int inventory_type){
	//inventory type should be 0 for ALL blocks EXCEPT assemblers & Refineries
	//they have 2 inventories, input (0) & output(1)
	//returns the inventory variable
	IMyTerminalBlock cargo_container = GridTerminalSystem.GetBlockWithName(CARGO_NAME) as IMyTerminalBlock;

	var cargo = cargo_container as IMyInventoryOwner;

	var inventory = cargo.GetInventory(inventory_type);

	return inventory;

}

public bool IsEmpty(string CARGO_NAME,int inventory_type){
	//Checks if inventory is empty

	bool output;

	IMyInventory inventory = AccessInventory(CARGO_NAME,inventory_type)

	if((double)inventory.CurrentVolume != 0){
		output = true;
	}else{
		output = false;
	}

	return output;
}

public bool AreEmpty(string[] CARGO_NAMES,int inventory_type){
	//Checks if all Cargo containers are empty
	bool output = true;

	for(CARGO_NAME in CARGO_NAMES){
		output = IsEmpty(CARGO_NAME,inventory_type);

		if(output == false){
			break;
		}
	}

	return output;
}

public Vector3 GetBlockPosition(BLOCK_NAME){
	//Returns Block Position//

	Vector3 output;

	IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(BLOCK_NAME) as IMyTerminalBlock;

	output = block.GetPosition();

	return output;
}

public void AddToWorkQueue(IMyTextPanel txt1, IMyTextPanel txt2){
	//txt1 is orders
	//txt2 is the queue
	//saves orders and adds it to queue

	txt2.WritePublicText(GetRaw(txt2) + "#" + GetRaw(txt1));

}

public string[] GetMemory(string MEM_TYPE){

	//Initialising//

	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(MEM_TYPE + "_memory") as IMyTextPanel;



	//Extracting memory//

	string[] txt0_ref;

	//CHECK THIS LINE !//
	txt0_ref = GetRef(txt0,';');


	//Returning Value//

	return txt0_ref;
}

public string[] ExctractFirstSimpleMemory(string MEM_TYPE){
	//Exctracts first element in memory of format info 00,info 01;etc

	string[] synapses;
	//array of all the memories

	string[] output;

	synapses = GetMemory(MEM_TYPE);

	output = synapses[0].Split(',');

	return output;
}

public string[] ExctractRelevantMemory(string MEM_TYPE, string RELEVANT_ID){
	//Extracts memory referring to given ID

	string[] synapses;
	//array of all the memories

	string[] output = {""};

	synapses = GetMemory(MEM_TYPE);

	for(int it = 0; it < synapses.Length; it++){
		//checking if relevant
		if(synapses[it].Split(',')[0] == RELEVANT_ID){

			output = synapses[it].Split(',');

		}
	}

	return output;
}

//For Remote Controls, makes you go to Pos
public void GoTo(IMyRemoteControl cntrl, Vector3 Pos, bool Relative){
	//orders ship to move to Vector location
	Vector3 obj;

	if (Relative == true){
		obj = Pos + cntrl.GetPosition();
	}else{
		obj=Pos;
	}

	cntrl.ClearWaypoints();
	cntrl.AddWaypoint(obj,"obj");
	cntrl.SetAutoPilotEnabled(true);
}

public Vector3 GetUnitVector(Vector3 v0, Vector3 v1){
	//gets unit vector in direction of v0-v1
	Vector3 output;

	output = v0 - v1;

	output.Normalize();

	return output;
}

public Vector3 GetUnitNormalVector(Vector3 v0,Vector3 v1){
	//gets unit vector in the normal direction to v0 v1

	Vector3 output;

	output = CrossProduct(v0,v1);

	output.Normalize();

	return output;
}

//picked up from the Internet, because its not built in
//by James Ramsden
Vector3 CrossProduct(Vector3 v1, Vector3 v2)
{
	double x, y, z;
	x = v1.Y * v2.Z - v2.Y * v1.Z;
	y = (v1.X * v2.Z - v2.X * v1.Z) * -1;
	z = v1.X * v2.Y - v2.X * v1.Y;

	var rtnvector = new Vector3(x, y, z);
	return rtnvector;
}










































//SINGLE-USE//
//SINGLE-USE//
//SINGLE-USE//
//SINGLE-USE//
//SINGLE-USE//
//When Put here

public void SendOrder(string[] fullorders/*, IMyBeacon bacon*/){

	IMyProgrammableBlock controller = GridTerminalSystem.GetBlockWithName(
	COMMS_CONTROLLER_NAME) as IMyProgrammableBlock;

	//full order[3] contains the orderID we are sending, then password etc.
	string message = fullorders[4] + ORD_SEP_IND + PASSWORD;

	//this is for special info
	for(int it = 5 ; it <= fullorders.Length-2;it++){
		message = message + ORD_SEP_IND + fullorders[it];
	}


	//prepping message in appropriate format
	//fullorder[2] contains the targetID


	string order = SEND_MESSAGE_IND + ARG_SEP_IND + "C" + MES_SEP_IND +
	fullorders[3] + MES_SEP_IND +
	message + MES_SEP_IND +
	fullorders[fullorders.Length-1];

	//bacon.SetCustomName(order);


	controller.TryRun(order);
}

//used to extract tactical formations in Action Manager
public Vector3 GetTactical(string tacticsID, string[] tactical_intel){
	Vector3 output;
	int relevant = 0;
	//checks for relevant formation
	for(int it = 1; it <= (tactical_intel.Length-2);it += 2){
		if(tacticsID == tactical_intel[it]){
			relevant = it;
			break;
		}
	}

	public bool CompareIDs(string ID0,string ID1,char sep){
		//Checks if ID0 contains or is equal to ID1
		//if yes, return true, else returns false

		bool output;

		output = true;

		string[] ID0_full = ID0.Split(sep);
		string[] ID1_full = ID1.Split(sep);

		for(int it = 0; it <= ID0_full.Length; it++){
			if(ID0_full[it] != ID1_full[it]){
				output = false;
				break;
			}
		}

		return output;
	}


	//For Receiving Messages

	string[] GetAntennaName(IMyTextPanel txt0){
		//This Method extracts the first antenna, whose name is stored in txt0 (the info txt panel)

		string[] ref0;

		string[] ref00;

		ref0 = GetRef(txt0,'$');

		ref00 = ref0[1].Split('%');

		return ref00;

	}

	string GetIntel(IMyLaserAntenna lsr){

		//This Method will get the intel from the laser antenna lsr is connected to.
		string raw_output;
		string[] part_output;

		raw_output = lsr.DetailedInfo;

		part_output = raw_output.Split(' ');

		return part_output[7];
	}

	//For ASSEMBLER SCRIPT ONLY
	public string[] MassDemandsToFullMessages( string[] demands){
		string memory_temp;

		string[] demand_temp;

		IMyTextPanel txttemp;

		string[] txttemp_ref;

		string[] output;

		for(it = 0 ; it < demands.Length ; it++){

			demand_temp = demands[it].Split(';');

			txttemp = GridTerminalSystem.GetBlockWithName("assembler_" + demand_temp[0] +"_memory");

			txttemp_ref = GetRef(txttemp);

			//So This program doesn't have the change preferred assembler feature
			//I will add this in another program that changes the order of assemblers in assembler memory

			output[it] = txttemp[1] + "&" + txttemp[0] + ";" + "GO" + demands[it];
		}

		return output
	}


	//Taken from the Initialise Explorer Script
	public string GetGridWaypoints(Vector3 obj){
		//generates the waypoints for the explorer
		//Scouts most of a cube

		const float sensor_range = 50;
		const float asteroid_radius = 10;
		//in order to reduce the amount of mostly empty space we need to scout we add asteroid radius to sensor range
		//the idea being that an asteroid being PERFECTLY between two scouted "chunks" is extremly unlikely

		//Assuming conventional Antenna range
		const float laser_range = 10000




		string output;

		// This actually gets removed by the Next Waypoint Module
		output = VectorToGPS(obj);


		for(int it0 = 0; it0 < n; it0 ++){
			for(int it1 = 0; it1 < n; it1 ++){

				//If Odd//
				if(it1/2 != Math.Floor(it1/2)){

					obj = new Vector3(obj.X , obj.Y + d * 2, obj.Z);

				}else{

					obj = new Vector3(obj.X , obj.Y - d * 2, obj.Z);

				}


				obj = new Vector3(obj.X + (sensor_range + asteroid_radius) * 2 * it0, obj.Y, obj.Z + (sensor_range + asteroid_radius) * 2 * it1);

				output = output + ";" + VectorToGPS(obj);
			}

		}

		return output;
	}


	//Used In assembler to have the ask for Ressources order in GetOfferAssembler
	public string GOShipyardToAssembler(string[] txt0_ref, string[] txt1_ref, string[] memory,int stage){

		//Stage is for if we have already contacted the refinery once we go to the next one
		//txt0 = info & txt1 = orders
		//Basically We need to Turn the old orders from the shipyard into orders for the Refinery
		//Shipyard orders :Shipyard ID; Assembler ID ;GO; Component ID; Amount
		//Refinery orders :Assembler ID; Refinery ID ;GO; Ressource ID 0, Ressource ID 1,...; Amount of ID 0, Amount of ID 1,...

		//Note that info for assemblers CONTAINS the intel we happen to need as info is of the form :
		//Ship ID $ Name of Laser Antenna 1 % Name of Laser Antenna 2 % ... $ Commander ID $ Commander GPS$Product ID$Costs per unit
		//costs per unit in format : Amount of ID 0$ Amount of ID 1$...

		string output;

		float[] costs;

		string[] costs_string;

		string[] relevant_memory;

		//We need to determin costs

		//We need to get costs per unit from info then turn it into a float
		costs = MassStringToFloat(txt0_ref[5].Split('%'));


		//txt1_ref[3] is the amount asked
		costs = MassMultiply(costs,float.Parse(txt1_ref[3]));

		// NOTE : orders array is NOT in the correct order (there is stuff in between)
		costs_string = MassFloatToString(costs);

		costs_string = RemoveZeroeCost(costs_string);

		relevant_memory = memory[stage].Split(',');

		//First we need our ID then the refinery ID THEN the order type and finally the costs
		output = relevant_memory[1]+"&"+ txt1_ref[1] + ";" + relevant_memory[0] + "GO" + ";" +  string.Join(";",costs_string);

		return output;
	}

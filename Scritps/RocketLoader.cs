/*
Whiplash's Weapon Sequencing Script v16 - Revision: 8/13/16
////PUBLIC RELEASE////
HOWDY!
______________________________________________________________________________________
Instructions:

    1.) Create a timer. Assign the actions to the following:
        * "trigger now" itself
        * Run this program [NO ARGUMENTS YET!]
    2.) Add the phrase "[Sequenced]" into the name of weapons u want to sequence (without quotes)
    3.) Start the timer
______________________________________________________________________________________
Arguments:

    Type in these arguments without quotes. These arguments can be input manually,
    through timers, or through sensors. Letter case is unimportant. Seperate
    multiple arguments with a semicolon (see examples further down)

    "rate [integer]"
        changes the rate of fire in rounds per second.
        > [Maximum ROF] = [Standard ROF] * [Number of sequenced weapons]
            NOTE: The script will round the ROF, this is not a bug!
    "delay [integer]"
        changes delay between shots to be in terms of frames (60 frames = 1 sec)
    "default"
        Lets the script to set the fire rate automatically based on the number of
        available weapons. The script will attempt to fire ALL sequenced weapons in the
        span of ONE second with this particular setting. The script will start in this
        mode by default (hence the name :P)
    "on"
        Toggles fire on only
    "off"
        Toggles fire off only
    "toggle"
        Toggles fire on/off
______________________________________________________________________________________
Examples:

    "on;default" will toggle the weapons on and use default rate of fire
    "rate 10" will set the rate of fire to 10 rounds per second
    "delay 3" will set the delay between weapons to 3 frames
______________________________________________________________________________________

    If you have any questions feel free to post them on the workshop page!
    Workshop link: http://steamcommunity.com/sharedfiles/filedetails/?id=510805229

    - Whiplash141 - http://steamcommunity.com/id/Whiplash141/
        Please do not send me random friend requests! Leave comments
        on my profile if you wish to contact me directly :)
*/

//-------------------------------------------------
//This is the ID string for the weapons that you want to fire
//You can place it anywhere in the weapon's name
string unique_identification_string = "[Port]";
//-------------------------------------------------

IMyUserControllableGun weaponToFire;
int weapon_count = 0;
int time_count = 0;
int delay;
int value_integer;
double delay_unrounded;
bool shouldFire = false;
bool executeToggle = false;  //if the script should toggle on/off
bool manualOverride = false;  //if player has overriden default values
bool isInteger = true; //for checking if input is an integer
bool isShooting = false;
string messageToggle;
string messageOverride;
string value;


bool isGun(IMyTerminalBlock block)
{
    var gun = block as IMyUserControllableGun;
    if (block is IMyLargeTurretBase)
    {
        gun = null;
    }
    return gun != null;
}

void Main(string argument)
{
    List<IMyTerminalBlock> UIS_list = new List<IMyTerminalBlock>();
    List<IMyTerminalBlock> sequence_weapons = new List<IMyTerminalBlock>();

    GridTerminalSystem.SearchBlocksOfName(unique_identification_string, sequence_weapons, isGun);

    if (sequence_weapons.Count == 0)
    {
        Echo("No weapons found to sequence. Weapon names must contain '[Sequenced]' in their name to be sequenced.");
        return;
    }


    //It's splittin' time!
    string[] argument_split = argument.Split(';');  //split at semi colons

    for (int i = 0; i < argument_split.Length; i++)
    {
        string[] argument_fields = argument_split[i].Split(' '); //splits commands in two fields

        if (argument_fields.Length == 2) //2 fields
        {
            value = argument_fields[1];
        }
        else
        {
            value = "null";
        }

        switch (argument_fields[0].ToLower())
        {
            case "rate": //change rate of fire manually
                isInteger = int.TryParse(value, out value_integer);
                if (isInteger == false) return;
                delay_unrounded = 60 / (double)value_integer; //Dont change this from 60
                delay = (int)Math.Ceiling(delay_unrounded);
                manualOverride = true;
                break;

            case "delay": //change delay (in frames )between shots; 60 frames = 1 sec     
                isInteger = int.TryParse(value, out value_integer);
                if (isInteger == false) return;
                delay = value_integer;
                manualOverride = true;
                break;

            case "default": //lets the script set fire rate
                delay_unrounded = 60 / (double)sequence_weapons.Count; //set delay between weapons
                delay = (int)Math.Ceiling(delay_unrounded);
                manualOverride = false;
                break;

            case "on": //toggle fire on
                executeToggle = true;
                break;

            case "off": //toggle fire off
                executeToggle = false;
                break;

            case "toggle": //toggle fire on or off
                if (executeToggle == false) //if false switch true
                {
                    executeToggle = true;
                }
                else
                { //if true switch false
                    executeToggle = false;
                }
                break;

            default:
                if (manualOverride == false)
                {
                    delay_unrounded = 60 / sequence_weapons.Count; //set delay between weapons
                    delay = Convert.ToInt32(Math.Ceiling(delay_unrounded));
                }
                break;
        }
        if (delay == 0)
            delay = 1; //stops divide by zero
    }

    for (int k = 0; k < sequence_weapons.Count; k++)
    {
        var thisWeapon = sequence_weapons[k] as IMyUserControllableGun;
        if (thisWeapon != null)
        {
            isShooting = thisWeapon.IsShooting;
            if (isShooting) break;
        }
        else
        {
            isShooting = false;
        }
    }

    //This will only run if delay has elapsed
    if (time_count >= delay)
    {
        time_count = 0; //start count over

        //Turns all weapons off
        for (int i = 0; i < sequence_weapons.Count; i++)
        {
            var weaponReset = sequence_weapons[i];
            if (executeToggle == true)
            {
                weaponReset.ApplyAction("OnOff_On"); //Turns all weapons on for toggle fire
            }
            else
            {
                weaponReset.ApplyAction("OnOff_Off");
            }
        }

        //Activates specified weapon and iterates count var
        if (weapon_count < sequence_weapons.Count)
        {
            var weaponToFire = sequence_weapons[weapon_count] as IMyUserControllableGun;
            weaponToFire.ApplyAction("OnOff_On");
            if (isShooting) weapon_count++; //counts once per delay

            if (executeToggle == true)
            {
                weaponToFire.ApplyAction("ShootOnce");
                messageToggle = "-Toggle Fire Enabled-";
            }
            else
            {
                messageToggle = "-Toggle Fire Disabled-";
            }

        }
        else
        {
            weapon_count = 0;
            var weaponToFire = sequence_weapons[weapon_count] as IMyUserControllableGun;
            weaponToFire.ApplyAction("OnOff_On");
            if (isShooting) weapon_count++; //counts once per delay

            if (executeToggle == true)
            {
                weaponToFire.ApplyAction("ShootOnce");
                messageToggle = "-Toggle Fire Enabled-";
            }
            else
            {
                messageToggle = "-Toggle Fire Disabled-";
            }
        }

    }
    else
    {
        time_count++;//continues to count until delay is hit
    }

    if (manualOverride == true)
    {
        messageOverride = "-Defaults Overriden-";
    }
    else
    {
        messageOverride = "-Defaults Applied-";
    }

    if (isInteger == false)
    {
        Echo("Error: value must be an integer!\n>Value ignored");
    }

    //Debug
    Echo(messageToggle + "\n" + messageOverride + "\nNo. Weapons:" + sequence_weapons.Count + "\nRate of Fire: " + 60 / delay + " RPS" + "\nDelay: " + delay + " frames" + "\nCurrent Time: " + time_count + "\nWeapon Count: " + weapon_count);
}

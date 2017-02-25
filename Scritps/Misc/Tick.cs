//Requires Standard LCD Lib
public Program()
{
    LcdSetupForDebugging("VarPanel");
}

public void Save()
{

}
// Example Tick code
public void Main(string argument)
{
    string n = LcdClear();
    int k = 1 + Convert.ToInt32(n);
    LcdPrint(k.ToString());
}
(<td> [0-9\.]* </td>\r\n){3}

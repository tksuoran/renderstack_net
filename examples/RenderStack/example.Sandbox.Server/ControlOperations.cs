
public class ControlFlags
{
    public const int Pressed    = 1;
    public const int Released   = 2;
    public const int InhibitOn  = 4;
    public const int InhibitOff = 8;

}

public class Control
{
    public const int NOP            =  0;
    public const int TranslateX     =  1;
    public const int TranslateY     =  2;
    public const int TranslateZ     =  3;
    public const int RotateX        =  4;
    public const int RotateY        =  5;
    public const int RotateZ        =  6;
    public const int LeftClick      =  7;
    public const int RightClick     =  8;
    public const int Jump           =  9;
    public const int SetFly         = 10;
    public const int SetMouseLook   = 11;
}

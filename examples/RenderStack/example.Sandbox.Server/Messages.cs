using RenderStack.Math;

public class ConnectRequest
{
    public System.UInt64    Time;
    public System.UInt32    Version;
    public string           Username;
}


public class ConnectAccept
{
    public System.UInt64    Time;
    public System.UInt32    Version;
    public string           Message;    //  welcome
}

public class Disconnected
{
    public string           Message;    //  quit / incompatible version / full / not whitelisted / banned
}

public class Chat
{
    public string           Message;
}

public class NewEntity
{
    public System.UInt64    Time;
    public System.UInt32    EntityType;
    public Vector3          Position;
    public Quaternion       Orientation;
};

public class ControlEvent
{
    public System.UInt64    Time;
    public int              Control;
    public int              Flags;
};

public class EntityUpdate
{
    public System.UInt64   Time;
    public System.UInt32   EntityID;
    public Vector3         Position;
    public Vector3         LinearVelocity;
    public Quaternion      AngularVelocity;
}
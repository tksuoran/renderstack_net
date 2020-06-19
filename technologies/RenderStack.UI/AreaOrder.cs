namespace RenderStack.UI
{
    /*  Comment: Mostly stable, somewhat experimental  */ 
    public enum AreaOrder
    {
        SelfFirst,      //  Draw self before children
        PostSelf,       //  Draw self last, after children
        Separate        //  Separate DrawSelf() call
    }
}
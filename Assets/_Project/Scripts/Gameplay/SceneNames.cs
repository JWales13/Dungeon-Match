namespace Game.Gameplay
{
    /// <summary>
    /// Scene names in one place so LoadScene calls can't drift from the actual
    /// scene assets. Each name must match a scene file that is added to
    /// File -> Build Settings.
    /// </summary>
    public static class SceneNames
    {
        public const string GreenRoom = "GreenRoom";
        public const string Floor = "Floor";
    }
}
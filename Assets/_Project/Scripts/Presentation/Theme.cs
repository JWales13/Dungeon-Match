namespace Game.Presentation
{
    /// <summary>
    /// Global access point for the active theme. Views read Theme.Current for
    /// every visual value. Swapping the whole look at runtime (or in a test)
    /// is a single assignment: Theme.Current = new SomeOtherTheme();
    /// </summary>
    public static class Theme
    {
        public static ITheme Current { get; set; } = new DefaultTheme();
    }
}
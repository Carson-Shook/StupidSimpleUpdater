namespace StupidSimpleUpdater.Updater
{
    /// <summary>
    /// Values describing how often the updater should check for updates.
    /// <para>Never: should only be used by end users who do not want to update your app for some reason.</para>
    /// <para>EveryStart: every time that the application is opened.</para>
    /// <para>Daily: only once per day.</para>
    /// <para>Weekly: only once per week.</para>
    /// <para>Monthly: only once per month.</para>
    /// </summary>
    public enum CheckFrequencies
    {
        Never,
        EveryStart,
        Daily,
        Weekly,
        Monthly,
    }
}

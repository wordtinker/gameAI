namespace World
{
    /// <summary>
    /// Locations of the city, everything is connected
    /// no reason to use graph.
    /// </summary>
    enum LocationType
    {
        shack1, // for bob
        shack2, // for jim
        goldmine,
        bank,
        saloon
    }

    /// <summary>
    /// Messages that are used in the game
    /// for agents communication.
    /// </summary>
    enum Messages
    {
        Invasion,
        FoundAGreatOne
    }
}

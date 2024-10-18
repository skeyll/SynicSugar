namespace  SynicSugar.Samples.Tank 
{
    /// <summary>
    /// We can't use user attributes on matchmaking in SynicSugar. <br />
    /// If we want to take over user data being used for lobby, we need to synchronize the data via p2p by carrying over from another scene with ScriptableObject, singleton or so on. <br />
    /// 
    /// The reasons for not using Lobby attributes in P2P are:
    /// 1.There is a delay in reflecting attributes to the Lobby.
    /// 2.Separating matchmaking from P2P allows for future scalability and simplification of the matchmaking API.
    /// </summary>
    public static class TankPassedData 
    {
        public static string PlayerName;
    }
}
namespace BluForTracker.Shared;
public record ConnectedUser
{
    public string Name { get; set; }
    public string UserIdentifier { get; set; }

    public List<Connection> Connections { get; set; }
}
public record Connection
{
    public string ConnectionID { get; set; }

}
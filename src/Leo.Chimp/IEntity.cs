namespace Leo.Chimp
{
    /// <summary>
    /// IEntity
    /// </summary>
    public interface IEntity<T>
    {
        T Id { get; set; }
    }

    public interface IEntity
    {

    }

}
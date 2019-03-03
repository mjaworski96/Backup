namespace Communication.Serialization
{
    public interface ISerialization
    {
        byte[] Serialize<T>(T obj);
        T Deserialize<T>(byte[] content);
    }
}

namespace zahadalba_backend.Services
{
    public interface ISha256HasherService
    {
        string GetHash(string rawData);
    }
}

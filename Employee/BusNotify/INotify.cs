namespace BusNotify
{
    public interface INotify
    {
        NotifyAction Action { get; set; }
        string DictionaryKey { get; set; }
        string Key { get; set; }
        string ServiceName { get; set; }
    }
}
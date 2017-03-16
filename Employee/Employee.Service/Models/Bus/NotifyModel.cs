namespace Employee.Service.Models.Bus
{
    /// <summary>
    ///     读取服务
    /// </summary>
    public enum Action
    {
        Add,
        Update,
        Delete
    }

    public class NotifyModel<T>
    {
        public Action Action { get; set; }
        public string AppName { get; set; }
        public string DictionaryKey { get; set; }
        public string Key { get; set; }
        public T Value { get; set; }
    }
}
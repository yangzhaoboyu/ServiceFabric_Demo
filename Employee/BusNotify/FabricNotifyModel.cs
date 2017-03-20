namespace BusNotify
{
    public class FabricNotifyModel : INotify
    {
        public string Value { get; set; }

        #region INotify Members

        public NotifyAction Action { get; set; }
        public string DictionaryKey { get; set; }
        public string Key { get; set; }
        public string ServiceName { get; set; }

        #endregion INotify Members
    }
}
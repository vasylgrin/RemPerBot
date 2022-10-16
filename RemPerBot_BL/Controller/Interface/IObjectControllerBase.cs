namespace RemBerBot_BL.Controller.Interface
{
    public interface IObjectControllerBase
    {
        public Task<bool> Add(string inputText, long chatId, CancellationToken cancellationToken);
        public bool Save(long chatId, out int objectId);
        public void PrintCurrentObject(long chatId, string callbackName);
    }
}

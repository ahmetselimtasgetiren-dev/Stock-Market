namespace StockMarket.Infrastructure.Persistence
{
    public readonly struct SaveLoadResult
    {
        public SaveLoadResult(bool succeeded, SaveGameData data, string error)
        {
            Succeeded = succeeded;
            Data = data;
            Error = error;
        }

        public bool Succeeded { get; }
        public SaveGameData Data { get; }
        public string Error { get; }
    }
}

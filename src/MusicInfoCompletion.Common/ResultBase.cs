namespace MusicInfoCompletion.Common
{
    public class ResultBase
    {
        public ResultCode ResultCode { get; set; }
        public string ExtraMessage { get; set; }
    }

    public enum ResultCode
    {
        Successful,
        Failed,
        Exception
    }
}

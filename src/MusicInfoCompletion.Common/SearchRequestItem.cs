namespace MusicInfoCompletion.Common
{
    public class SearchRequestItem
    {
        public string Title { get; set; }
        public string Album { get; set; }
        public string[] Singers { get; set; }
        public string[] Genres { get; set; }
        public string CustomSearchQuery { get; set; }

        public string Path { get; set; }
        public override string ToString()
        {
            return $"Title: {Title}, Album: {Album}, Singers: {string.Join(Constants.Separater, Singers)}, Genres: {string.Join(Constants.Separater, Genres)}, CustomSearchQuery:{CustomSearchQuery}";
        }
    }
}

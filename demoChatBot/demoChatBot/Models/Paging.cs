namespace demoChatBot.Models
{
    public abstract class Paging
    {
        public int NextPage { get; set; } = -1;
        public string? PathUrl { get; set; }
    }
}

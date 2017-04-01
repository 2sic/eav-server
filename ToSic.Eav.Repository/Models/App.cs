using ToSic.Eav.Repository.Interfaces;

namespace ToSic.Eav.Repository.Models
{
    public class RepoApp : IRepoApp
    {
        public int AppId { get; set; }
        public int ZoneId { get; set; }
        public string Name { get; set; }
    }
}

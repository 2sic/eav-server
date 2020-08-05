using ToSic.Eav.Repository.Interfaces;

namespace ToSic.Eav.Repository.Models
{
    public class RepoZone : IRepoZone
    {
        public int ZoneId { get; set; }
        public string Name { get; set; }
    }
}

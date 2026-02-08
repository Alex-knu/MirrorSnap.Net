using MirrorSnap.Core.Models;

namespace MirrorSnap.Core.Models
{
    public class SnapSpec<TEntity>
    {
        public SnapSettings Settings { get; set; }
        public TEntity Expected { get; set; }
    }
}
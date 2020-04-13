using NCVC.App.Models;

namespace NCVC.App.Services
{
    public partial class DatabaseService
    {
        public DatabaseContext Context { get; }
        public DatabaseService(DatabaseContext context) { this.Context = context; }

    }

}

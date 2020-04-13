using System;
using System.Threading.Tasks;

namespace NCVC.App.Services
{
    public class NotifierService
    {
        public async Task Update()
        {
            if (Notify != null)
            {
                await Notify.Invoke();
            }
        }

        public event Func<Task> Notify;
    }
}

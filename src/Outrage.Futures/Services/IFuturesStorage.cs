using Outrage.Futures.Models;
using System.Threading.Tasks;

namespace Outrage.Futures.Services
{
    public interface IFuturesStorage
    {
        bool HasNext { get; }
        Task WriteCallAsync(TaskReference taskReference);
        Task<TaskReference> ReadNextAsync();
        Task RetryAsync(TaskReference next);
        Task CompleteAsync(TaskReference next);
    }
}
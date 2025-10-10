using System;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IInstruksPdfService
    {
        Task<byte[]> GeneratePdfAsync(Guid instruksId);
    }
}
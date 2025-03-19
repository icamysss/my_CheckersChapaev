using Cysharp.Threading.Tasks;
using YG;

namespace Services.Interfaces
{
    public interface IAdService : IService

    {
        UniTask ShowInterAd(); 
        
    }
}
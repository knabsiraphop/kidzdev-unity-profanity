using System.Threading;
using Cysharp.Threading.Tasks;

namespace KidzDev.Unity.Profanity {
    public interface IProfanityDataLoader {
        UniTask<ProfanityData> LoadAsync(string path, CancellationToken ct = default);
    }
}

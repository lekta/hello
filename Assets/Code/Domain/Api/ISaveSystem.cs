using LH.Save;

namespace LH.Api {
    public interface ISaveSystem {
        void SetHiddenState(int id, HiddenObjectSave save);
        HiddenObjectSave GetHiddenState(int id);
    }
}
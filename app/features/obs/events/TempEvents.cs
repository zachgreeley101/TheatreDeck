using theatredeck.app.core.api.obs;
using theatredeck.app.features.obs.interfaces;

namespace theatredeck.app.features.obs.events
{
    public class TempEvents : IOBSEvents
    {
        private readonly OBSManager _obsManager;

        public TempEvents(OBSManager obsManager)
        {
            _obsManager = obsManager;
        }

        public async Task ExecuteAsync()
        {
            await _obsManager.DisableSourceVisibility("Screen", "Main", "Screen");
            await _obsManager.UpdateTextSource("Screen", "Main", "TestText", "Black Mode Active");
        }
    }
}

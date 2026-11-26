using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;

namespace StreamModeXIV.Replacers;

/// <summary>
/// On-screen pop-up messages (toasts), e.g. "Bean Annyx joins the party."
/// </summary>
public sealed class ToastReplacer : IDisposable
{
    public ToastReplacer()
    {
        Plugin.ToastGui.Toast += OnToast;
        Plugin.ToastGui.QuestToast += OnQuestToast;
        Plugin.ToastGui.ErrorToast += OnErrorToast;
    }

    private static void Rewrite(ref SeString message)
    {
        if (!Plugin.Config.StreamModeEnabled) return;
        PartySource.Learn(); // a member who just joined is not in our cache yet
        message = ChatReplacer.Rewrite(message);
    }

    private static void OnToast(ref SeString message, ref ToastOptions options, ref bool isHandled) => Rewrite(ref message);
    private static void OnQuestToast(ref SeString message, ref QuestToastOptions options, ref bool isHandled) => Rewrite(ref message);
    private static void OnErrorToast(ref SeString message, ref bool isHandled) => Rewrite(ref message);

    public void Dispose()
    {
        Plugin.ToastGui.Toast -= OnToast;
        Plugin.ToastGui.QuestToast -= OnQuestToast;
        Plugin.ToastGui.ErrorToast -= OnErrorToast;
    }
}

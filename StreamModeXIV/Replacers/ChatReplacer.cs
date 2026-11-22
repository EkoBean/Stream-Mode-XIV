using Dalamud.Game.Chat;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace StreamModeXIV.Replacers;

/// <summary>Optional: abbreviates sender and in-message player names in the chat log.</summary>
public sealed class ChatReplacer : IDisposable
{
    public ChatReplacer() => Plugin.ChatGui.ChatMessage += OnChat;

    private static void OnChat(IHandleableChatMessage message)
    {
        var cfg = Plugin.Config;
        if (!cfg.StreamModeEnabled || !cfg.ReplaceInChat) return;

        foreach (var p in message.Sender.Payloads)
            if (p is PlayerPayload pp) NameAbbreviator.Learn(pp.PlayerName);
        foreach (var p in message.Message.Payloads)
            if (p is PlayerPayload pp) NameAbbreviator.Learn(pp.PlayerName);

        message.Sender = Rewrite(message.Sender);
        message.Message = Rewrite(message.Message);
    }

    internal static SeString Rewrite(SeString s)
    {
        foreach (var p in s.Payloads)
            if (p is TextPayload tp && tp.Text != null)
                tp.Text = NameAbbreviator.ReplaceKnown(tp.Text);
        return s;
    }

    public void Dispose() => Plugin.ChatGui.ChatMessage -= OnChat;
}

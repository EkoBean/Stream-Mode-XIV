# FFXIV 介面中會出現玩家名稱的地方

替換方式分三種：

- **NamePlate**：`INamePlateGui.OnNamePlateUpdate`，直接改 handler 的 `Name`。最穩定。
- **Chat**：`IChatGui.ChatMessage`，改 `Sender` / `Message` 的 TextPayload（使用者可在設定中開關）。
- **Addon**：`IAddonLifecycle` PreDraw，走訪視窗的 AtkTextNode 改字。
  - **名單型**：只有「整段文字剛好就是一個名字」（例如招募者欄位、好友名）才縮寫，並記入已知名單。說明、標籤、副本名等較長文字完全不動。遊戲本身的文字（Addon 介面字串如分頁名、副本名、地名、稱號）從遊戲資料表讀出並排除，所以「Friend List」這類分頁名不會被縮寫。
  - **混合型**：只替換「已知玩家名」（從 ObjectTable、PartyList、名單型視窗、聊天中蒐集），避免把 Boss 名稱或「Critical Hit」這類兩個大寫單字的文字也縮寫掉。

| 位置 | Addon 名稱 | 方式 | 狀態 |
|---|---|---|---|
| 頭上名牌（自己與其他玩家） | — | NamePlate | 已實作 |
| Party List | `_PartyList` | 混合型 | 已實作 |
| Alliance List | `_AllianceList1/2` | 混合型 | 已實作 |
| Social › Party Members | `PartyMemberList` | 名單型 | 已實作 |
| Social › Friend List | `FriendList` | 名單型 | 已實作 |
| Social › Player Search 結果 | `SocialList` | 名單型 | 已實作 |
| Blacklist | `BlackList` | 名單型 | 已實作 |
| Mute List | `MuteList` | 名單型 | 已實作 |
| Contacts | `ContactList` | 名單型 | 已實作 |
| Free Company 成員 | `FreeCompanyMember(List)` | 名單型 | 已實作 |
| Free Company 主視窗（會長） | `FreeCompany`, `FreeCompanyProfile` | 混合型 | 已實作 |
| Linkshell / Cross-world Linkshell | `LinkShell`, `CrossWorldLinkshell` | 名單型 | 已實作 |
| Fellowship | `CircleList`, `CircleBook` | 名單型 | 已實作 |
| Duty 成員名單 | `ContentMemberList` | 名單型 | 已實作 |
| Target 欄（含 Boss 的目標 = Target of Target） | `_TargetInfo`, `_TargetInfoMainTarget`, `_TargetInfoCastBar` | 混合型 | 已實作 |
| Focus Target | `_FocusTargetInfo` | 混合型 | 已實作 |
| Adventurer Plate | `CharaCard` | 混合型 | 已實作 |
| Character 視窗（名字、稱號） | `Character` | 混合型 | 已實作 |
| Examine | `CharacterInspect` | 混合型 | 已實作 |
| Party Finder 招募者 | `LookingForGroup`, `LookingForGroupDetail` | 名單型（只換招募者欄位） | 已實作 |
| Ready Check / Trade / 右鍵選單標題 | `ReadyCheck`, `Trade`, `ContextMenu` | 混合型 | 已實作 |
| NPC 對話中叫你的名字 | `Talk`, `SelectString`, `SelectYesno` | 混合型 | 已實作 |
| 小地圖滑鼠提示、Tooltip | `_NaviMap`, `Tooltip` | 混合型 | 已實作 |
| PvP 計分板 | `MKSRecord`, `FrontlineRecord` | 混合型 | 已實作 |
| 物品詳細的製作者簽名 | `ItemDetail` | 混合型 | 已實作 |
| 房屋告示牌（屋主） | `HousingSignBoard` | 混合型 | 已實作 |
| 市場板賣家／雇員 | `ItemSearchResult` | 混合型 | 已實作（雇員名不算玩家名，不會被改） |
| 聊天視窗 | — | Chat | 已實作，預設關閉 |
| 聊天泡泡（頭上對話框） | `_MiniTalk` | — | 未做，內容是對話不是名字 |
| 角色選擇畫面 | `_CharaSelectListMenu` | — | 未做，登入前 plugin 通常不需要 |

## 注意

- Addon 名稱是依目前遊戲版本整理，**尚未在遊戲中逐一確認**。若某個視窗沒被替換，用 `/xldata` → Addon Inspector 找出真正的 addon 名稱，填到設定的「extra name-list windows」。
- 混合型視窗只認得已知玩家名：剛打開 Friend List 前，從未同屏過的好友名不會出現在 Adventurer Plate 等混合型視窗中被縮寫，打開過一次名單型視窗後就會被記住。
- 關閉 Stream Mode 時會把仍在畫面上的文字還原；已關閉的視窗下次打開時遊戲會自己重繪正確名字。

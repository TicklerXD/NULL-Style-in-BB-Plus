using MTM101BaldAPI.ObjectCreation;
using PlusLevelLoader;
using System.Collections.Generic;
using UnityEngine;

namespace DevTools.DevAPI.Extensions;


public static class NPCBuilderExtensions
{
    static Dictionary<string, Sprite> m_spriteStorage = [];
    public static T BuildNPC<T>(this NPCBuilder<T> b) where T : NPC
    {
        var npc = b.Build();
        if (m_spriteStorage.ContainsKey(npc.name))
        {
            npc.spriteRenderer[0].sprite = m_spriteStorage[npc.name];
            m_spriteStorage.Remove(npc.name);
        }
        
        PlusLevelLoaderPlugin.Instance.npcAliases.Add(npc.name, npc);
        return npc;
    }
    public static NPCBuilder<T> SetSprite<T>(this NPCBuilder<T> b, Sprite sprite) where T : NPC
    {
        m_spriteStorage.Add(b.objectName, sprite);
        return b;
    }

    public static NPCBuilder<T> SetSprite<T>(this NPCBuilder<T> b, string sprite) where T : NPC =>
        b.SetSprite(NULL.Manager.ModManager.m.Get<Sprite>(sprite));
}


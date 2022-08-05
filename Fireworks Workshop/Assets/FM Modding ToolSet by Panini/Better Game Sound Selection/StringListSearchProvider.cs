#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using UnityEngine.Events;

public class StringListSearchProvider : ScriptableObject, ISearchWindowProvider
{

    private string[] listItems;
    private UnityAction<string> onSetIndexCallback;

    public void setCallback(UnityAction<string> callback)
    {
        onSetIndexCallback = callback;
    }

    public void setItems(string[] items)
    {
        listItems = items;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> searchlist = new List<SearchTreeEntry>();
        searchlist.Add(new SearchTreeGroupEntry(new GUIContent("Game Sounds"), 0));

        List<string> groups = new List<string>();
        foreach (string item in listItems)
        {
            string[] entryTitle = item.Split('/');
            string groupName = "";
            for (int i = 0; i<entryTitle.Length-1; i++)
            {
                groupName += entryTitle[i];
                if (!groups.Contains(groupName))
                {
                    searchlist.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                    groups.Add(groupName);
                }
                groupName += "/";
            }

            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()));
            entry.level = entryTitle.Length;
            entry.userData = entryTitle.Last();
            searchlist.Add(entry);
        }

        return searchlist;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        onSetIndexCallback?.Invoke((string)SearchTreeEntry.userData);
        return true;
    }

}
#endif
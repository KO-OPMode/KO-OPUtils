#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace KOOPEditor
{
    /// <summary>
    /// An alternative for GenericMenu which handles parameters nicer :)
    /// </summary>
    public class BasicMenu
    {
        abstract class MenuItemBase
        {
            public GUIContent content = GUIContent.none;
            public bool ticked = false;
            public bool disabled = false;
            public object[] parameters = null;

            public abstract void Execute();
        }
        
        class Separator : MenuItemBase
        {
            public string path;
            public bool onlyShowIfThereAreFollowingItems;
            public override void Execute()
            {
                return;
            }
        }

        class MenuItem : MenuItemBase
        {
            public Action action = null;
            public override void Execute()
            {
                action.Invoke();
            }
        }

        class MenuItem<T1> : MenuItemBase
        {
            public Action<T1> action = null;
            public override void Execute()
            {
                action.Invoke((T1)parameters[0]);
            }
        }

        class MenuItem<T1, T2> : MenuItemBase
        {
            public Action<T1, T2> action = null;
            public override void Execute()
            {
                action.Invoke((T1)parameters[0], (T2)parameters[1]);
            }
        }

        class MenuItem<T1, T2, T3> : MenuItemBase
        {
            public Action<T1, T2, T3> action = null;
            public override void Execute()
            {
                action.Invoke((T1)parameters[0], (T2)parameters[1], (T3)parameters[2]);
            }
        }


        int menuItemCount = 0;
        GenericMenu menu;
        List<MenuItemBase> items = new List<MenuItemBase>();

        public BasicMenu()
        {
            menu = new GenericMenu();
        }

        public void ShowAsContext()
        {
            GenericMenu menu = CreateGenericMenu();
            menu.ShowAsContext();
        }

        public void ShowAsDropdown(Rect rect)
        {
            GenericMenu menu = CreateGenericMenu();
            menu.DropDown(rect);
        }

        GenericMenu CreateGenericMenu()
        {
            menuItemCount = 0;
            menu = new GenericMenu();
            foreach (MenuItemBase item in items)
            {
                if (item is Separator separator)
                {
                    if (!separator.onlyShowIfThereAreFollowingItems || menuItemCount < items.Count - 1)
                    {
                        menu.AddSeparator(separator.path);
                    }
                }
                else if (!item.disabled)
                {
                    menu.AddItem(item.content, item.ticked, ExecuteItem, menuItemCount);
                }
                else
                {
                    menu.AddDisabledItem(item.content, item.ticked);
                }

                menuItemCount++;
            }

            return menu;
        }

        void ExecuteItem(object data) { ExecuteItem((int)data); }
        void ExecuteItem(int index)
        {
            if (index < 0 || index >= items.Count)
            {
                Debug.LogWarning($"Attempted to select option from GenericMenu but failed: Selected item out of index of list? (Was the list modified after showing?)");
                return;
            }

            MenuItemBase item = items[index];
            item.Execute();
        }

        public void AddItem(string label, Action method) { AddItem(new GUIContent(label), true, method); }
        public void AddItem(string label, bool enabled, Action method) { AddItem(new GUIContent(label), enabled, false, method); }
        public void AddItem(string label, bool enabled, bool ticked, Action method) { AddItem(new GUIContent(label), enabled, ticked, method); }
        public void AddItem(GUIContent label, Action method) { AddItem(label, true, method); }
        public void AddItem(GUIContent label, bool enabled, Action method)  { AddItem(label, enabled, false, method); }
        public void AddItem(GUIContent label, bool enabled, bool ticked, Action method)
        {
            items.Add(new MenuItem() { content = label, disabled = !enabled, action = method, ticked = ticked, parameters = new object[] {}});
        }

        public void AddItem<T1>(string label, Action<T1> method, T1 param1) { AddItem(new GUIContent(label), true, method, param1); }
        public void AddItem<T1>(string label, bool enabled, Action<T1> method, T1 param1) { AddItem(new GUIContent(label), enabled, false, method, param1); }
        public void AddItem<T1>(string label, bool enabled, bool ticked, Action<T1> method, T1 param1) { AddItem(new GUIContent(label), enabled, ticked, method, param1); }
        public void AddItem<T1>(GUIContent label, Action<T1> method, T1 param1) { AddItem(label, true, method, param1); }
        public void AddItem<T1>(GUIContent label, bool enabled, Action<T1> method, T1 param1) { AddItem(label, enabled, false, method, param1); }
        public void AddItem<T1>(GUIContent label, bool enabled, bool ticked, Action<T1> method, T1 param1)
        {
            items.Add(new MenuItem<T1>() { content = label, disabled = !enabled, action = method, ticked = ticked, parameters = new object[] { param1 }});
        }

        public void AddItem<T1, T2>(string label, Action<T1, T2> method, T1 param1, T2 param2) { AddItem(new GUIContent(label), true, method, param1, param2); }
        public void AddItem<T1, T2>(string label, bool enabled, Action<T1, T2> method, T1 param1, T2 param2)  { AddItem(new GUIContent(label), enabled, method, param1, param2); }
        public void AddItem<T1, T2>(GUIContent label, Action<T1, T2> method, T1 param1, T2 param2)  { AddItem(label, true, method, param1, param2); }
        public void AddItem<T1, T2>(GUIContent label, bool enabled, Action<T1, T2> method, T1 param1, T2 param2) 
        { 
            items.Add(new MenuItem<T1, T2>() { content = label, disabled = !enabled, action = method, parameters = new object[] { param1, param2 }});
        }

        public void AddItem<T1, T2, T3>(string label, Action<T1, T2, T3> method, T1 param1, T2 param2, T3 param3) { AddItem(new GUIContent(label), true, method, param1, param2, param3); }
        public void AddItem<T1, T2, T3>(string label, bool enabled, Action<T1, T2, T3> method, T1 param1, T2 param2, T3 param3) { AddItem(new GUIContent(label), enabled, method, param1, param2, param3); }
        public void AddItem<T1, T2, T3>(GUIContent label, Action<T1, T2, T3> method, T1 param1, T2 param2, T3 param3) { AddItem(label, true, method, param1, param2, param3); }
        public void AddItem<T1, T2, T3>(GUIContent label, bool enabled, Action<T1, T2, T3> method, T1 param1, T2 param2, T3 param3)
        { 
            items.Add(new MenuItem<T1, T2, T3>() { content = label, disabled = !enabled, action = method, parameters = new object[] { param1, param2, param3 }});
        }

        public void AddSeparator() { AddSeparator(string.Empty); }
        public void AddSeparator(string path) { AddSeparator(path, false); }
        public void AddSeparator(string path, bool doNotShowIfThisIsLastItem)
        {
            items.Add(new Separator() { path = path, onlyShowIfThereAreFollowingItems = doNotShowIfThisIsLastItem });
        }
    }
}

#endif
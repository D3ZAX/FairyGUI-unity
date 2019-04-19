using System;
using System.Collections.Generic;
#if FAIRYGUI_TOLUA
using LuaInterface;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class UIObjectFactory
	{
		public delegate GComponent GComponentCreator();

		static Dictionary<string, GComponentCreator> packageItemExtensions = new Dictionary<string, GComponentCreator>();

        static Dictionary<ObjectType, Func<GObject>> objCreateFuncDic = new Dictionary<ObjectType, Func<GObject>>();

        static UIObjectFactory()
        {
            objCreateFuncDic.Add(ObjectType.Image, () => new GImage());
            objCreateFuncDic.Add(ObjectType.MovieClip, () => new GMovieClip());
            objCreateFuncDic.Add(ObjectType.Component, () => new GComponent());
            objCreateFuncDic.Add(ObjectType.Text, () => new GTextField());
            objCreateFuncDic.Add(ObjectType.RichText, () => new GRichTextField());
            objCreateFuncDic.Add(ObjectType.InputText, () => new GTextInput());
            objCreateFuncDic.Add(ObjectType.Group, () => new GGroup());
            objCreateFuncDic.Add(ObjectType.Graph, () => new GGraph());
            objCreateFuncDic.Add(ObjectType.Loader, () => new GLoader());
            objCreateFuncDic.Add(ObjectType.Button, () => new GButton());
            objCreateFuncDic.Add(ObjectType.Label, () => new GLabel());
            objCreateFuncDic.Add(ObjectType.ProgressBar, () => new GProgressBar());
            objCreateFuncDic.Add(ObjectType.Slider, () => new GSlider());
            objCreateFuncDic.Add(ObjectType.ScrollBar, () => new GScrollBar());
            objCreateFuncDic.Add(ObjectType.ComboBox, () => new GComboBox());
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="type"></param>
		public static void SetPackageItemExtension(string url, System.Type type)
		{
			SetPackageItemExtension(url, () => { return (GComponent)Activator.CreateInstance(type); });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="creator"></param>
		public static void SetPackageItemExtension(string url, GComponentCreator creator)
		{
			if (url == null)
				throw new Exception("Invaild url: " + url);

			PackageItem pi = UIPackage.GetItemByURL(url);
			if (pi != null)
				pi.extensionCreator = creator;

			packageItemExtensions[url] = creator;
		}

#if FAIRYGUI_TOLUA
		public static void SetExtension(string url, System.Type baseType, LuaFunction extendFunction)
		{
			SetPackageItemExtension(url, () =>
			{
				GComponent gcom = (GComponent)Activator.CreateInstance(baseType);

				extendFunction.BeginPCall();
				extendFunction.Push(gcom);
				extendFunction.PCall();
				gcom.SetLuaPeer(extendFunction.CheckLuaTable());
				extendFunction.EndPCall();

				return gcom;
			});
		}
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public static void SetLoaderExtension(System.Type type)
		{
			objCreateFuncDic[ObjectType.Loader] = () => { return (GLoader)Activator.CreateInstance(type); };
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="creator"></param>
		public static void SetLoaderExtension(Func<GLoader> creator)
		{
            objCreateFuncDic[ObjectType.Loader] = creator;
		}

        /// <summary>
        /// Extend FairyGUI obj class with custom class
        /// </summary>
        /// <param name="type">obj class type</param>
        /// <param name="creator">custom class create function</param>
        public static void SetObjectExtension(ObjectType type, Func<GObject> creator)
        {
            objCreateFuncDic[type] = creator;
        }

		internal static void ResolvePackageItemExtension(PackageItem pi)
		{
			if (!packageItemExtensions.TryGetValue(UIPackage.URL_PREFIX + pi.owner.id + pi.id, out pi.extensionCreator)
				&& !packageItemExtensions.TryGetValue(UIPackage.URL_PREFIX + pi.owner.name + "/" + pi.name, out pi.extensionCreator))
				pi.extensionCreator = null;
		}

		public static void Clear()
		{
			packageItemExtensions.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pi"></param>
		/// <returns></returns>
		public static GObject NewObject(PackageItem pi)
		{
			if (pi.extensionCreator != null)
			{
				Stats.LatestObjectCreation++;
				return pi.extensionCreator();
			}
			else
				return NewObject(pi.objectType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static GObject NewObject(ObjectType type)
		{
			Stats.LatestObjectCreation++;

			if (objCreateFuncDic.ContainsKey(type))
            {
                return objCreateFuncDic[type]();
            }
            else
            {
                return null;
            }
		}
	}
}

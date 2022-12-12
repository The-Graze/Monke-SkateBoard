using Utilla;
using System;
using BepInEx;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using UnityEngine.Networking;
using System.Collections.Generic;
namespace Monke_SkateBoard
{
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    class Plugin : BaseUnityPlugin
    {
        public static string imagePath;
        public static readonly List<string> imagesPublic = new List<string>();
        public static readonly List<string> imageNames = new List<string>();
        public static GameObject btexload;
        GameObject backbaord;

        public void Awake()
        {
            Events.GameInitialized += OnGameInitialized;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("Monke-SkateBoard.Assets.backboard");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            GameObject skate = bundle.LoadAsset<GameObject>("BackBoard");
            backbaord = Instantiate(skate);
            btexload = GameObject.CreatePrimitive(PrimitiveType.Plane);
            backbaord.AddComponent<CustomHoldable>();
            backbaord.GetComponent<CustomHoldable>().anchor = backbaord.transform;
            backbaord.GetComponent<CustomHoldable>().grabAnchor = backbaord.transform.GetChild(0);
            backbaord.GetComponent<CustomHoldable>().dockPositions = BodyDockPositions.DropPositions.RightBack;
            backbaord.GetComponent<CustomHoldable>().currentState = CustomHoldable.PositionState.OnRightShoulder;
            backbaord.GetComponent<CustomHoldable>().gripInteractor = backbaord.transform.GetChild(0).gameObject.AddComponent<InteractionPoint>();
            backbaord.transform.GetChild(0).gameObject.GetComponent<InteractionPoint>().forLocalPlayer = true;
            backbaord.transform.GetChild(0).gameObject.GetComponent<InteractionPoint>().enabled= true;
            backbaord.GetComponent<CustomHoldable>().myRig = GameObject.Find("Global/Local VRRig/Local Gorilla Player").GetComponent<VRRig>();
            backbaord.GetComponent<CustomHoldable>().storedZone = BodyDockPositions.DropPositions.RightBack;
            backbaord.transform.SetParent(GameObject.Find("Global/Local VRRig/Local Gorilla Player/Holdables").transform, false);
            GetImage();
            LoadImage();
            backbaord.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.mainTexture = btexload.GetComponent<Renderer>().material.mainTexture;
            btexload.GetComponent<MeshCollider>().enabled= false;
            btexload.SetActive(false);
        }
        void GetImage()
        {
            imagePath = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "Plugins", PluginInfo.Name.ToString(), "Board");
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }
            string[] images = Directory.GetFiles(imagePath);
            string[] imagename = new string[images.Length];
            for (int i = 0; i < imagename.Length; i++)
            {
                imagename[i] = Path.GetFileName(images[i]);
                imageNames.Add(imagename[i]);
                imagesPublic.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Board\\" + imagename[i]);

            }
        }
        public static void LoadImage()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.filterMode = FilterMode.Point;
            byte[] bytes = File.ReadAllBytes(imagesPublic[0]);
            tex.LoadImage(bytes);
            tex.Apply();
            btexload.GetComponent<Renderer>().material.mainTexture = tex;
        }
    }
}
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;

namespace WowAltHelper
{
    class Program
    {
        public class ToonInfo
        {
            public string Name { get; set; }
            public BaseInfo Items { get; set; }
        }

        public class BaseInfo
        {
            public int AverageItemLevel { get; set; }
            public class BackItem : ItemInfo { public BackItem() { SlotName = "Back"; } }
            public class ChestItem : ItemInfo { public ChestItem() { SlotName = "Chest"; } }
            public class FeetItem : ItemInfo { public FeetItem() { SlotName = "Feet"; } }
            public class FingerItem : ItemInfo { public FingerItem() { SlotName = "Ring"; } }
            public class HandsItem : ItemInfo { public HandsItem() { SlotName = "Gloves"; } }
            public class HeadItem : ItemInfo { public HeadItem() { SlotName = "Helmet"; } }
            public class LegItem : ItemInfo { public LegItem() { SlotName = "Pants"; } }
            public class MainHandItem : ItemInfo { public MainHandItem() { SlotName = "MainHand"; } }
            public class OffHandItem : ItemInfo { public OffHandItem() { SlotName = "OffHand"; } }
            public class NeckItem : ItemInfo { public NeckItem() { SlotName = "Neck"; } }
            public class ShoulderItem : ItemInfo { public ShoulderItem() { SlotName = "Shoulder"; } }
            public class TrinketItem : ItemInfo { public TrinketItem() { SlotName = "Trinket"; } }
            public class WaistItem : ItemInfo { public WaistItem() { SlotName = "Belt"; } }
            public class WristItem : ItemInfo { public WristItem() { SlotName = "Bracer"; } }

            public BackItem Back { get; set; }
            public ChestItem Chest { get; set; }
            public FeetItem Feet { get; set; }
            public FingerItem Finger1 { get; set; }
            public FingerItem Finger2 { get; set; }
            public HandsItem Hands { get; set; }
            public HeadItem Head { get; set; }
            public LegItem Legs { get; set; }
            public MainHandItem MainHand { get; set; }
            public OffHandItem OffHand { get; set; }
            public NeckItem Neck { get; set; }
            public ShoulderItem Shoulder { get; set; }
            public TrinketItem Trinket1 { get; set; }
            public TrinketItem Trinket2 { get; set; }
            public WaistItem Waist { get; set; }
            public WristItem Wrist { get; set; }

            public IEnumerable<ItemInfo> GetItems()
            {
                yield return Back;
                yield return Chest;
                yield return Feet;
                yield return Finger1;
                yield return Finger2;
                yield return Hands;
                yield return Head;
                yield return Legs;
                yield return MainHand;
                yield return OffHand;
                yield return Shoulder;
                yield return Trinket1;
                yield return Trinket2;
                yield return Waist;
                yield return Wrist;
            }
        }

        public class ItemInfo
        {
            public ItemInfo()
            {
                SlotName = "";
            }
            public string Name { get; set; }
            public int ItemLevel { get; set; }
            public string SlotName { get; set; }
        }

        public static List<string> characters = new List<string>
        {
            "Fream",
            "Eream",
            "Voidlight",
            "Freaim",
            "Freamo",
            "Freamz",
            "Freamy",
            "Monsoon",
            "Eerie",
            "Felhunter"
        };

        static List<ToonInfo> allInfo;

        static void Main(string[] args)
        {
            string token = "";
            allInfo = new List<ToonInfo>();
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://eu.battle.net/oauth/token"))
                {
                    var clientId = File.ReadAllText("clientId.txt");
                    var clientSecret = File.ReadAllText("secret.txt");
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(clientId + ":" + clientSecret));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                    var response = System.Threading.Tasks.Task.Run(() => httpClient.SendAsync(request)).Result;

                    var test = System.Threading.Tasks.Task.Run(() => response.Content.ReadAsStringAsync()).Result;
                    token = test.Split('"')[3];
                }

                var url = "https://us.api.blizzard.com/data/wow/token/?namespace=dynamic-us";
                foreach (var character in characters)
                {
                    var charUrl = "https://eu.api.blizzard.com/wow/character/grim-batol/" + character + "?fields=items";
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), charUrl))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
                        var response = System.Threading.Tasks.Task.Run(() => httpClient.SendAsync(request)).Result;
                        var content = System.Threading.Tasks.Task.Run(() => response.Content.ReadAsStringAsync()).Result;
                        var c = JsonConvert.DeserializeObject<object>(content);
                        var convertedData = JsonConvert.DeserializeObject<ToonInfo>(content);
                        convertedData.Name = character;
                        allInfo.Add(convertedData);

                    }
                }
                Printer();
                Console.ReadLine();
            }
        }

        public static void Printer()
        {
            foreach (var data in allInfo.OrderByDescending(o => o.Items.AverageItemLevel))
            {
                var ilvl = (data.Items.Neck.ItemLevel - 333) / 2;
                if(data.Items.Back.ItemLevel < 465)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.WriteLine(data.Name + " avg ilvl: " + data.Items.AverageItemLevel + " (" + ilvl + "/70)");

                foreach (var i in data.Items.GetItems())
                {
                    if (i != null && i.ItemLevel < 385 && i.ItemLevel > 1)
                    {
                        Console.WriteLine(
                            i.SlotName.PadRight(35)
                            + i.ItemLevel);
                    }
                }

                Console.WriteLine("----- o -----");
            }
        }
    }
}

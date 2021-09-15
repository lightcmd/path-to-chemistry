﻿using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class InventoryData
{
    public static string Slot = "Slot (1)";
    public static string Input { get; set; }
}

public class Inventory : MonoBehaviour
{
    private void Start()
    {
        Load();
        Load2();
        elementCheck();
        var playerData = PlayerData.Instance();
        GameObject.Find(InventoryData.Slot).GetComponent<Image>().color = Color.cyan;
        GameObject.Find("Energy").GetComponent<Text>().text = $"Energy: {playerData.Energy}";
        for (var i = 1; i <= 118; i++)
        {
            if (InventoryData.Slot == $"Slot ({i})")
            {
                var Name = playerData.Inventory.Keys.ElementAt(i - 1);
                var Quantity = playerData.Inventory.Values.ElementAt(i - 1);
                GameObject.Find("State").GetComponent<Text>().text = $"Name: {Name}\nQuantity: {Quantity}";
                break;
            }
        }
/*
        for (int i = 1; i <= 118; i++)
        {
            playerData.Inventory.Add(elementData.elements.Keys.ElementAt(i - 1), 0);
        }
        Save();
*/
    }
    
    public void Slider(float Value)
    {
        GameObject.Find("sliderValue").GetComponent<Text>().text = Convert.ToString(Math.Floor(Value));
    }

    public void getQuantity(GameObject getQuantityUI)
    {
        var playerData = PlayerData.Instance();
        var slotNum = Convert.ToInt32(InventoryData.Slot.Replace("Slot (", "").Replace(")", ""));
        if (playerData.Inventory.Values.ElementAt(slotNum - 1) > 1)
        {
            getQuantityUI.SetActive(true);
            GameObject.Find("getSliderQuantity/Slider").GetComponent<Slider>().maxValue = playerData.Inventory.Values.ElementAt(slotNum - 1);
            GameObject.Find("getSliderQuantity/Slider").GetComponent<Slider>().value = 0;
        }
        else if (playerData.Inventory.Values.ElementAt(slotNum - 1) > 0)
        {
            for (var i = 1; i <= 9; i = i + 1)
            {
                if (Convert.ToString(playerData.slotItem[$"Slot{i}"]["Element"]) == playerData.Inventory.Keys.ElementAt(slotNum - 1))
                {
                    playerData.slotItem[$"Slot{i}"]["Quantity"] = Convert.ToInt32(playerData.slotItem[$"Slot{i}"]["Quantity"]) + 1;
                    break;
                }
                else if (playerData.slotItem[$"Slot{i}"]["Element"] == null && playerData.slotItem[$"Slot{i}"]["Quantity"] == null)
                {
                    playerData.slotItem[$"Slot{i}"]["Element"] = playerData.Inventory.Keys.ElementAt(slotNum - 1);
                    playerData.slotItem[$"Slot{i}"]["Quantity"] = 1;
                    break;
                }
            }
            playerData.Inventory[playerData.Inventory.Keys.ElementAt(slotNum - 1)] -= 1;
            slotCheck();
            for (var i = 1; i <= 118; i++)
            {
                if (InventoryData.Slot == $"Slot ({i})")
                {
                    var Name = playerData.Inventory.Keys.ElementAt(i - 1);
                    var Quantity = playerData.Inventory.Values.ElementAt(i - 1);
                    GameObject.Find("State").GetComponent<Text>().text = $"Name: {Name}\nQuantity: {Quantity}";
                    break;
                }
            }
        }
    }

    public void returnQuantity(GameObject returnQuantityUI)
    {
        var playerData = PlayerData.Instance();
        if ((playerData.slotItem[$"Slot{hotbar.slotNum}"]["Element"] != null) && (playerData.slotItem[$"Slot{hotbar.slotNum}"]["Quantity"] != null))
        {
            if (Convert.ToInt32(playerData.slotItem[$"Slot{hotbar.slotNum}"]["Quantity"]) > 1)
            {
                returnQuantityUI.SetActive(true);
                GameObject.Find("returnSliderQuantity/Slider").GetComponent<Slider>().maxValue = Convert.ToInt32(playerData.slotItem[$"Slot{hotbar.slotNum}"]["Quantity"]);
                GameObject.Find("returnSliderQuantity/Slider").GetComponent<Slider>().value = 0;
            }
            else if (Convert.ToInt32(playerData.slotItem[$"Slot{hotbar.slotNum}"]["Quantity"]) > 0)
            {
                for (int i = 1; i <= 118; i++)
                {
                    if (Convert.ToString(playerData.slotItem[$"Slot{hotbar.slotNum}"]["Element"]) == playerData.Inventory.Keys.ElementAt(i - 1))
                    {
                        playerData.Inventory[playerData.Inventory.Keys.ElementAt(i - 1)] += 1;
                        GameObject.Find(InventoryData.Slot).GetComponent<Image>().color = Color.white;
                        InventoryData.Slot = $"Slot ({i})";
                        GameObject.Find(InventoryData.Slot).GetComponent<Image>().color = Color.cyan;
                        break;
                    }
                }
                playerData.slotItem[$"Slot{hotbar.slotNum}"]["Element"] = null;
                playerData.slotItem[$"Slot{hotbar.slotNum}"]["Quantity"] = null;
                slotCheck();
                for (var i = 1; i <= 118; i++)
                {
                    if (InventoryData.Slot == $"Slot ({i})")
                    {
                        var Name = playerData.Inventory.Keys.ElementAt(i - 1);
                        var Quantity = playerData.Inventory.Values.ElementAt(i - 1);
                        GameObject.Find("State").GetComponent<Text>().text = $"Name: {Name}\nQuantity: {Quantity}";
                        break;
                    }
                }
            }
        }
    }

    public void putQuantity(GameObject returnQuantityUI)
    {
        var playerData = PlayerData.Instance();
        var sliderValue = Convert.ToInt32(Math.Floor(GameObject.Find("returnSliderQuantity/Slider").GetComponent<Slider>().value));
        for (int i = 1; i <= 118; i++)
        {
            if (Convert.ToString(playerData.slotItem[$"Slot{hotbar.slotNum}"]["Element"]) == playerData.Inventory.Keys.ElementAt(i - 1))
            {
                playerData.Inventory[playerData.Inventory.Keys.ElementAt(i - 1)] += sliderValue;
                playerData.slotItem[$"Slot{hotbar.slotNum}"]["Quantity"] = Convert.ToInt32(playerData.slotItem[$"Slot{hotbar.slotNum}"]["Quantity"]) - sliderValue;
                GameObject.Find(InventoryData.Slot).GetComponent<Image>().color = Color.white;
                InventoryData.Slot = $"Slot ({i})";
                GameObject.Find(InventoryData.Slot).GetComponent<Image>().color = Color.cyan;
                break;
            }
        }
        if (Convert.ToInt32(playerData.slotItem[$"Slot{hotbar.slotNum}"]["Quantity"]) < 1)
        {
            playerData.slotItem[$"Slot{hotbar.slotNum}"]["Element"] = null;
            playerData.slotItem[$"Slot{hotbar.slotNum}"]["Quantity"] = null;
        }
        slotCheck();
        for (var i = 1; i <= 118; i++)
        {
            if (InventoryData.Slot == $"Slot ({i})")
            {
                var Name = playerData.Inventory.Keys.ElementAt(i - 1);
                var Quantity = playerData.Inventory.Values.ElementAt(i - 1);
                GameObject.Find("State").GetComponent<Text>().text = $"Name: {Name}\nQuantity: {Quantity}";
                break;
            }
        }
        returnQuantityUI.SetActive(false);
    }

    public void takeQuantity(GameObject getQuantityUI)
    {
        var playerData = PlayerData.Instance();
        var slotNum = Convert.ToInt32(InventoryData.Slot.Replace("Slot (", "").Replace(")", ""));
        var sliderValue = Convert.ToInt32(Math.Floor(GameObject.Find("getSliderQuantity/Slider").GetComponent<Slider>().value));
        for (var i = 1; i <= 9; i = i + 1)
        {
            if (Convert.ToString(playerData.slotItem[$"Slot{i}"]["Element"]) == playerData.Inventory.Keys.ElementAt(slotNum - 1))
            {
                playerData.slotItem[$"Slot{i}"]["Quantity"] = Convert.ToInt32(playerData.slotItem[$"Slot{i}"]["Quantity"]) + sliderValue;
                break;
            }
            else if (playerData.slotItem[$"Slot{i}"]["Element"] == null && playerData.slotItem[$"Slot{i}"]["Quantity"] == null)
            {
                playerData.slotItem[$"Slot{i}"]["Element"] = playerData.Inventory.Keys.ElementAt(slotNum - 1);
                playerData.slotItem[$"Slot{i}"]["Quantity"] = sliderValue;
                break;
            }
        }
        playerData.Inventory[playerData.Inventory.Keys.ElementAt(slotNum - 1)] -= sliderValue;
        slotCheck();
        for (var i = 1; i <= 118; i++)
        {
            if (InventoryData.Slot == $"Slot ({i})")
            {
                var Name = playerData.Inventory.Keys.ElementAt(i - 1);
                var Quantity = playerData.Inventory.Values.ElementAt(i - 1);
                GameObject.Find("State").GetComponent<Text>().text = $"Name: {Name}\nQuantity: {Quantity}";
                break;
            }
        }
        getQuantityUI.SetActive(false);
    }

    public void maxQuantity()
    {
        GameObject.Find("Slider").GetComponent<Slider>().value = GameObject.Find("Slider").GetComponent<Slider>().maxValue;
    }

    public void addQuantity()
    {
        if (GameObject.Find("Slider").GetComponent<Slider>().value + 1 <= GameObject.Find("Slider").GetComponent<Slider>().maxValue)
        {
            GameObject.Find("Slider").GetComponent<Slider>().value += 1;
        }
    }
    public void removeQuantity()
    {
        if (GameObject.Find("Slider").GetComponent<Slider>().value - 1 >= GameObject.Find("Slider").GetComponent<Slider>().minValue)
        {
            GameObject.Find("Slider").GetComponent<Slider>().value -= 1;
        }
    }

    public void indentifySellQuantity(GameObject SellUI)
    {
        var playerData = PlayerData.Instance();
        var slotNum = Convert.ToInt32(InventoryData.Slot.Replace("Slot (", "").Replace(")", ""));
        SellUI.SetActive(true);
        GameObject.Find("sellSliderQuantity/Slider").GetComponent<Slider>().maxValue = playerData.Inventory.Values.ElementAt(slotNum - 1);
        GameObject.Find("sellSliderQuantity/Slider").GetComponent<Slider>().value = 0;
    }

    public void sell(GameObject SellUI)
    {
        var playerData = PlayerData.Instance();
        var elementData = ElementData.Instance();
        var slotNum = Convert.ToInt32(InventoryData.Slot.Replace("Slot (", "").Replace(")", ""));
        var sellQuantity = Convert.ToInt32(Math.Floor(GameObject.Find("sellSliderQuantity/Slider").GetComponent<Slider>().value));
        for (int i = 1; i <= 94; i++)
        {
            if (elementData.rarity[i - 1] == playerData.Inventory.Keys.ElementAt(slotNum - 1))
            {
                playerData.Energy +=  sellQuantity * (i + 1) * 10;
                break;
            }
            else
            {
                playerData.Energy += sellQuantity * 10;
                break;
            }
        }
        playerData.Inventory[playerData.Inventory.Keys.ElementAt(slotNum - 1)] -= sellQuantity;
        GameObject.Find("Energy").GetComponent<Text>().text = $"Energy: {playerData.Energy}";
        for (var i = 1; i <= 118; i++)
        {
            if (InventoryData.Slot == $"Slot ({i})")
            {
                var Name = playerData.Inventory.Keys.ElementAt(i - 1);
                var Quantity = playerData.Inventory.Values.ElementAt(i - 1);
                GameObject.Find("State").GetComponent<Text>().text = $"Name: {Name}\nQuantity: {Quantity}";
                break;
            }
        }
        SellUI.SetActive(false);
    }

    public void elementSymbol()
    {
        var playerData = PlayerData.Instance();
        GameObject.Find(InventoryData.Slot).GetComponent<Image>().color = Color.white;
        InventoryData.Slot = EventSystem.current.currentSelectedGameObject.name;
        GameObject.Find(InventoryData.Slot).GetComponent<Image>().color = Color.cyan;
        for (var i = 1; i <= 118; i++)
        {
            if (EventSystem.current.currentSelectedGameObject.name == $"Slot ({i})")
            {
                var Name = playerData.Inventory.Keys.ElementAt(i - 1);
                var Quantity = playerData.Inventory.Values.ElementAt(i - 1);
                GameObject.Find("State").GetComponent<Text>().text = $"Name: {Name}\nQuantity: {Quantity}";
                break;
            }
        }
    }
    public void elementCheck()
    {
        var elementData = ElementData.Instance();
        for (var i = 1; i <= 118; i++)
        {
            GameObject.Find($"Slot ({i})").transform.GetComponentInChildren<Text>().text = elementData.elements.Keys.ElementAt(i - 1);
        }
    }

    private void slotCheck()
    {
        var playerData = PlayerData.Instance();
        for (var i = 1; i <= 9; i = i + 1)
            if (playerData.slotItem[$"Slot{i}"]["Element"] != null &&
                Convert.ToInt32(playerData.slotItem[$"Slot{i}"]["Quantity"]) == 1)
            {
                GameObject.Find($"HotbarSlot ({i})/Item").GetComponent<Text>().text = playerData.slotItem[$"Slot{i}"]["Element"].ToString();
                GameObject.Find($"HotbarSlot ({i})/ItemNum").GetComponent<Text>().text = "";
            }
            else if (playerData.slotItem[$"Slot{i}"]["Element"] != null && Convert.ToInt32(playerData.slotItem[$"Slot{i}"]["Quantity"]) > 1)
            {
                GameObject.Find($"HotbarSlot ({i})/Item").GetComponent<Text>().text = playerData.slotItem[$"Slot{i}"]["Element"].ToString();
                GameObject.Find($"HotbarSlot ({i})/ItemNum").GetComponent<Text>().text = playerData.slotItem[$"Slot{i}"]["Quantity"].ToString();
            }
            else if (playerData.slotItem[$"Slot{i}"]["Element"] == null && playerData.slotItem[$"Slot{i}"]["Quantity"] == null)
            {
                GameObject.Find($"HotbarSlot ({i})/Item").GetComponent<Text>().text = "";
                GameObject.Find($"HotbarSlot ({i})/ItemNum").GetComponent<Text>().text = "";
            }
    }

    private void Save()
    {
        print(Application.persistentDataPath);
        var playerData = PlayerData.Instance();
        var directory = $"{Application.persistentDataPath}/Data";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            var Settings = new JsonSerializerSettings();
            Settings.Formatting = Formatting.Indented;
            Settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            var Json = JsonConvert.SerializeObject(playerData, Settings);
            var filePath = Path.Combine(directory, "Saves.json");
            File.WriteAllText(filePath, Json);
        }
    }

    private void Load()
    {
        var filePath = Path.Combine(Application.dataPath, "Elements.json");
        var fileContent = File.ReadAllText(filePath);
        var elementData = JsonConvert.DeserializeObject<ElementData>(fileContent);
        ElementData.Instance().UpdateElementData(elementData);
    }

    private void Load2()
    {
        var directory = $"{Application.persistentDataPath}/Data";
        var filePath = Path.Combine(directory, "Saves.json");
        var fileContent = File.ReadAllText(filePath);
        var playerData = JsonConvert.DeserializeObject<PlayerData>(fileContent);
        PlayerData.Instance().UpdatePlayerData(playerData);
    }
}
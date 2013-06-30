using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using Mono.Cecil;
using UnityEngine;
using ScrollsModLoader.Interfaces;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;

namespace FavoriteChannels
{
	public class FavoriteChannels : BaseMod
	{
        private string favChannelsFilePath;
        private string fileName;

        List<string> FavoriteChannelsList = new List<string> { };

		//initialize everything here, Game is loaded at this point
		public FavoriteChannels ()
		{
            fileName = "favChannels.txt";
            favChannelsFilePath = this.OwnFolder() + Path.DirectorySeparatorChar + fileName;
		}

		public static string GetName ()
		{
            return "FavoriteChannels";
		}

		public static int GetVersion ()
		{
            return 1;
		}

		public static MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
            try
            {
                return new MethodDefinition[] {
                    scrollsTypes["MainMenu"].Methods.GetMethod("Start")[0],
                    scrollsTypes["ChatRooms"].Methods.GetMethod("ChatMessage", new Type[]{typeof(RoomChatMessageMessage)}),
                    scrollsTypes["ArenaChat"].Methods.GetMethod("RoomEnter")[0],
            	};
            }
            catch
            {
                Console.WriteLine("Fail van!");
                return new MethodDefinition[] { };
            }
		}


        public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {
            returnValue = null;

            if (info.targetMethod.Equals("Start"))
            {

                if (!File.Exists(favChannelsFilePath))
                {
                    File.Create(favChannelsFilePath);
                    return false;
                }

                string line;
                System.IO.StreamReader file = new System.IO.StreamReader(favChannelsFilePath);

                if (!isEmpty(favChannelsFilePath))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!line.Contains("trading") && !line.Contains("general"))
                        {
                            FavoriteChannelsList.Add(line);
                            App.ArenaChat.RoomEnter(line);
                        }
                    }

                    file.Close();
                }
            }  

            if (info.targetMethod.Equals("ChatMessage"))
            {
                RoomChatMessageMessage rcmm = (RoomChatMessageMessage)info.arguments[0];

                if (rcmm.text.StartsWith("/addchannel") || rcmm.text.StartsWith("/ac"))
                {
                    String[] splitted = rcmm.text.Split(' ');

                    if (splitted.Length >= 2)
                    {
                        String channelToAdd = splitted[1].ToLower();

                        if (!FavoriteChannelsList.Contains(channelToAdd) && (!channelToAdd.Contains("trading") && !channelToAdd.Contains("general"))) //If channel is not on list already
                        {
                            FavoriteChannelsList.Add(channelToAdd);

                            StreamWriter sw = new StreamWriter(favChannelsFilePath, true);
                            sw.WriteLine(channelToAdd);
                            sw.Close();

                            // splitted[1] instead of usernameToIgnore because of caps :)
                            msg("Added channel " + splitted[1] + " to FavoriteChannels list!");
                        }
                        else
                        {
                            msg(splitted[1] + " is already on the favorite channels list.");
                        }
                    }
                }

                if (rcmm.text.StartsWith("/removechannel") || rcmm.text.StartsWith("/rc"))
                {
                    String[] splitted = rcmm.text.Split(' ');

                    if (splitted.Length >= 2)
                    {
                        String channeltoRemove = splitted[1].ToLower();

                        if (FavoriteChannelsList.Contains(channeltoRemove)) //If channel is not on list already
                        {
                            FavoriteChannelsList.Remove(channeltoRemove);

                            File.Delete(favChannelsFilePath);

                            StreamWriter sw = new StreamWriter(favChannelsFilePath, true);
                            foreach (string s in FavoriteChannelsList)
                            {
                                sw.WriteLine(s);
                            }
                            sw.Close();

                            // splitted[1] instead of usernameToIgnore because of caps :)
                            msg("Removed channel " + splitted[1] + " from FavoriteChannels list!");
                        }
                        else
                        {
                            msg(splitted[1] + " was not on the Favorite Channels list!");
                        }
                    }                    
                }
              
                if(rcmm.text.StartsWith("/acc") || rcmm.text.StartsWith("/addcurrentchannel"))
                {
                    String channelToAdd = App.ArenaChat.ChatRooms.GetCurrentRoom().ToLower();

                    if (!FavoriteChannelsList.Contains(channelToAdd) && (!channelToAdd.Contains("trading") && !channelToAdd.Contains("general"))) //If channel is not on list already
                    {
                        FavoriteChannelsList.Add(channelToAdd);

                        StreamWriter sw = new StreamWriter(favChannelsFilePath, true);
                        sw.WriteLine(channelToAdd);
                        sw.Close();

                        // splitted[1] instead of usernameToIgnore because of caps :)
                        msg("Added channel " + channelToAdd + " to FavoriteChannels list!");
                    }
                    else
                    {
                        msg(channelToAdd + " is already on the favorite channels list.");
                    }
                }

                if (rcmm.text.StartsWith("/rcc") || rcmm.text.StartsWith("/removecurrentchannel") || rcmm.text.StartsWith("/remcurchan"))
                {
                    String channeltoRemove = App.ArenaChat.ChatRooms.GetCurrentRoom().ToLower();

                    if (FavoriteChannelsList.Contains(channeltoRemove)) //If channel is on list already
                    {
                        FavoriteChannelsList.Remove(channeltoRemove);

                        File.Delete(favChannelsFilePath);

                        StreamWriter sw = new StreamWriter(favChannelsFilePath, true);
                        foreach (string s in FavoriteChannelsList)
                        {
                            sw.WriteLine(s);
                        }
                        sw.Close();

                        // splitted[1] instead of usernameToIgnore because of caps :)
                        msg("Removed channel " + channeltoRemove + " from FavoriteChannels list!");
                    }

                    else
                    {
                        msg(channeltoRemove + " is already on the favorite channels list.");
                    }
                }

                if(rcmm.text.StartsWith("/listfavorites") || rcmm.text.Contains("/lf"))
                {
                    msg("Current favorite channels:");
                    
                    if (FavoriteChannelsList.Count == 0)
                        msg("There are currently no favorite channels added! You can add a channel by typing '/ac [channelname]' or if you want to add your current channel, you can type '/acc'");
                   
                    else
                    {
                        foreach (string s in FavoriteChannelsList)
                        {
                            msg(s);
                        }
                    }
                }
            }
            return false;
        }

		public override void AfterInvoke (InvocationInfo info, ref object returnValue)
		{
            return;
		}

        public static bool isEmpty(string filePath)
        {
            FileInfo f = new FileInfo(filePath);
            return (f.Length > 0) ? false : true;        
        }

        public void msg(String txt) //Cretids go t
        {
            RoomChatMessageMessage rcmm = new RoomChatMessageMessage();
            rcmm.from = "<color=#ffb400>FavoriteChannels</color>";
            rcmm.text = "<color=#f0dec5>" + txt + "</color>";
            rcmm.roomName = App.ArenaChat.ChatRooms.GetCurrentRoom();

            App.ChatUI.handleMessage(rcmm);
            App.ArenaChat.ChatRooms.ChatMessage(rcmm);
        }
	}
}


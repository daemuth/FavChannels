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
{/*
   ___                     _ _         ___ _                            _     
  / __\_ ___   _____  _ __(_) |_ ___  / __\ |__   __ _ _ __  _ __   ___| |___ 
 / _\/ _` \ \ / / _ \| '__| | __/ _ \/ /  | '_ \ / _` | '_ \| '_ \ / _ \ / __|
/ / | (_| |\ V / (_) | |  | | ||  __/ /___| | | | (_| | | | | | | |  __/ \__ \
\/   \__,_| \_/ \___/|_|  |_|\__\___\____/|_| |_|\__,_|_| |_|_| |_|\___|_|___/
                                
                            mod by levela.
                           Requested by TPC.
                         www.scrollsguide.com

                                                                              */
    public class FavoriteChannels : BaseMod
    {
        private string favChannelsFilePath;
        private string fileName;
        public string debug = "MANUAL DEBUG: ";

        List<string> FavoriteChannelsList = new List<string> { };

        //initialize everything here, Game is loaded at this point
        public FavoriteChannels()
        {
            fileName = "favChannels.txt";
            favChannelsFilePath = this.OwnFolder() + Path.DirectorySeparatorChar + fileName;
        }

        public static string GetName()
        {
            return "FavoriteChannels";
        }

        public static int GetVersion()
        {
            return 3;
        }

        public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
        {
            try
            {
                return new MethodDefinition[] {
                    scrollsTypes["MainMenu"].Methods.GetMethod("Start")[0],
                    scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[]{typeof(Message)}),
                    scrollsTypes["ArenaChat"].Methods.GetMethod("RoomEnter")[0],
            	};
            }
            catch
            {
                return new MethodDefinition[] { };
            }
        }

        public override bool WantsToReplace(InvocationInfo info)
        {
            if (info.targetMethod.Equals("sendRequest"))
            {
                if (info.arguments[0] is RoomChatMessageMessage)
                {
                    RoomChatMessageMessage rcmm = (RoomChatMessageMessage)info.arguments[0];

                    if (rcmm.text.Equals("/acc") || rcmm.text.StartsWith("/addcurrentchannel"))
                    {
                        String channelToAdd = App.ArenaChat.ChatRooms.GetCurrentRoomName().ToLower();

                        if (!FavoriteChannelsList.Contains(channelToAdd) && (!channelToAdd.Contains("trading") && !channelToAdd.Contains("general"))) //If channel is not on list already
                        {
                            FavoriteChannelsList.Add(channelToAdd);

                            StreamWriter sw = new StreamWriter(favChannelsFilePath, true);
                            sw.WriteLine(channelToAdd);
                            sw.Close();

                            // splitted[1] instead of usernameToIgnore because of caps :)
                            msg("Added channel " + channelToAdd + " to FavoriteChannels list!");
                            return true;
                        }
                        else if (channelToAdd.Contains("trading") || channelToAdd.Contains("general"))
                        {
                            msg("Sorry but you may not add trading / general rooms to favorites.");
                            return true;
                        }

                        else
                            msg("Channel " + channelToAdd + " is already on the favorite channels list.");

                        return true;
                    }


                    if (rcmm.text.StartsWith("/addchannel") || rcmm.text.StartsWith("/ac"))
                    {
                        String[] splitted = rcmm.text.Split(' ');

                        if (splitted.Length >= 2)
                        {
                            string channelToAdd = String.Join(" ", splitted, 1, splitted.Length - 1);

                            if (!FavoriteChannelsList.Contains(channelToAdd) && (!channelToAdd.Contains("trading") && !channelToAdd.Contains("general"))) //If channel is not on list already
                            {
                                FavoriteChannelsList.Add(channelToAdd);

                                StreamWriter sw = new StreamWriter(favChannelsFilePath, true);
                                sw.WriteLine(channelToAdd);
                                sw.Close();

                                msg("Added channel " + channelToAdd + " to FavoriteChannels list!");
                                return true;
                            }

                            else if (splitted[1].Contains("trading") || splitted[1].Contains("general"))
                            {
                                msg("Sorry but you may not add trading / general rooms to favorites.");
                                return true;
                            }

                            else
                                msg("Channel " + channelToAdd + " is already on the favorite channels list.");

                        }

                        return false;
                    }

                    if (rcmm.text.StartsWith("/removecurrentchannel") || rcmm.text.Equals("/rcc"))
                    {
                        String channeltoRemove = App.ArenaChat.ChatRooms.GetCurrentRoomName().ToLower();

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
                            msg("Channel " + channeltoRemove + " was not on the favorites list.");
                        }

                        return true;
                    }

                    if (rcmm.text.StartsWith("/removechannel") || rcmm.text.StartsWith("/rc"))
                    {
                        String[] splitted = rcmm.text.Split(' ');

                        if (splitted.Length >= 2)
                        {
                            string channeltoRemove = String.Join(" ", splitted, 1, splitted.Length - 1);

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

                                msg("Removed channel " + channeltoRemove + " from FavoriteChannels list!");
                            }

                            else
                                msg(channeltoRemove + " was not on the Favorite Channels list!");
                        }

                        return true;
                    }

                    if (rcmm.text.Equals("/listfavorites") || rcmm.text.Equals("/lf") || rcmm.text.Equals("/favorites"))
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

                        return true;
                    }

                    if (rcmm.text.Equals("/fhelp") || rcmm.text.Equals("/fcommands") || rcmm.text.Equals("/favoriteshelp"))
                    {
                        msg("Favorite Channels command list:");
                        printCommands();
                        return true;
                    }

                }

                return false;
            }

            return false;
        }

        public override void BeforeInvoke(InvocationInfo info)
        {
            if (info.targetMethod.Equals("Start"))
            {
                if (!File.Exists(favChannelsFilePath))
                     File.Create(favChannelsFilePath).Close();
                

                if (isEmpty(favChannelsFilePath))
                    return;

                else
                {
                    Console.WriteLine("File is not empty, gathering favorite channels!");
                    string line;

                    using (StreamReader reader = new StreamReader(favChannelsFilePath))
                    {
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!line.Contains("trading") && !line.Contains("general"))
                            {
                                FavoriteChannelsList.Add(line);
                                App.ArenaChat.RoomEnter(line);
                            }
                        }
                    }
                }
                
                return;
            }            
        }

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {
            return;
        }

        public static bool isEmpty(string filePath)
        {
            return (new FileInfo(filePath).Length == 0) ? true : false;
        }

        public void msg(String txt) //Cretids go t
        {
            RoomChatMessageMessage rcmm = new RoomChatMessageMessage();
            rcmm.from = "<color=#ffb400>FavoriteChannels</color>";
            rcmm.text = "<color=#f0dec5>" + txt + "</color>";
            rcmm.roomName = App.ArenaChat.ChatRooms.GetCurrentRoomName();

            App.ChatUI.handleMessage(rcmm);
            App.ArenaChat.ChatRooms.ChatMessage(rcmm);
        }

        public void printCommands()
        {
            msg("/acc, /addcurrentchannel -> Adds current channel to favorites");
            msg("/rcc, /removecurrentchannel -> Removes current channel from favorites");
            msg("/ac [channelname], /addchannel [channelname] -> Adds [channelname] to favorites");
            msg("/rc [channelname], /removechannel [channelname] -> Removes [channelname] from favorites");
            msg("/lf, /listfavorites -> Lists your favorite channels");
            msg("/fhelp, /fcommands -> Prints command list");
        }
    }
}


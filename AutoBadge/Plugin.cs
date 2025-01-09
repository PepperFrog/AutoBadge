using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs.Player;
using UnityEngine;


namespace AutoBadge
{
    public class Plugin : Plugin<Config>
    {
        public override string Name { get; } = "AutoBadge";
        public override string Author { get; } = "Antoniofo";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 2, 2);
        public static Plugin Instance = null;

        public override void OnEnabled()
        {
            Instance = this;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Instance = null;
            base.OnDisabled();
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.Group is null)
            {
                string playername = ev.Player.Nickname.ToLower();
                foreach (var word in Config.MagicWords)
                {
                    if (playername.Contains(word.ToLower()))
                    {
                        ev.Player.Group = Server.PermissionsHandler.GetGroup(Config.SpecialGroup);
                    }    
                }
            }
        }
    }

    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public List<string> MagicWords { get; set; } = new List<string>() {"pepper"};
        public string SpecialGroup { get; set; } = "frog";
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public class SpecialGroup : ICommand
    {
        public string Command { get; } = "refreshgroup";
        public string[] Aliases { get; } = { "regrp" };
        public string Description { get; } = "Refresh the special group. If you have the magic word but not the group";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player ply = Player.Get(sender);
            if (ply == null)
            {
                response = "Your are the DEDICATED_SERVER WTF DO YOU MEAN";
                return false;
            }
            
            if (ply.Group is null || ply.Group.BadgeText.Equals(Server.PermissionsHandler.GetGroup(Plugin.Instance.Config.SpecialGroup)
                    .BadgeText))
            {
                
                string playername = ply.Nickname.ToLower();
                foreach (var word in Plugin.Instance.Config.MagicWords)
                {
                    if (playername.Contains(word.ToLower()))
                    {
                        if (Server.PermissionsHandler.GetGroup(Plugin.Instance.Config.SpecialGroup) == null)
                        {
                            response = "The group doesn't exist contact staff";
                            return false;
                        }

                        ply.Group = Server.PermissionsHandler.GetGroup(Plugin.Instance.Config.SpecialGroup);
                        response = "Your group has been refreshed";
                        return true;
                    }
                    else
                    {
                        response = "Your name does not contain the magic word";
                        return false;
                    }
                }
                
            }
            
            response = "Your already are in a group and can't be in two at the same time";
            return false;
        }
    }
}
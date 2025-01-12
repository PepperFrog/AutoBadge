using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Core.UserSettings;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs.Player;
using UserSettings.ServerSpecific;


namespace AutoBadge
{
    public class Plugin : Plugin<Config, Translation>
    {
        public override string Name { get; } = "AutoBadge";
        public override string Author { get; } = "Antoniofo";
        public override Version Version { get; } = new Version(1, 1, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 3, 0);
        public static Plugin Instance = null;

        public override void OnEnabled()
        {
            Instance = this;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSetting;
            HeaderSetting header = new HeaderSetting("AutoBadge");
            IEnumerable<SettingBase> setttings = new SettingBase[]
            {
                new ButtonSetting(Config.ButtonId, Translation.Label, Translation.ButtonText,
                    hintDescription: Translation.HintDescription, header: header)
            };
            SettingBase.Register(setttings);
            SettingBase.SendToAll();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSetting;
            Instance = null;
            base.OnDisabled();
        }


        private void OnSetting(ReferenceHub hub, ServerSpecificSettingBase settings)
        {
            if (!Player.TryGet(hub, out Player ply))
            {
                return;
            }

            if (settings is SSButton button && button.SettingId == Config.ButtonId &&
                button.SyncLastPress.IsRunning == true)
            {
                if (ply.Group is null || ply.Group.BadgeText.Equals(Server.PermissionsHandler
                        .GetGroup(Config.SpecialGroup)?
                        .BadgeText))
                {
                    string playername = ply.Nickname.ToLower();
                    foreach (var word in Config.MagicWords)
                    {
                        if (playername.Contains(word.ToLower()))
                        {
                            if (Server.PermissionsHandler.GetGroup(Config.SpecialGroup) == null)
                            {
                                Log.Warn("The group " + Config.SpecialGroup + " doesn't exist.");
                            }

                            ply.Group = Server.PermissionsHandler.GetGroup(Config.SpecialGroup);
                        }
                    }
                }
            }
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            ServerSpecificSettingsSync.SendToPlayer(ev.Player?.ReferenceHub);
            if (ev.Player?.Group is null)
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

        public List<string> MagicWords { get; set; } = new List<string>() { "pepper" };
        public string SpecialGroup { get; set; } = "frog";

        [Description("Please change this if another setting has the same id")]
        public int ButtonId { get; set; } = 667;
    }

    public class Translation : ITranslation
    {
        public string Label { get; set; } = "Reload my badge";
        public string ButtonText { get; set; } = "Reload";
        public string HintDescription { get; set; } = "Reload your autobadge if you name contains a magic word.";
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

            if (ply.Group is null || ply.Group.BadgeText.Equals(Server.PermissionsHandler
                    .GetGroup(Plugin.Instance.Config.SpecialGroup)?
                    .BadgeText))
            {
                string playername = ply.Nickname.ToLower();
                foreach (var word in Plugin.Instance.Config.MagicWords)
                {
                    if (playername.Contains(word.ToLower()))
                    {
                        if (Server.PermissionsHandler.GetGroup(Plugin.Instance.Config.SpecialGroup) == null)
                        {
                            Log.Warn("The group " + Plugin.Instance.Config.SpecialGroup + " doesn't exist.");
                            response = "The group " + Plugin.Instance.Config.SpecialGroup +
                                       " doesn't exist. please contact staff";
                            return false;
                        }

                        ply.Group = Server.PermissionsHandler.GetGroup(Plugin.Instance.Config.SpecialGroup);
                        response = "Your group has been refreshed";
                        return true;
                    }
                }

                response = "Your name does not contain the magic word";
                return false;
            }

            response = "Your already are in a group and can't be in two at the same time";
            return false;
        }
    }
}
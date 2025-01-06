using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs.Player;


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
            if (ServerStatic.PermissionsHandler.GetGroup(Config.SpecialGroup) != null)
            {
                Exiled.Events.Handlers.Player.Verified += OnVerified;    
            }
            else
            {
                Log.Warn("The special group does not exist. please create it first and restart your server");
            }
            Instance = this;
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
                if (playername.Contains(Config.MagicWord.ToLower()))
                {
                    ev.Player.Group = ServerStatic.PermissionsHandler.GetGroup(Config.SpecialGroup);
                }
            }
        }
    }

    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public string MagicWord { get; set; } = "pepper";
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
            if (ply.ReferenceHub.IsHost)
            {
                response = "Your are the DEDICATED_SERVER WTF DO YOU MEAN";
                return false;
            }

            if (ply.Group is null)
            {
                string playername = ply.Nickname.ToLower();
                if (playername.Contains(Plugin.Instance.Config.MagicWord.ToLower()))
                {
                    ply.Group = ServerStatic.PermissionsHandler.GetGroup(Plugin.Instance.Config.SpecialGroup);
                    response = "Your group has been refreshed";
                    return true;
                }
                else
                {
                    response = "Your name does not contain the magic word";
                    return false;
                }
            }

            response = "Your already are in a group and can't be in two at the same time";
            return false;
        }
    }
}
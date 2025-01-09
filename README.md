# AutoBadge

## What it does

This plugin automatically adds a badge to a user that have some magic word in their name, only if he's not in a group already. You can reload the badge with the `.regrp` in the client console. 

## Installation

You can install this plugin, download the [.dll](https://github.com/pepperfrog/AutoBadge/releases) file and placing it in ``%AppData%\Roaming\EXILED\Plugins`` (Windows) or ``~/.config/EXILED/Plugins`` (Linux)

## Configuration

```yaml
auto_badge:
  is_enabled: true
  debug: false
  magic_words:
  - 'pepper'
  - 'frog'
  special_group: 'frog'
```

## Command

`.regrp` : Reload the badge
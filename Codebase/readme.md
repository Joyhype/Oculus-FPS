I have a couple of prefabs in the Codebase project, check it out (the quest for instance)

Player stuff:
- LootGrabber goes onto the player model (grabs collectibles from ground)
- DryThroat goes onto the player (Doesn't really do anything yet though)
- Pippa goes onto the gun (needs a bullet prefab, muzzleflash prefab, and a guntip)
- Bullet goes onto your bullet prefab
- Reticle goes onto the player as well, you need to create a layer named "Interactable" and put it on items that you want to be able to gaze shit

Collectible:
- Blinker goes onto collectible, makes the object blink & disappear after delay (always enable for now: IncreaseBlinkSpeed, otherwise it won't disappear)
- Collectible goes onto collectible as well, makes it disappear when you pick it up

Misc:
- LunarCycle goes on Moon and Sun object (with their lights etc as children so that they move with the cycle)

Quest:
- You can make quests with the KillQuest, CollectQuest, and KillCollectQuest
	You need to make tags for the items you need to collect and/or kill, you also have to put those on the actual prefabs of items/mobs
- Questgaze goes onto quest object

!!!!this is still a work in progress!!!!

```
  _    _       _                          _      __  __       _ _   _       _                            ___  
 | |  | |     (_)                        | |    |  \/  |     | | | (_)     | |                          |__ \ 
 | |  | |_ __  ___   _____ _ __ ___  __ _| |    | \  / |_   _| | |_ _ _ __ | | __ _ _   _  ___ _ __        ) |
 | |  | | '_ \| \ \ / / _ \ '__/ __|/ _` | |    | |\/| | | | | | __| | '_ \| |/ _` | | | |/ _ \ '__|      / / 
 | |__| | | | | |\ V /  __/ |  \__ \ (_| | |    | |  | | |_| | | |_| | |_) | | (_| | |_| |  __/ |        / /_ 
  \____/|_| |_|_| \_/ \___|_|  |___/\__,_|_|    |_|  |_|\__,_|_|\__|_| .__/|_|\__,_|\__, |\___|_|       |____|
                                                                     | |             __/ |                    
                                                                     |_|            |___/                     
```

I forget to update this readme a lot, so check the docs/commits/releases for more up to date info

What this can do right now:
- UDP, TCP, and HTTP connections (works with all combinations other than only UDP)
- Host server directly from game (peer to peer)
- WebGL build (working status can change from commit to commit since it's not important right now)
- Transform syncing with one script (and a prefab)
  - some cool optimizations:
      - freeze when there isnt movement
- Automatic and heafty ID system for:
  - clients
  - objects
  - variables
- Extremly chunky and flexible event system
- Two types of network variables:
  - global are kind of like player prefs
  - object based are linked to each object
- global methods (on connect, on player leave...)
- docs that I actually use and test
- auto client timeouts
- inspector variables for pretty much everything
  - can be used for essentially every multiplayer game type
- clients arent directly linked to gameobjects (like a player character)
- network variable callbacks that get triggered on change
- really easy package based updates
  

What this will be able to do in the future:
- Dedicated server (might even be written in c++ in the future)
- Websockets: while http is great, its neither fast nor 2 way
- animation syncing
- quick network objects (for particle systems, bullets...)
- LOD-like transform update optimizations
- programming beginner's test info (for docs and simplification)

What this will be able to do far in the future:
- Voice chat (this might actually not be that far off)
- Encrypted messages for at least a lil security

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
       
       

What this can do right now:
- UDP, TCP, and HTTP connections (works with all combinations other than only UDP)
- Host server directly from game (peer to peer)
- WebGL build (working status can change from commit to commit since it's not important right now)
- Transform syncing with one script (and a prefab)
- Automatic ID system for both synced objects and players
- Extremly chunky and flexible event system
- Variable syncing that is used like playerPrefs
- Good network debugging
- Network methods - call another client's method
- global methods (on connect...)

What this will be able to do in the future:
- Dedicated server (might even be written in c++ in the future)
- Websockets: while http is great, its neither fast nor 2 way
- animation syncing
- quick network objects (for particle systems, bullets...)

What this will be able to do far in the future:
- Voice chat (this might actually not be that far off)
- Encrypted messages for at least a lil security

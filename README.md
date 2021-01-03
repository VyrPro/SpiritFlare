# SpiritFlare
SpiritFlare is a in-dev game, powered by the GreyFlare game engine (built on the MonoGame Framework).
Above are all the current files for the client.

The engine is structured as such:
All of the class objects are located in GameClasses.cs, which involves the structure for the player, dieties, NPCs, etc.
All of the pre-runtime loading is done is LoadContent.cs, which involves the loading of sprites, and population of collections with data.
The game itself is processed in Game1.cs, including the rendering, drawing, movement, and Update() function.
Finally, all data handling (saving, loading, writing, etc.) is within DataHandler.cs.

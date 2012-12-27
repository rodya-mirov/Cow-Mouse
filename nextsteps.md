Next Steps for Operation Cow Mouse
==================================

Roughly speaking, there are four (mostly disjoint) directions
that all need to be gone down a significant way.

Tile Engine
-----------

Obviously, I need to fix the damn jittering.  Ugh!

Town Mode
---------

1. Task scheduling
> Currently, the townspeople have (extremely basic) AI, and have
> capacity for path finding/following and basic tasks.  But the
> current system has some limitations, and could be improved.
> 
> The natural way to do this would be to implement a "scheduled task"
> system, where the WorldManager could schedule universal tasks,
> like "build this wall," while the NPCs could schedule personal tasks,
> like "get a snack."  Then the "find thing to do" method in NPCs would
> just pick the highest priority task and go do that.
>
> This makes things like construction and crafting more natural to
> frame and accomplish.  To some degree, the current system would
> support that, but it's not as natural; it would be better to make
> tasks more natural, so they can be added more readily later on.

2. NPCs given more to do (probably best to do task scheduling first)
> 1. Walls actually built, instead of magically appearing.
> 2. Eating food (and food existing)
> 3. Owning rooms instead of just piling up
> 4. Crafting (generally)?

3. NPCs given more attributes
> 1. Names, randomly generated from Markov chains, or just from a premade list
> 2. Gender
> 3. Adventurer-type attributes (health/str/etc.)
> 4. All should be displayed on follow.

Adventure Mode
--------------

1. Something about violence :)

Graphics
--------

1. 8-directional sprites, where necessary
2. More human sprite types (e.g. children, and eventually some professions with specific sprites).

World
-----

1. More interesting world, with pieces that do things

> 1. Set pieces, with value: trees, at least
> 2. Wild animals? (this might fall more under adventure mode)
> 3. Terrain features (hills, water, something)


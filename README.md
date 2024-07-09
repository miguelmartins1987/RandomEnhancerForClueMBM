# RandomEnhancerForClueMBM

A silly little project whose goal is to (ultimately) "enhance" the random number generation in Clue: Murder at Boddy Mansion (version 2.2 only!).
Right now, it intercepts (i.e. hooks) the rand() routine, replacing the values returned by it, but only when the player rolls a die.

This project uses both https://github.com/EasyHook/EasyHook and https://api.random.org/

# Planned work

* Also "enhance" the random values for picking the suspect, weapon, and location.
* Fallback to an offline random number generator, in the event that it's impossible to obtain random numbers from https://api.random.org/

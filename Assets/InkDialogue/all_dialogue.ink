EXTERNAL relationship_points(characterName, points)
EXTERNAL get_relationship_status(characterName)
EXTERNAL get_relationship_points(characterName)
EXTERNAL update_relationship_status(characterName)

=== DEBUGnpc ===
VAR speakerName = "The NPC Guy"

{get_relationship_status("DebugNPC") == "Neutral": Hello there, stranger.} 
{get_relationship_status("DebugNPC") == "Aquainted": Oh, hi! Good to see you again.}
{get_relationship_status("DebugNPC") == "Friendly": Hey friend! How are you doing?}
{get_relationship_status("DebugNPC") == "Allied": My trusted ally! What brings you here?}
{get_relationship_status("DebugNPC") == "Romanced": My love! I've been waiting for you.}
This is the content of the knot.
Hellooooo.
* [Ask about her day] {relationship_points("DebugNPC", 2)}
    Thank you for asking! I've been working in the garden all morning.
* [Compliment her appearance] {relationship_points("DebugNPC", 3)}
    Oh my, thank you! That's very sweet of you to say.
    {get_relationship_status("DebugNPC") == "Friendly": You always know how to make me smile.}
* [Ask for help with something] {relationship_points("DebugNPC", 1)}
    Of course! I'm always happy to help a friend.
* [Be rude] {relationship_points("DebugNPC", -3)}
    Excuse me? That was quite rude. I think we're done here.
    -> END
- -> END

=== DEBUGnpc ===
VAR speakerName = "The NPC Guy"
This is the content of the knot.
Hellooooo.
* [erm hi]
    hi!
* [erm bai]
    kbai
* {can_accept_quest("find_artifact")} [Accept the artifact quest]
    ~ accept_quest("find_artifact")
    Great! You've accepted the quest to find the ancient artifact. Check your quest log for details.
* {can_accept_quest("delivery_mission")} [I can help with deliveries]
    ~ accept_quest("delivery_mission")
    Excellent! Please deliver this package to the merchant in town.
* [Ask about available quests] -> available_quests
- -> END

=== available_quests ===
Here are the quests I have available:
* {can_accept_quest("find_artifact")} [Tell me about the artifact quest]
    I need someone brave to find an ancient artifact hidden in the old ruins. It's dangerous, but the reward is substantial.
    ** {can_accept_quest("find_artifact")} [I'll do it!]
        ~ accept_quest("find_artifact")
        Wonderful! The artifact should be in the deepest chamber of the ruins. Be careful!
    ** [Maybe later]
        No problem, take your time to decide.
* {can_accept_quest("delivery_mission")} [What about delivery work?]
    I have a package that needs to be delivered to the merchant in the town square. Simple but important.
    ** {can_accept_quest("delivery_mission")} [I can handle that]
        ~ accept_quest("delivery_mission")
        Perfect! Here's the package. The merchant is expecting it today.
    ** [Not interested]
        Fair enough, delivery work isn't for everyone.
* [Never mind] -> END
- -> END

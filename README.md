# Project Warden

**Turn-Based RPG · Unity · C# · Procedural Generation · Developer Tools · Local AI**

Project Warden is a turn-based RPG set within a massive, dystopian tower inspired by the Tower of Babel and the same original universe first explored in *Echoes of Hubris*.

Imprisoned at the base of an ever-ascending labyrinth, the protagonist is forced into the service of its Warden, who exploits them to produce potions for his own purposes. As the player ventures deeper into the labyrinth, they encounter a recurring cast of characters while gradually uncovering more about their circumstances, the Warden, and the place in which they are trapped.

These recurring characters are also the focus of an experimental AI-driven dialogue system designed to support dynamic conversations, persistent character context, and fully local inference without requiring cloud services or paid API tokens.

Project Warden began development in May 2025 as an 8-person team project under 4Fates Studios. Following its initial team development period, active production was scaled back while core systems continued to be developed and integrated. Development currently focuses on bringing those systems and the team's existing work together into a cohesive vertical slice.

## Gameplay Systems

Project Warden is being built around modular systems intended to support a larger turn-based RPG while allowing content to be created and iterated on efficiently.

### Procedural Labyrinth Generation

The tower's explorable environments use a configurable procedural generation system capable of constructing labyrinth layouts from reusable rooms and hallways.

The system supports:

- Configurable room counts
- Variable hallway depth
- Seeded generation
- Reusable room and hallway content
- Rapid generation of unique level layouts

### Interaction System

A modular interaction framework provides reusable components for gameplay interactions throughout the project.

The system supports more than 10 interaction types while allowing new behaviors to be added without rebuilding the underlying interaction architecture.

### Inventory & Item Systems

The inventory architecture uses reusable item and slot systems designed to support different gameplay contexts.

Current systems include:

- Inventory and equipment management
- Multiple slot types
- Extensible item behaviors
- Reusable item definitions
- Integration with gameplay interactions

## Developer Tools

A major focus of Project Warden has been building tools that allow the development team to create and modify content without requiring repetitive manual setup.

### Room & Hallway Editor Tools

Custom Unity Editor tooling was developed for creating and configuring procedural-generation content.

The tools provide custom inspectors, procedural configuration options, and automatic linking between prefabs and ScriptableObjects.

This reduced the typical process of creating and configuring a new room from approximately **15 minutes to under 1 minute**.

### Data Import Pipeline

A custom CSV importer connects structured design data with Unity content.

The pipeline:

- Processes more than 100 entries in under 5 seconds
- Creates and updates project data automatically
- Detects removed entries
- Performs type validation
- Logs individual create, update, and delete operations

The workflow was designed around project documentation maintained in Confluence, allowing structured design information to be transferred into the game with significantly less manual data entry.

### Development Logging

A category-based logging system allows developers to isolate messages from specific gameplay systems rather than searching through unrelated Unity output.

The tool reduced average issue-location time by approximately **30%** during team development.

## AI-Driven Character Dialogue

One of Project Warden's central technical experiments is creating dynamic dialogue for recurring party characters using locally hosted language models.

Rather than relying on cloud inference or paid API tokens, the system is being designed with the long-term goal of running entirely on the player's machine without requiring an internet connection.

This introduces an additional engineering constraint: balancing dialogue quality against model size, inference performance, memory usage, and accessibility across consumer hardware.

The dialogue architecture is being explored around several concepts:

- Small locally hosted language models
- Character-specific prompting
- Retrieval-Augmented Generation (RAG)
- Character and world knowledge retrieval
- Persistent conversational context
- Conversation-to-context generation
- Multiple interchangeable model backends
- Human evaluation of generated dialogue

Recurring party members are the primary intended users of this system, allowing previous interactions and events to influence future conversations as the player progresses through the tower.

## Experimental AI Backend

The dialogue infrastructure is being developed separately from the primary Unity project as an experimental Python backend.

The architecture is designed to separate individual components such as:

- LLM inference backends
- Prompt management
- Character and world data
- Embedding and retrieval
- Conversation services
- Persistent context
- External clients
- Project-data ingestion

Character information can originate from Project Warden's existing Confluence documentation and be transformed into retrievable knowledge for the dialogue system.

Another area of experimentation is **context creation**. Instead of retaining entire conversation histories indefinitely, previous interactions can be condensed into new character context and stored for later retrieval, allowing important information from earlier conversations to contribute to future dialogue.

Internal Discord integration is also being explored as a development and evaluation interface. Generated conversations can be presented to team members for feedback, with responses intended to contribute to dialogue evaluation data and future model experimentation.

Longer-term experimentation includes using human-rated dialogue data to fine-tune smaller existing language models and compare their behavior against their original base models.

> The AI backend is experimental and under active development. Several components described above represent ongoing work rather than completed production features.

## Development & Team

Project Warden was initiated in May 2025 and originally developed by an **8-person team** under 4Fates Studios.

The initial development period established the project's gameplay systems, content, art, design, tooling, and technical foundations. As the project expanded beyond its original scope, active team production was later paused.

Development currently continues on a smaller scale, primarily focused on integrating and completing existing systems toward a playable vertical slice.

### Development Leadership

Project development has included:

- Establishing Git and GitHub development workflows
- Organizing tasks and milestones through Jira
- Maintaining design and technical documentation in Confluence
- Developing internal tools used across the team
- Documenting custom workflows and development systems

Custom development tools created for the project reached **100% adoption across the original development team**.

## Repository Structure

Project Warden is developed across multiple repositories under 4Fates Studios.

This repository contains the public Unity project and source code. Additional production assets and experimental AI infrastructure are maintained separately and are not currently public.

The AI backend is intentionally being developed with increasing separation from Project Warden itself, allowing its architecture and experimentation to potentially support projects beyond the game.

## Technologies

**Game Development**
- Unity
- C#

**AI & Backend Experimentation**
- Python
- FastAPI
- Ollama
- Retrieval-Augmented Generation
- Local language models

**Development & Collaboration**
- Git
- GitHub
- Jira
- Confluence

## Status

**In Development**

Current development is focused on consolidating Project Warden's existing gameplay, content, tooling, and experimental systems into a cohesive vertical slice.

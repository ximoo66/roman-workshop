# Roman Workshop

On-site augmented reality learning experience created for UNESCO Global Geopark Bergstraße-Odenwald.

[Experience, trailer and project details](https://www.omidameri.com/project-roman.html) · [Omid Ameri's portfolio](https://www.omidameri.com/)

## The experience

At Felsenmeer, visitors scan a marker to reveal a holographic Roman saw and a small workshop. They collect virtual wood, stone and leaves from corresponding features in their surroundings, drag them into an inventory, and use the workshop to produce saw components. The components are placed into the transparent saw. The completed mechanism animates with explanatory feedback.

## Explore the implementation

This public repository is a source-code showcase of a collaborative university project. It contains selected project scripts, their Unity metadata, the package manifest and the original editor version. It does not contain the complete playable project.

Project code is organized under:

- `Assets/Scripts/`


| Area | Entry point |
| --- | --- |
| Marker anchoring | [QRAnchor.cs](Assets/Scripts/QRAnchor.cs) |
| Resource inventory | [InventoryManager.cs](Assets/Scripts/InventoryManager.cs) |
| Crafting components | [WorkshopStation.cs](Assets/Scripts/WorkshopStation.cs) |
| Saw assembly | [ToolAssemblyStation.cs](Assets/Scripts/ToolAssemblyStation.cs) |
| Dragging components into the scene | [UIDragToWorldToolPart.cs](Assets/Scripts/UIDragToWorldToolPart.cs) |
| Progress feedback | [WorkshopProgressUI.cs](Assets/Scripts/WorkshopProgressUI.cs) |

## Dependencies and running the project

The original project uses Unity **6000.2.7f2**. Review `Packages/manifest.json` inside the project folder for its package dependencies. To use these scripts, create an appropriate Unity project and restore the required packages. Scenes, prefabs, input bindings, art, audio and third-party plugins must be obtained and configured separately; cloning this showcase alone will not reproduce the game or experience.

Third-party Asset Store packages, vendor SDK source, course starter code, tutorial examples, models, textures, audio, compiled builds and generated editor files are intentionally excluded. Any plugins referenced by the scripts must be installed from their original publishers under the applicable licenses.

## Authorship and provenance

Developed collaboratively by the project team, including Omid Ameri. The code is presented as team work, not as an assertion that every file was written by one person. The complete project, contributor history, branches and tags are preserved in a separate private archive. This public showcase begins with a fresh snapshot so excluded files cannot be recovered from older commits.

No blanket open-source license is granted by this publication. Existing authorship and rights remain applicable; obtain permission from the relevant authors before reusing code.

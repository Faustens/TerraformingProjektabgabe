# TerraformingAbgabe
Abgabedateien des Projekts "Kollaboratives Terraforming in VR" des großen Studienprojekts
1. Projekt starten
Das Projekt kann als .zip-Datei heruntergeladen werden. Diese muss dann entpackt werden. Man öffnet Unity Hub und drückt auf „ADD“ und wählt hier den Projekt Ordner namens TerraformingAbgabe-main aus. Dann startet sich das Projekt und man zieht aus den Dateien (unten) die Datei Assets/scenes/Demo auf die linke Seite und dort muss man dann noch Untitled Scene über die drei Punkte removen. Jetzt kann man entweder „Play“ drücken oder was wir empfehlen unter File Build and Run zu wählen und diesen Build in einem Ordner abzuspeichern. In dem ausgewählten Ordner befindet sich dann eine .exe-Datei. Diese kann man mehrfach ausführen (jede Anwendung ein neuer Client). Damit der jeweilige Client auch den Server findet, sollte der Server vorher gestartet sein. Um den Server zu starten gibt es auch mehrere Möglichkeiten: Der Server befindet sich im Ordner Terraform Server. Dieses Projekt kann man z.B. mit Visual Studio öffnen und hier die TerraformServer.cs-Datei ausführen. Alternativ kann man auch die exe in bin/Debug/netcodeapp3.1 ausführen. (Eventuell macht hier der Virenschutz Probleme)

2. Funktionen
Material kann man hinzufügen indem man die Taste „B“ auf der Tastatur bzw. den „PrimaryButton“ am Controller drückt. Man kann Material abtragen indem man die Taste „N“ auf der Tastatur bzw. den „SecondaryButton“ am Controller drückt. An der Stelle der blauen Kugel findet diese Aktion statt. Mit dem „GripButton“ oder mit den Tasten „Leer+G“ für die eine Hand oder mit den Tasten „Shift+G“ für die andere kann man sich zum gewünschten Ort teleportieren. Vorausgesetzt der Laser an der Hand ist weiß und nicht rot. Die folgende Steuerung sollte mit einem VR-Headset deutlich einfacher sein, da wir aber kein Headset zum Testen zur Verfügung hatten, folgt die Maus/Tastatur Steuerung (wichtigsten für mehr siehe XR Device Simulator):
Kopf bewegen: Mittlere Maustaste + Rechte Maustaste und dann Maus gewünscht bewegen
Linke Hand bewegen: Shift + Maus bewegen
Rechte Hand bewegen: Leer + Maus bewegen
Linke Hand drehen: Shift + Mittlere Maustaste + Maus bewegen
Rechte Hand drehen: Leer + Mittlere Maustaste + Maus bewegen
Man kann, wenn sich mehrere Nutzer auf dem Server aufhalten, ihre Position sehen und alle ihre Terrainänderungen die sie durchführen. Zudem kann man auch nachträglich zum Server verbinden und bekommt alle Änderungen gesendet.

3. externe Bibliotheken und Quellen
-"Scene Synchronization for Real-Time Interaction in Distributed Mixed Reality and Virtual Reality Environments" von  Hamza-Lup, Felix G., Rolland und Jannick P. 
- "Zero latency: Real-time synchronization of BIM data in virtual reality for collaborative decision-making" von Jing Du, Zhengbo Zou, Yangming Shi und Dong Zhao
- "Challenges in Networking to Support Augmented Reality and Virtual Reality" von Cedric Westphal Huawei
- Rose, Thomas J., Bakaoukas und Anastasios G. "Algorithms and Approaches for Procedural Terrain Generation - A Brief Review of Current Techniques"
- J. Alonso und R. Joan-Arinyo. "The Grounded Heightmap Tree - A New Data Structure for Terrain Representation"
- Burdea, G. C. & Coiffet, P. (2003). "Virtual reality technology"
- Elvezio, C., Ling, F., Liu, J.-S. & Feiner, S. (2018). Collaborative virtual reality for low-latency interaction.
- Gallup, D., Frahm, J.-M. & Pollefeys, M. (2011, 04). A heightmap model for efficient 3d reconstruction from street-level video.
- Gallup, D., Pollefeys, M. & Frahm, J.-M. (2010). 3d reconstruction using an n-layer heightmap.
- Helms, T. (2013). A voxel-based platform for game development
- Kundu, S. N., Muhammad, N. & Sattar, F. (2017). Using the augmented reality sandbox for advanced learning in geoscience education.
- Lorensen, W. & Cline, H. (1987, 08). Marching cubes: A high resolution 3d surface construction algorithm.
- Mulack, K. (2017). Parametrisierbare generierung einer dynamischen spielewelt
- Nguyen, H. (2014, 11). Proposition of new metaphors and techniques for 3d interaction and navigation preserving immersion and facilitating collaboration between distant users. - Parger, M., Mueller, J. H., Schmalstieg, D. & Steinberger, M. (2018). Human upper-body inverse kinematics for increased embodiment in consumergrade virtual reality.
- Perlin, K. (1985, jul). An image synthesizer.
- Sim, D. (2015). Real-time 3d reconstruction and semantic segmentation using multi-layer heightmaps.

XR Interaction Toolkit: https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/manual/index.html \n
XR Device Simulator: https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.10/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator.html \n
Compute Helper: https://github.com/SebLague/Reaction-Diffusion/tree/main/Assets/Scripts/Compute%20Helper \n
PerlinNoise: https://github.com/keijiro/NoiseShader
